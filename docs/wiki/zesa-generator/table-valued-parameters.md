---
title: Table-valued parameters
summary: Explains how the UI discovers SQL Server structured table types and how the TVP validation template compares them with table columns.
keywords: [SQL Server, TVP, structured type, table-valued parameter, validation]
source_paths: [MyGenerationTemplates/BusinesslogicGeneratorUI/BLGeneratorUI.cs, MyGenerationTemplates/ZESAPruefeTableValuedParameter.csgen, MyGenerationTemplates/ZESATableValuedTypeConfig.csgen, MyGenerationTemplates/BL.csgen]
---

# Table-valued parameters

A SQL Server table-valued parameter (TVP) is represented by a user-defined table type. The legacy code expects type names following `TT_{table}` for its discovery convention; it does not claim that every possible user-defined table type is a table's TVP.

## Discovery and handoff

`BLGeneratorUI.FillDataSet()` opens the ZESA development SQL connection and calls `GetSchema("StructuredTypeMembers")`. It selects `TYPE_NAME` values whose uppercase name starts with `TT_`, strips that prefix, deduplicates, and retains names for which `GeneratorHelper.FindNamespace(tvp)` returns a namespace. These values populate `ExistingTableValuedParameterForTables`. `FillGenInfo()` notes whether any generated selected object matches this list. `BL.csgen` passes that list into `input["ExistingTableValuedParameterForTables"]`.

The TVP validation template's GUI also passes the same property. The template checks for null and exits if no such collection is available; it creates output only when an existing TVP needs checking according to its selection logic. This is separate from TVP config generation.

## Validation template: `ZESAPruefeTableValuedParameter`

The template emits a generated validator class under `TargetPath/Server/ZESAServer/Allgemein/` (class and `.cs` filename are formed in the template) and reads SQL table-type column metadata through its own SQL access. It compares the ordered list of table-type columns with the corresponding MyMeta table columns; source explicitly checks both count and each ordinal/name (case-insensitive). A difference is reported with the type, ordinal, and conflicting names. It is therefore an order-sensitive shape check, not just a set-membership test.

`ZESATableValuedTypeConfig.csgen` is a related but distinct config producer: it emits per-table `P_INSUPDDEL_{ENTITY}.config` based on selected table columns. Do not conflate generation of that writer config with discovery/validation of an already-existing database TVP.

## SQL-function table metadata

Separately, `SqlFunctionAnalyzer.GetTvfReturnColumns()` queries `sys.objects`, `sys.columns`, and `sys.types` for table-valued functions (`TF`, `IF`) and returns column name, database type, and nullability in `TvfColumnInfo`. `BLGeneratorUI.GetInputParameterForFunction()` supplies function input parameter details. These records feed DAO templates; a function result shape is not the same thing as a user-defined TVP schema.

## Port implications

Keep SQL Server catalog discovery and ordinal type validation if the rewritten generator still supports TVPs. Make the `TT_` naming convention explicit/configurable and distinguish database catalog facts from selection policy. Use SqlClient structured parameter semantics in the runtime integration only if the generated application needs it; this repository's templates primarily generate configuration/validation and C# wrappers, not a standalone SQL deployment script for creating the types.

## Related pages

- [Generator pipeline](generator-pipeline.md)
- [Generator UI](generator-ui.md)
- [Configuration and utility templates](configuration-templates.md)
- [Metadata XML schema](metadata-xml-schema.md)
- [.NET 10 port notes](dotnet10-port-notes.md)
