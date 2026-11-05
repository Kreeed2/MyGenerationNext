---
title: SQL Server provider (MyMeta.Sql)
summary: Exact OLE DB schema rowsets and T-SQL queries the legacy SQL Server provider uses to populate each MyMeta entity, plus its type-name logic and known gaps.
keywords: [SQL Server, OleDbSchemaGuid, sp_columns, fn_listextendedproperty, syscomments, INFORMATION_SCHEMA, IDENT_SEED, identity, extended properties]
source_paths:
  - mygeneration-legacy-code/mymeta/Sql/ClassFactory.cs
  - mygeneration-legacy-code/mymeta/Sql/Databases.cs
  - mygeneration-legacy-code/mymeta/Sql/Database.cs
  - mygeneration-legacy-code/mymeta/Sql/Tables.cs
  - mygeneration-legacy-code/mymeta/Sql/Table.cs
  - mygeneration-legacy-code/mymeta/Sql/Views.cs
  - mygeneration-legacy-code/mymeta/Sql/View.cs
  - mygeneration-legacy-code/mymeta/Sql/Columns.cs
  - mygeneration-legacy-code/mymeta/Sql/Column.cs
  - mygeneration-legacy-code/mymeta/Sql/ForeignKeys.cs
  - mygeneration-legacy-code/mymeta/Sql/Indexes.cs
  - mygeneration-legacy-code/mymeta/Sql/Procedures.cs
  - mygeneration-legacy-code/mymeta/Sql/Procedure.cs
  - mygeneration-legacy-code/mymeta/Sql/Parameters.cs
  - mygeneration-legacy-code/mymeta/Sql/Parameter.cs
  - mygeneration-legacy-code/mymeta/Sql/ResultColumns.cs
  - mygeneration-legacy-code/mymeta/Sql/Domains.cs
  - mygeneration-legacy-code/mymeta/Sql/DatabaseSpecific.cs
---

# SQL Server provider (`mymeta/Sql/`)

Driver id `"SQL"`. Important: the provider does **not** query `sys.*` directly for most metadata. It calls OLE DB schema rowsets (`OleDbConnection.GetOleDbSchemaTable(OleDbSchemaGuid.X, filter[])` via `MetaObject.LoadData`) and augments them with a handful of T-SQL statements. A rewrite should reproduce the *resulting data*, using `sys.*`/`INFORMATION_SCHEMA` with `Microsoft.Data.SqlClient`.

## Registration / connection
`Sql/ClassFactory.Register()` registers sample `Provider=SQLOLEDB.1;Persist Security Info=False;User ID=sa;Initial Catalog=Northwind;Data Source=localhost` with `RequiredDatabaseName=true`. `CreateConnection()` returns an `OleDbConnection`; `ChangeDatabase` calls `connection.ChangeDatabase(db)`; provider types reuse core `ProviderType(s)`. Classes: `SqlDatabases/SqlDatabase`, `SqlTables/SqlTable`, `SqlViews/SqlView`, `SqlColumns/SqlColumn`, `SqlProcedures/SqlProcedure`, `SqlParameters/SqlParameter`, `SqlResultColumns/SqlResultColumn`, `SqlDomains/SqlDomain`, `SqlForeignKeys/SqlForeignKey` (empty subclass), `SqlIndexes/SqlIndex` (empty subclass).

