using System.Reflection;
using System.Runtime.Loader;
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
        var alc = new AssemblyLoadContext("single-file-probe", isCollectible: true);
        try
        {
            using var stream = new MemoryStream(File.ReadAllBytes(typeof(DotNetScriptCompiler).Assembly.Location));
            var bundled = alc.LoadFromStream(stream);
            Assert.Equal("", bundled.Location);

            var reference = DotNetScriptCompiler.CreateReference(bundled);
            Assert.NotNull(reference);
            Assert.Equal("MyGenerationNext.Core.dll", reference.Display);

            // framework ref pack + only the bundle-loaded core assembly, nothing read from disk
            var refs = DotNetScriptCompiler.CollectReferences([bundled]);
            Assert.True(CompilesWith(refs, "public class X { public Zeus.IZeusContext? Context; public MyMeta.dbRoot? Db; }"));
        }
        finally
        {
            alc.Unload();
        }
    }

    [Fact]
    public void CollectReferencesSkipsPrivateCoreLibAndDuplicateAssemblyNames()
    {
        var alc = new AssemblyLoadContext("duplicate-probe", isCollectible: true);
        try
        {
            using var stream = new MemoryStream(File.ReadAllBytes(typeof(DotNetScriptCompiler).Assembly.Location));
            var duplicateCore = alc.LoadFromStream(stream);

            var refs = DotNetScriptCompiler.CollectReferences([
                typeof(object).Assembly,
                typeof(DotNetScriptCompiler).Assembly,
                duplicateCore,
            ]);
            Assert.True(CompilesWith(refs, "public class X { public Zeus.IZeusContext? Context; public MyMeta.dbRoot? Db; }"));
        }
        finally
        {
            alc.Unload();
        }
    }
}
