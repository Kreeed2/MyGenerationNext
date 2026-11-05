---
title: Configuration and utility templates
summary: Describes the generated database configuration XML, reader/writer configuration, list-of-values files, and supporting project definitions.
keywords: [configuration, DbReader, DbWriter, XML, LOV, project files]
source_paths: [MyGenerationTemplates/ZESADatabaseConfig.csgen, MyGenerationTemplates/ZESADbReader.csgen, MyGenerationTemplates/ZESADbWriter.csgen, MyGenerationTemplates/ZESAListOfValues.csgen, MyGenerationTemplates/ZESATableValuedTypeConfig.csgen, MyGenerationTemplates/ZESADAODefsProject.csgen]
---

# Configuration and utility templates

These templates take the output root from `input["TargetPath"]`; most also take `input["Data"]`. They read selected table/view columns using MyMeta and use the persisted per-object `GenInfo` settings. They write XML/config or project files directly; formats should be treated as legacy consumer contracts, not assumed to be interchangeable with JSON. Project and reader/writer migrations are discussed in [config migration](config-migration.md).

## Per-object database config: `ZESADatabaseConfig`

For each selected object, `ZESADatabaseConfig.csgen` creates `{ObjectName}.config` in `TargetPath`. It builds configuration from table/view columns and includes selected function metadata (`FunctionGenInfoData`). The filename uses the database object name, rather than always the normalized entity name. It is the broad per-object config output consumed by the application framework; inspect its XML markup before mapping individual nodes because behavior varies with object type/column markers. It checks out the file and uses `output.save`.

## DbReader and DbWriter shared config

- `ZESADbReader.csgen` emits one root-level `DbReader.config`. It iterates selected objects, emits query definitions based on object columns plus `DbReaderDataTypeColumns` and `DbReaderOperatorColumns`, and observes LOV and `NORMTIMEFROM`/`NORMTIMETO` options. `GeneratorHelper.IsNumeric` also treats `datetime` as range-capable for Von/Bis parameters.
- `ZESADbWriter.csgen` emits root-level `DbWriter.config`, uses selected **tables** and their columns to produce writer definitions, and distinguishes write operations/config members based on schema keys and data types.
- `ZESAListOfValues.csgen` emits root-level `ListOfValues.config`; it uses objects marked `LOV` and their configured `LovColumns` for value-list retrieval.
- `ZESATableValuedTypeConfig.csgen` writes a `P_INSUPDDEL_{ENTITY}.config` per selected table, with column/type information used by write operations.

The exact config element structure and generated SQL are in the individual templates. Existing deployed configs and `XMLConfigMigration/DbConfigMigra/` provide historical examples, but don't infer missing schema semantics from filenames alone.

## `ZESADAODefsProject`

This standalone template emits `ZESAServerDAODefs.csproj` under `TargetPath`. It derives included object references from selected table/view data, producing an XML/MSBuild project definition. It does not appear in the `BL.csgen` include list, so it is an independently invoked template rather than a pipeline child.

## Operational details

Templates generally create the output directory, call TFS checkout before output where implemented, save via `output.save(filename, false)`, print the resulting directory, and call `MyMeta.SaveUserMetaData()`. The target code concatenates Windows separators and expects a source tree containing named legacy folders. A portable implementation should represent output paths as structured relative paths rather than copy these string concatenations blindly.

## Related pages

- [Generator pipeline](generator-pipeline.md)
- [Metadata XML schema](metadata-xml-schema.md)
- [Table-valued parameters](table-valued-parameters.md)
- [Config migration tools](config-migration.md)
- [.NET 10 port notes](dotnet10-port-notes.md)
