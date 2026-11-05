---
title: Mapping legacy UI to a Terminal.Gui application
summary: A focused TUI should preserve template generation, SQL Server metadata, settings, diagnostics, and generated-file workflows while discarding Windows-specific shell and integration surfaces.
keywords: [TUI, Terminal.Gui, rewrite, screen mapping, SQL Server, migration]
source_paths: [mygeneration-legacy-code/mygeneration/MyGeneration/MyGenerationMDI.cs, mygeneration-legacy-code/mygeneration/MyGeneration/DockableWindows/TemplateBrowser.cs, mygeneration-legacy-code/mygeneration/MyGeneration/TemplateForms/TemplateEditor.cs, mygeneration-legacy-code/mygeneration/MyGeneration/DockableWindows/MetaDataBrowser.cs, mygeneration-legacy-code/mygeneration/MyGenUtility/DefaultSettings.cs]
---

# TUI mapping

The mapping below is a design recommendation based on the implemented screens and actions. It is not a claim that the old layout is the only correct workflow. The new product target is a .NET 10 console/TUI app for SQL Server only, so preserve domain behavior and simplify desktop-era interaction patterns.

| Legacy screen or behavior | Terminal.Gui direction | Keep / change |
|---|---|---|
| `MyGenerationMDI` dock shell, menus, toolbars, recent files, child-window activation | Root application frame with menu/command palette, status line, and navigable primary views | Keep navigation and file/document concepts. Discard MDI activation, docking XML, toolbar merge, resize/drag-drop shell mechanics. |
| `TemplateBrowser` namespace/file tree, refresh, open, execute | Template list/tree with filter, details pane, and explicit Open/Run actions | Keep local discovery and execution. Decide separately whether web update/library and encrypted-template controls are in scope. |
| `TemplateEditor` source and template properties | Editor view plus a metadata form/panel; save, validate, run, diagnostics | Keep source, template identity, engine/language/options only where supported. Replace Scintilla Win32 control; consider a line editor if Terminal.Gui cannot provide suitable editing. Arbitrary `DynamicForm` template GUIs require a TUI-compatible input schema or removal. |
| `ProjectBrowser` `.zprj` module/saved-input tree and `ProjectExecuteStatus` | Project outline, add/edit forms, run selection, progress/cancel view | Keep only if batch projects and saved inputs remain supported. Show execution events in a stable log rather than a separate floating form. |
| `DefaultSettingsDialog` connection and preferences | Connection/profile screen plus focused application settings | Keep MSSQL connection, default database, template/output path, timeout, language/template options. Drop non-SQL Server provider choices and OLE DB COM browsing. |
| `MetaDataBrowser` and `MetaProperties` | SQL Server database/schema/object list and selected-object detail | Keep databases, tables, columns, views/procedures and metadata actually consumed by templates. Do not retain MyMeta's broad provider abstraction unless an abstraction remains strategically useful. |
| `DbTargetMappings`, `LanguageMappings`, user/global metadata | Focused mapping editor or template-specific configuration | Keep only mappings/overrides needed by supported templates; avoid carrying a generalized multi-language/multi-driver configuration UI by default. |
| Console, `ErrorsForm`, generated file list, project status | Output/log pane, diagnostics list/detail, generated files view, progress/cancel status | Keep results and actionable diagnostics; use structured log entries and line/column navigation. |
| `SqlTool` plugin | Optional SQL query view only if product requirements call for it | It is an optional editor/query result grid plugin, not core generation. Its current design borrows the active MyMeta connection. |
| Sample/plugin manager, IDE add-ins, CodeSmith converter, updates/about/help | Usually omit from first TUI | Sample UI and Visual Studio/SharpDevelop are desktop-era extras; CodeSmith conversion is a migration utility; update/web-library behavior is network-specific. Include only on explicit requirements. |

## Suggested interaction flow

1. Start with a clear connection state and choose/connect to a SQL Server profile.
2. Browse the connected catalog and inspect selected database objects.
3. Browse/filter templates and inspect template metadata before editing or running.
4. Collect any supported template inputs in TUI-native forms, then run with visible progress and cancellation.
5. Review generated files, output, and diagnostics; allow direct navigation from a diagnostic to template source when available.
6. Persist profiles and application settings independently of transient screen arrangement.

## Behavioral constraints surfaced by the code

Legacy template execution has both GUI and body phases (`TemplateBrowser.ExecuteTemplate`) and can write through the Zeus output abstraction; projects can execute nested modules or saved inputs. Runtime errors are currently displayed in modal WinForms forms, and successful generation can copy output to the clipboard. The new app should decide explicitly how template input, generated-file output, cancellation, clipboard replacement, and process lifetime work. Do not silently claim all arbitrary legacy templates are TUI-compatible.

The legacy settings store can persist raw connection strings in XML. A new TUI should not copy this credential handling without a deliberate secure-storage design. Likewise, current templates/provider metadata can be broader than SQL Server; define supported features at the new product boundary, rather than silently presenting unsupported settings.

## Related pages

- [Overview](overview.md)
- [Main application](main-application.md)
- [Template browser and editor](template-browser-and-editor.md)
- [Connection and settings](connection-and-settings.md)
- [Settings persistence](settings-persistence.md)
- [Plugins](plugins.md)
- [Scintilla editor](scintilla-editor.md)
