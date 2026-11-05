---
title: MyMeta overview
summary: What MyMeta is, how the root object `dbRoot` wires a connection, a driver ClassFactory and lazily loaded metadata together, and the end-to-end lifecycle a template sees.
keywords: [MyMeta, dbRoot, ClassFactory, Connect, lazy loading, overview, architecture]
source_paths:
  - mygeneration-legacy-code/mymeta/dbRoot.cs
  - mygeneration-legacy-code/mymeta/MetaObject.cs
  - mygeneration-legacy-code/mymeta/Single.cs
  - mygeneration-legacy-code/mymeta/Collection.cs
  - mygeneration-legacy-code/mymeta/Interfaces/IClassFactory.cs
  - mygeneration-legacy-code/mymeta/MyMeta.csproj
  - mygeneration-legacy-code/mygeneration/Zeus/ContextPreprocessors/DefaultContextProcessor.cs
---

# MyMeta overview

MyMeta (`mygeneration-legacy-code/mymeta/`, namespace `MyMeta`) is the database-metadata library of MyGeneration. Templates (Zeus scripts) get one `dbRoot` object named `MyMeta` and navigate it as an object graph: `Databases -> Tables/Views/Procedures/Domains -> Columns/ForeignKeys/Indexes/Parameters/ResultColumns`. It is a .NET Framework 4.0 library (`MyMeta.csproj`: `TargetFrameworkVersion v4.0`, `DefineConstants` includes `ENTERPRISE`, which turns on COM attributes `[ComVisible]`, `[GuidAttribute]`, and makes `dbRoot` a `ServicedComponent`).

## The core idea

1. `dbRoot` (file `dbRoot.cs`) is the root. Its constructor calls `ClassFactory.Register()` for every built-in provider (Access, Advantage, DB2, Firebird, ISeries, MySql, MySql5, Oracle, Pervasive, PostgreSQL, PostgreSQL8, Sql, SQLite and, unless `IGNORE_VISTA`, VistaDB) and then `Reset()`. Each `Register()` stores an `InternalDriver` (driver id, sample connection string, OLE-DB flag) in a static dictionary (`InternalDriver.Register`). See [plugin-system.md](plugin-system.md).
2. `dbRoot.Connect(driverString, connectionString)` selects a provider and assigns `dbRoot.ClassFactory` (an `IClassFactory`, `Interfaces/IClassFactory.cs`). For SQL Server the driver string is `"SQL"` and the factory is `MyMeta.Sql.ClassFactory`.
3. Every collection/entity is created through that factory (`CreateTables()`, `CreateColumn()`, ...). The generic base classes (`Tables`, `Table`, `Columns`, `Column`, ...) hold all shared logic; per-provider subclasses (`SqlTables`, `SqlColumns`, ...) override only the *loader* (`LoadAll`, `LoadForTable`, ...) and a few type-name properties.
4. Metadata is loaded **lazily, on first property access**, and cached in private fields of the parent (e.g. `Database._tables`, `Table._columns`). There is no separate cache class: the "cache" is the object graph itself plus the per-`Database` `Hashtable dataTypeTables` used by `Database.GetNativeType`. Re-`Connect()` calls `Reset()` which drops everything.
5. Every row-backed entity wraps a `System.Data.DataRow` (`Single._row`) and reads values through typed helpers (`GetString`, `GetBool`, `GetInt32` ...) keyed by a `DataColumn` that the parent collection bound once (`f_*` fields). See [object-model.md](object-model.md) and [data-binding-and-lazy-loading.md](data-binding-and-lazy-loading.md).
6. User-defined metadata (aliases and arbitrary key/value properties) lives in an XML document (`dbRoot.UserData`) and is layered on top of the discovered metadata. See [user-metadata.md](user-metadata.md).
7. Type translation (`Column.LanguageType`, `Column.DbTargetType`) is looked up in two XML mapping files selected by `dbRoot.Language` / `dbRoot.DbTarget`. See [language-and-dbtarget-mappings.md](language-and-dbtarget-mappings.md).

## Typical life cycle (as driven by Zeus)

`mygeneration/Zeus/ContextPreprocessors/DefaultContextProcessor.cs` performs, when `MyMeta` is not yet connected and `__dbDriver` and `__dbConnectionString` inputs exist:

