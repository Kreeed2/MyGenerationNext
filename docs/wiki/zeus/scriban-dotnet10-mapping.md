---
title: Zeus to Scriban / .NET 10 mapping and discard list
summary: Maps each Zeus concept to a Scriban/.NET 10 console equivalent and lists legacy parts to drop in the rewrite.
keywords: [Scriban, .NET 10, rewrite, mapping, discard, TUI, JSON config, SQL Server]
source_paths:
  - mygeneration-legacy-code/mygeneration/Zeus
  - mygeneration-legacy-code/mygeneration/EngineInterface
  - mygeneration-legacy-code/mygeneration/ZeusCmd
---

# Zeus -> Scriban / .NET 10

This page is a design proposal derived from the legacy behavior documented in sibling pages; Scriban specifics are recommendations, not legacy facts.

## Mapping
| Zeus concept | Legacy source | .NET 10 / Scriban equivalent |
|---|---|---|
| `##\|` header directives (TITLE, NAMESPACE, UNIQUEID, OUTPUT_LANGUAGE) | `ZeusTemplateParser` | Front-matter / sidecar JSON metadata next to a `.scriban` file |
| Body `<% %>`, `<%= %>`, literal text | `ZeusCodeParser.ParseMarkup` | Scriban `{{ code }}` and `{{ expr }}`; literal text native. No C# in templates; use custom functions |
| `<%#REF/NS/FILE %>` | `DotNetScriptCodeParser` | Scriban `include`, registered `ScriptObject` helpers |
| `output.write/writeln/autoTab/tabLevel` | `ZeusOutput` | Scriban text output + helper function for indentation |
| `output.save(path, action)` | `ZeusOutput.save` | Registered function `save_file(path, mode)` (backup/overwrite/append) -> `File.WriteAllText`; collect written paths |
| Preserve regions | `setPreserveSource/preserve` | Keep as helper if needed (`//:: PRESERVE_BEGIN key ://`) |
| `input[...]` | `ZeusInput` | `ScriptObject` `input` from JSON saved-input |
| `context`, `objects`, intrinsic objects (`MyMeta`, `ui`, `DnpUtils`) | `ZeusContext`, `ZeusConfig` | `TemplateContext` with `ScriptObject`s: `meta` (SQL Server model, see MyMeta wiki), `input`, `output` helpers |
| GUI segment, `ui.Add*` | `GuiController` | Declarative `inputs` list in JSON/metadata (id, type, label, default, bind: databases/tables/views/procs); TUI prompts (e.g. Spectre.Console) fill them |
| Event handlers (`onchange`) | `AttachEvent` | Parent/child binding rules only (database -> tables) |
| Group templates | `TYPE Group`, `TEMPLATE_INCLUDE` | JSON list of templates run against one input set |
| Saved input `.zinp` | `ZeusSavedInput` | JSON file (template path, id, input values) |
| Project `.zprj`/modules | `ZeusProject`, `ZeusModule` | JSON project with nested modules and saved instances; layered defaults |
| `ZeusConfig.xml`, `DefaultSettings.xml` | `ZeusConfig`, `DefaultSettings` | `appsettings`-style JSON (template root, output dir, connection string, timeout) |
| Preprocessor connecting MyMeta from `__db*` keys | `DefaultContextProcessor` | Build SQL Server metadata from connection string before render |
| ZeusCmd switches | `CmdInput` | `System.CommandLine` with template, input, output, collect, project, module, timeout, silent, log |
| `ILog`, `ZeusSimpleLog` | | `ILogger` / simple console + file logger |
| Timeout | `-e` | `CancellationTokenSource` / Scriban limits |

## Discard
- MSScriptControl COM engine, VBScript/JScript (`MicrosoftScriptEngine`), `Scripting.Dictionary`.
- CodeDom compile, `DotNetScriptEngine`, compiler versions, `$ZEUS$DEBUG$` files, `Compiled`/`Encrypted` source types, `ZeusEncryption`.
- WinForms `DynamicForm`/`FormBuilder`, `ZeusDisplayError`, Scintilla/docking `IMyGen*` interfaces, UserControl stubs.
- Legacy parser (`LegacyTemplateParser`), old `Zeus/ZeusConstants.cs`, dead root interfaces.
- Plugin loading by assembly (`DynamicAssemblyTools`, `-aio/-rio`, `ScriptingEngine`/`Serializer`/`Preprocessor` config), `WebUpdate`, `Dnp.Utils`, VS2005 add-in, `%ZEUSHOME%` and `E:\pub` paths, Windows-only `'\\'` parsing, `.chm` docs.
- Multi-DB providers/OLE DB `-tc` types (SQL Server only), non-C# output languages and language/target mapping files.

## Keep (behaviors)
Header metadata, namespace tree browsing, per-template input prompts with dependent combos, input persistence, save modes with `.NNN.bak` backups, preserve blocks, project/module batching with layered defaults, grouping, exit codes and generated-file reporting.

## Related pages
- [template-file-format.md](template-file-format.md)
- [script-engines.md](script-engines.md)
- [gui-controls.md](gui-controls.md)
- [execution-flow.md](execution-flow.md)
- [INDEX.md](INDEX.md)
