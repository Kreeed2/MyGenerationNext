---
title: Main WinForms application
summary: MyGenerationMDI is the launched dockable host that creates documents, restores panes, dispatches plugin commands, and coordinates shutdown.
keywords: [main application, MDI, docking, startup, documents, windows]
source_paths: [mygeneration-legacy-code/mygeneration/MyGeneration/Program.cs, mygeneration-legacy-code/mygeneration/MyGeneration/MyGenerationMDI.cs, mygeneration-legacy-code/mygeneration/EngineInterface/IMyGenerationMDI.cs]
---

# Main application

## Startup and configuration

`Program.Main` in `mygeneration-legacy-code/mygeneration/MyGeneration/Program.cs` creates `MyGenerationMDI` on the STA thread, installs `UnhandledExceptions` and `OnThreadException`, and enters the message loop. `MyGenerationMDI` accepts a startup directory and command-line arguments. Its constructor recognizes `-configfile <path>` and sets `DefaultSettings.SettingsFileName` when the resolved file exists; other arguments are retained as startup files. It initializes menu entries using `EditorManager.AddNewDocumentMenuItems`, `ContentManager.AddNewContentMenuItems`, and `PluginManager.AddHelpMenuItems`.

On load, `MyGenerationMDI_Load` restores window position/state from settings, loads Scintilla configuration, restores dock layout from `settings\dock.config` through `GetContentFromPersistString`, opens startup documents, and opens settings UI on first load. Layout deserialization maps persisted type names to known built-in pane instances; project document restoration includes file paths.

## Shell responsibilities

The `MyGenerationMDI` form has a dock panel and menu/toolbar, status area, and built-in content for template browsing, metadata browsing/properties, language and database-target mappings, local/global user metadata, options, console, errors, generated-file list, and error detail. Pane instances are exposed via properties used by the shell and plugins. Active document changes merge a document's `ToolStrip` into the shell toolbar (`dockPanel_ActiveContentChanged`).

The `IMyGenerationMDI` contract in `EngineInterface/IMyGenerationMDI.cs` exposes document creation/opening, document lookup, `DockPanel`, the Zeus controller, find/replace dialogs, alerts and host functions. This is the seam between a document/plugin and the desktop shell, rather than a user-facing standalone application API.

## Documents and plugin actions

- `OpenDocuments` loops over existing files, activates an already-open document or resolves a new one using `EditorManager.OpenDocument`, and adds successful paths to recent files.
- `CreateDocument` delegates a named file type to `EditorManager.CreateDocument`.
- `OpenContent` toggles a content manager's dock window and caches it in `dynamicContentWindows`.
- `ExecuteSimplePlugin` invokes a command-style `ISimplePluginManager` and routes exceptions to `ErrorsOccurred`.
- `PerformMdiFuntion` (spelling in source) provides host services such as opening the OLE DB dialog, exposing the active `dbRoot`, and updating execution/error UI.

## Shutdown and concurrency

`MyGenerationMDI_FormClosing` asks whether active template executions should be killed, prompts/consults documents through `CanClose`, records window state and saves settings. `Shutdown` closes hidden panes and saves the dock layout as XML. Execution queue status is driven through `ZeusProcessManager`; preserve clear running/cancel/error status in a TUI, but not WinForms docking mechanics.

There is a second `MDIParent` class in `mygeneration-legacy-code/mygeneration/MyGeneration/MDIParent.cs`, which is not the `Program.Main` startup target. It represents the older MDI design and should not be confused with the `MyGenerationMDI` execution path.

## Related pages

- [Overview](overview.md)
- [Template browser and editor](template-browser-and-editor.md)
- [Utilities](utilities.md)
- [Plugins](plugins.md)
- [TUI mapping](tui-mapping.md)