```
myMeta.Connect(driver, connectionString);
myMeta.ShowDefaultDatabaseOnly = (input["__showDefaultDatabaseOnly"] == "True");   // optional
myMeta.DbTargetMappingFileName = input["__dbTargetMappingFileName"];             // optional
myMeta.LanguageMappingFileName = input["__dbLanguageMappingFileName"];           // optional
myMeta.UserMetaDataFileName    = input["__userMetaDataFileName"];                // optional
myMeta.DbTarget  = input["__dbTarget"];                                          // optional
myMeta.Language  = input["__language"];                                          // optional
myMeta.DomainOverride = (bool)input["__domainOverride"];                         // optional
```

Order matters: the mapping **file name must be set before** `Language`/`DbTarget` (the setters only resolve an XML node if the corresponding `_languageDoc`/`_dbTargetDoc` is loaded; otherwise the assignment is silently ignored). The settings default file (`mygeneration/MyGeneration/Settings/DefaultSettings.xml`) stores `DbDriver`, `ConnectionString`, `Language`, `DbTarget` and per-connection `dbtargetpath`, `languagepath`, `usermetapath`.

Then a template does e.g. `MyMeta.DefaultDatabase.Tables`, `table.Columns`, `column.LanguageType`.

## Key `dbRoot` members (from `dbRoot.cs`)

| Member | Meaning |
|---|---|
| `Connect(string driver, string cs)`, `Connect(dbDriver, string)`, `Connect(dbDriver, string pluginName, string cs)` | Resets state, opens a probe connection to learn the default database, picks `ClassFactory`. See [plugin-system.md](plugin-system.md). |
| `BuildConnection(driver, cs)` | Returns an un-opened `IDbConnection` for non-OLE-DB drivers (MySql2 -> `MySqlConnection`, PostgreSQL* -> `NpgsqlConnection`, Firebird/Interbase -> `FbConnection`, SQLite -> `SQLiteConnection`, VistaDB, plugins). Note: **no `case` for `SQL`** in the visible switch, so SQL is not served by this method. |
| `IsConnected`, `Driver` (`dbDriver` enum), `DriverString`, `ConnectionString` | State of last `Connect`. |
| `Databases` / `DefaultDatabase` / `DefaultDatabaseName` | Lazily created `IDatabases` via `ClassFactory.CreateDatabases()` then `LoadAll()` (or `LoadDefault()` if `ShowDefaultDatabaseOnly`). `DefaultDatabase` resolves `_defaultDatabase` by name, else (if exactly one database) that one. |
| `ProviderTypes` | Lazily `ClassFactory.CreateProviderTypes()` + `LoadAll()` (OLE-DB `Provider_Types` schema). |
| `ShowSystemData` | If true, Tables/Views loaders ask the OLE-DB schema for `SYSTEM TABLE`/`SYSTEM VIEW` **instead of** `TABLE`/`VIEW` ("ONLY system data is shown"). |
| `ShowDefaultDatabaseOnly` | Setting it to a different value nulls `_databases` (forces reload). |
| `DomainOverride` | Default `true`; when a column has a domain, `DataTypeName`, `DataTypeNameComplete`, `LanguageType`, `DbTargetType` come from the domain. |
| `IgnoreCase` (default true), `requiredDatabaseName`, `requiresSchemaName`, `StripTrailingNulls`, `TrailingNull` | Behaviour flags set per driver in `Connect`. `IgnoreCase` drives `Collection.CompareStrings`. |
| `UserData` (`XmlDocument`), `UserMetaDataFileName`, `SaveUserMetaData()`, `MergeUserMetaDataFiles(...)` | User metadata persistence. |
| `LanguageMappingFileName`, `Language`, `LanguageNode`, `GetLanguageMappings()`; `DbTargetMappingFileName`, `DbTarget`, `DbTargetNode`, `GetDbTargetMappings()` | Mapping files. |
| `UserDataDatabaseMappings` (`Dictionary<string,string>`) | Maps a physical DB name to the key used in user metadata XML (`Database.XmlMetaDataKey`). |
| `LastConnectionError`, `LastConnectionException` | Populated when `Connect` catches an exception. |
| `Plugins` (static `Hashtable`), `PluginSpecificData`, `DriverSpecificData` | Plugin registry. |

