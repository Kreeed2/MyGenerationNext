using EHA.ZESA.Util.GeneratorUI;

using Zeus;

namespace MyGenerationNext.Tests;

[CollectionDefinition("TFVC shared state", DisableParallelization = true)]
public class TfsHelperTestCollection { }

[Collection("TFVC shared state")]
public class TfsHelperTests : IDisposable
{
    public TfsHelperTests()
    {
        TfsHelper.Enabled = true;
        TfsHelper.TfExePath = null;
        TfsHelper.Log = null;
        TfsHelper.Instance.CurrentTfSWorkspace = "";
        Environment.SetEnvironmentVariable("MGN_TF_EXE", null);
    }

    public void Dispose()
    {
        TfsHelper.Enabled = true;
        TfsHelper.TfExePath = null;
        TfsHelper.Log = null;
        TfsHelper.Instance.CurrentTfSWorkspace = "";
        Environment.SetEnvironmentVariable("MGN_TF_EXE", null);
    }

    private static string TempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "mgn-tfs-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static void SetReadOnly(string path)
    {
        if (OperatingSystem.IsWindows())
            File.SetAttributes(path, FileAttributes.ReadOnly);
        else
            File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.GroupRead | UnixFileMode.OtherRead);
    }

    [Fact]
    public void CheckOut_DoesNothing_WhenFileDoesNotExist()
    {
        var missing = Path.Combine(TempDir(), "does-not-exist.cs");
        TfsHelper.Instance.CheckOut(missing);
        Assert.False(File.Exists(missing));
    }

    [Fact]
    public void CheckOut_DoesNothing_WhenFileIsWritable()
    {
        var dir = TempDir();
        var file = Path.Combine(dir, "writable.cs");
        File.WriteAllText(file, "original");

        var logged = new List<string>();
        TfsHelper.Log = logged.Add;
        TfsHelper.Instance.CheckOut(file);

        Assert.Equal("original", File.ReadAllText(file));
        Assert.False(new FileInfo(file).IsReadOnly);
        Assert.Empty(logged);
    }

    [Fact]
    public void CheckOut_DoesNothing_WhenDisabled()
    {
        var dir = TempDir();
        var file = Path.Combine(dir, "locked.cs");
        File.WriteAllText(file, "original");
        SetReadOnly(file);

        TfsHelper.Enabled = false;
        TfsHelper.Instance.CheckOut(file);

        Assert.True(new FileInfo(file).IsReadOnly);
        Assert.Equal("original", File.ReadAllText(file));
    }

    [Fact]
    public void CheckOut_FallbackClearsReadOnly_WhenTfNotFound()
    {
        var dir = TempDir();
        var file = Path.Combine(dir, "checked-in.cs");
        File.WriteAllText(file, "original");
        SetReadOnly(file);

        // force "tf.exe not found" so the fallback path is exercised deterministically
        TfsHelper.TfExePath = Path.Combine(dir, "no-such-tf.exe");
        var logged = new List<string>();
        TfsHelper.Log = logged.Add;

        TfsHelper.Instance.CheckOut(file);

        Assert.False(new FileInfo(file).IsReadOnly);
        Assert.Equal("original", File.ReadAllText(file));
        Assert.Contains(logged, m => m.Contains("no pending edit", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void CheckOut_And_Add_DoNotPassWorkspaceSwitch_ToTf()
    {
        if (OperatingSystem.IsWindows()) return; // fake tf script is a POSIX shell script

        var dir = TempDir();
        var file = Path.Combine(dir, "checked-in.cs");
        File.WriteAllText(file, "original");
        SetReadOnly(file);

        // fake tf.exe: appends its arguments to a log, clears the write protection of its last argument, exits 0
        var argsLog = Path.Combine(dir, "tf-args.log");
        var fakeTf = Path.Combine(dir, "tf");
        File.WriteAllText(fakeTf,
            "#!/bin/sh\nprintf '%s\\n' \"$@\" >> \"" + argsLog + "\"\nfor last; do :; done\nchmod u+w \"$last\"\nexit 0\n");
        File.SetUnixFileMode(fakeTf,
            UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
            UnixFileMode.GroupRead | UnixFileMode.GroupExecute |
            UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
        Environment.SetEnvironmentVariable("MGN_TF_EXE", fakeTf);

        // tf checkout / tf add do not support /workspace: — the override must not be forwarded
        TfsHelper.Instance.CurrentTfSWorkspace = "TEST-WORKSPACE";

        TfsHelper.Instance.CheckOut(file);
        TfsHelper.Instance.Add(file);

        Assert.False(new FileInfo(file).IsReadOnly);
        var argLines = File.ReadAllLines(argsLog);
        Assert.Contains(argLines, a => a.Equals("checkout", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(argLines, a => a.Equals("add", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(argLines, a => a.StartsWith("/workspace:", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void CheckOut_UsesTfExe_WhenAvailable()
    {
        if (OperatingSystem.IsWindows()) return; // fake tf script is a POSIX shell script

        var dir = TempDir();
        var file = Path.Combine(dir, "checked-in.cs");
        File.WriteAllText(file, "original");
        SetReadOnly(file);

        // fake tf.exe: clears the write protection of its last argument and exits 0
        var fakeTf = Path.Combine(dir, "tf");
        File.WriteAllText(fakeTf, "#!/bin/sh\nfor last; do :; done\nchmod u+w \"$last\"\nexit 0\n");
        File.SetUnixFileMode(fakeTf,
            UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
            UnixFileMode.GroupRead | UnixFileMode.GroupExecute |
            UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
        Environment.SetEnvironmentVariable("MGN_TF_EXE", fakeTf);

        var logged = new List<string>();
        TfsHelper.Log = logged.Add;

        TfsHelper.Instance.CheckOut(file);

        Assert.False(new FileInfo(file).IsReadOnly);
        Assert.Equal("original", File.ReadAllText(file));
        Assert.Contains(logged, m => m.Contains("checked out", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ZeusOutput_Save_OverwritesReadOnlyFile()
    {
        var dir = TempDir();
        var file = Path.Combine(dir, "generated.cs");
        File.WriteAllText(file, "old content");
        SetReadOnly(file);

        // no tf.exe available -> fallback clears the read-only flag so the save succeeds
        TfsHelper.TfExePath = Path.Combine(dir, "no-such-tf.exe");

        var output = new ZeusOutput();
        output.write("new content");
        output.save(file, "o");

        Assert.Equal("new content", File.ReadAllText(file));
        Assert.False(new FileInfo(file).IsReadOnly);
    }

    [Fact]
    public void ZeusOutput_Append_WorksOnReadOnlyFile()
    {
        var dir = TempDir();
        var file = Path.Combine(dir, "generated.cs");
        File.WriteAllText(file, "old");
        SetReadOnly(file);

        TfsHelper.TfExePath = Path.Combine(dir, "no-such-tf.exe");

        var output = new ZeusOutput();
        output.write("+new");
        output.append(file);

        Assert.Equal("old+new", File.ReadAllText(file));
        Assert.False(new FileInfo(file).IsReadOnly);
    }

    [Fact]
    public void ZeusOutput_Save_DoesNotCheckOut_WhenActionDoesNotOverwrite()
    {
        var dir = TempDir();
        var file = Path.Combine(dir, "generated.cs");
        File.WriteAllText(file, "old content");
        SetReadOnly(file);

        // even with tf.exe missing, a no-overwrite save must not touch (or check out) the file at all
        TfsHelper.TfExePath = Path.Combine(dir, "no-such-tf.exe");
        var logged = new List<string>();
        TfsHelper.Log = logged.Add;

        var output = new ZeusOutput();
        output.write("new content");
        output.save(file, "d");

        Assert.Equal("old content", File.ReadAllText(file));
        Assert.True(new FileInfo(file).IsReadOnly);
        Assert.Empty(logged);
    }

    [Fact]
    public void ZeusOutput_Save_DAction_StillWritesNewFile()
    {
        var dir = TempDir();
        var file = Path.Combine(dir, "new-file.cs");

        var output = new ZeusOutput();
        output.write("content");
        output.save(file, "d");

        Assert.Equal("content", File.ReadAllText(file));
    }
}
