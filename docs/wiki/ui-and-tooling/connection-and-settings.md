---
title: Connection and settings UI
summary: DefaultSettingsControl edits database, metadata mapping, template, output, timeout, and execution preferences; MetaDataBrowser connects through MyMeta and exposes schema browsing.
keywords: [database, connection, settings, MyMeta, SQL Server, mappings]
source_paths: [mygeneration-legacy-code/mygeneration/MyGenUtility/Forms/DefaultSettings/DefaultSettingsControl.cs, mygeneration-legacy-code/mygeneration/MyGenUtility/Forms/DefaultSettings/DefaultSettingsDialog.cs, mygeneration-legacy-code/mygeneration/MyGeneration/DockableWindows/MetaDataBrowser.cs, mygeneration-legacy-code/mygeneration/MyGeneration/TestConnectionForm.cs, mygeneration-legacy-code/mygeneration/MyGeneration/DockableWindows/DbTargetMappings.cs]
---

# Connection and settings UI

## Default settings view

`DefaultSettingsDialog` hosts `DefaultSettingsControl`, initializes it with the `IMyGenerationMDI` host and supplies a callback for the OLE DB connection-string dialog. On load it calls `Populate`. The control binds driver and connection string, language mapping file/language, database target mapping file/target, user metadata file, database-user-metadata aliases, template/output directories, timeout, tab width, clipboard, asynchronous browser execution, console detail, default-database-only, line numbering, document-style settings, domain override, and update checks. Some driver choices disable the OLE DB browse button.

`DefaultSettingsControl.Save` persists settings and connection-profile changes; a successful save triggers `UpdateDefaultSettings` via `mdi.SendAlert`. The dialog's closing handler detects modified settings or connection profile, asks to save/discard/cancel, and signals the shell after a successful save. The dialog callback delegates `ShowOLEDBDialog` through `PerformMdiFuntion` to the host's Windows OLE DB Data Links prompt. In the `.NET 10` rewrite, do not depend on that COM prompt; expose SQL Server connection fields/provider parsing and an explicit test action instead.

## Connection testing and metadata browser

`MetaDataBrowser.Setup` creates a MyMeta `dbRoot`, optionally tests the configured driver/connection in non-debug builds, calls `Connect`, then assigns language/database-target mapping paths and values, user metadata path, `DomainOverride`, and `ShowSystemData`. Errors are passed to `InitializeTree`, which displays an error root rather than silently presenting an empty schema. `DefaultSettingsChanged` reruns setup when settings change. The tree expands database/table/view/procedure, column, parameter, result-column, index, foreign-key, and domain nodes; selection is exposed in `MetaProperties`.

The `TestConnectionForm.Test` static method performs the legacy connection test and the form reports progress/state; `DefaultSettingsControl.TestConnection` is used by the settings dialog. It is an asynchronous, animated Windows dialog, not a distinct domain concept. Preserve the underlying test result and error text, not its animation.

Connection settings are not only a driver and string: profile data can include metadata file paths, language and target values, and default-database/alias settings. `ConnectionInfo` in `MyGenUtility/DefaultSettings.cs` is the profile data model. For the requested MSSQL-only product, make SQL Server the only configured provider and clarify which metadata mapping options remain useful; providers visible in the legacy UI are historical support, not a mandate.

## Database and language target mappings

`DbTargetMappings` presents mappings from database metadata type names to generated-language target types; `LanguageMappings` configures language-specific type mappings. These support generated templates, not SQL Server connection authentication. `MetaProperties` is a read-only property view over selected MyMeta objects, while `UserMetaData` and `GlobalUserMetaData` expose local/global overrides. Schema browsing and these mapping/metadata panes can be modeled as TUI tree/detail and table-edit views if still needed.

## Related pages

- [Settings persistence](settings-persistence.md)
- [Main application](main-application.md)
- [Template browser and editor](template-browser-and-editor.md)
- [TUI mapping](tui-mapping.md)
