using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Zeus;

namespace MyGenerationNext.Core.Zeus;

public sealed class TemplateCompileException(string message, IReadOnlyList<string> diagnostics) : Exception(message)
{
    public IReadOnlyList<string> Diagnostics { get; } = diagnostics;
}

/// <summary>
/// Compiles a parsed template segment (GUI or body) with Roslyn, the way the legacy DotNetScriptEngine did:
/// usings + user code + generated DotNetScriptTemplate/DotNetScriptGui base class exposing the intrinsic objects.
/// </summary>
public static class DotNetScriptCompiler
{
    private static readonly Lazy<IReadOnlyList<MetadataReference>> BaseReferences = new(() =>
    {
        var paths = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") ?? "")
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries).ToList();
        var core = typeof(DotNetScriptCompiler).Assembly.Location;
        if (!paths.Contains(core)) paths.Add(core);
        return paths.Select(p => (MetadataReference)MetadataReference.CreateFromFile(p)).ToList();
    });

    public static Assembly Compile(ZeusTemplate template, ParsedSegment parsed, string assemblyName)
    {
        var refs = BaseReferences.Value.ToList();
        foreach (var r in parsed.References)
        {
            var candidates = new[] { r, Path.Combine(template.DirectoryPath ?? ".", r.Replace('\\', Path.DirectorySeparatorChar)) };
            var found = candidates.FirstOrDefault(File.Exists);
            if (found is not null && !refs.Any(x => string.Equals(Path.GetFileName(x.Display), Path.GetFileName(found), StringComparison.OrdinalIgnoreCase)))
                refs.Add(MetadataReference.CreateFromFile(found));
        }

        var probe = CSharpCompilation.Create("probe", references: refs);
        var usings = new StringBuilder();
        foreach (var ns in parsed.Namespaces.Distinct().Where(n => NamespaceExists(probe.GlobalNamespace, n)))
            usings.Append("using ").Append(ns).Append(";\r\n");
        // templates build Windows-style paths; on other platforms route basic file system calls through a normalizing shim
        if (Path.DirectorySeparatorChar != '\\')
            usings.Append("using Directory = global::MyGenerationNext.Core.Zeus.WinPath.Directory;\r\n")
                  .Append("using File = global::MyGenerationNext.Core.Zeus.WinPath.File;\r\n");
        var header = usings.ToString();

        var source = header + parsed.Code + "\r\n" + Footer(parsed.IsGui);
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest),
            path: template.FilePath ?? "template", encoding: Encoding.UTF8);
        var compilation = CSharpCompilation.Create(assemblyName, [tree], refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: parsed.Debug ? OptimizationLevel.Debug : OptimizationLevel.Release,
                nullableContextOptions: NullableContextOptions.Disable));

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);
        if (!result.Success)
        {
            var headerLines = header.Count(c => c == '\n');
            var errors = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d =>
                {
                    var pos = d.Location.GetLineSpan().StartLinePosition;
                    return $"{Path.GetFileName(template.FilePath)}({pos.Line + 1 - headerLines},{pos.Character + 1}) {d.Id}: {d.GetMessage()}";
                }).ToList();
            throw new TemplateCompileException(
                $"Compilation of {Path.GetFileName(template.FilePath)} ({(parsed.IsGui ? "GUI" : "body")}) failed with {errors.Count} error(s):\n" +
                string.Join("\n", errors.Take(25)), errors);
        }
        ms.Position = 0;
        return AssemblyLoadContext.Default.LoadFromStream(ms);
    }

    public static string Footer(bool isGui)
    {
        var (cls, baseCls) = isGui ? ("DotNetScriptGui", "_DotNetScriptGui") : ("DotNetScriptTemplate", "_DotNetScriptTemplate");
        return $$"""
            namespace Zeus.DotNetScript
            {
                public abstract class {{cls}} : global::Zeus.DotNetScript.{{baseCls}}
                {
                    protected global::Zeus.UserInterface.GuiController ui;
                    protected global::MyMeta.dbRoot MyMeta;

                    public {{cls}}(global::Zeus.IZeusContext context) : base(context)
                    {
                        ui = (global::Zeus.UserInterface.GuiController)context.Objects["ui"];
                        MyMeta = (global::MyMeta.dbRoot)context.Objects["MyMeta"];
                    }
                }
            }
            """;
    }

    /// <summary>Instantiates the first concrete subclass of the generated base and runs Setup()/Render().</summary>
    public static void Run(Assembly assembly, bool isGui, IZeusContext context)
    {
        var baseName = isGui ? "Zeus.DotNetScript.DotNetScriptGui" : "Zeus.DotNetScript.DotNetScriptTemplate";
        var type = assembly.GetTypes().FirstOrDefault(t => !t.IsAbstract && IsSubclassOfName(t, baseName))
            ?? throw new InvalidOperationException($"No class deriving from {baseName} found in compiled template.");
        var instance = Activator.CreateInstance(type, context)!;
        try
        {
            if (isGui) ((global::Zeus.DotNetScript._DotNetScriptGui)instance).Setup();
            else ((global::Zeus.DotNetScript._DotNetScriptTemplate)instance).Render();
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            throw ex.InnerException;
        }
    }

    private static bool IsSubclassOfName(Type t, string name)
    {
        for (var b = t.BaseType; b is not null; b = b.BaseType)
            if (b.FullName == name) return true;
        return false;
    }

    private static bool NamespaceExists(INamespaceSymbol root, string ns)
    {
        var cur = root;
        foreach (var part in ns.Split('.'))
        {
            cur = cur.GetNamespaceMembers().FirstOrDefault(n => n.Name == part);
            if (cur is null) return false;
        }
        return true;
    }
}