### Important behavioural quirks of `Connect`

* `Connect(dbDriver, pluginName, cs)` wraps everything in `try/catch`; on failure it only records `_lastConnectionError`/`_lastConnectionException` and **still sets `_isConnected = true` and returns `true`**. A rewrite must surface connection errors properly.
* `Connect` strips all double quotes from the connection string (`connectionString.Replace("\"", "")`).
* For `SQL` it opens an `OleDbConnection` once (`ConnectUsingOleDb`), reads `cn.Database` as the default database (`GetDefaultDatabase`), then closes it. Subsequent metadata loads re-open connections (`dbRoot.TheConnection` for schema rowsets; new `OleDbConnection(dbRoot.ConnectionString)` for ad-hoc SQL).
* Driver flags for SQL Server: `StripTrailingNulls=false`, `requiredDatabaseName=true`, `ClassFactory = new MyMeta.Sql.ClassFactory()`, `_driverString="SQL"`.
* `Connect(string driverIn, ...)` upper-cases the driver id; unknown ids that are keys of `dbRoot.Plugins` become `dbDriver.Plugin`; unknown and unregistered returns `false`; `"NONE"` returns `true` without connecting.

## Where the code lives (map)

| Path under `mygeneration-legacy-code/mymeta/` | Role |
|---|---|
| `dbRoot.cs` | Root object, driver enum `dbDriver`, string constants `MyMetaDrivers`, XML user/language/dbtarget handling, plugin loader |
| `MetaObject.cs`, `Single.cs`, `Collection.cs` | Base classes of every entity / collection |
| `Database*.cs`, `Table*.cs`, `View*.cs`, `Column*.cs`, `ForeignKey*.cs`, `Index*.cs`, `Procedure*.cs`, `Parameter*.cs`, `ResultColumn*.cs`, `Domain*.cs`, `ProviderType*.cs` | Generic (provider-neutral) entities and collections |
| `Property.cs`, `PropertyCollection.cs`, `PropertyCollectionAll.cs`, `KeyValueCollection.cs` | User properties |
| `Interfaces/I*.cs` | Public API (the contract templates use; COM-visible) |
| `IMyMetaPlugin.cs`, `IMyMetaPluginExt.cs`, `IMyMetaPluginContext.cs`, `MyMetaPluginContext.cs`, `Plugin/*.cs`, `InternalDriver.cs`, `PluginDriver.cs`, `FileDbDriver.cs` | Plugin/driver infrastructure |
| `Sql/*.cs` | SQL Server provider (the only one an MSSQL rewrite needs) |
| `Access/ Advantage/ DB2/ Firebird/ ISeries/ MySql/ MySql5/ Oracle/ Pervasive/ PostgreSQL/ PostgreSQL8/ SQLite/ VistaDB/ VistaDB3x/` | Other built-in providers, see [other-providers-skim.md](other-providers-skim.md) |
| `ThirdParty/` | Binaries (Firebird, MySql.Data, Npgsql, SQLite, Mono.Security) |
| `Utility.cs` | `GetDatabaseName(catalog, schema)` returns catalog if non-empty else schema |
| `Enumerator.cs`, `CollectionEnumerator.cs` | Non-generic enumerators used by a few collections |
| `IClassFactory.cs` (root) | Stale duplicate in namespace `dbMeta` returning concrete types; **not** compiled (`MyMeta.csproj` includes `Interfaces\IClassFactory.cs` only). Ignore. |

Settings XML (`Languages.xml`, `DbTargets.xml`) live outside MyMeta in `mygeneration-legacy-code/mygeneration/MyGeneration/Settings/`.

## Related pages

- [INDEX.md](INDEX.md)
- [object-model.md](object-model.md)
- [data-binding-and-lazy-loading.md](data-binding-and-lazy-loading.md)
- [plugin-system.md](plugin-system.md)
- [sql-server-provider.md](sql-server-provider.md)
- [user-metadata.md](user-metadata.md)
- [language-and-dbtarget-mappings.md](language-and-dbtarget-mappings.md)
- [other-providers-skim.md](other-providers-skim.md)
- [discard-vs-keep.md](discard-vs-keep.md)
