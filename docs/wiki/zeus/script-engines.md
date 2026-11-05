---
title: Script engines and languages (.Net Script, Microsoft Script)
summary: Documents the pluggable scripting engines (CodeDom C#/VB.NET compilation and COM MSScriptControl VBScript/JScript), their custom tags and how templates are compiled/run.
keywords: [DotNetScriptEngine, MicrosoftScriptEngine, CodeDom, CSharpCodeProvider, MSScriptControl, COM, VBScript, JScript, ZeusFactory, IZeusScriptingEngine]
source_paths:
  - mygeneration-legacy-code/mygeneration/Zeus/ZeusFactory.cs
  - mygeneration-legacy-code/mygeneration/Zeus/DotNetScriptEngine/DotNetScriptEngine.cs
  - mygeneration-legacy-code/mygeneration/Zeus/DotNetScriptEngine/DotNetScriptCodeParser.cs
  - mygeneration-legacy-code/mygeneration/Zeus/DotNetScriptEngine/DotNetScriptExecutioner.cs
  - mygeneration-legacy-code/mygeneration/Zeus/DotNetScriptEngine/_DotNetScriptTemplate.cs
  - mygeneration-legacy-code/mygeneration/Zeus/DotNetScriptEngine/_DotNetScriptGui.cs
  - mygeneration-legacy-code/mygeneration/Zeus/MicrosoftScriptEngine/MicrosoftScriptExecutioner.cs
  - mygeneration-legacy-code/mygeneration/Zeus/MicrosoftScriptEngine/MicrosoftScriptCodeParser.cs
  - mygeneration-legacy-code/mygeneration/EngineInterface/ZeusInterfaces/IZeusScriptingEngine.cs
---

# Script engines

## Registry: ZeusFactory
`ZeusFactory.LoadEngines` registers:
- `.Net Script` -> `DotNetScript.DotNetScriptEngine` (C#, VB.Net)
- `Microsoft Script` -> `MicrosoftScript.MicrosoftScriptEngine` (JScript, VBScript)
- extra engines from `ZeusConfig.ScriptingEngines` (loaded by `DynamicAssemblyTools`).

`ZeusFactory` also exposes `Preprocessors`, `Serializers`, `IntrinsicObjectsArray`, `AvailableLanguages`, `TemplateModes`, `TemplateTypes`, `EngineNames`, `GetEngine(name)`.
Dead code (excluded from `Zeus.csproj`): root `IZeusExecutioner.cs`, `IZeusScriptingEngine.cs`, `IZeusCodeParser.cs`, `IZeusFunctionExecutioner.cs`, `ZeusScriptingEngineFactory.cs`, folder `Zeus/DotNetScript/`.

## Engine contracts (EngineInterface/ZeusInterfaces)
- `IZeusScriptingEngine`: `EngineName`, `SupportedLanguages`, `CodeParser`, `ExecutionHelper`, `IsSupportedLanguage`, `GetNewTemplateText`, `GetNewGuiText`.
- `IZeusCodeParser`: `EscapeLiteral`, `BuildOutputCommand`, `ParseCustomTag`, `GetCustomHeaderCode`, `GetCustomFooterCode`.
- `IZeusExecutionHelper`: `EngineExecuteGuiCode`, `EngineExecuteCode`, `SetShowGuiHandler`, `Timeout`, `Cleanup`, `Errors/HasErrors/ClearErrors`, `ExecuteFunction`.

## .Net Script (DotNetScriptEngine)
Templates are C#/VB classes compiled at run time.
- Author writes a class deriving `DotNetScriptTemplate` with `public override void Render()` (body) or `DotNetScriptGui` with `Setup()` (GUI); constructor takes `ZeusContext` (`: base(context)`). Default text from `GetNewTemplateText`/`GetNewGuiText` (`GeneratedTemplate`/`GeneratedGui`).
- Base classes: `_DotNetScriptTemplate` (fields `context`, `input`, `output`, `objects`; abstract `Render`), `_DotNetScriptGui` (`context`, `input`, `objects`; abstract `Setup`). `DotNetScriptEngine.BuildBodyClass/BuildGuiClass` (appended as footer) generate abstract `Zeus.DotNetScript.DotNetScriptTemplate`/`DotNetScriptGui` with one protected field per enabled intrinsic object (type = ClassPath, name = VariableName, assigned from `context.Objects[name]`) — that is how `MyMeta`, `ui`, `DnpUtils` appear as variables.
- Header (`GetCustomHeaderCode`): using `System`, `System.Collections`, `Zeus`, `Zeus.Data`, `Zeus.DotNetScript` (+ `Zeus.UserInterface` for GUI), `ZeusConfig.TemplateNamespaces`, intrinsic-object namespaces, plus `USE` items.
- Custom tags (`DotNetScriptCodeParser.ParseCustomTag`):
  - `<%#FILE path%>` include file text (relative to template path via `DotNetScriptEngine.MakeAbsolute`)
  - `<%#REFERENCE a.dll,b.dll%>` / `REF` -> ExtraData `DLLREF`
  - `<%#NAMESPACE ns1,ns2%>` / `NS` -> ExtraData `USE`
  - `<%#DEBUG%>` compile to file with debug info
  - `<%#COMPILERVERSION v3.5%>` / `VERSION`
- Output commands: `output.write(...)` / `output.writeln(...)`; `EscapeLiteral` escapes `\\ \" \r \n \t` (C#) or `""` and `" & vbcrlf & "` (VB).
- `DotNetScriptExecutioner`:
  - `EngineExecuteGuiCode`/`EngineExecuteCode` -> `LoadAssembly` -> `InstantiateClass` (first non-abstract subclass of base; `constructors[0]` invoked with the context) -> `Setup()`/`Render()`. GUI: `OnShowGUI(context.Gui)` fires if `ShowGui || ForceDisplay`.
  - `CreateAssembly` uses CodeDom `CSharpCodeProvider`/`VBCodeProvider` with `CompilerVersion`, options `/target:library /optimize`, in memory by default.
  - References: mscorlib, System, System.EnterpriseServices, Zeus.dll, PluginInterfaces.dll, `ZeusConfig.TemplateReferences`, intrinsic `DllReference`s, `DLLREF` items.
  - DEBUG or Compiled writes `$ZEUS$DEBUG$<guid>.cs/.vb/.dll` into the app folder; `CleanupOldDebugFiles` at engine construction.
  - Errors -> `DotNetScriptError` (from `CompilerError`). Compiler versions v2.0/v3.0/v3.5(/v4.0 via `ZeusConfig`, default v4.0).
  - Compiled templates cache the assembly in `ZeusCodeSegment.CachedAssembly` (`BASE64:` text).

## Microsoft Script (MicrosoftScriptEngine)
COM interop with `MSScriptControl.ScriptControlClass` (COMReference in `Zeus.csproj`).
- `MicrosoftScriptExecutioner`: sets `Language`, `Timeout` (seconds x1000, -1 = none), `AllowUI=true`, `UseSafeSubset=false`; `AddObject` for `input`, `context`, `output` (body only), and each `context.Objects` key (`ui`, `MyMeta`, ...).
- GUI entry function: `setup`. Body entry: generated stub `zQh__rw_ZEUS` (VBScript Sub / JScript function) appended after the code. GUI callbacks: `ScriptControl.Run(functionName, params)` (`ExecuteFunction`).
- COMException -> `MicrosoftScriptError`. A nested control stack is used when `context.ExecutionDepth > 1`.
- `MicrosoftScriptCodeParser` tags: `INCLUDE_FILE`/`FILE` (not implemented), `INCLUDE_SCRIPT`/`SCRIPT` (inline file), `INCLUDE_TEMPLATE`/`TEMPLATE` (inline another template body if UniqueID and language differ). Header/footer empty; `GetNew*Text` return empty.
- `Configuration/DefaultPreprocessor.zeus` is a JScript-GUI/Pure-body template.

## Related pages
- [template-parsing.md](template-parsing.md)
- [execution-flow.md](execution-flow.md)
- [context-and-intrinsic-objects.md](context-and-intrinsic-objects.md)
- [scriban-dotnet10-mapping.md](scriban-dotnet10-mapping.md)
- [INDEX.md](INDEX.md)
