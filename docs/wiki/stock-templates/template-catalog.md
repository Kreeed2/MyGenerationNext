---
title: Stock Template Catalog
summary: Per-template reference (purpose, GUI inputs, MyMeta objects used, output files) for the C#, SQL Server, HTML, Other, Samples and out-of-scope stock templates.
keywords: [templates, catalog, BusinessEntity, ConcreteClass, View, StoredProc, SQL_StoredProcs, DataInserts, HTML_DatabaseReport, MyMeta]
source_paths: [mygeneration-legacy-code/templates/C#, mygeneration-legacy-code/templates/Microsoft_SQL_Server, mygeneration-legacy-code/templates/HTML, mygeneration-legacy-code/templates/Other, mygeneration-legacy-code/templates/Samples, mygeneration-legacy-code/templates/UnitTestTemplates, MyGenerationTemplates/_MyGeneration13_2025/Templates]
---

# Template catalog

Paths relative to `mygeneration-legacy-code/templates/`; identical copies exist under `MyGenerationTemplates/_MyGeneration13_2025/Templates/`.

## In scope for the port

### `C#/CSharp_SQL_dOOdads_BusinessEntity.vbgen` - "dOOdads Business Entity"
- UNIQUEID `1b2935a3-16f8-45ed-b50b-5cc6d1855428`, namespace `dOOdads.C#`, 1165 lines. Supports SQL Server, Access, Firebird, PostgreSQL, Oracle through `MyMeta.DbTarget` branches (only SqlClient matters).
- **GUI inputs**: output path, namespace (default `Your.Namespace`), database combo, table multi-list, checkboxes "prefix file with _" and "trim spaces", Firebird-only dialect 3.
- **MyMeta used**: `Databases[x].Tables[y]`; `Table.Alias/Name/Schema/Columns/PrimaryKeys/Properties`; `Column.Alias/Name/LanguageType/DbTargetType/CharacterMaxLength/IsAutoKey/IsComputed/Default/DataTypeName`; `MyMeta.DbTarget/DriverString/DefaultDatabase/Language`.
- **Output** per table `[_]<Alias>.cs`: `public abstract class _<Alias> : SqlClientEntity`. Full member list in [doodads-architecture.md](doodads-architecture.md#generated-entity-shape).
- **Helpers** (end of file): `GetAlias` (TrimSpaces + capitalize first letter), `GetName`, `TrimSpaces` (strips spaces and dots), `GetIDbCommand`, `GetIDataParameter`, `GetParameterPrefix`, `GetDataNameSpace`, `GetBaseClass`, `CreateProcedureName`, `FirebirdSP`. For SqlClient: base `SqlClientEntity`, namespace `System.Data.SqlClient`, `SqlParameter`, prefix `@`.
- Header comment "NEVER EDIT THIS FILE"; subclass in concrete class.

### `C#/CSharp_SQL_dOOdads_ConcreteClass.vbgen` - concrete class
- UNIQUEID `545956e4-940d-450e-af5f-dfdf8dbdc3b4`, 196 lines, usable for all DBs.
- Emits `using System;` + `namespace` + `public class <Alias> : _<Alias> { }`, saved to `<Alias>.cs` with `output.save(file,"d")` (skip if exists).

### `C#/CSharp_SQL_dOOdads_View.vbgen` - "dOOdads Business View"
- UNIQUEID `b8311e7d-09d3-4b21-a6a6-4df5549c7c9a`, 773 lines. GUI: `lstViews`, abstract and class-prefix checkboxes; reads `database.Views`.
- `LoadAll()` calls `base.Query.Load()`. Same nested `Parameters`, `ColumnNames`, `PropertyNames`, `StringPropertyNames`, `WhereClause`, `AggregateClause` as the entity. Insert/Update/Delete override stubs exist (not inspected in detail); treat views as read-only.

### `C#/CSharp_dOOdads_StoredProc.vbgen` - proc wrapper
- UNIQUEID `bb997abc-46b9-418d-a4fd-192586a5bd87`, OUTPUT_LANGUAGE `None`.
- Inputs: database, single-select proc (`lstProcs`), `cmbProcType` (N = execute only, R = return DataReader), driver and language combos (sets `MyMeta.DbTarget`/`MyMeta.Language`), `txtSpecialChar` (default `@`), Firebird dialect.
- MyMeta: `Procedure.Parameters` (`Direction` 1=in, 2=out, 3=inout, 4=return; `Name/Alias/LanguageType/DbTargetType/CharacterMaxLength`).
- Output `<proc alias>.cs`: `public virtual void|IDataReader <Name>(params)` building a `ListDictionary` of parameters then `LoadFromSqlNoExec` or `return LoadFromSqlReader`. Out params become `ref` arguments (values read from `p_<name>.Value`). `GetReturnType`/`GetMethod` also define an unused "L" type (bool / `return LoadFromSql`).

### `Microsoft_SQL_Server/SQL_StoredProcs.vbgen` - "dOOdad Stored Procedures"
- UNIQUEID `10998336-5037-496c-a163-050060de065a`.
- Inputs: path, database, tables, `chkAlter` (ALTER vs DROP+CREATE), `chkGenDoc` (default true; PRINT status lines), `chk2005`.
- Per table: `USE [db]`, then five procs `proc_<Table>LoadByPrimaryKey|LoadAll|Update|Insert|Delete`. No PK -> writes `-- ERROR: must have a primary key` and stops the loop.
- Parameter types from `Column.DataTypeNameComplete`; param name = column name minus spaces/dots.
- **Update**: nullable/computed params `= NULL`, computed ones `output`. SET excludes PK, AutoKey, computed. WHERE = PK + `TSEQUAL` for `timestamp` columns (the 2005 branch tests undeclared `chkYukon`; bug, `chk2005` effectively ignored). Re-selects computed columns afterwards. Not generated if every column is PK or computed.
- **Insert**: nullable/AutoKey/computed params `= NULL`; AutoKey/computed `output`. uniqueidentifier PK: `IF @x IS NULL SET @x = NEWID()`. Column list excludes AutoKey/computed. `SELECT @id = SCOPE_IDENTITY()`; re-selects computed columns.
- Each proc: `SET NOCOUNT ON` (Insert/Update/Delete use OFF), `DECLARE @Err`, `SET @Err = @@Error`, `RETURN @Err`.
- Output `<Table>.sql` per table and `MicrosoftSQL_ALL.SQL`. `BuildCreateAlterStatement` emits the `sysobjects` IF EXISTS DROP pattern with a "Script Date".
- Other DBs have equivalent `*_StoredProcs` templates under `Oracle`, `Firebird`, `PostgreSQL`, `Microsoft_Access`, `IBM_ISeries` (out of scope).

### Other SQL Server scripts
| File | Purpose |
|---|---|
| `SQL_StoredProcs.jgen` | Older JScript variant: 3 procs named `<Table>Insert/Update/Delete` (no `proc_` prefix), file `sql_procs_<table>.sql`. Known bugs (JS `&` concatenation in `insertComputedCode`, truncated DROP IF EXISTS). Discard. |
| `SQL_DataInserts.jgen` | `database.ExecuteSQL("SELECT * FROM [t]")`, emits `INSERT` per row inside `BEGIN TRANSACTION`; `chkAutoKey` adds `SET IDENTITY_INSERT ON`; skips timestamp/computed; uses `escapeDataItem`. |
| `SQL_DataReplication.jgen` | Per row: UPDATE by PK if exists, else INSERT, with `@updateCount/@insertCount`; handles NULL PK with `IS NULL`. |
| `SQL_DeleteAllData.jgen` | `DELETE FROM` in FK-dependency order (`getTableDep(database,true)`) in a transaction. |
| `sql_library.js` | `escapeDataItem(dbtype,datatype,data)` (only `dbtype=="SQL"`, i.e. `MyMeta.DriverString`): quotes/doubles `'` for varchar/nvarchar/char/nchar/text/ntext; uniqueidentifier strips `{}` and quotes; datetime/smalldatetime as `MM/dd/yyyy HH:mm:ss` quoted; bit -> 1/0; throws for image and sql_variant. `getTableDep`/`getViewDep`: topological sort over `ForeignKeys` (`PrimaryTable`/`ForeignTable`) with leftovers appended. |

### HTML / Other / Samples
| File | Purpose / MyMeta |
|---|---|
| `HTML/HTML_DatabaseReport.csgen` (.Net Script, ns "Microsoft SQL Server.Reports") | Inputs `cmbDatabase`, `lstTables`, `lstViews`, `lstProcs`, `txtPath`; output `db_report_<db>.html` with TOC, columns (`IsNullable`, `IsInPrimaryKey`, `IsComputed`, `IsAutoKey`, `Default`, `DataTypeNameComplete`, `Description`), indexes (`Unique/Clustered/FillFactor/Collation/Type`), foreign keys (`ForeignTable.Name`), `ViewText`, `ProcedureText`, `Parameters`, preserve block `customHeaders`. |
| `HTML/HTML_TableDefinition.vbgen` | Inputs `cmbLanguages`, `chkMultiFiles`; sets `MyMeta.Language`; uses `Column.CharacterOctetLength/Alias/Description`, `ForeignKeys`. |
| `Other/UserMetaData.vbgen/.jgen` | Adds key/values via `Column.Properties.AddKeyValue(...)` then `MyMeta.SaveUserMetaData`. |
| `Other/TemplateGroupExample.jgen` | `TYPE Group`; GUI only, chains templates. |
| `Other/WinformDemo.vbgen`, `Samples/GuiTest.zeus`, `Samples/Linq.zeus`, `UnitTestTemplates/CSharpTestTemplate001.csgen` | Demos/samples; discard. |

## Out of scope (other DBs/languages)
- Deprecated `C#/CSharp_SQL_BusinessObject.vbgen` (706 lines), `CSharp_Access_BusinessObject.vbgen` (668), `CSharp_AllDb_BusinessObject.vbgen` (732, legacy only); namespace `Samples.Deprecated.C#`, superseded by dOOdads. Not read in depth.
- MySQL4 / SQLite / VistaDB entity templates (inline SQL, no procs) in C# and VB.Net; VB.Net mirrors of everything; `PHP/PHP_BusinessObject.jgen`; `Opf3/C#`; `Gentle.NET`; `Xsd3b/*` (13_2025 only); Oracle/Firebird/PostgreSQL/Access/iSeries templates.

## Related pages
- [overview.md](overview.md)
- [doodads-architecture.md](doodads-architecture.md)
- [tutorials.md](tutorials.md)
- [zeus-api.md](zeus-api.md)
- [scriban-port-notes.md](scriban-port-notes.md)
- [INDEX.md](INDEX.md)
