using System.Text;

namespace MyGenerationNext.Core.Zeus;

/// <summary>Result of turning a template segment into compilable C#.</summary>
public sealed class ParsedSegment
{
    /// <summary>The user code (literal text turned into output.write calls, tags expanded).</summary>
    public string Code { get; set; } = "";
    public List<string> Namespaces { get; } = [];
    public List<string> References { get; } = [];
    public string? CompilerVersion { get; set; }
    public bool Debug { get; set; }
    public bool IsGui { get; init; }
}

/// <summary>
/// Port of the legacy ZeusCodeParser + DotNetScriptCodeParser (C# only).
/// Markup mode: text -> output.write, &lt;%= expr %&gt; -> output.write(expr), &lt;% code %&gt; -> verbatim,
/// &lt;%# directive %&gt; -> FILE / REFERENCE (REF) / NAMESPACE (NS) / VERSION / DEBUG.
/// </summary>
public static class DotNetScriptCodeParser
{
    private static readonly string[] DefaultNamespaces =
        ["System", "System.Collections", "Zeus", "Zeus.Data", "Zeus.DotNetScript"];

    public static ParsedSegment Parse(ZeusTemplate template, CodeSegment segment, bool isGui)
    {
        if (!string.Equals(segment.Language, "C#", StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException($"Template language '{segment.Language}' is not supported; only C#.");
        if (!string.Equals(segment.Engine, ".Net Script", StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException($"Script engine '{segment.Engine}' is not supported; only '.Net Script'.");

        var result = new ParsedSegment { IsGui = isGui };
        var builder = new StringBuilder();
        result.Namespaces.AddRange(DefaultNamespaces);
        if (isGui) result.Namespaces.Add("Zeus.UserInterface");
        result.Namespaces.Add("MyMeta");

        if (!segment.IsEmpty)
        {
            if (segment.Mode.Equals("Markup", StringComparison.OrdinalIgnoreCase))
                ParseMarkup(template, segment, result, builder);
            else
                ParsePure(template, segment, result, builder);
        }
        result.Code = builder.ToString();
        return result;
    }

    private static void ParseMarkup(ZeusTemplate t, CodeSegment seg, ParsedSegment result, StringBuilder builder)
    {
        string tagStart = t.TagStart, tagSpecial = t.TagStartSpecial, tagShortcut = t.TagStartShortcut, tagEnd = t.TagEnd;
        var reader = new StringReader(seg.Code);
        var sb = new StringBuilder();
        var inBlock = false;
        var isShortcut = false;
        var isCustom = false;

        var line = reader.ReadLine();
        while (line != null)
        {
            var nextLine = reader.ReadLine();
            var index = line.IndexOf(inBlock ? tagEnd : tagStart, StringComparison.Ordinal);
            while (index >= 0)
            {
                if (inBlock)
                {
                    inBlock = false;
                    if (isShortcut)
                    {
                        sb.Append(line.AsSpan(0, index));
                        builder.Append(Output(sb.ToString(), false, false));
                        sb.Clear();
                        isShortcut = false;
                    }
                    else if (isCustom)
                    {
                        builder.Append(CustomTag(t, result, line[..index]));
                        isCustom = false;
                    }
                    else builder.Append(line[..index].Trim()).Append("\r\n");
                    line = line[(index + tagEnd.Length)..];
                }
                else
                {
                    inBlock = true;
                    if (index > 0) builder.Append(Output(line[..index], true, false));

                    if (index == line.IndexOf(tagShortcut, StringComparison.Ordinal))
                    {
                        (isCustom, isShortcut) = (false, true);
                        line = line[(index + tagShortcut.Length)..];
                    }
                    else if (index == line.IndexOf(tagSpecial, StringComparison.Ordinal))
                    {
                        (isCustom, isShortcut) = (true, false);
                        line = line[(index + tagSpecial.Length)..];
                    }
                    else
                    {
                        (isCustom, isShortcut) = (false, false);
                        line = line[(index + tagStart.Length)..];
                    }
                }
                index = line.IndexOf(inBlock ? tagEnd : tagStart, StringComparison.Ordinal);
            }

            isCustom = false; // custom tags must start and end on the same line

            if (isShortcut) sb.Append(line.Trim()).Append("\r\n");
            else if (inBlock) builder.Append(line.Trim()).Append("\r\n");
            else if (!(nextLine == null && line.Length == 0)) builder.Append(Output(line, true, true));

            line = nextLine;
        }
    }

    private static void ParsePure(ZeusTemplate t, CodeSegment seg, ParsedSegment result, StringBuilder builder)
    {
        string tagSpecial = t.TagStartSpecial, tagEnd = t.TagEnd;
        var reader = new StringReader(seg.Code);
        var line = reader.ReadLine();
        while (line != null)
        {
            var nextLine = reader.ReadLine();
            var inBlock = false;
            var index = line.IndexOf(tagSpecial, StringComparison.Ordinal);
            while (index >= 0)
            {
                if (inBlock)
                {
                    inBlock = false;
                    builder.Append(CustomTag(t, result, line[..index]));
                    line = line[(index + tagEnd.Length)..];
                }
                else
                {
                    inBlock = true;
                    if (index > 0) builder.Append(line.AsSpan(0, index));
                    line = line[(index + tagSpecial.Length)..];
                }
                index = line.IndexOf(inBlock ? tagEnd : tagSpecial, StringComparison.Ordinal);
            }
            builder.Append(line.Trim()).Append("\r\n"); // pure-mode tags can't span lines
            line = nextLine;
        }
    }

    private static string Output(string text, bool literal, bool newLine) =>
        (newLine ? "output.writeln(" : "output.write(") + (literal ? Escape(text) : text) + ");\r\n";

    private static string Escape(string s) =>
        "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t") + "\"";

    private static string CustomTag(ZeusTemplate t, ParsedSegment result, string text)
    {
        static string After(string s, string prefix) => s[prefix.Length..].Trim();
        static IEnumerable<string> List(string s) =>
            s.Split(',').Select(x => x.Trim()).Where(x => x.Length > 0);

        if (text.StartsWith("FILE "))
        {
            var path = Path.GetFullPath(Path.Combine(t.DirectoryPath ?? ".",
                After(text, "FILE ").Replace('\\', Path.DirectorySeparatorChar)));
            return File.Exists(path) ? TemplateFileReader.ReadAllText(path) : "";
        }
        if (text.StartsWith("COMPILERVERSION")) result.CompilerVersion = After(text, "COMPILERVERSION");
        else if (text.StartsWith("VERSION")) result.CompilerVersion = After(text, "VERSION");
        else if (text.StartsWith("REFERENCE ")) result.References.AddRange(List(After(text, "REFERENCE ")));
        else if (text.StartsWith("REF ")) result.References.AddRange(List(After(text, "REF ")));
        else if (text.StartsWith("NAMESPACE ")) result.Namespaces.AddRange(List(After(text, "NAMESPACE ")));
        else if (text.StartsWith("NS ")) result.Namespaces.AddRange(List(After(text, "NS ")));
        else if (text.StartsWith("DEBUG")) result.Debug = true;
        return "";
    }
}
