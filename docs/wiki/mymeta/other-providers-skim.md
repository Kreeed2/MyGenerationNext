---
title: Other built-in providers (skim)
summary: Table of the non-SQL-Server providers in mymeta, their driver ids, client libraries and sample connection strings, for reference only.
keywords: [providers, Oracle, PostgreSQL, MySql, Firebird, SQLite, Access, DB2, VistaDB, Npgsql]
source_paths:
  - mygeneration-legacy-code/mymeta/Access/ClassFactory.cs
  - mygeneration-legacy-code/mymeta/Oracle/ClassFactory.cs
  - mygeneration-legacy-code/mymeta/PostgreSQL8/ClassFactory.cs
  - mygeneration-legacy-code/mymeta/Firebird/ClassFactory.cs
  - mygeneration-legacy-code/mymeta/SQLite/ClassFactory.cs
  - mygeneration-legacy-code/mymeta/MyMeta.csproj
  - mygeneration-legacy-code/plugins/
---

# Other providers (skim only; out of scope for rewrite)

Each folder under `mygeneration-legacy-code/mymeta/` has the same 20-ish files as `Sql/` (`ClassFactory`, `Database(s)`, `Table(s)`, ... ) subclassing the core classes, plus its own `ClassFactory.Register()` sample connection string.

| Folder | Access method | Sample connection string (from `Register`) |
|---|---|---|
| Access | OLE DB schema rowsets (7 files) | `Provider=Microsoft.Jet.OLEDB.4.0;Data Source=c:\access\Northwind.mdb;...` |
| Advantage | OLE DB | `Provider=Advantage.OLEDB.1;...` |
| DB2 | OLE DB + some catalog SQL | `Provider=IBMDADB2.1;...` |
| ISeries | OLE DB + catalog SQL | `PROVIDER=IBMDA400;...` |
| MySql | OLE DB (MySQLProv) | `Provider=MySQLProv;...` |
| MySql5 (driver MySql2) | native `MySql.Data` + `information_schema` | `Database=Test;Data Source=...;User Id=...` |
| Oracle | OLE DB + catalog SQL | `Provider=OraOLEDB.Oracle.1;...` |
| Pervasive | OLE DB | `Provider=PervasiveOLEDB.8.60;...` |
| PostgreSQL / PostgreSQL8 | native `Npgsql`, `pg_catalog`/`information_schema` | `Server=127.0.0.1;Port=5432;...` |
| Firebird (also Interbase) | `FirebirdSql.Data.FirebirdClient`, `RDB$` tables | `Database=C:\firebird\EMPLOYEE.GDB;User=SYSDBA;...` |
| SQLite | `System.Data.SQLite`, `sqlite_master` | `Data Source=database.db;...` |
| VistaDB, VistaDB3x | VistaDB MetaHelper/OLE DB | (VistaDB-specific) |

Binaries for native clients are in `mymeta/ThirdParty/` and referenced by `MyMeta.csproj` (`FirebirdSql.Data.FirebirdClient 2.5.2`, `MySql.Data 6.2.3`, `Npgsql 2.0.9`, `System.Data.SQLite 1.0.66`, `ADODB`). Several providers also reuse `ShowSystemData` filtering (`"SYSTEM TABLE"`), and Firebird/Advantage `Column` classes honour `DomainOverride`.

External plugins (separate assemblies, loaded via [plugin-system.md](plugin-system.md)) in `mygeneration-legacy-code/plugins/`: SqlCe, SybaseASA, SybaseASE, Ingres2006, FoxPro, EffiProz, TextFile, Xsd3b, VistaDB3x/4x, MyMetaPluginTestTool. Changelog: `mymeta/MyMetaChanges.txt`.

## Related pages
- [INDEX.md](INDEX.md)
- [plugin-system.md](plugin-system.md)
- [sql-server-provider.md](sql-server-provider.md)
- [discard-vs-keep.md](discard-vs-keep.md)
