---
title: Template browser and editor
summary: TemplateBrowser discovers and operates on Zeus templates, while TemplateEditor edits template metadata and source and executes templates through the Zeus runtime.
keywords: [templates, browser, editor, Zeus, execution, projects]
source_paths: [mygeneration-legacy-code/mygeneration/MyGeneration/DockableWindows/TemplateBrowser.cs, mygeneration-legacy-code/mygeneration/MyGeneration/DockableWindows/TemplateTreeBuilder.cs, mygeneration-legacy-code/mygeneration/MyGeneration/TemplateForms/TemplateEditor.cs, mygeneration-legacy-code/mygeneration/MyGeneration/DockableWindows/ProjectEditor/ProjectBrowser.cs, mygeneration-legacy-code/mygeneration/MyGeneration/DockableWindows/TemplateEditorManager.cs]
---

# Template browser and editor

## Browser model and navigation

`DockableWindows/TemplateBrowser.cs` constructs a `TemplateTreeBuilder` against its `TreeView` and initially calls `LoadTemplates`. `TemplateTreeBuilder` builds a hierarchy from template headers in the configured default template directory and groups namespaced templates under dotted namespace nodes (`LoadTemplates`). Its other entry points include `LoadTemplatesByFile` (folder/file view) and `LoadTemplatesFromWeb` (online library). `RefreshTree` and `ChangeMode` repopulate the selected browsing mode; F5 triggers a local reload. `TemplateTreeNode` stores header metadata such as path, unique ID, title, comments, URL, and source type.

Opening a selected unlocked template calls `IMyGenerationMDI.OpenDocuments`; locked templates are rejected by the browser. The browser's command surface includes execute, open, refresh/mode, web update, delete, save collected input, and execute saved input. Web updates call `TemplateWebUpdateHelper.WebUpdate` and notify the shell with `UpdateTemplate`; bulk updates prompt before passing the selected node to `WebUpdate(TreeNode)`. That method iterates the node’s immediate children; nested folders take its non-template/error branch rather than being recursively traversed. Online retrieval is network-dependent and should not be assumed available in a local TUI.

## Executing a template

`TemplateBrowser.ExecuteTemplate(ZeusTemplate)` creates a `ZeusContext`, populates it through `DefaultSettings.PopulateZeusContext`, applies `ScriptTimeout`, installs `DynamicGUI_Display` for any template-provided GUI segment, then executes the GUI segment followed by the body segment. Runtime exceptions are presented using `ZeusDisplayError`; successful rendering may copy output to the clipboard when `EnableClipboard` is true and displays a success dialog. `SaveInput` invokes `template.Collect` and serializes collected values as `.zinp`; `ExecuteLoadedInput` reloads saved input and executes the referenced template.

The `DynamicGUI_Display` callback hosts the runtime-provided `IZeusGuiControl` in a WinForms `DynamicForm`. This is a separate interaction mode from editing source and needs an explicit product decision for a TUI (for example a supported prompt schema vs. dropping arbitrary WinForms controls).

## Editing and file types

`DockableWindows/TemplateEditorManager.cs` maps `.jgen`, `.vbgen`, `.csgen`, and `.zeus` files to `TemplateEditor`. Its `Create` method selects a scripting engine and language for new templates: C# and VB.Net templates use the .NET script engine; VBScript and JScript use the Microsoft script engine. The file editor is in `TemplateForms/TemplateEditor.cs` and maintains both template source and its metadata. The metadata UI populates fields for engine, body language, GUI engine/language, mode/type, output language, included template files, start tag, and GUID (see dropdown population/selection handlers and `RefreshControlFromTemplate` / `RefreshTemplateFromControl`).

The editor supports new/open/save/save-as, dirty-state prompts, execute, encrypted/compiled save variants, saved-input collection, find/replace, go-to-line, and Scintilla view options. `FileSave`, `FileSaveAs`, `_Initialize`, `_Execute`, and `CanClose` are the principal lifecycle methods. On execute it dispatches to the template's GUI/body runtime and reports errors through the editor and shell. The TUI rewrite can keep a single source editor plus a structured properties form; platform-specific Scintilla configuration, docking, and legacy scripting-language choices are not automatically required.

## Projects

`DockableWindows/ProjectEditorManager.cs` maps `.zprj` to `ProjectBrowser`. `ProjectBrowser.CreateNewProject` creates a `ZeusProject`; `LoadProject` and `LoadModule` reconstruct nested `ZeusModule` nodes and saved template-input objects. The tree lets the user add/edit/remove modules and saved objects, save/save-as, and execute the selection. `ProjectBrowser.Execute` executes a selected module/project or saved input using the configured timeout and a `ProjectExecuteStatus` progress/log window. The project is a batch/container workflow, not a second template source format.

## Related pages

- [Main application](main-application.md)
- [Connection and settings](connection-and-settings.md)
- [Scintilla editor](scintilla-editor.md)
- [Utilities](utilities.md)
- [TUI mapping](tui-mapping.md)
