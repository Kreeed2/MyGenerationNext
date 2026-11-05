---
title: Data binding and lazy loading
summary: How MyMeta base classes (MetaObject, Single, Collection) bind DataTable columns to entity properties and lazily load children through the ClassFactory.
keywords: [Single, Collection, MetaObject, DataRow, DataColumn, BindToColumns, PopulateArray, LoadData, lazy]
source_paths:
  - mygeneration-legacy-code/mymeta/MetaObject.cs
  - mygeneration-legacy-code/mymeta/Single.cs
  - mygeneration-legacy-code/mymeta/Collection.cs
  - mygeneration-legacy-code/mymeta/Utility.cs
  - mygeneration-legacy-code/mymeta/Tables.cs
  - mygeneration-legacy-code/mymeta/Databases.cs
---

# Data binding and lazy loading

Paths relative to `mygeneration-legacy-code/mymeta/`.

## Base classes
- `MetaObject`: holds `dbRoot`. `LoadData(Guid oleDbSchemaGuid, object[] filter)` calls `dbRoot.TheConnection.GetOleDbSchemaTable(...)`; on any exception it returns an **empty DataTable** (errors hidden). Virtual `UserDataXPath`, `GlobalUserDataXPath`, `GetXmlNode(out XmlNode, bool forceCreate)`, `CreateUserMetaData(XmlNode)`, plus `GetUserData/SetUserData` helpers (see [user-metadata.md](user-metadata.md)).
- `Single : MetaObject`: one entity. Virtual `Alias`, `Name`. Field `DataRow _row`. Typed getters taking a `DataColumn`: `GetString` (strips trailing `\0` when `dbRoot.StripTrailingNulls`), `GetGuid`, `GetDateTime` (default `new DateTime(1,1,1,1,1,1,1)`), `GetBool`, `GetInt16`, `GetInt32`, `GetDecimal`; `GetByteArray` always returns null. A null `DataColumn` (column not in rowset) yields the default. `Properties` lazy `PropertyCollection`. `DatabaseSpecificMetaData(key)` returns null by default.
- `Collection : MetaObject`: backed by `ArrayList _array`; `CompareStrings` honours `dbRoot.IgnoreCase`; indexers by name/int; contains dummy `IList`-style stubs (not meaningful).

## Binding pattern
Each collection owns `f_*` `DataColumn` fields (e.g. `Columns.f_TypeName`). `BindToColumns(DataTable)` sets a field only if `metaData.Columns.Contains("NAME")`; thus provider rowsets that lack a column simply yield defaults. `PopulateArray(DataTable)` loops rows, calls `dbRoot.ClassFactory.CreateX()`, sets `dbRoot`, parent (`Tables`, `Columns`...) and `Row`, and adds to `_array`. Providers add synthetic columns to the DataTable before binding (e.g. SQL Server adds `TYPE_NAME`, `AUTO_INCREMENT`, `DESCRIPTION`; see [sql-server-provider.md](sql-server-provider.md)).

## Loading
Parent property getter -> `ClassFactory.CreateX()` -> assign parents -> provider-specific `LoadAll()/LoadForTable()/LoadForView()` -> cached. `Utility.GetDatabaseName(catalog, schema)` returns catalog if non-empty else schema (used to resolve FK target databases). Per-`Database` caches exist for global user properties (`_tableProperties`, `_columnProperties`, ...) and `dataTypeTables`.

## Related pages
- [INDEX.md](INDEX.md)
- [overview.md](overview.md)
- [object-model.md](object-model.md)
- [sql-server-provider.md](sql-server-provider.md)
- [discard-vs-keep.md](discard-vs-keep.md)
