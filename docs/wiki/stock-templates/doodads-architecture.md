---
title: dOOdads Runtime Architecture
summary: How the dOOdads C# runtime (BusinessEntity, DynamicQuery, WhereParameter, AggregateParameter, TransactionMgr, SqlClientEntity) works and how generated entity classes derive from it.
keywords: [dOOdads, BusinessEntity, DynamicQuery, WhereParameter, AggregateParameter, TransactionMgr, SqlClientEntity, SqlClientDynamicQuery, generated entity, tests]
source_paths: [mygeneration-legacy-code/doodads/CSharp/MyGeneration.dOOdads, mygeneration-legacy-code/doodads/CSharp/dOOdad_Demo, mygeneration-legacy-code/doodads/Tests/CSharp/SQL, mygeneration-legacy-code/doodads/dOOdad_UnitTesting/CSharp/SQL, MyGenerationTemplates/_MyGeneration13_2025/Architectures/dOOdads/CSharp]
---

# dOOdads architecture

Namespace `MyGeneration.dOOdads` (copyright 2004/2005 MyGeneration Software), folder `mygeneration-legacy-code/doodads/CSharp/MyGeneration.dOOdads/`. VB.NET folder `doodads/VB.NET` mirrors it (not analysed in detail).

## Class hierarchy
```
BusinessEntity (abstract, ADO.NET DataTable/DataRow)
 └─ SqlClientEntity          (DbAdapters/SqlClientEntity.cs)
     └─ _<Table>  (generated, abstract)
         └─ <Table>  (generated once, user code)
DynamicQuery (abstract) └─ SqlClientDynamicQuery
```
Other adapters (each Entity + DynamicQuery pair): MySql4, OleDb, Oracle, Postgres, SQLite, VistaDB, Firebird, plus VisualFoxPro in the legacy folder only. README.TXT: all except VistaDB/SQLite/MySQL4 use stored procs; each adapter must be set to "Compile" in the project.

## BusinessEntity
Abstract factory methods implemented by each adapter: `CreateDynamicQuery`, `CreateIDbCommand`, `CreateIDbDataAdapter`, `CreateIDbConnection`, `CreateIDataParameter` (2 overloads), `ConvertIDbDataAdapter`, and virtual `_LoadFromRawSql`.

**Properties**: `QuerySource` (= `SchemaTableView + _querySource`), `MappingName`, `SchemaGlobal` (sets `SchemaTableView` and `SchemaStoredProcedure`; value needs the trailing period, e.g. `"HR."`), `ConnectionString`, `ConnectionStringConfig`, static `DefaultConnectionStringConfig` (default AppSettings key `"dbConnection"`; read via `ConfigurationManager.AppSettings` under `#if VS2005`, else `ConfigurationSettings`), `NotRecommendedConnection`, `StringFormat` (`"MM/dd/yyyy"`), `Filter`, `Sort`, `DefaultView`, `RowCount`, `EOF`, `Query`.

**Navigation/shape**: `Rewind()`, `MoveNext()`, `FlushData()`, `AddColumn`, `RowState()`; protected `DataTable`/`DataRow`.

**Loading**: `LoadFromSql(sp[, ListDictionary[, CommandType]])` (dictionary keys are an `IDataParameter` - value assigned - or a string name; each enlisted in `TransactionMgr`, filled by a DbDataAdapter, returns `RowCount>0`); `LoadFromRawSql(sql, params object[])` (replaces `{0}` with `@p0`); `LoadFromSqlNoExec` (rows affected; timeout overload); `LoadFromSqlScalar`; `LoadFromSqlReader` (CommandBehavior.CloseConnection).

**Editing**: `AddNew()` (if no table, loads schema with `SELECT * FROM [QuerySource] WHERE 1=0`), `MarkAsDeleted`, `DeleteAll`, `RejectChanges`, `AcceptChanges`, `GetChanges()` / `GetChanges(states)`.

**Save()**: scans `RowState` for Added/Modified/Deleted; sets only needed Insert/Update/Delete commands (virtual `GetInsertCommand/GetUpdateCommand/GetDeleteCommand`, overridden by generated class) on a `DbDataAdapter`; `BeginTransaction`; enlist commands; `HookupRowUpdateEvents` (OleDbEntity overrides to fetch identity); `dbDataAdapter.Update(table)`; `CommitTransaction`; `AcceptChanges` only when `NestingCount==1`, otherwise registered via `AddBusinessEntity`. On exception `RollbackTransaction` and rethrow. `_canSave` is false after `AddResultColumn`, aggregates or `CountAll`; `Save()` then throws.

