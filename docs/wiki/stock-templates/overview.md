---
title: Stock Templates and dOOdads - Overview
summary: Orientation to the legacy MyGeneration repository layout, the Zeus template file format, intrinsic objects, and the three-step dOOdads generation workflow.
keywords: [overview, zeus, template-format, vbgen, csgen, intrinsic-objects, ZeusOutput, workflow, folder-map]
source_paths: [mygeneration-legacy-code/templates, mygeneration-legacy-code/doodads, mygeneration-legacy-code/additionalfiles, mygeneration-legacy-code/documentation, MyGenerationTemplates/_MyGeneration13_2025]
---

# Overview

MyGeneration is a legacy .NET Framework code generator. A user connects to a database (MyMeta reads the schema), picks a *template* (Zeus script), answers the template's GUI prompts, and the template writes text files through the `output` object. The "stock" content is the set of templates plus the **dOOdads** runtime library (`BusinessEntity` etc.) that the main C# templates generate code against.

The rewrite target is a .NET 10 console/TUI app, SQL Server only, C# output, Scriban templates. See [scriban-port-notes.md](scriban-port-notes.md).

## Folder map

| Path | Contents |
|---|---|
| `mygeneration-legacy-code/templates/` | Source-of-truth stock templates. Subfolders: `C#`, `VB.Net`, `Microsoft_SQL_Server`, `MySQL`, `Oracle`, `Firebird`, `SQLite`, `VistaDB`, `Microsoft_Access`, `PostgreSQL`, `IBM_ISeries`, `PHP`, `HTML`, `Other`, `Samples`, `Tutorials`, `UnitTestTemplates`, `Opf3/C#`, `Gentle.NET`. |
| `mygeneration-legacy-code/doodads/` | `CSharp/MyGeneration.dOOdads` (runtime), `CSharp/dOOdad_Demo`, `CSharp/Templates` (copies of entity templates), `VB.NET/` (mirror), `Tests/{CSharp,VBNet}/<DB>`, `dOOdad_UnitTesting/{CSharp,VBNet}/<DB>` (generated samples). |
| `mygeneration-legacy-code/additionalfiles/` | Sample databases and type-test scripts (see [additional-files-and-docs.md](additional-files-and-docs.md)). |
| `mygeneration-legacy-code/documentation/` | Sandcastle/HelpNDoc help sources. |
| `MyGenerationTemplates/_MyGeneration13_2025/` | An installed snapshot of v1.3.0.3 (changelog last entry 2007-12-16): `Templates/`, `Architectures/dOOdads/{CSharp,VB.NET}`, `Settings/`, binaries (MyMeta.dll, Zeus.dll, ZeusCmd.exe, chm help). |

### Legacy vs the 13_2025 snapshot
- `_MyGeneration13_2025/Templates` is the legacy template tree minus `AllDb_BusinessObject`, the Firebird dialect templates, `Opf3`, `UnitTestTemplates`, `WinformDemo`, `Linq.zeus`; it adds `Xsd3b/` (ToXsd.csgen, ToXsd3b.csgen, Modelfile2XSD3b.csgen, xsl, xmi samples) and `GentleNET/`.
- `Architectures/dOOdads/CSharp/MyGeneration.dOOdads` is identical to `doodads/CSharp/MyGeneration.dOOdads` except AssemblyInfo; the legacy copy also has `VisualFoxPro*` adapters, an `.ndoc` file and a 2008 csproj.
- The snapshot holds custom, **non-stock** company files (`BLGeneratorUIMeta.xml`, `BLGeneratorUISettings.xml`, `ZESABLGeneratorUI.dll`). Ignore them for the port (see [settings-files.md](settings-files.md)).

## Template file format
Extensions encode the body language: `.vbgen` = VBScript, `.jgen` = JScript, `.csgen` = C# (.Net Script), `.zeus` = generic. A file is a header + GUI section + body section:

```
##|TYPE Template            (or Group)
##|UNIQUEID <guid>
##|TITLE ...
##|NAMESPACE dOOdads.C#
##|OUTPUT_LANGUAGE C#
##|COMMENTS_BEGIN / COMMENTS_END
##|GUI_ENGINE Microsoft Script | .Net Script
##|GUI_LANGUAGE VBScript ...
##|GUI_BEGIN / GUI_END
##|BODY_MODE Markup
##|BODY_ENGINE ...  BODY_LANGUAGE ...
##|BODY_TAG_START <%   BODY_TAG_END %>
##|BODY_BEGIN / BODY_END
```
Body is "markup": literal text is emitted as-is, `<% ... %>` is script, `<%= expr %>` emits a value. `<%#SCRIPT sql_library.js%>` includes a shared script; `<%#REFERENCE ...%>` / `<%#NAMESPACE ...%>` appear in .Net Script templates.

## Intrinsic objects (Settings/ZeusConfig.xml)
- `ui` - `Zeus.UserInterface.GuiController` (GUI building).
- `MyMeta` - `MyMeta.dbRoot` (schema metadata).
- `DnpUtils` - `Dnp.Utils.Utils` (`ReadInputFromCache`/`SaveInputToCache` persist GUI inputs between runs).
- `input` (`IZeusInput`), `output` (`ZeusOutput`), `context`.

See [zeus-api.md](zeus-api.md) for the API details.

## Generation workflow with dOOdads (README.TXT)
1. Generate stored procedures (`Microsoft_SQL_Server/SQL_StoredProcs.vbgen`) and run them in the DB.
2. Generate `_<Table>` abstract entity (`C#/CSharp_SQL_dOOdads_BusinessEntity.vbgen`), always overwritten; never edit.
3. Generate concrete `<Table> : _<Table>` once (`CSharp_SQL_dOOdads_ConcreteClass.vbgen`), saved with mode `"d"` so it is never overwritten; user code lives here.
Views use `CSharp_SQL_dOOdads_View.vbgen`; existing procs are wrapped by `CSharp_dOOdads_StoredProc.vbgen`.

## Related pages
- [INDEX.md](INDEX.md)
- [template-catalog.md](template-catalog.md)
- [doodads-architecture.md](doodads-architecture.md)
- [zeus-api.md](zeus-api.md)
- [tutorials.md](tutorials.md)
- [settings-files.md](settings-files.md)
- [scriban-port-notes.md](scriban-port-notes.md)
- [additional-files-and-docs.md](additional-files-and-docs.md)
