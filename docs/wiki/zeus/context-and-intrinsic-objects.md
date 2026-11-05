---
title: Zeus context, input, log, intrinsic objects, config and preprocessors
summary: Describes ZeusContext and the objects visible to templates (input, output, ui, MyMeta, log), ZeusConfig, intrinsic object registration and the default context preprocessor.
keywords: [ZeusContext, ZeusInput, ZeusIntrinsicObject, ZeusConfig, ZeusSimpleLog, preprocessor, MyMeta, DefaultContextProcessor, __dbDriver]
source_paths:
  - mygeneration-legacy-code/mygeneration/Zeus/ZeusContext.cs
  - mygeneration-legacy-code/mygeneration/Zeus/ZeusInput.cs
  - mygeneration-legacy-code/mygeneration/Zeus/ZeusIntrinsicObject.cs
  - mygeneration-legacy-code/mygeneration/Zeus/ZeusSimpleLog.cs
  - mygeneration-legacy-code/mygeneration/Zeus/Configuration/ZeusConfig.cs
  - mygeneration-legacy-code/mygeneration/Zeus/Configuration/DefaultZeusConfig.xml
  - mygeneration-legacy-code/mygeneration/Zeus/ContextPreprocessors/DefaultContextProcessor.cs
  - mygeneration-legacy-code/mygeneration/MyGenUtility/DefaultSettings.cs
---

# Context and intrinsic objects

## ZeusContext
Hierarchy: `ZeusTemplateContext : IZeusContext` -> `ZeusGuiContext` -> `ZeusContext`.
Members: `Input` (`ZeusInput`), `Output` (`ZeusOutput`), `Gui` (`GuiController`), `Log` (`ILog`, default `ZeusSimpleLog`), `Objects` (Hashtable of intrinsic objects by variable name), `IntrinsicObjects`, `ExecutionDepth`, `ExecutingTemplate`, `TemplateStack`, `Describe(prefix, obj)` (reflection dump, depth<=2).
- Constructor sets `objects["ui"] = gui`. `ZeusTemplateContext(IZeusInput, Hashtable)` is used by saved-input runs.
- `Execute(path, copyContext)` (and deprecated `ExecuteTemplate`) runs another template: `new ZeusTemplate(abs path).Execute(this or Copy(), 0, true)`. `Copy()` shares input, objects, log, gui, intrinsic objects, template stack but gets a fresh `ZeusOutput`.

## ZeusInput (`input` in templates)
Hashtable wrapper: indexer, `Contains`, `ContainsKeys`, `Remove`, `Keys`, `Values`, `__Variables`, `CreateSimpleTable()`, `AddItems(GuiController | Hashtable | NameValueCollection | InputItemCollection)` (GuiController: only `IsDataControl` controls, `ID` -> `Value`).
Conventional keys filled by `DefaultSettings.PopulateZeusContext` (MyGenUtility): `__version`, `defaultTemplatePath`/`defaultOutputPath` (legacy) and `__defaultTemplatePath`/`__defaultOutputPath`, and when a driver is set: `dbDriver`, `dbConnectionString` (legacy), `__dbDriver`, `__dbConnectionString`, `__showDefaultDatabaseOnly`, `__domainOverride`, `__dbTarget`, `__dbTargetMappingFileName`, `__dbLanguageMappingFileName`, `__language`, `__userMetaDataFileName`, `__dbmap__<name>`. Values persist in `DefaultSettings.xml` (user dir `\Settings\`).

## Intrinsic objects
`ZeusIntrinsicObject` (assembly, ClassPath, VariableName). `Disabled` when the assembly file is missing; `DllReference` = file name; `Namespace` derived from ClassPath.
Default list in `DefaultZeusConfig.xml`: `ui` = `Zeus.UserInterface.GuiController`; `MyMeta` = `MyMeta.dbRoot`; `DnpUtils` = `Dnp.Utils.Utils`.
`ZeusExecutioner.PopulateContextObjects` instantiates each (`FileTools.ResolvePath` with `%ZEUSHOME%`, `DynamicAssemblyTools.InstantiateClassesByType`) into `context.Objects[VariableName]`.

## ZeusConfig
Singleton `ZeusConfig.Current`, loaded from `<ApplicationPath>\Settings\ZeusConfig.xml`, else embedded `DefaultZeusConfig.xml`. Elements: `CompilerParams version`, `ProjectRoot path`, `TemplateRoot path`, `WebUpdate url`, `Preprocessor assembly`, `Serializer assembly`, `TemplateReference add`, `TemplateNamespace add`, `ScriptingEngine assembly`, `IntrinsicObject assembly classpath varname`. `Save()` is used by ZeusCmd `-aio/-rio`.

## Preprocessors
`IZeusContextProcessor.Process(context)`; run in `ZeusExecutioner` for every GUI-skipped run and before GUI code. `ContextPreprocessors/DefaultContextProcessor`: if `Objects["MyMeta"]` is a `dbRoot` that is not connected, read `__dbDriver` + `__dbConnectionString` and connect, then apply `__showDefaultDatabaseOnly`, `__dbTargetMappingFileName`, `__dbLanguageMappingFileName`, `__userMetaDataFileName`, `__dbTarget`, `__language`, `__domainOverride`. This is how saved input reconnects the metadata.

## Log
`ILog`: `Write(string)`, `Write(format,args)`, `Write(Exception)`. `ZeusSimpleLog` keeps `LogItems`, `Exceptions`, raises `LogEntryAdded`. CLI uses `ZeusCmd/Log.cs` (see [zeuscmd.md](zeuscmd.md)).

## Related pages
- [execution-flow.md](execution-flow.md)
- [output-and-file-saving.md](output-and-file-saving.md)
- [gui-controls.md](gui-controls.md)
- [engine-interface.md](engine-interface.md)
- [scriban-dotnet10-mapping.md](scriban-dotnet10-mapping.md)
- [INDEX.md](INDEX.md)
