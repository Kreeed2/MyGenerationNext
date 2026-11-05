---
title: Data-access template group
summary: Documents generated DAO, verified DAO, manager, and shared parameter/column-definition classes.
keywords: [DAO, server, verified, manager, C-sharp, MyMeta]
source_paths: [MyGenerationTemplates/ZESAServerDAO.csgen, MyGenerationTemplates/ZESAVerifiedDAO.csgen, MyGenerationTemplates/ZESAManager.csgen, MyGenerationTemplates/ZESAParameter.csgen, MyGenerationTemplates/ZESAColDefs.csgen]
---

# Data-access template group

The templates receive selected objects as `input["Data"]`, target root as `input["TargetPath"]`, and, for the two DAO templates, `input["FunctionGenInfoData"]`. They iterate `GenInfo` records and obtain table/view columns using MyMeta (`MyMeta.DefaultDatabase.Tables[...]` / `Views[...]`). Table names are normalized through `GeneratorHelper.ConvertName(info.EntityName)`; `BuildEntityName` commonly strips a first prefix segment. Function support is special and limited to templates whose source explicitly handles `ObjectType == "Function"`.

## ZESAServerDAO

`ZESAServerDAO.csgen` handles table, view, and function object shapes, detects framework-significant columns (for example `VERSION_NUM`, `MANDANT`, visibility and deletion flags), emits server DAO methods and column constants, and writes one file per supported item under `TargetPath/Server/ZESAServer/DAO/{Namespace}/{Entity}.cs`. The namespace/class naming expression in the generator is `EHA.ZESA.Server.DAO.{Namespace}.{classname}`. It checks out the file and may associate it with `Server\ZESAServer\ZESAServer.csproj` through `GeneratorHelper.AddFileToProject(Project.Server, ...)` when applicable. It includes a marked preserve-source region and consumes function signatures for generated function access. Source: `ZESAServerDAO.csgen` (`Render`, `OutputAttributes`, validation and CRUD method emitters).

## ZESAVerifiedDAO

`ZESAVerifiedDAO.csgen` emits one client-side DAO class per selected table/view/function under `Communication/RemotingServiceInterface/DAO/{Namespace}/{Entity}/{Entity}DAO.cs` (generated class namespace starts `EHA.ZESA.Communication.RemotingServiceInterface.DAO...`). It emits constants from MyMeta columns and methods/parameters using function metadata where applicable. When generated source is missing from the remoting-interface project, it checks out and calls `GeneratorHelper.AddFileToProject(Project.RemotingServiceInterface, ...)`. It saves with `output.save(..., false)` and supports marked preservation regions. Source: `ZESAVerifiedDAO.csgen` (`Render`, `OutputAttributes`).

## ZESAManager

`ZESAManager.csgen` generates managers for **tables only**. It reads table columns, checks for `VERSION_NUM`, and writes `{Entity}Manager.cs` to `Server/ZESAServer/BusinessFacade/{Namespace}/`. The generated type is in `EHA.ZESA.Server.BusinessFacade.{Namespace}` and consumes the remoting interface DAO and client-context APIs. The source records the server project path and uses TFS checkout/preserve markers; unlike the DAO templates, do not assume project membership insertion unless implemented in the checked-out source version. Source: `ZESAManager.csgen` (`Render`, version-column handling).

## ZESAParameter and ZESAColDefs

- `ZESAParameter.csgen` emits `{Entity}Parameter.cs` under `Communication/RemotingServiceInterface/DAO/{Namespace}/{Entity}/`. It represents selected table/view columns as parameter members and uses column language types and generated naming. The template skips/does not build parameter classes for functions where its type handling says so.
- `ZESAColDefs.csgen` emits `{Entity}ColDefs.cs` in the same entity directory for old DataSet logic only (`info.UsesOldDataSetLogic`); it skips functions. The constants encode column/table names and legacy DataSet table identifiers. Whether a table uses this path is sourced from `MemberUseOldDataSetLogic.xml` via `GeneratorHelper.GetMemberWhichUseOldDataSetLogic()`.

Both obtain table/view columns via MyMeta, create directories, call `TfsHelper.CheckOut`, preserve user sections, save generated content, and log artifacts. These templates do not return a neutral schema: their generated APIs are coupled to legacy EHA.ZESA client/server framework types.

## Input and naming rules

The UI's `GenInfo` is the authoritative per-object input; this is not passed as JSON and is not just the raw database catalog. It includes explicit entity naming, namespace assignment, data type overrides, integer primary/foreign-key markers, and generation flags. `GeneratorHelper.ConvertName()` title-cases underscore-separated segments, removes `@p` fragments in a case-insensitive manner, and preserves underscores; `BuildEntityName()` removes the leading segment for tables/views but keeps all segments for functions. Verify exact edge-case behavior in that helper before translating it.

## Related pages

- [Generator pipeline](generator-pipeline.md)
- [Configuration and utility templates](configuration-templates.md)
- [Remoting, client, and project templates](remoting-client-templates.md)
- [Table-valued parameters](table-valued-parameters.md)
- [.NET 10 port notes](dotnet10-port-notes.md)
