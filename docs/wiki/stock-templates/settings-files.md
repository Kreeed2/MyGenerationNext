---
title: Settings Files
summary: Contents and roles of the XML settings in _MyGeneration13_2025/Settings, including the SQL-to-C# and SQL-to-SqlClient type maps and their gaps.
keywords: [settings, DefaultSettings.xml, Languages.xml, DbTargets.xml, UserMetaData.xml, ZeusConfig.xml, type mapping, datetime2]
source_paths: [MyGenerationTemplates/_MyGeneration13_2025/Settings]
---

# Settings files

Folder `MyGenerationTemplates/_MyGeneration13_2025/Settings/`.

## DefaultSettings.xml
`<DefaultSettings Version="1.3.0.3" FirstTime="false">` with `<Setting Name=... value=...>` entries: FontFamily, DbDriver (`SQL`), ConnectionString (OLE DB `SQLOLEDB.1`), LanguageMappingFile, Language (`C#`), DbTargetMappingFile, DbTarget (`SqlClient`), UserMetaDataFileName, EnableLineNumbering, EnableClipboard, CodePage (-1), Tabs (4), ScriptTimeout (-1), DefaultTemplateDirectory, DefaultOutputDirectory, proxy settings (UseProxyServer, ProxyServerUri, ProxyAuth*), window geometry, CheckForNewBuild, CompactMemoryOnStartup, DomainOverride. Also a per-driver key (`<Setting Name="SQL" value=conn>`), `<SavedSettings name driver connstr dbtargetpath languagepath usermetapath language dbtarget>` (saved connection profiles) and `<RecentFile>` entries. Values are environment-specific (internal servers/paths); do not copy them. Port: reduce to config for connection string, output dir, namespace, DbTarget=SqlClient, Language=C#.

## DbTargets.xml
`<DbTargets><DbTarget From="SQL" To="SqlClient"><Type From="bigint" To="SqlDbType.BigInt"/>...` about 891 Type entries. Targets for SQL: SqlClient, OleDb, AllDbClient, ADODB, DbType (e.g. `DbType.Int64`); other source DBs: SQLCE, ACCESS, ORACLE, FIREBIRD, INTERBASE, DB2, VISTADB, SQLITE, ADVANTAGE, POSTGRESQL, etc.

SQL -> SqlClient (`Column.DbTargetType`) covers: bigint, binary, bit, char, datetime, decimal, float, image, int, money, nchar, ntext, numeric (=Decimal), nvarchar, real, smalldatetime, smallint, smallmoney, text, timestamp, tinyint, uniqueidentifier, varbinary, varchar, xml (=Xml), sql_variant (=Variant); each prefixed `SqlDbType.`.

## Languages.xml
`<Languages><Language From="SQL" To="C#"><Type From to/>...` about 80 KB. SQL targets: `C#`, `C# System Types`, `VB.NET`, `VB.NET System Types`, `VB60`. Other From values: SQLCE, ACCESS, ORACLE, POSTGRESQL(8), FIREBIRD, DB2, VISTADB(3X), SQLITE, ADVANTAGE, MYSQL2, PERVASIVE, ISERIES, SYBASEASA, XSD3B.

SQL -> C# (`Column.LanguageType`):

| SQL | C# |
|---|---|
| bigint | long |
| binary | object |
| bit | bool |
| char, nchar, ntext, nvarchar, varchar, text, xml | string |
| datetime, smalldatetime | DateTime |
| decimal, money, numeric, smallmoney | decimal |
| float | double |
| image, timestamp, varbinary | byte[] |
| int | int |
| real | float |
| smallint | short |
| tinyint | byte |
| uniqueidentifier | Guid |
| sql_variant | object |

"C# System Types" uses `System.Int64` etc.

**Gap**: no `date`, `time`, `datetime2`, `datetimeoffset`, `geography`, `hierarchyid`, `rowversion` (grep counts for datetime2/datetimeoffset/geography are 0). The port must add them (e.g. date -> DateOnly/DateTime, time -> TimeSpan/TimeOnly, datetime2 -> DateTime, datetimeoffset -> DateTimeOffset, rowversion -> byte[]) - these are port decisions, not legacy facts.

## UserMetaData.xml
`<MyMeta><Global><Databases><Database p="..."><Table><Properties/></Table><Column><Properties/></Column>...` Stores user key/value properties and alias overrides (written by `MyMeta.SaveUserMetaData`; the dOOdads `SEQ:`/`GEN:` keys live here). Optional in the port.

## ZeusConfig.xml (UTF-16)
WebUpdate url, ProjectRoot, TemplateRoot (`%ZEUSHOME%\Projects|Templates`), 3 IntrinsicObject entries: `ui` (Zeus.dll `Zeus.UserInterface.GuiController`), `MyMeta` (MyMeta.dll `MyMeta.dbRoot`), `DnpUtils` (Dnp.Utils.dll).

## Others
- `MyGeneration.xml`: placeholder options and Scintilla include of `ScintillaNET.xml` (editor syntax; irrelevant). `dock.config`: UI layout (not read). `changelog.txt` (last 2007.12.16, 1.3.0.3), `todo.txt`.
- Non-stock company files in the parent folder: `BLGeneratorUISettings.xml` (TFS URL, mandatory-data-transfer table lists), `BLGeneratorUIMeta.xml` (2.7 MB; MetaData entries with ObjectName, Namespace, LOV, Generate, Type, EntityName...). Ignore.

## Related pages
- [overview.md](overview.md)
- [doodads-architecture.md](doodads-architecture.md)
- [scriban-port-notes.md](scriban-port-notes.md)
- [INDEX.md](INDEX.md)
