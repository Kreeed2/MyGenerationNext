---
title: Generator pipeline
summary: Follows the legacy generation path from MyGeneration UI selection through XML persistence and template output.
keywords: [pipeline, MyGeneration, GenInfo, MyMeta, generated files]
source_paths: [MyGenerationTemplates/BL.csgen, MyGenerationTemplates/BusinesslogicGeneratorUI/BLGeneratorUI.cs, MyGenerationTemplates/BusinesslogicGeneratorUI/GenInfo.cs, MyGenerationTemplates/BusinesslogicGeneratorUI/GeneratorHelper.cs]
---

# Generator pipeline

## 1. MyGeneration launches the group UI

`BL.csgen` is a MyGeneration `TYPE Group` template. Its `GeneratedGui.Setup()` creates `BLGeneratorUI(MyMeta)`, calls `ShowDialog()`, then assigns five outputs into Zeus `input`: `Data`, `TargetPath`, `StammdatenWithForeignKeys`, `ExistingTableValuedParameterForTables`, and `FunctionGenInfoData`. The group includes `ZESADatabaseConfig`, `ZESATableValuedTypeConfig`, `ZESAListOfValues`, `ZESAColDefs`, `ZESAParameter`, remoting interface/service, manager, verified DAO, server DAO, and TVP validation templates.

## 2. UI discovers database objects and overlays saved choices

`BLGeneratorUI.FillDataSet()` reads the default database through MyMeta (`ITable`, `IView`, and `IProcedure` collections). It also connects to SQL Server using `SqlConnection`: `GetSchema("StructuredTypeMembers")` finds names beginning `TT_`, and a query over `sys.objects` identifies existing `f_sel%` functions. Tables with a namespace found by `GeneratorHelper.FindNamespace` and all discovered views are represented as rows; procedures become rows only when present in the discovered `f_sel%` set. Defaults include `Generate=true`, while optional fields start false/empty. The saved rows are merged back by `ObjectName` in `MergeData()` (the code also manages values absent in older XML).

The selected rows are translated by `FillGenInfo()` into `GenInfo`; its properties preserve object name/type, namespace, entity name, LOV/range flags, reader overrides, integer-key information, and migration selection. When type is `Function`, it separately fills `FunctionGenInfo` with return columns from `SqlFunctionAnalyzer.GetTvfReturnColumns()` and inputs from `BLGeneratorUI.GetInputParameterForFunction()`. For selected data-transfer tables, it constructs `StammdatenuebernahmeInfo` including foreign keys and unique-index columns. TVP matches set the condition consumed by the validation template.

## 3. Persisted XML is UI state, not the output artifact manifest

`LoadSavedLocalMetadata()` reads `BLGeneratorUIMeta.xml` into the typed dataset using `ReadXml(..., IgnoreSchema)`. `SaveLocalMetadata()` serializes the live dataset with `WriteXml(..., IgnoreSchema)`, recording the output path and per-object UI choices. These values are defaults for later invocations, not a complete list of generated files. The current source hard-codes an absolute metadata path in `BLGeneratorUI.FILENAME_METADATA`; this deployment constraint should not be mistaken for a portable lookup rule.

## 4. Each child template independently queries MyMeta and renders

Templates receive shared input values but commonly query `MyMeta.DefaultDatabase.Tables[info.ObjectName]` or `.Views[...]` again for columns. A handful of templates also consume the function metadata or TVP/master-data side lists. `GeneratorHelper` centralizes naming, type override, settings, and project-file helper operations. A template iterates `Data`, skips unsupported object types as coded, uses `output.setPreserveSource(filename, "/***", "***/")` for hand-maintained regions where present, and saves each artifact through `output.save(filename, false)`.

The output root is `TargetPath`, but most code appends fixed legacy project subpaths such as `Server\ZESAServer\DAO\...` or `Communication\RemotingServiceInterface\DAO\...`. Other outputs are root-level config files. See the [template group pages](data-access-templates.md), [configuration templates](configuration-templates.md), and [remoting/client templates](remoting-client-templates.md) for exact names.

## 5. TFS/project and completion behavior

Several templates call `TfsHelper.Instance.CheckOut()` before saving and update an existing `.csproj` via `GeneratorHelper.AddFileToProject()` when needed. This is separate from the generation core and assumes a configured TFS workspace. Templates print completion/log output and often call `MyMeta.SaveUserMetaData()`; preserve-source behavior is also runtime-dependent. These actions are detailed in [TFS and auto-update](tfs-and-autoupdate.md).

## Important boundaries

The source shows C# script templates using MyGeneration/Zeus and MyMeta APIs; it does not define a standalone deterministic “all output files” graph. In particular, standalone templates outside `BL.csgen` are not run merely because they share the directory. A port should model a deliberate job plan rather than assuming these are all implicitly invoked.

## Related pages

- [Overview](overview.md)
- [Generator UI](generator-ui.md)
- [Metadata XML schema](metadata-xml-schema.md)
- [Data-access templates](data-access-templates.md)
- [Configuration and utility templates](configuration-templates.md)
- [Remoting, client, and project templates](remoting-client-templates.md)
- [Table-valued parameters](table-valued-parameters.md)