**Serialization**: `Serialize()`/`Deserialize` (DataSet XML string), `ToXml()`/`ToXml(XmlWriteMode)`, `FromXml(xml[, XmlReadMode])`.

**Row accessors**: `IsColumnNull`, `SetColumnNull`, `SetColumn`, `GetColumn`; typed `Get<T>/Set<T>` for Guid, bool, string, int, uint, long, ulong, short, ushort, DateTime, TimeSpan, decimal, float, double, byte, sbyte, object; `...Array` variants (byte[], bool[], string[], int[], long[], short[], DateTime[], decimal[], float[], double[]); `Get/Set<T>AsString`. Quirks: `Getint` etc. cast raw value and throw on DBNull; `GetGuid`/`Getstring` return empty on null; `Setstring("")` and `SetGuid(Guid.Empty)` store DBNull.

## DynamicQuery
Abstract; `_Load(conjuction)` implemented per DB. Members: `Load()`/`Load(conjuction)`, `GenerateSQL()`, `LastQuery`, `ReturnReader()`, `Top`, `Distinct`, `CountAll`/`CountAllAlias`, `WithRollup`, `AddWhereParameter`, `AddAggregateParameter`, `AddOrderBy` (string / DynamicQuery / AggregateParameter overloads), `AddGroupBy` (string / aggregate), `AddResultColumn`, `ResultColumnsClear`, `AddConjunction`, `OpenParenthesis`/`CloseParenthesis`, `FlushWhereParameters`/`FlushAggregateParameters`, `ParameterCount`, `AggregateCount`. Default conjunction `"AND"`. `Load` sets `_canSave=true` when no aggregates/result columns/countAll.

## WhereParameter
Ctor `(column, IDataParameter)`. `Operand` enum: Equal, NotEqual, GreaterThan, GreaterThanOrEqual, LessThan, LessThanOrEqual, Like, IsNull, IsNotNull, Between, In, NotIn, NotLike. Enums `Dir {ASC, DESC}`, `Conj {And, Or, UseDefault}`. Properties: `Value`, `Operator` (default Equal), `Conjuction`, `BetweenBeginValue`, `BetweenEndValue`, `IsDirty` (set by setters; only dirty parameters are emitted), `Column`, `Param`.

## AggregateParameter
`Func` enum {Avg, Count, Max, Min, StdDev, Var, Sum}; `Function` default Sum; `Alias` default the column name; `Distinct`; `IsDirty`.

## TransactionMgr
- One manager per thread (`Thread.GetData`/`AllocateDataSlot`): `ThreadTransactionMgr()`, `ThreadTransactionMgrReset()`.
- Nesting counter: `BeginTransaction` increments; `CommitTransaction` decrements and commits all at 0, then `AcceptChanges` on registered entities; `RollbackTransaction` rolls back and sets count 0.
- One real connection/transaction per connection-string key (`Enlist`/`DeEnlist`). Entities with `NotRecommendedConnection` never join. Static `IsolationLevel` (default Unspecified).
- Quirk: `hasRolledBack` is declared/checked but never set true (doomed-transaction behavior claimed in the demo is not implemented).

## SqlClientEntity / SqlClientDynamicQuery
`SqlClientEntity : BusinessEntity` creates `SqlParameter`, `SqlCommand`, `SqlDataAdapter`, `SqlConnection`, `SqlClientDynamicQuery`; `ConvertIDbDataAdapter` casts to `DbDataAdapter`; `_LoadFromRawSql` maps `{n}` to `@pn`.

`SqlClientDynamicQuery` SQL dialect: `[col]` brackets; `SELECT [DISTINCT] [TOP n] cols | COUNT(*) | aggregates FROM [QuerySource]`; parameters named `@<Column><counter>`; `Between` adds two parameters; **`In`/`NotIn` embed `Value` raw in the SQL (injection risk)**; `GROUP BY ... [WITH ROLLUP]`, `ORDER BY`; aggregate aliases as `AS 'alias'`; function names `STDEV`, `VAR`; parenthesis tokens skip the conjunction; `AddParameter` uses `cmd.Parameters.AddWithValue` under `VS2005`. `AddGroupBy(AggregateParameter)` emits an aggregate in GROUP BY via `GetAggregate`, which SQL Server rejects.

