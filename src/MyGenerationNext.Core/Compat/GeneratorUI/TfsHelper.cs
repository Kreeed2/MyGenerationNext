using System.Diagnostics;

namespace EHA.ZESA.Util.GeneratorUI
{
    /// <summary>
    /// TFVC integration for the headless port. Unlike the legacy WinForms version (TFS object model,
    /// see MyGenerationTemplates/BusinesslogicGeneratorUI/TfsHelper.cs), this drives the TFVC command
    /// line (<c>tf.exe checkout</c> / <c>tf add</c>), so no Team Explorer assemblies are required.
    /// <para>
    /// TFVC marks checked-in files read-only on disk; writing them fails with UnauthorizedAccessException.
    /// <see cref="CheckOut"/> pends an edit via <c>tf checkout</c> so files can be modified automatically.
    /// All operations are best-effort: when tf.exe cannot be found or fails, the read-only attribute is
    /// cleared as a fallback so generation can still proceed (the file is then NOT a pending edit in TFVC
    /// and shows up only via "detect local changes").
    /// </para>
    /// </summary>
    public class TfsHelper
    {
        private static readonly TfsHelper _instance = new();
        private static readonly Lock _tfLock = new();

        public static TfsHelper Instance => _instance;

        /// <summary>
        /// Master switch for the automatic checkout before editing (default true).
        /// Set to false for purely local runs or tests that must not touch the file system flags.
        /// </summary>
        public static bool Enabled { get; set; } = true;

        /// <summary>Optional log sink for checkout / fallback messages.</summary>
        public static Action<string>? Log { get; set; }

        /// <summary>
        /// Explicit path to tf.exe. When null, the environment variable MGN_TF_EXE, the PATH and the
        /// Visual Studio install locations (via vswhere) are searched. A set-but-missing path counts as
        /// "not found" and triggers the read-only fallback.
        /// </summary>
        public static string? TfExePath { get; set; }

        private string? _workspace;

        /// <summary>
        /// Workspace name (legacy CurrentTfSWorkspace). Informational only: <c>tf checkout</c> and
        /// <c>tf add</c> do not support <c>/workspace:</c>, so this value is never passed to tf.exe —
        /// tf resolves the workspace that owns the local path. Defaults to the machine name like the legacy UI.
        /// </summary>
        public string CurrentTfSWorkspace
        {
            get => _workspace ?? Environment.MachineName;
            set => _workspace = string.IsNullOrWhiteSpace(value) ? null : value;
        }

        /// <summary>No automatic check-in in the headless port (legacy checked the metadata server path in).</summary>
        public void CheckInMetaDataFile() { }

        /// <summary>Checks out the local metadata file so it can be overwritten.</summary>
        public void CheckOutMetaDataFile(string pMetadataFile) => CheckOut(pMetadataFile);

        /// <summary>Server-path specific ($/ZESA/Utilities); no-op in the headless port.</summary>
        public void GetUtilities() { }

        /// <summary>
        /// Pends an edit for the given local file when it exists and is read-only (i.e. checked in).
        /// New files and already writable files are left alone. If the checkout is not possible
        /// (tf.exe missing or failing), the read-only attribute is cleared as a fallback — the change
        /// is then NOT a pending edit in TFVC (visible only via "detect local changes").
        /// </summary>
        public void CheckOut(string pFullFilenameAndPath)
        {
            if (!Enabled || string.IsNullOrWhiteSpace(pFullFilenameAndPath)) return;

            var fi = new FileInfo(pFullFilenameAndPath);
            if (!fi.Exists || !fi.IsReadOnly) return;

            // tf checkout has no /workspace: switch — tf resolves the workspace from the local path
            RunTf("checkout", "/noprompt", "/lock:none", fi.FullName);

            fi.Refresh();
            if (fi.IsReadOnly)
            {
                fi.IsReadOnly = false;
                Log?.Invoke($"TFVC: tf.exe unavailable or checkout failed for '{fi.FullName}'; cleared the read-only attribute as fallback (no pending edit created).");
            }
            else
            {
                Log?.Invoke($"TFVC: checked out '{fi.FullName}'.");
            }
        }

        /// <summary>Pends an add for a new local file (best effort; "already added" is only logged).</summary>
        public void Add(string pFullFilenameAndPath)
        {
            if (!Enabled || string.IsNullOrWhiteSpace(pFullFilenameAndPath)) return;
            if (!File.Exists(pFullFilenameAndPath)) return;

            // tf add has no /workspace: switch — tf resolves the workspace from the local path
            RunTf("add", "/noprompt", pFullFilenameAndPath);
        }

