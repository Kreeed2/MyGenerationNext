---
title: EngineInterface (PluginInterfaces) contracts
summary: Lists the interfaces in the EngineInterface project that separate the Zeus engine from the IDE and plugins, and which are worth keeping.
keywords: [EngineInterface, PluginInterfaces, IZeusController, IZeusContext, IMyGenerationMDI, plugin, IZeusTemplate, IEditorManager]
source_paths:
  - mygeneration-legacy-code/mygeneration/EngineInterface/PluginInterfaces.csproj
  - mygeneration-legacy-code/mygeneration/EngineInterface/ZeusInterfaces
  - mygeneration-legacy-code/mygeneration/EngineInterface/ZeusConstants.cs
  - mygeneration-legacy-code/mygeneration/EngineInterface/IMyGenerationMDI.cs
  - mygeneration-legacy-code/mygeneration/EngineInterface/UserInterfaces
---

# EngineInterface

Assembly `PluginInterfaces` (WinForms, ScintillaNET, WeifenLuo docking deps). Contains only contracts; implementations live in Zeus / MyGeneration.

## Zeus contracts (namespace `Zeus`)
`ILog`, `ITypeSerializer`, `IZeusCodeParser`, `IZeusCodeSegment`(+Stub), `IZeusContext`, `IZeusContextProcessor`, `IZeusController`, `IZeusExecutionError`, `IZeusExecutionHelper` (+ `ShowGUIEventHandler`), `IZeusFunctionExecutioner`, `IZeusGuiControl`, `IZeusInput`, `IZeusIntrinsicObject`, `IZeusOutput`, `IZeusSavedInput`, `IZeusSavedTemplateInput`, `IZeusScriptingEngine`, `IZeusTemplate`(+Stub), and `ZeusConstants` (directive names, `SourceTypes`, etc.).

`IZeusController` is what the IDE exposes to plugins to run generation: `CollectTemplateInput`, `ExecuteTemplateAndCollectInput`, `ExecuteTemplate` (two overloads), `ExecuteProject`, `ExecuteProjectModule`, `ReleaseList` (`IAppRelease`).

## IDE contracts (namespace `MyGeneration`)
`IMyGenerationMDI`, `IMyGenContent`, `IMyGenDocument`, `IMyGenConsole : ILog`, `IMyGenError`/`IMyGenErrorList`, `IEditorManager`, `IContentManager`, `ISimplePluginManager`, `IScintillaEditControl`/`IScintillaNet`. `EngineInterface.UserInterfaces` holds empty UserControl stubs (`TemplateBrowser`, `TemplateDetails`, `MyMetaBrowser`, `MyMetaObjectProperties`).

## Rewrite guidance
- Keep conceptually: `IZeusContext`/`IZeusInput`/`IZeusOutput`/`ILog`, template model, saved input, controller operations.
- Discard: all `IMyGen*`, editor/docking/Scintilla, UserControl stubs, `IZeusFunctionExecutioner`, engine-plugin interfaces (single Scriban engine).

## Related pages
- [context-and-intrinsic-objects.md](context-and-intrinsic-objects.md)
- [script-engines.md](script-engines.md)
- [execution-flow.md](execution-flow.md)
- [scriban-dotnet10-mapping.md](scriban-dotnet10-mapping.md)
- [INDEX.md](INDEX.md)
