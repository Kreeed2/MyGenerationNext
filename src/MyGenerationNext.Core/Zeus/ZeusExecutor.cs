using Zeus;

namespace MyGenerationNext.Core.Zeus;

public sealed class ZeusRunOptions
{
    /// <summary>Metadata exposed to templates as the 'MyMeta' intrinsic object.</summary>
    public required global::MyMeta.dbRoot MyMeta { get; init; }
    /// <summary>Replaces the interactive GUI: receives the context after the GUI phase has been skipped or run, to fill 'input'.</summary>
    public Action<IZeusContext, ZeusTemplate>? ProvideInput { get; init; }
    /// <summary>Run the template's GUI Setup() code before ProvideInput (default true).</summary>
    public bool RunGuiCode { get; init; } = true;
    /// <summary>Redirects file saves (dry run). Receives (path, content).</summary>
    public Action<string, string>? FileWriteHook { get; init; }
    public Action<string>? Log { get; init; }
}

public sealed class ZeusRunResult
{
    public List<string> SavedFiles { get; } = [];
    public List<string> RenderedTemplates { get; } = [];
}

/// <summary>Executes a Zeus template: GUI phase, body phase and, for groups, all included templates.</summary>
public static class ZeusExecutor
{
    public static ZeusRunResult Run(string templatePath, ZeusRunOptions options)
    {
        var template = ZeusTemplate.Load(templatePath);
        var output = new ZeusOutput { FileWriteHook = options.FileWriteHook };
        var context = new ZeusContext(output);
        context.Objects["MyMeta"] = options.MyMeta;
        context.Objects["ui"] = context.Gui;
        if (options.Log is { } log) ((ZeusSimpleLog)context.Log).LogEntryAdded += log;

        if (!template.Gui.IsEmpty && options.RunGuiCode)
        {
            var parsed = DotNetScriptCodeParser.Parse(template, template.Gui, true);
            var asm = DotNetScriptCompiler.Compile(template, parsed, "ZeusGui_" + Guid.NewGuid().ToString("N"));
            DotNetScriptCompiler.Run(asm, true, context);
        }
        options.ProvideInput?.Invoke(context, template);

        var result = new ZeusRunResult();
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        RunBody(template, context, output, result, visited, options);
        foreach (var f in output.SavedFiles) result.SavedFiles.Add((string)f!);
        return result;
    }

    private static void RunBody(ZeusTemplate template, ZeusContext context, ZeusOutput output,
        ZeusRunResult result, HashSet<string> visited, ZeusRunOptions options)
    {
        var key = string.IsNullOrEmpty(template.UniqueId) ? template.FilePath ?? template.Title : template.UniqueId;
        if (!visited.Add(key)) return;

        output.clear();
        output.tabLevel = 0;
        if (!template.Body.IsEmpty)
        {
            options.Log?.Invoke($"Rendering {Path.GetFileName(template.FilePath)}");
            var parsed = DotNetScriptCodeParser.Parse(template, template.Body, false);
            var asm = DotNetScriptCompiler.Compile(template, parsed, "ZeusBody_" + Guid.NewGuid().ToString("N"));
            DotNetScriptCompiler.Run(asm, false, context);
            result.RenderedTemplates.Add(template.FilePath ?? template.Title);
        }

        foreach (var inc in template.IncludedTemplatePaths)
        {
            var path = template.ResolveInclude(inc);
            if (!File.Exists(path)) throw new FileNotFoundException($"Included template not found: {inc}", path);
            RunBody(ZeusTemplate.Load(path), context, output, result, visited, options);
        }
    }
}
