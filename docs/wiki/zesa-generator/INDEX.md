---
title: ZESA generator wiki index
summary: Entry point to source-backed documentation of the legacy MyGeneration ZESA business-logic generator.
keywords: [ZESA, generator, index, MyGeneration]
source_paths: [MyGenerationTemplates/]
---

# ZESA generator wiki

This wiki describes the implementation in `MyGenerationTemplates/` to help future agents understand and reimplement it. Claims are grounded in the cited repository files; where behavior depends on the MyGeneration/Zeus runtime, that dependency is called out rather than inferred.

## Pages

- [Overview](overview.md) — Components, terminology, and boundaries of the generator.
- [Generator pipeline](generator-pipeline.md) — UI inputs through metadata objects, templates, and generated files.
- [Data-access template group](data-access-templates.md) — Server DAO, verified DAO, managers, and data-transfer/helper templates.
- [Configuration and utility templates](configuration-templates.md) — Generated XML configs and table-valued parameter checks.
- [Remoting, client, and project templates](remoting-client-templates.md) — Interface/service classes, client controls, and project manifests.
- [Table-valued parameters](table-valued-parameters.md) — Discovery, generated validation, and SQL Server table types.
- [Stammdatenuebernahme](stammdatenuebernahme.md) — Release-change master-data transfer generation and dependency behavior.
- [Generator UI](generator-ui.md) — Windows Forms selection UI, metadata discovery, and helper types.
- [Metadata XML schema](metadata-xml-schema.md) — `MetaData.xsd`, persisted object rows, and UI settings XML.
- [TFS and auto-update](tfs-and-autoupdate.md) — Check-out/check-in/project edits and DLL update flow.
- [Config migration tools](config-migration.md) — Historical XML/project-file conversion utilities and their limits.
- [.NET 10 port notes](dotnet10-port-notes.md) — Evidence-based mapping to a SQL Server-only C# console/TUI, Scriban, and JSON.

## Navigation guidance

Start with [overview](overview.md), then follow the [pipeline](generator-pipeline.md). For an implementation rewrite, use the relevant template group pages together with [metadata schema](metadata-xml-schema.md) and [port notes](dotnet10-port-notes.md). The files under `MyGenerationTemplates/_MyGeneration13_2025/` are a bundled MyGeneration runtime/deployment snapshot, not the primary template source.

## Related pages

- [Overview](overview.md)
- [Generator pipeline](generator-pipeline.md)
- [.NET 10 port notes](dotnet10-port-notes.md)
