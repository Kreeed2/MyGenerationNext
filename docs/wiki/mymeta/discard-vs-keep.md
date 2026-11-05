---
title: Discard vs keep for an MSSQL-only .NET 10 rewrite
summary: Recommendations of which MyMeta behaviours to drop, keep or fix when rebuilding the metadata layer for SQL Server only with C# output.
keywords: [rewrite, discard, keep, MSSQL, .NET 10, SqlClient, sp_describe_first_result_set, type mapping]
source_paths:
  - mygeneration-legacy-code/mymeta/dbRoot.cs
  - mygeneration-legacy-code/mymeta/Sql/
  - mygeneration-legacy-code/mygeneration/MyGeneration/Settings/Languages.xml
  - mygeneration-legacy-code/mygeneration/MyGeneration/Settings/DbTargets.xml
---

# Discard vs keep

## Discard
- All non-SQL providers (see [other-providers-skim.md](other-providers-skim.md)) and the `dbDriver` enum / `MyMetaDrivers` / `GetDbDriverFromName`.
- Plugin machinery: `IMyMetaPlugin(Ext)`, `MyMetaPluginContext`, `Plugin/*`, assembly scanning in `dbRoot.Plugins`, `InternalDriver`, `PluginDriver`, `FileDbDriver` (WinForms/ADODB UI helpers).
- OLE DB / ADODB / COM: `OleDbSchemaGuid` rowsets, `dbRoot.TheConnection`, `ADODB.Recordset ExecuteSql`, `ServicedComponent`, `[ComVisible]`, `[GuidAttribute]`, `DispId`.
- `StripTrailingNulls`/`TrailingNull`, `requiredDatabaseName`, `requiresSchemaName`, `ParsedConnectionString` hashtable hacks, `Collection` dummy IList stubs, the stale root `IClassFactory.cs`.
- `ProviderTypes`/`ProviderType` (OLE DB `Provider_Types`; only feeds `GetNativeType`).
- Non-SQL `From` groups and VB/VB60/other language groups in the mapping XML (unless wanted).
- Silent exception swallowing in `Connect`, `LoadData`, `ForeignKeys.PopulateArray`, `SqlResultColumns`.

## Keep / re-implement
- Object hierarchy: Database -> Table/View/Procedure/Domain -> Column/PK/FK/Index/Parameter/ResultColumn ([object-model.md](object-model.md)); `Alias` concept; physical-name indexers with `IgnoreCase`.
- Lazy loading (use `Lazy<T>`/async loaders) per parent.
- SQL Server specifics ([sql-server-provider.md](sql-server-provider.md)): identity seed/increment, computed columns (`timestamp`/rowversion), `(MAX)` handling and `DataTypeNameComplete` rules, `;1` procedure suffix stripping (not needed with `sys.procedures`), `sysdiagrams` exclusion, `ShowSystemData` and `ShowDefaultDatabaseOnly` semantics, MS_Description extended properties (all schemas; fix the `dbo`-only bug), view/procedure source text, view dependency (SubTables/SubViews), composite FKs with rules and deferrability, index flags (unique, clustered, fill factor, column order), domains = user-defined alias types (`DomainOverride`).
- Procedure result columns: replace execute-with-nulls with `sys.sp_describe_first_result_set`.
- Type mapping ([language-and-dbtarget-mappings.md](language-and-dbtarget-mappings.md)): keep the SQL->C# table and `SqlDbType.*` DbTarget; **add** date, time, datetime2, datetimeoffset, geography, geometry, hierarchyid, sysname, and nullable variants; return an explicit error instead of `"Unknown"` silently. Key on bare type name.
- User metadata XML ([user-metadata.md](user-metadata.md)): aliases (`n`), `Properties/Property k v`, global properties, merge; fix XPath quoting.
- Data access: `Microsoft.Data.SqlClient` with `sys.*`/`INFORMATION_SCHEMA`; the `Create*DataTable` column names in `MyMetaPluginContext` are a good neutral schema.
- Connection errors surfaced (legacy `Connect` always returns true).

## Related pages
- [INDEX.md](INDEX.md)
- [overview.md](overview.md)
- [sql-server-provider.md](sql-server-provider.md)
- [plugin-system.md](plugin-system.md)
