---
title: UI and tooling overview
summary: The legacy product is a WinForms MDI host around Zeus templates, MyMeta metadata, Scintilla editing, XML settings, and loadable UI plugins.
keywords: [overview, architecture, WinForms, MDI, Zeus, MyMeta]
source_paths: [mygeneration-legacy-code/mygeneration/MyGeneration/Program.cs, mygeneration-legacy-code/mygeneration/MyGeneration/MyGenerationMDI.cs, mygeneration-legacy-code/mygeneration/MyGenUtility/DefaultSettings.cs, mygeneration-legacy-code/mygeneration/EngineInterface]
---

# UI and tooling overview

## Executable and host

The checked-in application entry point is `Program.Main` in `mygeneration-legacy-code/mygeneration/MyGeneration/Program.cs`. It is `[STAThread]`, constructs `MyGenerationMDI(Application.StartupPath, args)`, attaches AppDomain and WinForms thread exception handlers, and runs the form with `Application.Run`. This is the live startup route represented by that project. `MDIParent` is another, older-looking `Form`/`IMyGenerationMDI` implementation in the same project; do not treat it as the entry point when tracing current startup.

`MyGenerationMDI` owns the Windows Forms / WeifenLuo docking host, settings singleton, browser/property panes, dynamic content windows, recent-file menu, and plugin/editor manager hookups. `EngineInterface/IMyGenerationMDI.cs`, `IMyGenContent.cs`, `IMyGenDocument.cs`, `IEditorManager.cs`, `IContentManager.cs`, and `ISimplePluginManager.cs` define the host contracts used by plugin and document implementations.

## Important code layers

- **WinForms shell and views:** `mygeneration-legacy-code/mygeneration/MyGeneration` contains `MyGenerationMDI`, `TemplateBrowser`, `TemplateEditor`, `ProjectBrowser`, metadata panes, and settings UI wrappers.
- **Reusable application components:** `mygeneration-legacy-code/mygeneration/MyGenUtility` supplies `DefaultSettings`, `PluginManager`, abstract content/editor managers, common panes, and utility dialogs. The main project imports these shared types.
- **Template runtime:** `mygeneration-legacy-code/mygeneration/Zeus` includes template and project execution model; UI calls into that runtime rather than implementing generation itself.
- **Metadata:** MyMeta (`mygeneration-legacy-code/mymeta`) abstracts providers and schema objects. The metadata browser binds to `dbRoot` and interfaces such as `IDatabase`, `ITable`, and `IColumn`.
- **Editor:** the in-repository ScintillaNET sources and native `SciLexer.dll` implement the code editor; the app's `ZeusScintillaControl` specializes it for template languages.
- **Deployment/compatibility:** `plugins`, `ideplugins`, `codesmith2mygen`, `installer`, `lib/thirdparty`, and `documentation/help` hold optional integrations and packaging artifacts, not the essential TUI workflow.

## Scope signal for the rewrite

The legacy browser/settings expose multiple MyMeta drivers and multiple template scripting/output languages. That is historical breadth, not a requirement for the requested SQL Server-only, .NET 10 console/TUI rewrite. Preserve only behavior that is explicitly needed in that narrower product; do not infer that every legacy plugin/provider or WinForms layout must be ported.

## Related pages

- [Main application](main-application.md)
- [Template browser and editor](template-browser-and-editor.md)
- [Connection and settings](connection-and-settings.md)
- [Plugins](plugins.md)
- [TUI mapping](tui-mapping.md)