        /// <summary>Convenience entry point used by all file-writing paths: makes the file writable, checking it out of TFVC if necessary.</summary>
        public static void EnsureWritable(string path) => Instance.CheckOut(path);

        private bool RunTf(params string[] args)
        {
            lock (_tfLock)
            {
                var tf = FindTfExe();
                if (tf is null)
                {
                    Log?.Invoke("TFVC: tf.exe not found (set MGN_TF_EXE or TfsHelper.TfExePath to override).");
                    return false;
                }
                try
                {
                    var psi = new ProcessStartInfo(tf)
                    {
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    };
                    foreach (var a in args) psi.ArgumentList.Add(a);

                    using var p = Process.Start(psi);
                    if (p is null) return false;
                    var stdout = p.StandardOutput.ReadToEndAsync();
                    var stderr = p.StandardError.ReadToEndAsync();
                    if (!p.WaitForExit(60_000))
                    {
                        try { p.Kill(entireProcessTree: true); } catch { /* best effort */ }
                        Log?.Invoke($"TFVC: tf {args[0]} timed out for '{args[^1]}'.");
                        return false;
                    }
                    Task.WaitAll(stdout, stderr);
                    if (p.ExitCode != 0)
                    {
                        var output = (stdout.Result + stderr.Result).Trim();
                        Log?.Invoke($"TFVC: tf {args[0]} exited with {p.ExitCode} for '{args[^1]}'{(output.Length > 0 ? ": " + output : "")}");
                        return false;
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Log?.Invoke($"TFVC: failed to run tf {args[0]}: {ex.Message}");
                    return false;
                }
            }
        }

        internal static string? FindTfExe()
        {
            var env = Environment.GetEnvironmentVariable("MGN_TF_EXE");
            if (!string.IsNullOrEmpty(env))
                return File.Exists(env) ? env : null;

            if (!string.IsNullOrEmpty(TfExePath))
                return File.Exists(TfExePath) ? TfExePath : null;

            if (!OperatingSystem.IsWindows()) return null;

            foreach (var dir in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator))
            {
                if (dir.Length == 0) continue;
                try
                {
                    var candidate = Path.Combine(dir, "tf.exe");
                    if (File.Exists(candidate)) return candidate;
                }
                catch { /* ignore malformed PATH entries */ }
            }

            // Visual Studio install path via vswhere (Team Explorer component ships tf.exe)
            var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            if (!string.IsNullOrEmpty(programFilesX86))
            {
                var vswhere = Path.Combine(programFilesX86, "Microsoft Visual Studio", "Installer", "vswhere.exe");
                if (File.Exists(vswhere))
                {
                    var installPath = Capture(vswhere,
                        "-latest -products * -requires Microsoft.VisualStudio.Component.TeamExplorer -property installationPath");
                    if (!string.IsNullOrWhiteSpace(installPath))
                    {
                        var tf = Path.Combine(installPath.Trim(), "Common7", "IDE", "CommonExtensions",
                            "Microsoft", "TeamFoundation", "Team Explorer", "tf.exe");
                        if (File.Exists(tf)) return tf;
                    }
                }
            }

            // Well-known VS layouts as a fallback when vswhere is unavailable
            foreach (var root in new[]
                     {
                         Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                         programFilesX86,
                     })
            {
                if (string.IsNullOrEmpty(root)) continue;
                foreach (var year in new[] { "2022", "2019", "2017", "18" })
                foreach (var edition in new[] { "Enterprise", "Professional", "Community", "BuildTools" })
                {
                    var tf = Path.Combine(root, "Microsoft Visual Studio", year, edition, "Common7", "IDE",
                        "CommonExtensions", "Microsoft", "TeamFoundation", "Team Explorer", "tf.exe");
                    if (File.Exists(tf)) return tf;
                }
            }
            return null;
        }

        private static string Capture(string fileName, string arguments)
        {
            try
            {
                var psi = new ProcessStartInfo(fileName)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                foreach (var a in arguments.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    psi.ArgumentList.Add(a);
                using var p = Process.Start(psi);
                if (p is null) return "";
                var stdout = p.StandardOutput.ReadToEndAsync();
                if (!p.WaitForExit(15_000)) { try { p.Kill(entireProcessTree: true); } catch { } return ""; }
                return stdout.GetAwaiter().GetResult();
            }
            catch { return ""; }
        }
    }
}
