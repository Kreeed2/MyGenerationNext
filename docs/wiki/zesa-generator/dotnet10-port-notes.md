---
title: .NET 10 port notes
summary: Maps evidenced legacy generator responsibilities to a SQL Server-only C# console/TUI, Scriban templates, and JSON configuration.
keywords: [.NET 10, port, Scriban, JSON, SQL Server, architecture]
source_paths: [MyGenerationTemplates/BL.csgen, MyGenerationTemplates/BusinesslogicGeneratorUI/, MyGenerationTemplates/*.csgen, MyGenerationTemplates/BLGeneratorMetaData/, MyGenerationTemplates/XMLConfigMigration/, MyGenerationTemplates/BlGeneratorAutoUpdate/]
---

# .NET 10 port notes

This is a translation guide, not a claim that the old generated APIs are automatically compatible with .NET 10. The current generator is .NET Framework 3.5/WinForms, MyGeneration/Zeus-script based, uses MyMeta abstractions and legacy TFS APIs, and emits source/config/project files for a particular EHA.ZESA solution. A rewrite target is a .NET 10 console/TUI for **MS SQL Server only**, with **C# output**, **Scriban templates**, and **JSON configuration**.

## Replace the orchestration, preserve explicit behavior

| Legacy source responsibility | Port direction |
|---|---|
| `BL.csgen` launches UI and wires five values into Zeus inputs | Replace with explicit CLI/TUI command flow: connect/discover, load JSON selection/config, validate, render selected targets. Model the set of requested output groups, since not all standalone templates are included by the legacy group. |
| `BLGeneratorUI.FillDataSet()` reads MyMeta objects and SQL Server catalogs | Use SQL Server metadata queries/catalog APIs behind a typed schema model. Keep SQL Server-only assumptions; remove MyMeta provider abstraction if only SQL Server is supported. Preserve discovery filters (`f_sel%`, `TT_` types, namespace mapping) only if they are still intended policies. |
| `GenInfo` + UI dataset rows | Replace with typed JSON DTOs for object selection and overrides. Validate supplied object names/types against discovered metadata; separate discovered schema from user overrides. |
| `GeneratorHelper.ConvertName`, `BuildEntityName`, namespace/key logic | Port as isolated, unit-tested naming/policy functions before Scriban. These are legacy conventions, not safe to substitute with generic PascalCase. |
| `.csgen` files use C# script blocks, MyMeta and `output.save` | Convert each retained output group to Scriban `.sbn` with a typed, precomputed render model. Keep filesystem writing outside templates; templates should render strings, with path resolution, preservation, and diagnostics owned by C# services. |
| `TfsHelper` checks out/changes files and project entries | Make source-control integration optional or drop it. Use ordinary filesystem operations and explicit project-manifest generation; never require TFS just to render. |
| WinForms dialog and host settings XML | Replace with console arguments and an interactive TUI front-end over the same application service. Store user choices in versioned JSON; do not make UI event handlers the domain model. |
| XML migration utilities | Provide deliberate importers for old metadata/config as needed, with dry-run, backup, validation and a clear one-way migration. Do not copy their overwrite/delete behavior. |
| `BlGeneratorAutoUpdate` copies all DLLs and kills/restarts MyGeneration | Drop this deployment approach for a .NET 10 app. Use normal package/release deployment; the source updater does not perform version comparison. |

## Define the JSON contract narrowly

Separate at least these concepts:

1. **Connection/profile**: SQL Server connection selection or secure external credential reference; never put secrets in checked-in config.
2. **Generation selection**: object identity, type, `Generate`, namespace/entity override, LOV columns, date-range columns, DbReader type/operator overrides, integer-key info, transfer selection, and identifier column. These correspond to `MetaData.xsd` and `GenInfo`, but can be normalized and validated.
3. **Generator options**: target root, output groups, namespace maps, naming rules, whether legacy configs/project files are emitted, and TVP convention.

Use explicit JSON versioning/migration rather than DataSet `ReadXml(..., IgnoreSchema)`. The current `BLGeneratorSettings` XML (TFS and transfer include/exclude lists) is a different settings object from `BLGeneratorUIMeta.xml`; do not collapse them without deciding the semantics.

## Build a metadata/render model before template work

A sound pipeline is: SQL Server catalog discovery → normalized table/view/function/type models → merge validated JSON overrides → derive generation plans and dependencies → render selected Scriban templates → write files/report results. Do not have Scriban execute SQL or call filesystem/TFS APIs. Model `ITable`/`IView` column facts, function input and TVF output schemas, primary/foreign-key markers, and user overrides independently.

The legacy source has important behavioral distinctions to preserve or consciously retire:

- Table entity names commonly lose their first underscore prefix; function names do not (`BuildEntityName`). `ConvertName` title-cases underscore segments and handles `@p` specially.
- Functions are only selected from procedures whose names appear in a SQL query for `f_sel%`; TVF return columns are fetched for `TF` and `IF` objects in `SqlFunctionAnalyzer`.
- TVP catalog names are `TT_`-prefixed and validation compares column order and names, not only sets.
- Views begin with an empty namespace in UI defaults; tables need a recognized namespace. Preserve explicit namespace mappings instead of assuming all types work alike.
- Selected master-data transfer can imply generation of foreign-key-related objects, and settings can mark required/excluded objects. The code includes business-specific exceptions.
- `MemberUseOldDataSetLogic.xml` gates the ColDefs path; decide whether that compatibility branch is still needed.
- Several code templates preserve user edits in marked source regions. A Scriban-only regeneration strategy needs an explicit alternative (partial files, extension hooks, or three-way merge) before dropping that behavior.

## Output groups and migration scope

For C# output, distinguish generated implementation classes from ancillary artifacts. The legacy suite also creates XML configs and old MSBuild `.csproj` fragments/manifests; a C#-only promise does not itself settle whether JSON or modern project files replace these. Decide which target artifacts the .NET 10 application consumes. Retain the C# equivalents only where the new runtime still needs their purpose; do not reproduce obsolete remoting, WinForms user controls, .NET Framework project files, or old `DataSet` adapters by default.

Likely candidates to retain as explicit groups are schema/DAO, service/API boundary, business facade, TVP metadata/config, and optional master-data migration support. Legacy .NET Remoting has no drop-in .NET 10 equivalent; choose a contemporary transport only if a real requirement exists. The `ZESAComboBox`, `ZESAListViews`, and unit-test templates produce framework-specific artifacts and should be excluded from a console-only output unless separately requested.

## Tests and safety rails for the rewrite

Create fixture databases/catalog snapshots and JSON cases for: naming/namespace mapping, tables vs views vs functions, nullable/type mapping, TVF output/input shape, TVP order mismatch, reader overrides/ranges, master-data dependency expansion, and empty/no-op selections. Snapshot-test Scriban output and assert safe relative paths. Provide a dry-run manifest of files and writes; reject path traversal/collisions. For old config import, compare parsed output and preserve XML namespaces/comments only when consumers require them.

## Related pages

- [Overview](overview.md)
- [Generator pipeline](generator-pipeline.md)
- [Metadata XML schema](metadata-xml-schema.md)
- [Data-access template group](data-access-templates.md)
- [Configuration and utility templates](configuration-templates.md)
- [Table-valued parameters](table-valued-parameters.md)
- [Stammdatenuebernahme generation](stammdatenuebernahme.md)
- [Config migration tools](config-migration.md)
- [TFS and generator auto-update](tfs-and-autoupdate.md)
