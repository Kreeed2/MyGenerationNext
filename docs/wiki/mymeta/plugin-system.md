---
title: Provider and plugin system
summary: How MyMeta registers built-in drivers, loads external IMyMetaPlugin assemblies, and selects a ClassFactory at Connect time.
keywords: [plugin, IMyMetaPlugin, IMyMetaPluginExt, MyMetaPluginContext, InternalDriver, PluginDriver, ClassFactory, dbDriver]
source_paths:
  - mygeneration-legacy-code/mymeta/dbRoot.cs
  - mygeneration-legacy-code/mymeta/InternalDriver.cs
  - mygeneration-legacy-code/mymeta/PluginDriver.cs
  - mygeneration-legacy-code/mymeta/FileDbDriver.cs
  - mygeneration-legacy-code/mymeta/IMyMetaPlugin.cs
  - mygeneration-legacy-code/mymeta/IMyMetaPluginExt.cs
  - mygeneration-legacy-code/mymeta/IMyMetaPluginContext.cs
  - mygeneration-legacy-code/mymeta/MyMetaPluginContext.cs
  - mygeneration-legacy-code/mymeta/Plugin/
  - mygeneration-legacy-code/mymeta/Interfaces/IClassFactory.cs
---

# Provider and plugin system

Paths relative to `mygeneration-legacy-code/mymeta/`.

## IClassFactory (`Interfaces/IClassFactory.cs`)
`CreateConnection()`, `ChangeDatabase(IDbConnection, string)`, and `CreateX()`/`CreateXs()` for Database, Table, Column, Procedure, View, Parameter, ForeignKey, Index, ResultColumn, Domain, ProviderType. Each provider has a `ClassFactory` with a static `Register()` (built-ins) and instance create methods returning its subclasses. The root-level `IClassFactory.cs` (namespace `dbMeta`) is stale and not in `MyMeta.csproj`.

## Built-in driver selection (`dbRoot.cs`)
- Enum `dbDriver`: SQL, Oracle, Access, MySql, MySql2, DB2, ISeries, Pervasive, PostgreSQL, PostgreSQL8, Firebird, Interbase, SQLite, VistaDB, Advantage, Plugin, None. Static `MyMetaDrivers` holds the id strings ("SQL", "ORACLE", ...); `GetDbDriverFromName` maps string -> enum, returning `Plugin` for unknown names.
- `Connect(string, string)`: upper-case; `"NONE"` -> true; built-in -> `Connect(dbDriver, name, cs)`; name in static `Plugins` -> Plugin; else false.
- `Connect(dbDriver, pluginName, cs)`: `Reset()`, strip `"`, then per driver: probe connection (OleDb via `ConnectUsingOleDb` for SQL/Oracle/Access/DB2/...; native client for MySql2, PostgreSQL*, Firebird, SQLite, VistaDB), set `_driverString`, flags and `ClassFactory = new MyMeta.<X>.ClassFactory()`. Exceptions are captured into `LastConnectionError`/`LastConnectionException` yet `IsConnected` becomes true.
- `InternalDriver` (static registry via `Register`/`Get`): factory `Type`, sample `ConnectString`, `IsOleDB`, `RequiredDatabaseName`, `StripTrailingNulls`, `BrowseConnectionString()` and a `ShowOLEDBDialog` delegate; UI-only (WinForms/ADODB). `FileDbDriver` = file-chooser variant; `PluginDriver` wraps an `IMyMetaPlugin` (`ConnectString` falls back to `plugin.SampleConnectionString`; browsing via `GetDatabaseSpecificMetaData(null,"CanBrowseDatabase"/"BrowseDatabase")`).

