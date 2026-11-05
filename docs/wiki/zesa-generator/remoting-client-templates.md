---
title: Remoting, client, and project templates
summary: Covers generated remoting contracts and services, client list controls, unit tests, and legacy project manifests.
keywords: [remoting, client, list control, unit tests, csproj]
source_paths: [MyGenerationTemplates/ZESARemotingServiceInterface.csgen, MyGenerationTemplates/ZESARemotingService.csgen, MyGenerationTemplates/ZESARemotingServiceProject.csgen, MyGenerationTemplates/ZESAComboBox.csgen, MyGenerationTemplates/ZESAListViews.csgen, MyGenerationTemplates/ZESAUnitTest.csgen, MyGenerationTemplates/ZESAUnitTestProject.csgen]
---

# Remoting, client, and project templates

All listed templates are separate `.csgen` files; the three remoting code templates are included by `BL.csgen` only for the contract and implementation classes, while the remoting project manifest is independent. Client controls and unit-test templates are not included in the group. Their generated code assumes the legacy EHA.ZESA namespaces/framework and .NET project layouts.

## Remoting contract and service

- **`ZESARemotingServiceInterface.csgen`** iterates `Data`, reads MyMeta table/view columns, and emits one class per selected object under `Communication/RemotingServiceInterface/{Namespace}/{Entity}/` in the namespace `EHA.ZESA.Communication.RemotingServiceInterface.{Namespace}.{Entity}`. The generated filename is `I{Entity}Service.cs`. It creates contract/transfer-facing methods and member declarations, checks out the file, preserves marked custom regions, saves, and updates `RemotingServiceInterface.csproj` through `GeneratorHelper.AddFileToProject` when appropriate.
- **`ZESARemotingService.csgen`** produces server service classes for selected objects under `Server/ZESAServer/RemotingService/{Namespace}/{Entity}.cs`, namespace `EHA.ZESA.Server.RemotingService.{Namespace}`. It uses selected table columns and emits operations that delegate through the server layer. It checks out and saves output and integrates it with the server `.csproj` where coded.
- **`ZESARemotingServiceProject.csgen`** creates `RemotingserviceInterfaceDefs.csproj` at the target root, deriving project entries from selected table/view objects. Note the source spelling `RemotingserviceInterfaceDefs` (lowercase “s” in service) rather than silently normalizing it.

## Client list controls

- **`ZESAListViews.csgen`** emits `{Entity}ListUC.cs` into `TargetPath/Client/ZESAClient.{Namespace}/` (source constructs namespace `EHA.ZESA.Client.{Namespace}`); it generates a user-control list view based on selected table/view columns.
- **`ZESAComboBox.csgen`** emits `{Entity}ComboUC.cs` under `Client/ZESAClient/{Namespace}/Allgemein/`, namespace rooted at `EHA.ZESA.Client.{Namespace}.Allgemein`. Its implementation builds a combo-box user control and binds generated data tables using corresponding `ColDefs` IDs. It uses MyMeta table/view columns and selection metadata.

Both controls call out to the legacy client/DAO API; they are not generic UI templates. Output names are formed from `GeneratorHelper.ConvertName` and selected `EntityName`.

## Unit tests

- **`ZESAUnitTest.csgen`** generates `{Entity}Test.cs` under the test DAO namespace/path (`EHA.ZESA.Test.DAO.{Namespace}`) from selected table/view columns.
- **`ZESAUnitTestProject.csgen`** emits `ZESAUnitTests.csproj` at the configured target path and includes selected data objects in the project manifest.

The templates reveal the shape and project conventions of the generated tests, but do not establish that the generated test suite is comprehensive or that a test run occurs during generation.

## Naming caveats

C# namespace and output path may not be identical: inspect each template's `fullClassname`, `filepath`, and `filename` code. A matching entity filename convention is common but not universal; for example projects use fixed names, and UI controls use `ComboUC`/`ListUC` suffixes. The per-object namespace can be empty for views in the UI defaults, so paths/namespaces for views need explicit validation.

## Related pages

- [Generator pipeline](generator-pipeline.md)
- [Data-access template group](data-access-templates.md)
- [Configuration and utility templates](configuration-templates.md)
- [TFS and auto-update](tfs-and-autoupdate.md)
- [.NET 10 port notes](dotnet10-port-notes.md)
