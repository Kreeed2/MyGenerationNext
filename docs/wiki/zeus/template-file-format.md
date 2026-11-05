---
title: Zeus template file format (##| directives, GUI/BODY sections)
summary: Describes the on-disk text format of Zeus templates (.zeus/.csgen/.vbgen/.jgen), its ##| directives, section blocks, template groups, and the legacy format.
keywords: [template, zeus, csgen, vbgen, jgen, directive, GUI_BEGIN, BODY_BEGIN, group, ZeusWriter, legacy]
source_paths:
  - mygeneration-legacy-code/mygeneration/Zeus/ZeusTemplateParser.cs
  - mygeneration-legacy-code/mygeneration/Zeus/ZeusWriter.cs
  - mygeneration-legacy-code/mygeneration/Zeus/ZeusTemplate.cs
  - mygeneration-legacy-code/mygeneration/EngineInterface/ZeusConstants.cs
  - mygeneration-legacy-code/mygeneration/Zeus/Legacy/LegacyTemplateParser.cs
  - mygeneration-legacy-code/templates/Tutorials/Chapter1(C#).zeus
  - mygeneration-legacy-code/templates/Other/TemplateGroupExample.jgen
---

# Zeus template file format

## File extensions
Extensions are **not** interpreted by the Zeus library; `ZeusTemplate(path)` just opens a path. The extensions are only a UI convention in other projects:
- `MyGenUtility/Forms/TemplateEditor/TemplateEditor.cs` (`FILE_TYPES`): `.jgen` (JScript), `.vbgen` (VBScript), `.csgen` (C#), `.zeus`.
- `MyGenUtility/Forms/TemplateBrowser/TemplateTreeBuilder.cs` accepts `.jgen`, `.vbgen`, `.csgen`, `.zeus`.
- The actual engine/language is decided by directives inside the file, so all extensions share the same format (the extension may not match the language, e.g. `templates/MySQL/MySQL4_CSharp_BusinessEntity.vbgen`).
- Other file kinds: `.zinp` saved input, `.zprj` project (UI filter; the code class is `ZeusProject`) — see [saved-input-and-projects.md](saved-input-and-projects.md).

## Structure
A template is line-oriented. Directive lines start with `##|` (`ZeusConstants.START_DIRECTIVE`): key up to first whitespace, value = trimmed rest. The **first line must be `##|TYPE ...`**, otherwise `ZeusTemplateParser` throws `ZeusParseException(OutdatedTemplateStructure)` and falls back to `Legacy/LegacyTemplateParser`.

Real example (`templates/UnitTestTemplates/CSharpTestTemplate001.csgen`):
```
##|TYPE Template
##|UNIQUEID 52300a10-536d-422b-ba1e-60d0db52474e
##|TITLE C# Test Template 001
##|NAMESPACE UnitTestTemplates
##|SOURCE_TYPE Source
##|OUTPUT_LANGUAGE None
##|GUI_ENGINE .Net Script
##|GUI_LANGUAGE C#
##|BODY_MODE Markup
##|BODY_ENGINE .Net Script
##|BODY_LANGUAGE C#
##|BODY_TAG_START <%
##|BODY_TAG_END %>
##|BODY_BEGIN
<%#VERSION v3.5%><%#REF System.Core.dll %><%#NS System.Linq %><%
public class GeneratedTemplate : DotNetScriptTemplate { ... Render() ... }
%>
##|BODY_END
```

## Directives (`ZeusConstants.Directives`)
| Directive | Meaning / property set in `ZeusTemplateParser.AssignProperty` |
|---|---|
| `TYPE` | `Template` or `Group` (`ZeusConstants.TemplateTypes`) |
| `UNIQUEID` | GUID; used for identity, recursion guard in groups |
| `NAMESPACE` | dotted path -> `NamespacePath` (browser tree) |
| `TITLE` | display title |
| `SOURCE_TYPE` | `Source` / `Encrypted` / `Compiled` (only in EngineInterface's `ZeusConstants`, class `SourceTypes`) |
| `OUTPUT_LANGUAGE` | free text label of generated language, default `None` |
| `INPUT_VARIABLES` | written by `ZeusWriter`, but only stored in `Directives`; the parser does not fill `RequiredInputVariables`. `ZeusExecutioner.ExecuteGuiCode` skips the GUI when `context.Input.Contains(reqVars)` |
| `GUI_ENGINE`, `GUI_LANGUAGE` | engine/language of the GUI segment |
| `BODY_ENGINE`, `BODY_LANGUAGE` | engine/language of body |
| `BODY_MODE` | `Markup` (default; text with tags) or `Pure` (all code) |
| `BODY_TAG_START`, `BODY_TAG_END` | tag delimiters, default `<%` and `%>` |
| `TEMPLATE_INCLUDE` | for `Group`: path of a child template (repeatable) |

Every directive (including unknown/custom ones) is also placed in `ZeusTemplate.Directives` (`NameValueCollection`).

## Block sections
`##|COMMENTS_BEGIN/END`, `##|GUI_BEGIN/END`, `##|BODY_BEGIN/END`. Lines between are stored verbatim (joined with `\r\n`, `TrimEnd`) into `ZeusTemplate.Comments`, `GuiSegment.CodeUnparsed`, `BodySegment.CodeUnparsed`.
- GUI segment: mode always `Pure` (the whole block is code). Body: per `BODY_MODE`.
- Defaults: engine `.Net Script`, language `C#`; the parser sets them from directives.

## Template groups
`TYPE Group`: no meaningful body; has `TEMPLATE_INCLUDE` lines. GUI collects input once, then every included template body is run with that input (see [execution-flow.md](execution-flow.md)). Example: `templates/Other/TemplateGroupExample.jgen` (JScript GUI via Microsoft Script).

## Header-only load
`ZeusTemplateParser.LoadTemplateHeader` returns `ZeusTemplateHeader` (UniqueID, Title, Namespace, SourceType, Comments, FileName, FilePath) so browsers can list templates cheaply.

## Writing back
`ZeusWriter` serializes a `ZeusTemplate` back to the `##|` format (including `INPUT_VARIABLES`). Path splitting in the parser uses `'\\'` (Windows-only).

## Legacy format
`Legacy/LegacyTemplateParser`: directives `GROUP`(namespace), `ENGINE`, `SCRIPT`(language), `OUTPUT`, `MODE` (`taggedtemplate`/`purescript`/`templategroup`), `TAG_START`, `TAG_END`, `INTERFACE_BEGIN/END` (= GUI), `BODY_BEGIN/END`, `TEMPLATE_INCLUDE`. Defaults engine `Microsoft Script`, language `JScript`; invalid MODE throws `ZeusParseError.InvalidTemplateMode`. Discard in a rewrite.

## Notes
- `Zeus/ZeusConstants.cs` is an older copy not in `Zeus.csproj`; use `EngineInterface/ZeusConstants.cs`.
- A BOM is present at the start of sample templates.

## Related pages
- [template-parsing.md](template-parsing.md)
- [script-engines.md](script-engines.md)
- [execution-flow.md](execution-flow.md)
- [scriban-dotnet10-mapping.md](scriban-dotnet10-mapping.md)
- [INDEX.md](INDEX.md)
