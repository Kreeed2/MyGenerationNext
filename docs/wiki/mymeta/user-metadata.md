---
title: User metadata (aliases and properties XML)
summary: Format and behaviour of the user-metadata XML document that stores aliases and custom key/value properties for MyMeta entities.
keywords: [user metadata, UserData, alias, Properties, PropertyCollection, PropertyCollectionAll, XPath, global properties, MergeUserMetaDataFiles]
source_paths:
  - mygeneration-legacy-code/mymeta/dbRoot.cs
  - mygeneration-legacy-code/mymeta/MetaObject.cs
  - mygeneration-legacy-code/mymeta/Property.cs
  - mygeneration-legacy-code/mymeta/PropertyCollection.cs
  - mygeneration-legacy-code/mymeta/PropertyCollectionAll.cs
  - mygeneration-legacy-code/mymeta/KeyValueCollection.cs
  - mygeneration-legacy-code/mymeta/Database.cs
  - mygeneration-legacy-code/mygeneration/MyGeneration/Settings/DefaultSettings.xml
---

# User metadata

Paths relative to `mygeneration-legacy-code/mymeta/`. No sample `UserMetaData.xml` is in the repo; `DefaultSettings.xml` just references `.\Settings\UserMetaData.xml` (`usermetapath`).

## Document shape (derived from the XPath builders)
```xml
<MyMeta>
  <Databases>
    <Database p="Northwind" n="NW">          <!-- p = physical name key, n = alias -->
      <Tables>
        <Table p="Orders" n="Order">
          <Properties><Property k="Key" v="Value"/></Properties>
          <Columns><Column p="OrderID" n="Id"><Properties/></Column></Columns>
          <ForeignKeys/> <Indexes/>
        </Table>
      </Tables>
      <Views/> <Procedures><Procedure p n><Parameters/><ResultColumns/></Procedure></Procedures> <Domains/>
    </Database>
  </Databases>
  <Global>... same Databases/Database/<Entity>/Properties layout (global properties) ...</Global>
</MyMeta>
```
`dbRoot.UserDataXPath` = `//MyMeta`. `Reset()` creates an empty `<MyMeta>` document. Database key in XML may differ from the physical name via `dbRoot.UserDataDatabaseMappings` (`Database.XmlMetaDataKey`).

## API on dbRoot
- `UserMetaDataFileName` setter loads the file; load failure leaves an empty document. `SaveUserMetaData()` creates the directory and saves `UserData`.
- `MergeUserMetaDataFiles(f1, db1, f2, db2, fm)`: copies f1 to fm and merges `//MyMeta/Databases/Database[@p='db']` from f2 via `MergeXml`; node key from `GetXmlNodeKey` (`/Name[@p='..']` or `[@k='..']`); missing `n`/`v` attributes are copied and missing child nodes imported.

## Entity-level behaviour (`MetaObject`, `Single`)
- `Alias` getter: attribute `n` if non-empty, else `Name`. Setter: `GetXmlNode(out node, forceCreate:true)` then `SetUserData`. Nodes are created lazily by `CreateUserMetaData(parent)` with `p=<physical name>` and `n=""`.
- `UserDataXPath` / `GlobalUserDataXPath` virtuals build string-concatenated XPaths (names containing a quote would break them). `Domain.GlobalUserDataXPath` returns the same as `UserDataXPath` (copy/paste quirk). Global node: `//MyMeta/Global/Databases/Database[@p=..]/...` via `Database.CreateGlobalXmlNode`.
- Properties: `Properties` (local), `GlobalProperties` (cached in per-`Database` fields like `_tableProperties`), `AllProperties`.

## Property classes
- `PropertyCollection`: `SortedList` keyed by `k`; `AddKeyValue(key,value)` creates the XML node + `Property`; `RemoveKey`, `Clear`, `ContainsKey`, indexer; `LoadAll()` reads local `Properties`; `LoadAllGlobal(node)` creates the `Properties` node if missing, loads with `IsGlobal=true`.
- `PropertyCollectionAll`: merges local and global; local wins; its `AddKeyValue`, `RemoveKey`, `Clear` throw `NotImplementedException`.
- `Property`: `Key`, `Value` (setter writes `v` via `GetXmlNode`), `IsGlobal`.
- `KeyValueCollection`/`KeyValuePair`: in-memory `IPropertyCollection` used for SQL Server extended properties (not XML).

## Related pages
- [INDEX.md](INDEX.md)
- [object-model.md](object-model.md)
- [sql-server-provider.md](sql-server-provider.md)
- [discard-vs-keep.md](discard-vs-keep.md)