## Population by entity
| Entity | Source | Detail |
|---|---|---|
| Databases | `OleDbSchemaGuid.Catalogs` (`SqlDatabases.LoadAll`) | then base `Databases.PopulateSchemaData` runs `Schemata` per DB for `SCHEMA_NAME, SCHEMA_OWNER, DEFAULT_CHARACTER_SET_*`. `LoadDefault` keeps only `dbRoot.DefaultDatabaseName`. |
| Tables | `Tables` schema, filter `{db,null,null,type}`, type = `"TABLE"` or (`ShowSystemData`) `"SYSTEM TABLE"` | Row named `sysdiagrams` removed. |
| Table descriptions | `SELECT objName, value FROM ::fn_listextendedproperty ('MS_Description','user','dbo','table',null,null,null)` | Sets row field `DESCRIPTION`. **Only schema `dbo`.** |
| Primary keys | `OleDbSchemaGuid.Primary_Keys` `{db,schema,table}` (`SqlTable.PrimaryKeys`) | Column `COLUMN_NAME`. |
| Views | `Tables` schema type `"VIEW"`/`"SYSTEM VIEW"`; descriptions via same `fn_listextendedproperty(... 'dbo','view' ...)` | |
| View text | `SqlView.ViewText` -> `LoadViewSource()`: `SELECT CASE WHEN encrypted=1 THEN NULL ELSE com.text END as Source FROM sysobjects o, syscomments com WHERE o.id=object_id(N'[schema].[name]') and com.id=o.id and com.status=2 order by colid;` | If empty, retries without `com.status=2`; concatenates chunks; `TrimStart(' ','\r','\n','\t')`. |
| Sub views/tables | `LoadSubViewInfo`: `exec sp_refreshView 'schema.name'; SELECT * FROM INFORMATION_SCHEMA.VIEW_TABLE_USAGE WHERE VIEW_NAME=.. AND VIEW_SCHEMA=..` | Each `TABLE_NAME` is looked up in `Database.Tables` then `Views` to fill `SubTables`/`SubViews`. |
| Columns | `OleDbSchemaGuid.Columns` `{db,schema,name}` (`SqlColumns.LoadForTable/LoadForView`) | then `LoadExtraData` (below). |
| Foreign keys | `OleDbSchemaGuid.Foreign_Keys` twice, rows merged: `{db,schema,table}` (FK side) and `{null,null,null,db,schema,table}` (PK side) | then base `PopulateArray` groups by `FK_NAME`. `LoadAllIndirect` is **not** overridden -> `IndirectForeignKeys` always empty. |
| Indexes | `OleDbSchemaGuid.Indexes` `{db,schema,null,null,table}` | columns UNIQUE, CLUSTERED, TYPE, FILL_FACTOR, ..., `COLUMN_NAME` per row. |
| Procedures | `OleDbSchemaGuid.Procedures` `{db,null,null}`; descriptions via `fn_listextendedproperty(..'dbo','procedure'..)` | `SqlProcedure.Name`/`Alias` split on `;` and use part 0 (removes `;1`). |
| Procedure text | same `syscomments` query as views (`LoadProcedureSource`) | |
| Parameters | `OleDbSchemaGuid.Procedure_Parameters` `{db,schema,proc}`; descriptions via `fn_listextendedproperty(default,'user','dbo','procedure',procName,'parameter',default)` | |
| Result columns | `SqlResultColumns.LoadAll`: builds `EXEC [db].schema.[proc] null,null,...` (one `null` per non-ReturnValue parameter), executes with `CommandBehavior.SchemaOnly` on a `SqlConnection` rebuilt from the OleDb string | reads `GetSchemaTable()`: `ColumnName, DataTypeName, ColumnOrdinal, ColumnSize, NumericPrecision, NumericScale` (precision/scale >=255 -> 0). Errors swallowed. `SqlResultColumn.CharacterOctetLength` doubles size when type name starts with "n". |
| Domains | `SqlDomains.LoadAll`: `ChangeDatabase` then `SELECT * FROM INFORMATION_SCHEMA.DOMAINS` | |
| Provider types | `OleDbSchemaGuid.Provider_Types` (core `ProviderTypes.LoadAll`) | |

Connection-string conversion in `SqlResultColumns`: drops `Provider` and `Extended Properties`; maps `Server`/`Data Source`->`Data Source`, `User ID`/`UID`, `Password`/`PWD`, `Initial Catalog`/`Database`, and handles `marsconn`.

