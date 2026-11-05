---
title: Template parsing (ZeusTemplateParser, ZeusCodeParser, tags)
summary: Explains how a template file is loaded into ZeusTemplate and how body/GUI code is converted from <% %>, <%= %>, <%# %> markup into executable script.
keywords: [ZeusTemplateParser, ZeusCodeParser, ZeusCodeSegment, markup, pure, tags, output.write, custom tag, ParseMarkup]
source_paths:
  - mygeneration-legacy-code/mygeneration/Zeus/ZeusTemplateParser.cs
  - mygeneration-legacy-code/mygeneration/Zeus/ZeusCodeParser.cs
  - mygeneration-legacy-code/mygeneration/Zeus/ZeusCodeSegment.cs
  - mygeneration-legacy-code/mygeneration/Zeus/ZeusTemplate.cs
  - mygeneration-legacy-code/mygeneration/Zeus/ZeusEncryption.cs
---

# Template parsing

Two stages: (1) file -> `ZeusTemplate` model (`ZeusTemplateParser`); (2) segment text -> engine script (`ZeusCodeParser.ParseCode`).

## Stage 1: ZeusTemplateParser
See [template-file-format.md](template-file-format.md). Reads directive lines, assigns properties (`AssignProperty`), captures block text into `ZeusCodeSegment.CodeUnparsed`. `ZeusTemplate` holds `GuiSegment` (Pure), `BodySegment`, `Comments`, `IncludedTemplatePaths`, `Directives`, `TagStart/TagEnd`, `ShortcutTagStart` (= start + `=`), `SpecialTagStart` (= start + `#`).

## Stage 2: ZeusCodeParser.ParseCode(segment)
1. Skip if template `SourceType` is Compiled and `segment.CachedAssembly` exists.
2. If Encrypted, `ZeusEncryption.Decrypt` the text.
3. Pick the engine: `ZeusFactory.GetEngine(segment.Engine)` -> its `IZeusCodeParser`.
4. Clear `segment.ExtraData`; call `ParseMarkup` or `ParsePure` depending on `segment.Mode`; result goes to `segment.Code`.

### Markup mode (`ParseMarkup`)
Scans line by line for tag start/end.
- Text outside tags -> `codeParser.BuildOutputCommand(language, text, isLiteral:true, addNewLine)` (emits `output.write("...")` / `output.writeln(...)`, literal escaped via `EscapeLiteral`).
- `<%= expr %>` (shortcut) -> same command with `isLiteral:false` (expression output). May span lines.
- `<% code %>` -> code appended verbatim (trimmed + `\r\n`).
- `<%# ... %>` (special/custom) -> `codeParser.ParseCustomTag(segment, text)`; must start and end on a single line. Engine-specific (see [script-engines.md](script-engines.md)).
- Final code = `GetCustomHeaderCode` + body + `GetCustomFooterCode`.
- Quirk: the trailing empty last line is skipped.

Note: `<%# %>` is not data binding; it is the engine directive tag (references, namespaces, includes).

### Pure mode (`ParsePure`)
Whole segment is code; only `<%# %>` tags (single line) are processed; header/footer added the same way. GUI segments use this.

## ZeusCodeSegment state
`CodeUnparsed`, `Code`, `Engine` (default `.Net Script`), `Language` (default `C#`), `Mode`, `ExtraData` (Hashtable for e.g. `DLLREF`, `USE`), `CachedAssembly`. For compiled templates `CodeUnparsed` holds `BASE64:` + assembly bytes.

## Encryption / compile
`ZeusTemplate.Encrypt()` uses `ZeusEncryption` (custom scramble + base64); `ZeusTemplate.Compile()` flips to `Compiled` only if both engines are .Net Script. Discard both in a rewrite.

## Related pages
- [template-file-format.md](template-file-format.md)
- [script-engines.md](script-engines.md)
- [execution-flow.md](execution-flow.md)
- [scriban-dotnet10-mapping.md](scriban-dotnet10-mapping.md)
- [INDEX.md](INDEX.md)
