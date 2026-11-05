---
title: Stammdatenuebernahme generation
summary: Describes the generated release-change master-data transfer class and the UI logic that builds its dependency order.
keywords: [Stammdatenuebernahme, master data, release migration, foreign keys]
source_paths: [MyGenerationTemplates/ZESAStammdatenuebernahme.csgen, MyGenerationTemplates/BusinesslogicGeneratorUI/BLGeneratorUI.cs, MyGenerationTemplates/BusinesslogicGeneratorUI/StammdatenuebernahmeInfo.cs, MyGenerationTemplates/BusinesslogicGeneratorUI/GeneratorHelper.cs]
---

# Stammdatenübernahme (master-data transfer)

The German term **Stammdatenübernahme** means transfer of master/reference data, here in the context of a release change. It is an opt-in workflow in the selection UI and a separate standalone template (`ZESAStammdatenuebernahme.csgen`), not one of the files listed in the `BL.csgen` include block.

## Selection and dependency construction

`BLGeneratorUI` holds master-data flags in each `MetaData` row and returns a `StammdatenWithForeignKeys` list. Before building selected `GenInfo` records, `FillGenInfo()` traverses rows marked for generation when the “ignore master-data transfer” checkbox is not checked. If a selected row requests transfer, `GetForeignKeys()` finds related records and marks their corresponding rows `Generate=true` and `Stammdatenuebernahme=true`. For each such object, `StammdatenuebernahmeInfo` carries `ObjectName`, `Namespace`, `ForeignKeys`, `ColumnsForUniqueIndices`, and `AliasKennungColumn`.

`GetForeignKeys()` and `GetColumnsForUniqueIndices()` derive dependencies and unique-index columns from MyMeta table metadata. The UI also verifies that marked objects match registered master-data tables and prevents deselection of configured mandatory entities. `GeneratorHelper.DeserializeSettings()` provides mandatory/excluded entity lists; the code has fallback mandatory entries including `BA_ANWENDUNGSFALL`, `BA_DOMAENE`, `BA_FUNKTION`, `BA_FUNKTION_ANWENDUNGSFALL_DOMAENE`, and `BA_ONLINEHILFE` if no settings file exists. Some logic has explicit entity-name exceptions, so the relationship ordering is business policy, not purely a foreign-key topological sort.

## Generated artifact

The GUI passes both `Data` and `StammdatenWithForeignKeys` through `BL.csgen`'s GUI input. `ZESAStammdatenuebernahme.csgen` emits one `Datenabgleich.cs` at `TargetPath/Server/ZESAServer/Allgemein/`, in `EHA.ZESA.Server.Allgemein`. It incorporates generated per-table code and interacts with the existing DAO/manager classes. The implementation uses SQL connection/adapter operations, `DbWriter` transactions, and includes routines for matching and copying columns; its preserve markers (`Individuelle_Usings`, `Individuelle_Anpassungen`) allow hand-maintained content. The generated class is not a generic ETL engine.

The template exits without output when the list is empty. Generation requires the expected server source tree and framework types and invokes TFS checkout/save behavior when writing the file.

## Port considerations

Separate the actual data-transfer policy (dependencies, unique keys, alias identifier, mandatory/excluded objects) from file rendering. Preserve ordering, explicit exceptions, transaction/error behavior, and field-matching semantics only if they remain requirements. Do not copy the legacy hard-coded database/repository assumptions into a new console app. This page documents generator behavior, not a recommendation to run a production data migration.

## Related pages

- [Generator UI](generator-ui.md)
- [Metadata XML schema](metadata-xml-schema.md)
- [Generator pipeline](generator-pipeline.md)
- [Data-access template group](data-access-templates.md)
- [.NET 10 port notes](dotnet10-port-notes.md)
