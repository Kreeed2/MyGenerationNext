---
title: Shared UI utilities
summary: MyGenUtility centralizes settings, extension loading, document/content factories, execution coordination, and reusable WinForms diagnostic/output panes.
keywords: [utilities, MyGenUtility, console, errors, generated files, execution]
source_paths: [mygeneration-legacy-code/mygeneration/MyGenUtility, mygeneration-legacy-code/mygeneration/EngineInterface, mygeneration-legacy-code/mygeneration/MyGeneration/DockableWindows/ProjectEditor/ProjectExecuteStatus.cs]
---

# Shared UI utilities

## Shared library responsibility

`mygeneration-legacy-code/mygeneration/MyGenUtility/MyGenUtility.csproj` is a shared assembly referenced by the main application and related integration projects. It is not merely a collection of generic helpers: it owns the shared `DefaultSettings` singleton, `PluginManager`, abstract `EditorManager`/`ContentManager` factories, error objects, process coordination, and reusable Windows Forms. The public UI/plugin contracts reside in `EngineInterface`.

Key components include:

- `Forms/Console/ConsoleForm`: output/console pane used for generation details.
- `Forms/Errors/ErrorsForm`, `ErrorDetailControl`, and `ErrorDetail`: aggregate/display runtime errors and details. `MyGenerationMDI.ErrorsOccurred` and `PerformMdiFuntion` connect execution errors back to the shell and editor.
- `Forms/GeneratedFileList/GeneratedFilesForm`: tracks generated output files and provides a view of output produced during execution.
- `Forms/ProjectEditor/ProjectExecuteStatus.cs` (the shared-utility counterpart) and `MyGeneration/DockableWindows/ProjectEditor/ProjectExecuteStatus.cs`: progress/status windows associated with project execution; distinguish files by project path before attributing behavior.
- `Forms/TemplateBrowser` and `Forms/TemplateEditor`: shared browser/editor controls used in MyGeneration and the IDE add-ins, separate from dockable wrappers under `MyGeneration/DockableWindows`.
- `ZeusController` and `ZeusProcessManager`: host/execution coordination. `MyGenerationMDI` displays queue status through host callbacks and can stop active execution on close.

Some tree, project, connection-test, and editor form implementations occur both under `MyGenUtility/Forms` and `MyGeneration`. They are not necessarily interchangeable; cite and trace the version called by the active host rather than generalizing by class name alone.

## TUI replacements

Map the console to a scrollable output log, errors to a selectable diagnostics list/detail, generated files to a result list with open/reveal actions, and project progress to a cancellable status view. Use structured events from the execution layer to update these views. Do not retain the old global static controls, WinForms form ownership, modal message boxes, or dock-pane assumptions as architectural dependencies.

## Related pages

- [Main application](main-application.md)
- [Template browser and editor](template-browser-and-editor.md)
- [Plugins](plugins.md)
- [IDE plugins and installer](ide-plugins-installer.md)
- [TUI mapping](tui-mapping.md)
