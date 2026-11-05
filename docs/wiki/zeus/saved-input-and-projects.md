---
title: Saved input (.zinp), projects (.zproj/.zprj) and serializers
summary: Explains how collected GUI input is serialized, saved templates instances, the project/module XML model and how projects execute.
keywords: [ZeusSavedInput, SavedTemplateInput, ZeusProject, ZeusModule, InputItem, MasterSerializer, zinp, zproj, zprj]
source_paths:
  - mygeneration-legacy-code/mygeneration/Zeus/SavedInput/ZeusSavedInput.cs
  - mygeneration-legacy-code/mygeneration/Zeus/SavedInput/SavedTemplateInput.cs
  - mygeneration-legacy-code/mygeneration/Zeus/SavedInput/SavedTemplateInputCollection.cs
  - mygeneration-legacy-code/mygeneration/Zeus/SavedInput/InputItem.cs
  - mygeneration-legacy-code/mygeneration/Zeus/Serializers/MasterSerializer.cs
  - mygeneration-legacy-code/mygeneration/Zeus/Serializers/MyMeta
  - mygeneration-legacy-code/mygeneration/Zeus/Projects/ZeusProject.cs
  - mygeneration-legacy-code/mygeneration/Zeus/Projects/ZeusModule.cs
  - mygeneration-legacy-code/mygeneration/Zeus/Projects/ZeusModuleCollection.cs
---

# Saved input and projects

Extension note: UI file dialogs use `*.zinp` (saved input) and `*.zprj` (project, `MyGeneration/DockableWindows/ProjectBrowser.cs`). The class is named `ZeusProject`; some code comments say `.zproj`. The user sidecar is `<projectpath>usr`.

## Input items and serialization
- `InputItem` = (name, type name, serialized string). `InputItemCollection` is SortedList-based IDictionary; `Add(IZeusInput)` stores an unserializable type's exception message.
- `MasterSerializer.Dissolve` / `Reconstitute`: simple types (String, ints, Decimal, Char, Guid, floats, Boolean); DateTime as `ToUniversalTime().ToString()`; TimeSpan via `ToString`; others via `ITypeSerializer`s from `ZeusFactory.Serializers`: Hashtable, ArrayList, SimpleTable (`CollectionSerializer`), MyMeta serializers (`MyMetaSerializer` = `dbRoot` as `;`-delimited connection string, target mapping, domain override, driver, language, language mapping, user metadata file; IDatabase/ITable/IView/IProcedure/IColumn serializers use `|`-delimited strings with escapes `{~P~}`, `{~S~}`, `{~C~}`), plus config-listed serializers.

## SavedTemplateInput
XML: `<obj name uid path><items><item name type val/></items></obj>`.
`Execute(timeout, log)`: `new ZeusTemplate(FileTools.ResolvePath(path))`; new `ZeusInput` from items; `ApplyOverrideData`; `new ZeusContext(zin, new Hashtable())`; `template.Execute(context, timeout, true)` (skipGui); collects `context.Output.SavedFiles`. Also `Copy()`. Collection: `SavedTemplateInputCollection` (executes each, aggregates `SavedFiles`).

## ZeusSavedInput (.zinp)
File wrapper (default `default.zinp`) with one `<obj>` (`InputData`: `TemplatePath`, `TemplateUniqueID`, `InputItems`), `Load()`/`Save()`.

## ZeusProject / ZeusModule
`ZeusProject : ZeusModule`. XML root `<project name description defaultSettingsOverride>` containing `<item>` (inherited settings), `<obj>` (saved template inputs) and nested `<module>`s (`ZeusModuleCollection`). User file stores per-user overrides (`BuildUserXML`/`ReadUserXML`). `ProjectPath` is `/Root/Module`.
- `Execute(timeout, log)`: executes SavedObjects, then ChildModules, aggregating `SavedFiles`.
- `PopulateZeusContext` / `FillZeusInputRecursive`: layer parent->child module items, then default settings (via `GetDefaultSettingsDelegate`) if `DefaultSettingsOverride`, then user overrides. `ApplyRuntimeOverrides` hooked through `ApplyOverrideDataDelegate`. `OverrideSavedData(items)`.

## Related pages
- [execution-flow.md](execution-flow.md)
- [zeuscmd.md](zeuscmd.md)
- [context-and-intrinsic-objects.md](context-and-intrinsic-objects.md)
- [scriban-dotnet10-mapping.md](scriban-dotnet10-mapping.md)
- [INDEX.md](INDEX.md)
