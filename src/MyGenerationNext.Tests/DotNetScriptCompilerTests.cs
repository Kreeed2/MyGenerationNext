using System.Reflection;
using MyGenerationNext.Core.Zeus;

namespace MyGenerationNext.Tests;

public class DotNetScriptCompilerTests
{
    [Fact]
    public void CollectReferencePathsUsesTpaAndCoreAssembly()
    {
        var tpa = (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
        var paths = DotNetScriptCompiler.CollectReferencePaths(tpa, typeof(DotNetScriptCompiler).Assembly);
        Assert.NotEmpty(paths);
        Assert.All(paths, p => Assert.True(File.Exists(p), p));
        Assert.Contains(paths, p => p.EndsWith("MyGenerationNext.Core.dll", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void CollectReferencePathsHandlesBundleLoadedAssemblyWithoutLocation()
    {
        // assemblies loaded from a single-file bundle (or from bytes) have an empty Location
        var bundled = Assembly.Load(File.ReadAllBytes(typeof(DotNetScriptCompiler).Assembly.Location));
        Assert.Equal("", bundled.Location);
        var paths = DotNetScriptCompiler.CollectReferencePaths("", bundled);
        Assert.NotEmpty(paths);
        Assert.All(paths, p => Assert.False(string.IsNullOrEmpty(p)));
        Assert.All(paths, p => Assert.True(File.Exists(p), p));
        Assert.Contains(paths, p => p.EndsWith("MyGenerationNext.Core.dll", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ResolveAssemblyPathFallsBackToBaseDirectoryWhenLocationIsEmpty()
    {
        var bundled = Assembly.Load(File.ReadAllBytes(typeof(DotNetScriptCompiler).Assembly.Location));
        Assert.Equal("", bundled.Location);
        Assert.Equal(Path.Combine(AppContext.BaseDirectory, "MyGenerationNext.Core.dll"),
            DotNetScriptCompiler.ResolveAssemblyPath(bundled));
    }
}
