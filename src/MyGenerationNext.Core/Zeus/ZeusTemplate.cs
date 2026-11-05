using System.Text;

namespace MyGenerationNext.Core.Zeus;

public sealed class ZeusParseException(string message) : Exception(message);

/// <summary>A GUI or BODY block of a template.</summary>
public sealed class CodeSegment
{
    public string Engine { get; set; } = ".Net Script";
    public string Language { get; set; } = "C#";
    public string Mode { get; set; } = "Markup";
    public string Code { get; set; } = "";
    public bool IsEmpty => string.IsNullOrWhiteSpace(Code);
}

/// <summary>A parsed Zeus template (.csgen/.zeus/.vbgen/.jgen). Mirrors the legacy ZeusTemplate model.</summary>
public sealed class ZeusTemplate
{
    public const string TypeTemplate = "Template";
    public const string TypeGroup = "Group";

    public string Type { get; set; } = TypeTemplate;
    public string UniqueId { get; set; } = "";
    public string Title { get; set; } = "";
    public string Namespace { get; set; } = "";
    public string SourceType { get; set; } = "Source";
    public string OutputLanguage { get; set; } = "None";
    public string Comments { get; set; } = "";
    public string TagStart { get; set; } = "<%";
    public string TagEnd { get; set; } = "%>";
    public string TagStartShortcut => TagStart + "=";
    public string TagStartSpecial => TagStart + "#";
    public CodeSegment Gui { get; } = new() { Mode = "Pure" };
    public CodeSegment Body { get; } = new();
    public List<string> IncludedTemplatePaths { get; } = [];

    /// <summary>Every directive in file order, including unknown ones.</summary>
    public List<KeyValuePair<string, string>> Directives { get; } = [];

    public string? FilePath { get; set; }
    public string? DirectoryPath => FilePath is null ? null : Path.GetDirectoryName(FilePath);
    public bool IsGroup => string.Equals(Type, TypeGroup, StringComparison.OrdinalIgnoreCase);

    public string? GetDirective(string key) =>
        Directives.Where(d => d.Key == key).Select(d => d.Value).FirstOrDefault();

    /// <summary>Resolves an include path ('.\x.csgen' style) relative to this template's directory.</summary>
    public string ResolveInclude(string include)
    {
        var rel = include.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
        return Path.GetFullPath(Path.IsPathRooted(rel) ? rel : Path.Combine(DirectoryPath ?? ".", rel));
    }

    public static ZeusTemplate Load(string path)
    {
        var t = Parse(TemplateFileReader.ReadAllText(path));
        t.FilePath = Path.GetFullPath(path);
        return t;
    }

    public static ZeusTemplate Parse(string text)
    {
        var t = new ZeusTemplate();
        var lines = text.TrimStart('\uFEFF').Split('\n');
        if (!lines[0].TrimEnd('\r').StartsWith("##|TYPE"))
            throw new ZeusParseException("Not a Zeus template: the first line must start with '##|TYPE'.");

        StringBuilder? block = null;
        foreach (var raw in lines)
        {
            var line = raw.TrimEnd('\r');
            if (line.StartsWith("##|"))
            {
                var ws = line.IndexOfAny([' ', '\t']);
                var end = ws < 0 ? line.Length : ws;
                var key = line[3..end];
                if (block is null)
                {
                    if (key is "BODY_BEGIN" or "GUI_BEGIN" or "COMMENTS_BEGIN") block = new StringBuilder();
                    else t.Assign(key, ws < 0 ? "" : line[ws..].Trim());
                }
                else if (key is "BODY_END" or "GUI_END" or "COMMENTS_END")
                {
                    var code = block.ToString().TrimEnd();
                    switch (key)
                    {
                        case "BODY_END": t.Body.Code = code; break;
                        case "GUI_END": t.Gui.Code = code; break;
                        default: t.Comments = code; break;
                    }
                    block = null;
                }
            }
            else block?.Append(line).Append("\r\n");
        }
        return t;
    }

    private void Assign(string key, string val)
    {
        Directives.Add(new(key, val));
        switch (key)
        {
            case "TYPE": Type = val; break;
            case "SOURCE_TYPE": SourceType = val; break;
            case "BODY_ENGINE": Body.Engine = val; break;
            case "BODY_LANGUAGE": Body.Language = val; break;
            case "BODY_MODE": Body.Mode = val; break;
            case "GUI_ENGINE": Gui.Engine = val; break;
            case "GUI_LANGUAGE": Gui.Language = val; break;
            case "OUTPUT_LANGUAGE": OutputLanguage = val; break;
            case "UNIQUEID": UniqueId = val; break;
            case "TITLE": Title = val; break;
            case "NAMESPACE": Namespace = val; break;
            case "BODY_TAG_START": TagStart = val; break;
            case "BODY_TAG_END": TagEnd = val; break;
            case "TEMPLATE_INCLUDE": IncludedTemplatePaths.Add(val); break;
        }
    }
}

/// <summary>Reads template/source files: UTF-8 (with or without BOM), falling back to Windows-1252 as legacy tools did.</summary>
public static class TemplateFileReader
{
    private static readonly Encoding Strict = new UTF8Encoding(false, true);

    static TemplateFileReader() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    public static string ReadAllText(string path)
    {
        var bytes = File.ReadAllBytes(path);
        try { return Strict.GetString(bytes).TrimStart('\uFEFF'); }
        catch (DecoderFallbackException) { return Encoding.GetEncoding(1252).GetString(bytes); }
    }
}
