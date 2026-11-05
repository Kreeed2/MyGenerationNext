---
title: Legacy config migration tools
summary: Explains the three source-backed utilities for metadata XML, DbReader/DbWriter config, and project-file migration.
keywords: [migration, XML, DbReader, DbWriter, csproj, legacy]
source_paths: [MyGenerationTemplates/XMLConfigMigration/BlGeneratorMetaMigra/BlGeneratorMetaMigra/Program.cs, MyGenerationTemplates/XMLConfigMigration/DbConfigMigra/DbConfigMiga/Program.cs, MyGenerationTemplates/XMLConfigMigration/BlGeneratorProjectFiles/BlGeneratorProjectFiles/Program.cs, MyGenerationTemplates/XMLConfigMigration/DbConfigMigra/]
---

# Legacy config migration tools

`MyGenerationTemplates/XMLConfigMigration/` holds small, one-off console utilities, not a general migration framework. Their filenames and `Program` methods disclose narrow transformations. Read and test these on copies of legacy inputs; the code can delete/recreate output directories and overwrite files.

## `BlGeneratorMetaMigra`

`BlGeneratorMetaMigra/Program.cs` looks for `BLGeneratorUIMetaQuelle.xml` and `BLGeneratorUIMetaZiel.xml` in the current working directory. It scans source metadata rows for objects whose `ObjectName` begins `V_`, obtains non-empty namespace values, finds matching view rows in the target metadata, and fills only a blank target namespace for an equal object name. It saves `_NEU_BLGeneratorUIMeta.xml`. Before processing, `ErstelleEinzelConfigs()` creates or empties a `Configurations` subdirectory, though this particular transformation writes the metadata output at the working-directory root. The use of `Descendants("MetaData")` and unqualified `Elements()` means input XML shape/namespaces must match what the code expects; do not assume arbitrary namespace-qualified files are supported.

## `DbConfigMigra`

`DbConfigMiga/Program.cs` takes `DbReader.config` and `DbWriter.config` in the current directory and writes split `.config` files into `DbReader_Configurations/` (despite the directory name, it handles writer entries too). It extracts `Query` nodes from the reader config, `DataAdapter` and `SqlCommand` nodes from the writer config, guesses output filenames from preceding comments or an `id`/`ID` attribute, then wraps each resulting file in `<configuration>` and reconstructs nested `<DbReader><DataAccess><Queries>` and `<DbWriter><DataAdapters>/<SqlCommands>` groups when present. Its comment-based filename logic strips suffix portions and `_List` under specific conditions. `DbConfigMigra/_Configs/` contains examples (`DbReader.config`, `DbWriter.config`, and sample files), useful as historical fixtures.

The converter empties the destination directory at startup and uses string/XML transformations; check the resulting XML and filenames before replacing runtime configs. It is not a bidirectional converter.

## `BlGeneratorProjectFiles`

`BlGeneratorProjectFiles/Program.cs` reads `ZESAConfiguration_pre.csproj`, creates an XML `<None Include="Configuration\\Allgemein\\Database\\TA_NNEAT_NNENTGELT_Sicherung.config" />` line, and attempts to insert it alphabetically among existing `<None Include...>` lines. It writes `ZESAConfiguration_post.csproj`. The hard-coded one-file insertion is a historical example, not a generic regeneration/migration command. Its setup also empties a `Configurations` directory but the shown transformation writes the project file in the current directory.

## Relation to the current metadata schema

`BLGeneratorUIMeta.xml` is a typed DataSet XML document described by `BusinesslogicGeneratorUI/MetaData.xsd`; `BlGeneratorMetaMigra` changes namespaces in selected view rows and outputs an alternate file, while the UI reads/saves the canonical metadata file via `ReadXml`/`WriteXml`. `DbConfigMigra` handles runtime reader/writer XML configs, a different format. See [metadata XML schema](metadata-xml-schema.md).

## Related pages

- [Metadata XML schema](metadata-xml-schema.md)
- [Configuration and utility templates](configuration-templates.md)
- [TFS and auto-update](tfs-and-autoupdate.md)
- [.NET 10 port notes](dotnet10-port-notes.md)
