---
title: Language and DbTarget mappings
summary: How Languages.xml and DbTargets.xml map SQL Server type names to C#/other languages and ADO.NET types, how dbRoot selects them, and which modern SQL types are missing.
keywords: [Languages.xml, DbTargets.xml, LanguageType, DbTargetType, SqlDbType, C#, type mapping, DomainOverride]
source_paths:
  - mygeneration-legacy-code/mymeta/dbRoot.cs
  - mygeneration-legacy-code/mymeta/Column.cs
  - mygeneration-legacy-code/mymeta/Parameter.cs
  - mygeneration-legacy-code/mymeta/Domain.cs
  - mygeneration-legacy-code/mymeta/ResultColumn.cs
  - mygeneration-legacy-code/mygeneration/MyGeneration/Settings/Languages.xml
  - mygeneration-legacy-code/mygeneration/MyGeneration/Settings/DbTargets.xml
---

# Language and DbTarget mappings

## Files
`mygeneration-legacy-code/mygeneration/MyGeneration/Settings/Languages.xml` (root `<Languages>`, 61 `<Language>` nodes) and `DbTargets.xml` (root `<DbTargets>`). Structure:
```xml
<Languages><Language From="SQL" To="C#"><Type From="bigint" To="long"/> ... </Language></Languages>
<DbTargets><DbTarget From="SQL" To="SqlClient"><Type From="bigint" To="SqlDbType.BigInt"/> ...
```
`From` = driver id (`dbRoot._driverString`), `To` = language / target name. (`plugins/MyMetaPluginTestTool/Settings/Languages.xml` is a differing copy.)

## Selection in `dbRoot.cs`
- `LanguageMappingFileName` setter loads an `XmlDocument` and clears the current language. `GetLanguageMappings()` XPath `//Languages/Language[@From='<DRIVER>']` returns the `To` attributes (for UI lists). `Language` setter sets `LanguageNode` = `//Languages/Language[@From='<_driverString>' and @To='<lang>']`.
- `DbTargetMappingFileName`, `GetDbTargetMappings()`, `DbTarget`, `DbTargetNode`: same, with `//DbTargets/DbTarget`.
- File name must be set before `Language`/`DbTarget` (else silently ignored). `Language` must be re-set after reconnecting (Reset clears).

## Lookup
`Column.LanguageType` / `Column.DbTargetType` (`Column.cs`): if `DomainOverride` and `HasDomain` -> `Domain.LanguageType` / `DbTargetType`; else XPath `./Type[@From='<DataTypeName>']` under the node, return its `To`; no node or no match -> `"Unknown"`. `Parameter` keys on `TypeName`; `Domain` and `ResultColumn` on `DataTypeName`. Parameter and ResultColumn do not apply the domain override. Note the lookup key is the bare type name (e.g. `varchar`), never `DataTypeNameComplete`.

## SQL Server entries
Language `From="SQL"`: "C#", "C# System Types", "VB.NET", "VB.NET System Types", "VB60", plus `SQLCE` entries as a separate From. Each has 26 `<Type>` entries (bigint ... sql_variant).

C# map: bigint->long, binary->object, bit->bool, char/nchar/nvarchar/varchar/text/ntext/xml->string, datetime/smalldatetime->DateTime, decimal/money/numeric/smallmoney->decimal, float->double, real->float, image/timestamp/varbinary->byte[], int->int, smallint->short, tinyint->byte, uniqueidentifier->Guid, sql_variant->object. "C# System Types" is the same with `System.*` names.

**Gaps:** no `date`, `time`, `datetime2`, `datetimeoffset`, `geography`, `geometry`, `hierarchyid`, `sysname` -> they resolve to `"Unknown"`. Also `binary->object` and `timestamp->byte[]` are legacy choices.

DbTargets for `From="SQL"`: `SqlClient` (`SqlDbType.*`), `OleDb` (`OleDbType.*`; bit->Boolean, datetime->DBTimeStamp, int->Integer), `AllDbClient` (`AllDbType.*`), `ADODB` (`adBigInt`...), `DbType` (`DbType.*`, includes xml), and `SQLCE`->`SqlServerCe`. Other From values present (not needed for MSSQL): ACCESS, ORACLE, FIREBIRD, INTERBASE, DB2, VISTADB, VISTADB3X, SQLITE, ADVANTAGE, POSTGRESQL, POSTGRESQL8, MYSQL2, PERVASIVE, ISERIES, SYBASEASA, XSD3B, INGRES2006ODBC, VISUALFOXPRO, EFFIPROZ. `DefaultSettings.xml` stores the chosen `Language` ("C#") and `DbTarget`.

## Related pages
- [INDEX.md](INDEX.md)
- [overview.md](overview.md)
- [object-model.md](object-model.md)
- [sql-server-provider.md](sql-server-provider.md)
- [discard-vs-keep.md](discard-vs-keep.md)
