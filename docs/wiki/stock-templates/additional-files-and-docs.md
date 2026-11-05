---
title: Additional Files and Documentation Folders
summary: What lives in additionalfiles/ (sample databases, type test scripts) and documentation/ (Sandcastle/HelpNDoc sources) and their relevance to the port.
keywords: [additionalfiles, documentation, sandcastle, helpndoc, sample database, northwind]
source_paths: [mygeneration-legacy-code/additionalfiles, mygeneration-legacy-code/documentation]
---

# additionalfiles and documentation

## `additionalfiles/`
| File | Notes |
|---|---|
| `Efz.zip`, `SampleEffiProzDB.sql` | EffiProz (embedded DB) sample. The SQL is HSQL-style (`CREATE CACHED TABLE Customer/Product/Invoice/Item`, FKs with `ON DELETE CASCADE`, insert rows). |
| `FoxProNorthwind.zip`, `VistaDB4Northwind.zip`, `Northwind.db3` (SQLite), `TestAccessTypes.mdb` | Sample/type-test DBs for non-SQL-Server providers. Binaries, not opened. |
| `TestSqlServer2000Types.sql` | Small (1.3 KB) script exercising SQL Server 2000 types - the only item relevant to the port (idea for a type-mapping test fixture). |
| `TestSybaseASETypes.sql` | Sybase type test. Not relevant. |

## `documentation/help/`
- `zeus/ZeusClassReference/`: copies of Zeus source for Sandcastle plus `UpdateProject.bat` (copies from `..\..\..\mygeneration\Zeus\`) and `ZeusAPIHelpFile.SandcastleGUI`. See [zeus-api.md](zeus-api.md).
- `mygeneration/mygeneration.hnd`: a HelpNDoc project (a zip containing `default.xml`); user-manual source.
- `tools/`: 7z archives of Sandcastle/HelpNDoc/htmlhelp plus `readme.txt`.

None of this is needed in the rewrite except as reference for the template API.

## Related pages
- [overview.md](overview.md)
- [zeus-api.md](zeus-api.md)
- [INDEX.md](INDEX.md)
