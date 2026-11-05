---
title: MyMeta object model
summary: Every MyMeta entity (Database, Table, View, Column, ForeignKey, Index, Procedure, Parameter, ResultColumn, Domain, ProviderType) with its navigation and key properties.
keywords: [Database, Table, View, Column, ForeignKey, Index, Procedure, Parameter, ResultColumn, Domain, ProviderType, properties]
source_paths:
  - mygeneration-legacy-code/mymeta/Database.cs
  - mygeneration-legacy-code/mymeta/Table.cs
  - mygeneration-legacy-code/mymeta/View.cs
  - mygeneration-legacy-code/mymeta/Column.cs
  - mygeneration-legacy-code/mymeta/ForeignKey.cs
  - mygeneration-legacy-code/mymeta/ForeignKeys.cs
  - mygeneration-legacy-code/mymeta/Index.cs
  - mygeneration-legacy-code/mymeta/Indexes.cs
  - mygeneration-legacy-code/mymeta/Procedure.cs
  - mygeneration-legacy-code/mymeta/Parameter.cs
  - mygeneration-legacy-code/mymeta/ResultColumn.cs
  - mygeneration-legacy-code/mymeta/Domain.cs
  - mygeneration-legacy-code/mymeta/ProviderType.cs
  - mygeneration-legacy-code/mymeta/Interfaces/
---

# MyMeta object model

Public contracts are in `mymeta/Interfaces/I*.cs` (`IDatabase`, `ITabularEntity`, `ITable`, `IView`, `IColumn`, `IProcedure`, `IParameter`, `IResultColumn`, `IDomain`, `IForeignKey`, `IIndex`, `IProviderType`, plus `I*s` collections, `IPropertyCollection`, `IProperty`, `IClassFactory`). Implementations are the same-named classes in `mymeta/`. All paths below are relative to `mygeneration-legacy-code/mymeta/`.

## Hierarchy

```
dbRoot
 ├─ Databases -> Database
 │    ├─ Tables  -> Table
 │    │     ├─ Columns -> Column         (LoadForTable)
 │    │     ├─ PrimaryKeys -> Columns    (virtual; null in base, provider overrides)
 │    │     ├─ ForeignKeys -> ForeignKey (LoadAll)  [PrimaryColumns/ForeignColumns]
 │    │     ├─ IndirectForeignKeys       (LoadAllIndirect)
 │    │     └─ Indexes -> Index          (LoadAll)   [Columns]
 │    ├─ Views -> View
 │    │     ├─ Columns (LoadForView), SubTables, SubViews
 │    ├─ Procedures -> Procedure
 │    │     ├─ Parameters -> Parameter
 │    │     └─ ResultColumns -> ResultColumn
 │    └─ Domains -> Domain
 └─ ProviderTypes -> ProviderType
```
`Database.Tables/Views/Procedures/Domains` each do: `ClassFactory.CreateX()`, set `dbRoot` and `Database`, call `LoadAll()`, cache. Collections implement indexer by `string` (physical name via `GetByPhysicalName`, case per `dbRoot.IgnoreCase`) and by `int`, plus enumeration.

## Common to most entities
`Alias` (get/set; user-metadata `n` attribute else `Name`), `Name` (physical, from the schema rowset), `Properties` / `GlobalProperties` / `AllProperties` (see [user-metadata.md](user-metadata.md)), `UserDataXPath`, `DatabaseSpecificMetaData(key)` (SQL Server: `"ExtendedProperties"`), `Database`/parent back-reference. `ITabularEntity` (Table and View share): `Alias, Name, Schema, Database, Columns, Properties, GlobalProperties, AllProperties, UserDataXPath, Type, Guid, Description, PropID, DateCreated, DateModified`.

## Entities and key properties

**Database** (`Database.cs`): `Name` (`CATALOG_NAME`), `Description`, `SchemaName`, `SchemaOwner`, `DefaultCharSetCatalog/Schema/Name`, `Tables`, `Views`, `Procedures`, `Domains`, `Root`; methods `ExecuteSql(sql)` (returns ADODB `Recordset`), `ResultColumnsFromSQL(sql)`, `GetNativeType(...)`. Schema info comes from `Databases.PopulateSchemaData` (OLE DB `Schemata`).

**Table** (`Table.cs`): `Name, Schema, Type, Guid, Description, PropID, DateCreated, DateModified, Columns, PrimaryKeys, ForeignKeys, IndirectForeignKeys, Indexes`. `Equals` compares `dbRoot` and `_row`.

**View** (`View.cs`): same tabular props plus `ViewText`, `CheckOption`, `IsUpdateable`, `SubTables`, `SubViews` (created empty; filled by provider `LoadSubViewInfo`).

