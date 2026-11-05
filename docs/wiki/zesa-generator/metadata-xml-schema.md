---
title: Metadata XML schema
summary: Describes the typed DataSet schema and the persisted BLGeneratorUIMeta.xml and BLGeneratorUISettings.xml roles.
keywords: [MetaData.xsd, BLGeneratorUIMeta.xml, XML, DataSet, settings]
source_paths: [MyGenerationTemplates/BusinesslogicGeneratorUI/MetaData.xsd, MyGenerationTemplates/BusinesslogicGeneratorUI/MetaData.Designer.cs, MyGenerationTemplates/BusinesslogicGeneratorUI/BLGeneratorUI.cs, MyGenerationTemplates/BLGeneratorMetaData/BLGeneratorUIMeta.xml, MyGenerationTemplates/BLGeneratorMetaData/MemberUseOldDataSetLogic.xml, MyGenerationTemplates/_MyGeneration13_2025/BLGeneratorUISettings.xml]
---

# Metadata XML schema

## `MetaData.xsd` and generated DataSet

`BusinesslogicGeneratorUI/MetaData.xsd` defines a typed dataset named `MetaData` in namespace `http://www.tempuri.org/MetaData.xsd` with `msdata:Locale="de-DE"`. The `MetaData` root may contain repeated `MetaData` rows and `PathInfo` rows. `MetaData.Designer.cs` is the generated typed DataSet implementation; do not edit it as if it were the source schema.

### `PathInfo`

- `TargetPath` (string, optional): selected generated-source root. `BLGeneratorUI.SaveLocalMetadata()` adds/updates the first row when the path field is non-empty.

### `MetaData` object row

| Field | Type/occurrence | Meaning in code |
|---|---|---|
| `ObjectName` | string, required | Database object identifier; used to match persisted settings and look up MyMeta objects. |
| `Namespace` | string, required | ZESA namespace/category assigned to the object. |
| `LOV` | boolean, required | Whether the object is used as a list of values. |
| `Generate` | boolean, required | Include object in generation. |
| `Type` | string, optional | `Table`, `View`, or `Function` in UI code. |
| `EntityName` | string, optional | Explicit entity naming override used by templates. |
| `LovColumns` | string, optional | Comma-separated columns selected for LOV output. |
| `NORMTIMEFROM` | boolean, optional | Generate from-date normalization/range behavior. |
| `NormTimeFromColumns` | string, optional | Comma-separated affected columns. |
| `NORMTIMETO` | boolean, optional | Generate to-date normalization/range behavior. |
| `NormTimeToColumns` | string, optional | Comma-separated affected columns. |
| `Stammdatenuebernahme` | boolean, required | Master-data transfer selection. |
| `AliasKennungColumn` | string, optional | Identifier column used in transfer; UI fills `KENNUNG` when empty for selected generated rows in the transfer workflow. |
| `DbReaderDataTypeColumns` | string, optional | Per-column DbReader type override data, parsed by `GeneratorHelper.GetDbReaderDatatypeColumns`. |
| `DbReaderOperatorColumns` | string, optional | Per-column DbReader operator overrides. |
| `PrimaryKeyIntegerColumnNames` | string, optional | Comma-separated integer primary/foreign-key column names. |
| `IsPrimaryKeyInteger` | boolean, optional | Integer-primary-key marker used by generation. |

The XSD makes the root and fields explicit, but the source does not define a separate normalized grammar for comma-separated override strings; consumers parse them in helper/template code. Treat their exact syntax as a legacy contract to inspect, not as arbitrary CSV.

## `BLGeneratorUIMeta.xml`

`MyGenerationTemplates/BLGeneratorMetaData/BLGeneratorUIMeta.xml` is an instance of the dataset. Its first `PathInfo` row stores a target path; subsequent `MetaData` rows preserve individual UI choices. For example, rows include database object, namespace, `Generate`, type and entity, data-transfer selection, alias field, and integer-key field values. It includes a real namespace-qualified XML root. `BLGeneratorUI.LoadSavedLocalMetadata()` reads this file ignoring schema, merges saved rows with the current catalog, and `SaveLocalMetadata()` rewrites it ignoring schema.

Do not treat this file as only a schema sample: it contains object-specific generator state. Do not assume it should be copied unchanged into a new app/database environment. The main UI source hardcodes its absolute file path.

## Other XML files with similar names

- `BLGeneratorUISettings.xml` serializes `BLGeneratorSettings`, which contains TFS workspace/server and master-data required/excluded entity lists. It is distinct from the DataSet metadata XML. `GeneratorHelper.DeserializeSettings()` uses `XmlSerializer`; missing settings get defaults and can be created.
- `MemberUseOldDataSetLogic.xml` is read by `GeneratorHelper.GetMemberWhichUseOldDataSetLogic()` to determine legacy DataSet behavior. It is not part of the DataSet schema.
- `_MyGeneration13_2025/Settings/*.xml` configure MyGeneration itself (e.g. database connection, language and target mappings); these are bundled host configuration, not ZESA object metadata.

## Related pages

- [Generator UI](generator-ui.md)
- [Generator pipeline](generator-pipeline.md)
- [TFS and auto-update](tfs-and-autoupdate.md)
- [Config migration tools](config-migration.md)
- [.NET 10 port notes](dotnet10-port-notes.md)
