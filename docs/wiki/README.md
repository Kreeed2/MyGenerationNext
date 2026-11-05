---
title: MyGeneration legacy code wiki (master index)
summary: Entry point and table of contents for agents; links to every feature page and a task-to-page routing table.
keywords: [index, toc, mygeneration, mymeta, zeus, templates, migration, .net 10, mssql]
source_paths: [mygeneration-legacy-code, MyGenerationTemplates]
---

# MyGeneration legacy code wiki

Explains how the legacy .NET Framework MyGeneration solution works, so it can be rewritten as a .NET 10 console/TUI app for **MS SQL Server only**, **C# output**, **Scriban** templates and **JSON** configuration.

## How agents should use this wiki

1. Read this file, pick a section from the routing table or TOC.
2. Open that section's `INDEX.md` for one-line summaries, then open the page.
3. Every page has YAML front matter (`title`, `summary`, `keywords`, `source_paths`) and ends with `Related pages`. `source_paths` point at the real code to verify against.
4. `docs/wiki/manifest.json` is the machine-readable form of this TOC.

Pages that propose a design (`*-port-notes`, `*-mapping`, `discard-vs-keep`, `tui-mapping`) are recommendations, not legacy facts.

## Three pillars

| Pillar | What it is | Section |
|---|---|---|
| MyMeta | Database metadata abstraction with per-RDBMS providers | [mymeta](mymeta/INDEX.md) |
| Zeus | Template engine and script host (VBScript/JScript/C#/VB.NET) | [zeus](zeus/INDEX.md) |
| WinForms UI | MDI shell, editor, browser, plugins | [ui-and-tooling](ui-and-tooling/INDEX.md) |

Plus the shipped content: [stock-templates](stock-templates/INDEX.md) (stock templates, dOOdads runtime, settings XML) and [zesa-generator](zesa-generator/INDEX.md) (custom company business-logic generator under `MyGenerationTemplates/`).

## Task routing

| I need to... | Read |
|---|---|
| Replace MyMeta with SQL Server queries | [mymeta/sql-server-provider](mymeta/sql-server-provider.md), [mymeta/object-model](mymeta/object-model.md), [mymeta/discard-vs-keep](mymeta/discard-vs-keep.md) |
| Fix SQL-to-C# type mapping | [mymeta/language-and-dbtarget-mappings](mymeta/language-and-dbtarget-mappings.md), [stock-templates/settings-files](stock-templates/settings-files.md) |
| Understand template syntax | [zeus/template-file-format](zeus/template-file-format.md), [zeus/template-parsing](zeus/template-parsing.md) |
| Trace how a template runs | [zeus/execution-flow](zeus/execution-flow.md), [zeus/context-and-intrinsic-objects](zeus/context-and-intrinsic-objects.md) |
| Port templates to Scriban | [zeus/scriban-dotnet10-mapping](zeus/scriban-dotnet10-mapping.md), [stock-templates/scriban-port-notes](stock-templates/scriban-port-notes.md), [zesa-generator/dotnet10-port-notes](zesa-generator/dotnet10-port-notes.md) |
| Understand output file writing | [zeus/output-and-file-saving](zeus/output-and-file-saving.md) |
| Design the JSON config | [zeus/saved-input-and-projects](zeus/saved-input-and-projects.md), [ui-and-tooling/connection-and-settings](ui-and-tooling/connection-and-settings.md), [zesa-generator/metadata-xml-schema](zesa-generator/metadata-xml-schema.md) |
| Design the Terminal.Gui UI | [ui-and-tooling/tui-mapping](ui-and-tooling/tui-mapping.md), [zeus/gui-controls](zeus/gui-controls.md) |
| Understand a stock template | [stock-templates/template-catalog](stock-templates/template-catalog.md) |
| Understand generated DAO/entity code | [stock-templates/doodads-architecture](stock-templates/doodads-architecture.md), [zesa-generator/data-access-templates](zesa-generator/data-access-templates.md) |
| Understand the ZESA generator end to end | [zesa-generator/overview](zesa-generator/overview.md), [zesa-generator/generator-pipeline](zesa-generator/generator-pipeline.md) |
| Handle table-valued parameters | [zesa-generator/table-valued-parameters](zesa-generator/table-valued-parameters.md) |
| Know what to discard | [ui-and-tooling/tui-mapping](ui-and-tooling/tui-mapping.md), [mymeta/discard-vs-keep](mymeta/discard-vs-keep.md), [zeus/scriban-dotnet10-mapping](zeus/scriban-dotnet10-mapping.md) |

## Table of contents

Each section index lists all of its pages with summaries.

- **[mymeta](mymeta/INDEX.md)**: overview, object-model, plugin-system, sql-server-provider, user-metadata, language-and-dbtarget-mappings, data-binding-and-lazy-loading, other-providers-skim, discard-vs-keep
- **[zeus](zeus/INDEX.md)**: template-file-format, template-parsing, script-engines, context-and-intrinsic-objects, output-and-file-saving, gui-controls, saved-input-and-projects, zeuscmd, engine-interface, execution-flow, unit-tests, scriban-dotnet10-mapping
- **[ui-and-tooling](ui-and-tooling/INDEX.md)**: overview, main-application, template-browser-and-editor, connection-and-settings, settings-persistence, plugins, scintilla-editor, utilities, ide-plugins-installer, tui-mapping
- **[stock-templates](stock-templates/INDEX.md)**: overview, template-catalog, doodads-architecture, tutorials, settings-files, zeus-api, additional-files-and-docs, scriban-port-notes
- **[zesa-generator](zesa-generator/INDEX.md)**: overview, generator-pipeline, generator-ui, metadata-xml-schema, data-access-templates, configuration-templates, remoting-client-templates, stammdatenuebernahme, table-valued-parameters, tfs-and-autoupdate, config-migration, dotnet10-port-notes

## Known legacy gaps (verified by authors)

- SQL Server provider mostly uses OLE DB schema rowsets, not direct `sys.*` queries; only some extras are raw T-SQL.
- No mapping for `date`, `time`, `datetime2`, `datetimeoffset`, `geography`, `geometry`, `hierarchyid`, `sysname` (they resolve to "Unknown").
- `.zeus/.csgen/.vbgen/.jgen` extensions are UI filter conventions only; the parser does not interpret them.
- `UnitTests` project contains only empty stubs.
- Some `Zeus/` files and `Zeus/DotNetScript/` are excluded from the csproj (dead code).

## Suggested rewrite order

1. JSON config models and loader ([ui-and-tooling/connection-and-settings](ui-and-tooling/connection-and-settings.md))
2. SQL Server schema reader ([mymeta/sql-server-provider](mymeta/sql-server-provider.md))
3. Scriban rendering and file output ([zeus/scriban-dotnet10-mapping](zeus/scriban-dotnet10-mapping.md), [zeus/output-and-file-saving](zeus/output-and-file-saving.md))
4. Terminal.Gui front end ([ui-and-tooling/tui-mapping](ui-and-tooling/tui-mapping.md))