**Column** (`Column.cs`):
- Identity/position: `Name, Guid, PropID, Ordinal`.
- Default/null: `HasDefault, Default, Flags, IsNullable`.
- Type: `DataType` (int OLE DB type), `TypeGuid`, `DataTypeName`, `DataTypeNameComplete`, `CharacterMaxLength, CharacterOctetLength, NumericPrecision, NumericScale, DateTimePrecision`.
- Charset/domain: `CharacterSetCatalog/Schema/Name`, `DomainCatalog/Schema/Name`, `HasDomain` (`DOMAIN_NAME` column non-null), `Domain` (looks up `Columns.GetDatabase().Domains[DomainName]`).
- Collation: `LCID, CompFlags, SortID, TDSCollation` (always null).
- Flags: `IsComputed, IsInPrimaryKey` (name match against `Table.PrimaryKeys`), `IsAutoKey, AutoKeySeed, AutoKeyIncrement`, `IsInForeignKey, ForeignKeys` (populated by `ForeignKey.AddForeignColumn` -> `Column.AddForeignKey` on a clone; empty static `_emptyForeignKeys` otherwise).
- Mapped types: `LanguageType`, `DbTargetType` (see [language-and-dbtarget-mappings.md](language-and-dbtarget-mappings.md)).
- Navigation: `Table`, `View`, `Description`.

**ForeignKey** (`ForeignKey.cs`): `Name` (`FK_NAME`), `Ordinal`, `UpdateRule`, `DeleteRule`, `PrimaryKeyName`, `Deferrability` (1 INITIALLY_DEFERRED, 2 INITIALLY_IMMEDIATE, 3 NOT_DEFERRABLE, else UNKNOWN), `PrimaryTable`, `ForeignTable` (resolved via `dbRoot.Databases[catalogOrSchema].Tables[name]`), `PrimaryColumns`, `ForeignColumns`. `ForeignKeys.PopulateArray` groups rows by FK name (`GetByName`) and appends `PK_COLUMN_NAME`/`FK_COLUMN_NAME` per row (composite keys); per-row exceptions swallowed.

**Index** (`Index.cs`): `Name, Schema, Unique, Clustered, FillFactor, InitialSize, SortBookmarks, AutoUpdate, Cardinality, Pages, FilterCondition, Integrated`, `Type` (1 BTREE, 2 HASH, 3 CONTENT, else OTHER), `NullCollation` (1 END, 2 HIGH, 4 LOW, 8 START, else UNKNOWN), `Collation` (1 ASCENDING, 2 "DECENDING" [sic], else UNKNOWN), `Columns` (built by `Indexes.PopulateArray` grouping on `INDEX_NAME` + `Index.AddColumn(COLUMN_NAME)`, which looks the column up in `Indexes.Table.Columns`).

**Procedure** (`Procedure.cs`): `Name, Schema, Type` (Int16), `ProcedureText, Description, DateCreated, DateModified, Parameters, ResultColumns`.

**Parameter** (`Parameter.cs`): `Name, Ordinal, ParameterType, Direction` (`ParamDirection`: Unknown, Input=1, InputOutput=2, Output=3, ReturnValue=4), `HasDefault, Default` ("<null>" if DBNull), `IsNullable, DataType, CharacterMaxLength, CharacterOctetLength, NumericPrecision, NumericScale, Description, TypeName, LocalTypeName, DataTypeNameComplete, LanguageType, DbTargetType`. LanguageType uses `TypeName` and does **not** apply domain override.

**ResultColumn** (`ResultColumn.cs`): `Name, DataType, DataTypeName, DataTypeNameComplete, Ordinal, LanguageType, DbTargetType`.

**Domain** (`Domain.cs`): `Name, HasDefault, Default, IsNullable, CharacterMaxLength, CharacterOctetLength, NumericPrecision, NumericScale, DateTimePrecision, CharacterSetCatalog/Schema/Name, DomainCatalog/Schema/Name, DataTypeName` (from `DATA_TYPE`), `DataTypeNameComplete` (base returns "Unknown"; `SqlDomain` overrides), `LanguageType, DbTargetType`.

**ProviderType** (`ProviderType.cs`, from OLE DB `Provider_Types`): `Type, DataType, ColumnSize, LiteralPrefix, LiteralSuffix, CreateParams, IsNullable, IsCaseSensitive, Searchable, IsUnsigned, HasFixedPrecScale, CanBeAutoIncrement, LocalType, MinimumScale, MaximumScale, TypeGuid, TypeLib, Version, IsLong, BestMatch, IsFixedLength`. Quirk: `ProviderTypes.BindToColumns` tests `Contains("COLUMN_SIZE ")` etc. with trailing spaces (verify in `ProviderTypes.cs` before relying on it). Used only by `Database.GetNativeType`.

## Related pages

- [INDEX.md](INDEX.md)
- [overview.md](overview.md)
- [data-binding-and-lazy-loading.md](data-binding-and-lazy-loading.md)
- [sql-server-provider.md](sql-server-provider.md)
- [user-metadata.md](user-metadata.md)
- [discard-vs-keep.md](discard-vs-keep.md)
