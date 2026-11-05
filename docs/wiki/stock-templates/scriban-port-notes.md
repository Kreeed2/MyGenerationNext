---
title: Scriban Port Notes
summary: How each stock C#/SQL Server template maps to a Scriban template for a .NET 10 SQL Server-only tool, what to keep, discard and fix.
keywords: [scriban, port, .NET 10, SQL Server, model, templates, discard, bugs]
source_paths: [mygeneration-legacy-code/templates/C#, mygeneration-legacy-code/templates/Microsoft_SQL_Server, mygeneration-legacy-code/doodads/CSharp/MyGeneration.dOOdads, MyGenerationTemplates/_MyGeneration13_2025/Settings]
---

# Scriban port notes

These are recommendations derived from the legacy behavior documented elsewhere in this wiki.

## Model to expose to Scriban
Replace `MyMeta` with plain objects loaded from SQL Server (`INFORMATION_SCHEMA` / `sys.*`):
- **Table/View**: `Name`, `Alias`, `Schema`, `Columns`, `PrimaryKeys`, (`ForeignKeys`, `Indexes` for report/data scripts).
- **Column**: `Name`, `Alias`, `LanguageType` (from Languages.xml SQL->C#), `DbTargetType` (SqlDbType name), `DataTypeName`, `DataTypeNameComplete` (e.g. `varchar(25)`), `CharacterMaxLength`, `IsNullable`, `IsInPrimaryKey`, `IsAutoKey`, `IsComputed`, `Default`.
- **Procedure**: `Name`, `Alias`, `Parameters` (`Direction`, `Name`, `LanguageType`, `DbTargetType`, `CharacterMaxLength`).
- Options (namespace, output dir, prefix, trim spaces) as a typed settings object.
Type maps from [settings-files.md](settings-files.md) become embedded static tables (and need `date`, `time`, `datetime2`, `datetimeoffset`, `rowversion` added).
Alias rules: `GetAlias` = strip spaces/dots + capitalize first letter - implement as a C# function exposed to Scriban.

## Template mapping
| Legacy | Scriban template(s) | Notes |
|---|---|---|
| `CSharp_SQL_dOOdads_BusinessEntity.vbgen` | `entity.scriban` (`_Table.cs`) | Keep SqlClient branch only. Loops: columns -> Parameters, ColumnNames, PropertyNames, StringPropertyNames, typed properties, `s_` properties, Where/Aggregate clause properties, Insert/Update/Delete command builders. Always overwrite. |
| `CSharp_SQL_dOOdads_ConcreteClass.vbgen` | `concrete.scriban` | Write only if file absent. |
| `CSharp_SQL_dOOdads_View.vbgen` | `view.scriban` | Same as entity, `LoadAll` = `Query.Load()`, no write commands. |
| `CSharp_dOOdads_StoredProc.vbgen` | `storedproc-wrapper.scriban` | Per procedure; ref args for output params. Optional. |
| `SQL_StoredProcs.vbgen` | `procs.sql.scriban` | Five procs per table; keep names `proc_<Table>LoadByPrimaryKey/LoadAll/Update/Insert/Delete` (must match entity). Fix `chkYukon`; decide TSEQUAL vs `=` for timestamp/rowversion. Keep ALTER vs DROP+CREATE option (prefer `CREATE OR ALTER`). |
| `SQL_DataInserts/DataReplication/DeleteAllData.jgen`, `sql_library.js` | optional data scripts | Need C# helpers for `escapeDataItem` and FK topological sort (`getTableDep`). |
| `HTML_DatabaseReport.csgen` | optional `report.html.scriban` | Low priority. |

Scriban idioms: `{{ for c in table.columns }}`, `{{ if c.is_computed }}`; the legacy "if DbTarget == X" branches vanish. Generate into a string and write with an overwrite policy (replaces `output.save` modes `o`/`d`).

## What to discard
- GUI engines (Microsoft Script/.Net Script), `ui` API, `DnpUtils` input cache -> TUI/CLI.
- VBScript/JScript, VB.Net templates, `.jgen` duplicates (`SQL_StoredProcs.jgen`).
- Multi-DB branches: OleDb, Oracle, Firebird (dialect, `FirebirdSP`), PostgreSQL, Access (@@IDENTITY hack), Advantage, `SEQ:`/`GEN:` user keys, MySQL/SQLite/VistaDB/iSeries/PHP/Opf3/Gentle/Xsd3b templates, deprecated BusinessObject templates.
- Tutorials, Samples, WinformDemo, UnitTestTemplates, help/Sandcastle, BLGenerator ZESA files, Scintilla/dock/proxy settings.
- Preserve-region machinery unless the report template is ported.

## Runtime decisions
- dOOdads depends on `System.Data.SqlClient`, `DataTable` adapters and AppSettings key `"dbConnection"`; for .NET 10 use `Microsoft.Data.SqlClient` and `IConfiguration`. Alternatively keep only `BusinessEntity` + `SqlClientEntity` + `SqlClientDynamicQuery` + `WhereParameter` + `AggregateParameter` + `TransactionMgr` (see [doodads-architecture.md](doodads-architecture.md)); drop the other adapters.
- Fix known defects: `SqlClientDynamicQuery` `In`/`NotIn` raw value embedding (injection); `TransactionMgr.hasRolledBack` never set; aggregate in GROUP BY; `chkYukon` in proc template; `Getint` throwing on DBNull is by design (use `IsColumnNull`).
- `TransactionMgr` uses thread-slot storage, which is unsafe with async; consider `AsyncLocal`.
- Port the NUnit SQL fixtures (`doodads/Tests/CSharp/SQL`) as regression tests for the runtime.

## Related pages
- [template-catalog.md](template-catalog.md)
- [doodads-architecture.md](doodads-architecture.md)
- [settings-files.md](settings-files.md)
- [overview.md](overview.md)
- [INDEX.md](INDEX.md)