## Generated entity shape
Produced by `templates/C#/CSharp_SQL_dOOdads_BusinessEntity.vbgen` (see [template-catalog.md](template-catalog.md)). `public abstract class _<Alias> : SqlClientEntity` containing:
- Constructor: sets `QuerySource` (table name) and `MappingName`; Oracle also `SchemaGlobal`.
- `AddNew()` override: assigns `Guid.NewGuid()` to columns whose Default is `newid()` or `GenGUID()`. `FlushData()` override.
- `LoadAll()` -> `LoadFromSql("[" + this.SchemaStoredProcedure + "proc_<T>LoadAll]")`; `LoadByPrimaryKey(pk...)` -> `proc_<T>LoadByPrimaryKey`.
- Nested `Parameters` (static `SqlParameter` properties with `SqlDbType`/size), `ColumnNames` (constants + `ToPropertyName`), `PropertyNames` (+ `ToColumnName`), `StringPropertyNames` (`s_` prefix).
- Typed properties via `base.Get<type>/Set<type>(ColumnNames.X)` (array types use `...Array`); setters omitted for `timestamp` and computed columns. `s_<Name>` string properties: null -> `string.Empty`; assigning empty -> `SetColumnNull`.
- `WhereClause` class (`TearOff`, `TearOffWhereParameter`, per-column `WhereParameter` properties, `WhereClauseReset()`) exposed by `Where`; `AggregateClause` class (same pattern, `AggregateParameter`, `AggregateClauseReset()`) exposed by `Aggregate`.
- `GetInsertCommand/GetUpdateCommand/GetDeleteCommand`: `SqlCommand` of type StoredProcedure. `CreateParameters(cmd)` adds every column parameter with `SourceColumn=ColumnNames.X`, `SourceVersion=Current`; delete adds PK parameters only. `IsOutParameter(col, props, isInsert)`: insert+IsAutoKey -> Output; IsComputed -> Output (update: SQL `timestamp` becomes InputOutput). User-property keys `SEQ:I:`, `GEN:I:`, `GEN:I:T:`, `SEQ:U:`, `GEN:U:`, `GEN:U:T:` mark sequences/generators for non-SQL-Server DBs.
- Proc name format: `"[" + SchemaStoredProcedure + "proc_<Table><Suffix>]"` - this must match `SQL_StoredProcs.vbgen`.

Sample generated output: `doodads/CSharp/dOOdad_Demo/Generated/_Employees.cs` and `_Products.cs` (`_Employees : SqlClientEntity`, `QuerySource="Employees"`).

## Demo and tests
- `dOOdad_Demo/TheMasterSample.cs`: SimpleLoad, MoreComplexLoad, TheDeluxeQuery (AddResultColumn, AddOrderBy, Distinct, Top), GenerateSql, DataReader, Iteration, FilterAndSort, DemonstrateBulkUpdates, Transactions (Begin/Commit/Rollback, `ThreadTransactionMgrReset`, ASP.NET per-request reset advice), FillComboBox, AddColumn, Serialize, DataSetSerialize, AggregateTest (Avg, CountAll, Count, tearoff for two aggregates on one column, group-by).
- `doodads/Tests/{CSharp,VBNet}/<DB>`; SQL fixtures (NUnit, TestFixtureSetUp/SetUp/Test): `SqlAggregateFixture`, `SqlEOFFixture` (9 tests), `SqlGroupByFixture` (9), `SqlViewAggregateFixture` (~13), `SqlViewGroupByFixture` (10), `SqlXmlFixture` (2: SerializeDeserialize, ToXmlFromXml). `SqlBase.cs` `UnitTestBase.RefreshDatabase()` seeds `AggregateTest` via `s_` properties with `ConnectionStringConfig="SQLConnection"`; scripts `AggregateDbCreate.sql`, `AggregateDbView.sql`, `AggregateTestData.sql`; generated `_AggregateTest.cs`, `_FullNameView.cs`. Table `AggregateTest`: ID INT IDENTITY PK, DepartmentID, FirstName VARCHAR(25), LastName VARCHAR(15), Age, HireDate DATETIME, Salary NUMERIC(8,4), IsActive BIT.
- `dOOdad_UnitTesting/{CSharp,VBNet}/<DB>/`: generated Employees/_Employees samples per DB.

## Related pages
- [overview.md](overview.md)
- [template-catalog.md](template-catalog.md)
- [scriban-port-notes.md](scriban-port-notes.md)
- [settings-files.md](settings-files.md)
- [INDEX.md](INDEX.md)
