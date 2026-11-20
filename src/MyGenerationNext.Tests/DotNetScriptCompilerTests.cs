using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MyGenerationNext.Core.Zeus;

namespace MyGenerationNext.Tests;

public class DotNetScriptCompilerTests
{
    private static bool CompilesWith(IEnumerable<MetadataReference> refs, string code)
    {
        var compilation = CSharpCompilation.Create("probe",
            [CSharpSyntaxTree.ParseText(code)],
            refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        return !compilation.GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void CollectReferencesContainsFrameworkAndCoreAssembly()
    {
        var refs = DotNetScriptCompiler.CollectReferences();
        Assert.NotEmpty(refs);
        Assert.True(CompilesWith(refs, "public class X { public Zeus.IZeusContext? Context; public MyMeta.dbRoot? Db; }"));
    }

    [Fact]
    public void CreateReferencePrefersFileWhenLocationExists()
    {
        var reference = DotNetScriptCompiler.CreateReference(typeof(DotNetScriptCompiler).Assembly);
        Assert.NotNull(reference);
        Assert.EndsWith("MyGenerationNext.Core.dll", reference.Display ?? "");
    }

    [Fact]
    public void CompilesWithInMemoryMetadataWhenLocationIsEmpty()
    {
        // assemblies loaded from bytes — like single-file bundles — have an empty Location and no loose dll;
        // the reference must come from the raw in-memory metadata blob instead
        var bundled = Assembly.Load(File.ReadAllBytes(typeof(DotNetScriptCompiler).Assembly.Location));
        Assert.Equal("", bundled.Location);

        var reference = DotNetScriptCompiler.CreateReference(bundled);
        Assert.NotNull(reference);

        // framework ref pack + only the bundle-loaded core assembly, nothing read from disk
        var refs = DotNetScriptCompiler.CollectReferences([bundled]);
        Assert.True(CompilesWith(refs, "public class X { public Zeus.IZeusContext? Context; public MyMeta.dbRoot? Db; }"));
    }
}
