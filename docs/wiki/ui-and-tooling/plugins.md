---
title: UI and provider plugins
summary: PluginManager discovers selected UI extension contracts from adjacent assemblies, while MyMeta providers and standalone test tools follow separate extension paths.
keywords: [plugins, extension points, SqlTool, MyMeta, CodeSmith, SampleUI]
source_paths: [mygeneration-legacy-code/mygeneration/MyGenUtility/Forms/PluginManager.cs, mygeneration-legacy-code/mygeneration/MyGenUtility/Forms/ContentManager.cs, mygeneration-legacy-code/mygeneration/MyGenUtility/Forms/EditorManager.cs, mygeneration-legacy-code/mygeneration/EngineInterface, mygeneration-legacy-code/plugins]
---

# Plugins

## UI extension mechanism

`MyGenUtility/Forms/PluginManager.cs` seeds editor managers with `TemplateEditorManager` and `ProjectEditorManager`, then scans the directory containing the executing assembly for `MyGeneration.UI.Plugins.*.dll`. It uses reflection to load assemblies and instantiate types implementing `IEditorManager`, `IContentManager`, or `ISimplePluginManager`; load errors are retained in `PluginLoadErrors`. Managers are keyed by their `Name`, and duplicate names are ignored. This is a desktop assembly convention, not a general package ecosystem.

`EditorManager` discovers file types/extensions and routes open/create document operations. `ContentManager` contributes toggled dockable content to the Plugins menu and optional toolbar buttons. `ISimplePluginManager` adds commands run by `MyGenerationMDI.ExecuteSimplePlugin`; `IContentManager` and `IEditorManager` create `IMyGenContent` and `IMyGenDocument` instances. The host interfaces are in `mygeneration-legacy-code/mygeneration/EngineInterface` and carry WinForms/DockPanel types, so a TUI should not try to preserve binary compatibility with these interfaces.

## Representative plugins in this repository

- **SqlTool:** `plugins/MyGeneration.UI.Plugins.SqlTool/SqlToolPluginManager.cs` registers an “Open in SqlTool” simple command. `SqlToolUserControl` hosts a Scintilla query editor and result grid, obtains the active MyMeta connection through host functions, and executes commands. This depends on MyMeta's active connection and Windows editor controls; it is optional rather than the core generator.
- **SampleUIPlugin:** `SampleSimplePluginManager` launches a sample modal form; sample content/editor managers illustrate extension contracts. It is an SDK/example implementation, not product workflow.
- **CodeSmith UI plugin:** `plugins/MyGeneration.UI.Plugins.CodeSmith2MyGen/CodeSmith2MyGenPluginManager.cs` opens `FormConvertCodeSmith`. It selects CodeSmith template files, parses/rewrites them to `.zeus`, logs conversion errors, and can open generated templates in the host. `CodeSmithConversionLibrary` contains parser, language conversion, mapping, and plugin hooks.
- **MyMetaXsd3bPlugin:** `Xsd3bProvider` implements `IMyMetaPlugin` and presents XSD/XSD3B/XML-backed metadata as a provider; `MyGen2Xsd3b` converts schema metadata to Xsd3b. It is not a UI plugin and illustrates that MyMeta providers are a separate extension surface.
- **MyMetaPluginTestTool:** standalone WinForms test harness. `MyMetaPluginTests.Test` initializes a plugin and checks connection, database, table, view, and procedure metadata. It tests providers, not the MyGeneration UI extension loader.

## Rewrite boundary

Keep user-facing capabilities that actually serve the MSSQL-only workflow (connection, schema inspection, template execution, useful diagnostics). Re-evaluate the SqlTool and converter separately. Drop reflection loading of arbitrary legacy assemblies, sample UI, alternate provider plugins, and DockContent-based extension contracts unless the new product has an explicit supported plugin API.

## Related pages

- [Main application](main-application.md)
- [Utilities](utilities.md)
- [IDE plugins and installer](ide-plugins-installer.md)
- [TUI mapping](tui-mapping.md)
