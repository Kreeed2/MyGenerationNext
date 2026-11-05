---
title: ZESA generator overview
summary: Describes the legacy MyGeneration template suite, its UI, metadata model, and generated application layers.
keywords: [ZESA, MyGeneration, C-sharp, templates, architecture]
source_paths: [MyGenerationTemplates/BL.csgen, MyGenerationTemplates/BusinesslogicGeneratorUI/, MyGenerationTemplates/BLGeneratorMetaData/]
---

# Overview

The ZESA generator is a MyGeneration/Zeus template group, not a standalone generator executable. `MyGenerationTemplates/BL.csgen` declares the group title **ZESA Business Logic Generator**, launches `EHA.ZESA.Util.GeneratorUI.BLGeneratorUI`, places the returned values in the template input dictionary, and includes eleven child templates. The child templates contain the actual C# and XML/project-file generation logic. Their source is `.csgen` (MyGeneration .NET Script markup with embedded C#), even when their generated artifact is C#.

The UI is a Windows Forms dialog compiled as `ZESABLGeneratorUI.dll` by `BusinesslogicGeneratorUI.csproj`; the project targets .NET Framework 3.5 and references MyMeta and Team Foundation Version Control libraries. It reads schema metadata from the connected database, combines that with editable persisted settings, and returns `GenInfo` records and specialized companion data to the templates (`BLGeneratorUI.Data`, `FunctionGenInfoData`, `StammdatenWithForeignKeys`, and `ExistingTableValuedParameterForTables`).

## Main responsibilities

- **Selection and metadata:** tables, views, and selected SQL functions are presented with generation flags, namespace and per-object options. See [generator UI](generator-ui.md) and [metadata schema](metadata-xml-schema.md).
- **Data access:** server DAO, verified/client DAO, manager, and helper/config artifacts are produced by independent templates. See [data-access templates](data-access-templates.md) and [configuration templates](configuration-templates.md).
- **Client/remoting/project integration:** serializable remoting contracts/services, client list controls, unit tests, and legacy `.csproj` edits/manifests are generated separately. See [remoting/client templates](remoting-client-templates.md).
- **Special workflows:** TVP validation and master-data transfer have SQL Server-specific and business-specific behavior, documented in [TVPs](table-valued-parameters.md) and [Stammdatenuebernahme](stammdatenuebernahme.md).
- **Legacy integration:** selected files are checked out or added to TFS projects, and companion tooling migrates config formats and copies UI DLLs. See [TFS and auto-update](tfs-and-autoupdate.md) and [config migration](config-migration.md).

## Terms and naming

- **DAO** (*Data Access Object*) is the generated database-facing class layer.
- **DbReader/DbWriter** are the framework's read/write data-access components and their XML configurations.
- **LOV** (*List of Values*, German UI: *Werteliste*) marks objects from which choice lists are populated.
- **Stammdatenübernahme** means transfer/migration of master/reference data, here during a release change.
- **Kennung** is an identifying/code column; the UI calls its configurable field the alias identifier column.
- **Von/Bis** means inclusive “from/to” range parameters. `NORMTIMEFROM`/`NORMTIMETO` capture date-time normalization options.
- **TVP** means SQL Server table-valued parameter. `TT_`-prefixed SQL structured types are used by the discovery code.
- **Entity name** is often the table name with its first underscore-delimited prefix removed. Functions are handled differently by `GeneratorHelper.BuildEntityName`.

Do not treat every `.csgen` file as included in `BL.csgen`: its `TEMPLATE_INCLUDE` list is explicit. `ZESADbReader`, `ZESADbWriter`, `ZESAComboBox`, `ZESAListViews`, `ZESAStammdatenuebernahme`, project templates, and test templates exist as separate templates, while the group includes only the eleven listed children.

## Source map

`MyGenerationTemplates/BusinesslogicGeneratorUI/` contains the UI, data model, helper algorithms, SQL function analyzer, TFS integration, and project definition. `BLGeneratorMetaData/BLGeneratorUIMeta.xml` is a checked-in object-specific settings dataset; the schema is `BusinesslogicGeneratorUI/MetaData.xsd`. `BusinesslogicGeneratorUI/GeneratorDll/Version*` stores versioned deployment DLL snapshots, while `_MyGeneration13_2025/` is a wider MyGeneration installation snapshot.

## Related pages

- [Generator pipeline](generator-pipeline.md)
- [Generator UI](generator-ui.md)
- [Metadata XML schema](metadata-xml-schema.md)
- [Data-access templates](data-access-templates.md)
- [.NET 10 port notes](dotnet10-port-notes.md)