## External plugin loading (`dbRoot.Plugins`, static, lazy)
Base folder = directory of `Assembly.GetCallingAssembly().Location`. Scans `MyMeta.Plugins.*.dll` (or all subdirectories if `PLUGINS_FROM_SUBDIRS` is defined). `loadPlugin` -> `Assembly.LoadFile`, finds a type implementing `IMyMetaPlugin`, invokes constructor[0], `InternalDriver.Register(plugin.ProviderUniqueKey, new PluginDriver(plugin))`, `plugins[key]=plugin`. Failures are swallowed (Trace log). Plugin connect: `GetConnectionFromPlugin` builds `MyMetaPluginContext(providerName, connectionString)`, calls `plugin.Initialize(context)`, uses `plugin.NewConnection`; factory is `new MyMeta.Plugin.ClassFactory(plugin)`; `dbDriver.Plugin`, `_driverString = pluginName`. Repo plugins live in `mygeneration-legacy-code/plugins/` (SqlCe, SybaseASA/ASE, Ingres2006, FoxPro, EffiProz, TextFile, Xsd3b, VistaDB3x/4x, MyMetaPluginTestTool).

## IMyMetaPlugin contract
Identity/config: `Initialize(IMyMetaPluginContext)`, `ProviderUniqueKey`, `ProviderName`, `ProviderAuthorInfo`, `ProviderAuthorUri`, `StripTrailingNulls`, `RequiredDatabaseName`, `SampleConnectionString`, `NewConnection` (unopened `IDbConnection`), `DefaultDatabase`.
Data (return `DataTable`): `Databases`, `GetTables(db)`, `GetViews(db)`, `GetProcedures(db)`, `GetDomains(db)`, `GetProcedureParameters(db,proc)`, `GetProcedureResultColumns(db,proc)`, `GetViewColumns(db,view)`, `GetTableColumns(db,table)`, `GetTableIndexes(db,table)`, `GetForeignKeys(db,table)`. Lists: `GetPrimaryKeyColumns(db,table)`, `GetViewSubViews`, `GetViewSubTables`. Extras: `GetDatabaseSpecificMetaData(object, key)` (keys "ExtendedProperties", "CanBrowseDatabase", "BrowseDatabase").
`IMyMetaPluginExt : IMyMetaPlugin` adds `ChangeDatabase(IDbConnection, string)` and `GetProviderTypes(db)`; `Plugin/ClassFactory.ChangeDatabase` uses it when implemented, while provider types still use core classes (Ext branch commented out).
`IMyMetaPluginContext`/`MyMetaPluginContext`: `IncludeSystemEntities`, `ProviderName`, `ConnectionString`, and helpers `CreateDatabasesDataTable`, `CreateTablesDataTable`, `CreateViewsDataTable`, `CreateColumnsDataTable`, `CreateIndexesDataTable`, `CreateForeignKeysDataTable`, `CreateProceduresDataTable`, `CreateParametersDataTable`, `CreateResultColumnsDataTable`, `CreateDomainsDataTable` whose column names are the contract (e.g. columns table has `TYPE_NAME`, `TYPE_NAME_COMPLETE`; views `VIEW_TEXT`; procedures `PROCEDURE_DEFINITION`).
`Plugin/*.cs` (`PluginDatabases`, `PluginTables`, `PluginTable.PrimaryKeys` via `GetPrimaryKeyColumns`, `PluginColumns`, `PluginViews`, ...) subclass core classes and delegate to the plugin; `PluginColumn.DataTypeName` reads the plugin type-name columns with no domain override; `Plugin/ClassFactory.CreateConnection` returns `plugin.NewConnection`.

## Rewrite implication
For an MSSQL-only tool this whole mechanism collapses to one concrete implementation (see [discard-vs-keep.md](discard-vs-keep.md)); the `Create*DataTable` column names are however a handy list of "what a provider must supply".

## Related pages
- [INDEX.md](INDEX.md)
- [overview.md](overview.md)
- [sql-server-provider.md](sql-server-provider.md)
- [other-providers-skim.md](other-providers-skim.md)
- [discard-vs-keep.md](discard-vs-keep.md)
