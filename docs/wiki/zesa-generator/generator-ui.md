---
title: Generator UI
summary: Documents the Windows Forms object-selection workflow and the metadata records handed from the UI to templates.
keywords: [BLGeneratorUI, WinForms, database metadata, GenInfo, selection]
source_paths: [MyGenerationTemplates/BusinesslogicGeneratorUI/BLGeneratorUI.cs, MyGenerationTemplates/BusinesslogicGeneratorUI/BusinesslogicGeneratorUI.cs, MyGenerationTemplates/BusinesslogicGeneratorUI/GenInfo.cs, MyGenerationTemplates/BusinesslogicGeneratorUI/GeneratorHelper.cs, MyGenerationTemplates/BusinesslogicGeneratorUI/SqlFunctionAnalyzer.cs]
---

# Generator UI

`BLGeneratorUI` is a Windows Forms dialog hosted by the MyGeneration template GUI. The C# project compiles it as a .NET Framework 3.5 library (`ZESABLGeneratorUI.dll`); the UI is not a stand-alone app. `BusinesslogicGeneratorUI.cs` contains WinForms event handlers such as `ViewGeneratorUI_Load` and selection actions, while `BLGeneratorUI.cs` owns metadata acquisition, merge/persistence, and selected-record preparation.

## Database discovery

The constructor receives the MyMeta `dbRoot` from `BL.csgen`. During setup, `FillDataSet()` uses `mMetaData.DefaultDatabase.Tables`, `.Views`, and `.Procedures`. Tables with namespaces resolvable by `GeneratorHelper.FindNamespace` become candidate entries. Views are listed too; `f_sel%` function procedures found in the direct SQL query are the subset exposed as `Function` objects. For tables/views the code derives initial entity names with `BuildEntityName`, obtains primary/integer key information from columns and relationships, and initializes selection flags. It separately reads SQL Server structured types for TVP detection.

Database metadata is authoritative for schema shape. Saved UI metadata is then merged by object name to retain choices that the database catalog cannot tell the generator, such as namespace, LOV columns, date-range options, per-column reader overrides, transfer flags, and alias identifier settings. The UI comments describe earlier PowerDesigner input, but the current source path and discovery are database-centric; do not assume a live PDM import pipeline without finding one in the implementation.

## Data handed to the templates

Public properties on `BLGeneratorUI` expose:

- `Data`: an `ArrayList` of `GenInfo` for selected objects.
- `TargetPath`: configured output source root.
- `FunctionGenInfoData`: functions' return/input parameter details.
- `StammdatenWithForeignKeys`: dependency information for transfer generation.
- `ExistingTableValuedParameterForTables`: database-discovered TVP table names.

`GenInfo` is the primary per-object record; fields include `ObjectName`, `Namespace`, `ObjectType`, `EntityName`, `Lov`, `LovColumns`, `Generate`, `NormTimeFrom`/columns, `NormTimeTo`/columns, `Stammdatenuebernahme`, `AliasKennungColumn`, reader type/operator override strings, integer primary-key columns, and `UsesOldDataSetLogic`. `UsesOldDataSetLogic` checks the object against `GeneratorHelper.GetMemberWhichUseOldDataSetLogic()`.

For SQL table-valued functions, `FunctionGenInfo` holds `FunctionName`, return `TvfColumnInfo` values and input parameter values. `SqlFunctionAnalyzer` reads return columns from SQL Server catalog views; function input parameters are built by `GetInputParameterForFunction()` in the UI.

## Naming and helper responsibilities

`GeneratorHelper` implements `ConvertName`, `BuildEntityName`, numeric/range classification, column padding/maximum lengths, namespace discovery, typed settings serialization, project file insertion, per-column override dictionaries, generated-object list handling, and reading the old-DataSet exception XML. See its method bodies before porting: conversion is a legacy naming policy with case and underscore behavior, not standard PascalCase.

## Persistence/configuration caveat

`BLGeneratorUI.FILENAME_METADATA` is hard-coded to a repository-specific absolute path for `BLGeneratorMetaData/BLGeneratorUIMeta.xml`. The dialog loads it using the typed DataSet's `ReadXml(..., IgnoreSchema)` and saves using `WriteXml(..., IgnoreSchema)`. The UI output directory is separately saved in `PathInfo`. More schema detail is in [metadata XML schema](metadata-xml-schema.md).

## Related pages

- [Generator pipeline](generator-pipeline.md)
- [Metadata XML schema](metadata-xml-schema.md)
- [Table-valued parameters](table-valued-parameters.md)
- [Stammdatenuebernahme generation](stammdatenuebernahme.md)
- [TFS and auto-update](tfs-and-autoupdate.md)