## Column extras (`SqlColumns.LoadExtraData`, `LoadAutoKeyInfo`, `LoadDescriptions`)
1. `EXEC [db].dbo.sp_columns 'name','schema'` -> `TYPE_NAME` added to the DataTable and filled **by row index** (assumes same order as the Columns rowset). A trailing `" identity"` is stripped and `"()"` removed.
2. `SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE table_schema/table_catalog/table_name ... AND DATA_TYPE in ('nvarchar','varchar','varbinary') AND character_maximum_length=-1 AND character_octet_length=-1` -> overrides type name for `(max)` columns.
3. Tables only, `LoadAutoKeyInfo`: `SELECT TABLE_NAME, COLUMN_NAME, IDENT_SEED('[tbl]') AS AUTO_KEY_SEED, IDENT_INCR('[tbl]') AS AUTO_KEY_INCREMENT FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME=.. AND TABLE_SCHEMA=.. AND columnproperty(object_id('[schema].[tbl]'),COLUMN_NAME,'IsIdentity')=1`; adds `AUTO_KEY_SEED`, `AUTO_KEY_INCREMENT`, and bool `AUTO_INCREMENT` (bound as `f_AutoKey`; only exists when identity rows exist, otherwise `IsAutoKey` defaults false).
4. Tables only, `LoadDescriptions`: UNION of `fn_listextendedproperty('MS_Description','user'|'schema', schema,'table',tableName,'column',null)`.

## Column/type naming (`Sql/Column.cs`, `Sql/Columns.cs`)
- `SqlColumn.DataTypeName`: domain override first (if `DomainOverride` and `HasDomain`), else `TYPE_NAME`.
- `SqlColumn.IsComputed`: true if `DataTypeName == "timestamp"`, else the `IS_COMPUTED` column.
- `DataTypeNameComplete` via `GetFullDataTypeName`: varchar/nvarchar/varbinary -> `(MAX)` if `CharacterMaxLength > 1000000` else `(len)`; binary/char/nchar -> `(len)`; decimal/numeric -> `(p,s)`; else bare name. Reused by `SqlResultColumn`.
- `SqlParameter.DataTypeNameComplete` (from `TypeName`) and `SqlDomain.DataTypeNameComplete`: binary/char/nchar/nvarchar/varchar/varbinary -> `(CharacterMaxLength)`; decimal/numeric -> `(precision,scale)`.
- `SqlDatabase.GetNativeType` override: OleDbType VarChar->`varchar(len)`, VarWChar/BSTR->`nvarchar(len)`, Char->`char(len)`, WChar->`nchar(len)`, LongVarChar->`text`, LongVarWChar->`ntext`, LongVarBinary->`image`, VarBinary->`varbinary`, else `sql_variant` (caches in `dataTypeTables`). Used by `ResultColumnsFromSQL`.

## Extended properties (`Sql/DatabaseSpecific.cs`)
Key `"ExtendedProperties"`: `SELECT [name],[value] FROM ::fn_listextendedproperty (NULL,'user',{schema},{type},{entity},{'column'|null},{column|null})`, invoked through `SqlDatabase.DBSpecific(key, single)` from `Table`/`View`/`Procedure`/`Column.DatabaseSpecificMetaData`. Returns a `KeyValueCollection` (`IPropertyCollection` of `KeyValuePair : IProperty`, `IsGlobal=true`). Runs via `Database.ExecuteSql` (ADODB `Recordset`).

## Ad-hoc SQL for templates
`Database.ExecuteSql(sql)` (ADODB) and `Database.ResultColumnsFromSQL(sql)` (reads `GetSchemaTable` of the reader and maps with `GetNativeType`/`GetADOType`). Typical in templates that describe a custom query.

## Known gaps/bugs to avoid in a rewrite
- Descriptions limited to schema `dbo`; table/column/parameter descriptions use `'user'`/`'schema'` level-0 types of the pre-2005 style.
- `sp_columns` merge by row index; `sysdiagrams` special-case; uses legacy `sysobjects/syscomments` (use `sys.sql_modules`/`OBJECT_DEFINITION`).
- Procedure result sets discovered by *executing* the procedure with NULLs (side effects, failures swallowed); `sp_describe_first_result_set` is the modern replacement.
- Indirect foreign keys never populated.
- No handling of modern types in type mapping (see [language-and-dbtarget-mappings.md](language-and-dbtarget-mappings.md)).

## Related pages
- [INDEX.md](INDEX.md)
- [overview.md](overview.md)
- [object-model.md](object-model.md)
- [data-binding-and-lazy-loading.md](data-binding-and-lazy-loading.md)
- [language-and-dbtarget-mappings.md](language-and-dbtarget-mappings.md)
- [user-metadata.md](user-metadata.md)
- [discard-vs-keep.md](discard-vs-keep.md)
