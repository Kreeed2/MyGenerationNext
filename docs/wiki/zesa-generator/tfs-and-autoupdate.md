---
title: TFS and generator auto-update
summary: Documents legacy source-control side effects, versioned generator DLL deployment, and the standalone updater behavior.
keywords: [TFS, Team Foundation Version Control, auto-update, DLL, project files]
source_paths: [MyGenerationTemplates/BusinesslogicGeneratorUI/TfsHelper.cs, MyGenerationTemplates/BusinesslogicGeneratorUI/GeneratorHelper.cs, MyGenerationTemplates/BusinesslogicGeneratorUI/GeneratorDll/, MyGenerationTemplates/BusinesslogicGeneratorUI/BLGeneratorUI.cs, MyGenerationTemplates/BlGeneratorAutoUpdate/BlGeneratorAutoUpdate/Program.cs]
---

# TFS and generator auto-update

## TFS integration

`TfsHelper` is a lazy singleton wrapping Team Foundation Version Control APIs from the Microsoft.TeamFoundation client libraries. On initialization it deserializes `BLGeneratorSettings`, creates a `TfsTeamProjectCollection` from `TfsServerUrl`, gets `VersionControlServer`, chooses a workspace name, and obtains the workspace for the current domain/user. The workspace name defaults to the machine name unless overridden by settings or UI; `CurrentTfSWorkspace` can force an override and `CheckForChangedWorkspace()` reinitializes when changed.

Methods and side effects:

- `CheckOutMetaDataFile()` gets the latest content for the fixed server path `$/ZESA/Utilities/MyGenerationTemplates/BLGeneratorMetaData` and then checks out the given local file.
- `CheckOut()` calls `PendEdit` only if the local file exists.
- `Add()` calls `PendAdd` only if the local file exists.
- `GetUtilities()` gets the latest full `$/ZESA/Utilities` tree.
- `CheckInMetaDataFile()` gathers pending changes recursively for the metadata server path, checks conflicts, and if conflict-free checks in with message “Automatischer Eincheckvorgang MyGeneration”. The code sets policy override/gated-check-in options based on server capability.

`BLGeneratorUI.LoadSavedLocalMetadata()` and `SaveLocalMetadata()` call checkout/check-in around the shared UI metadata file. Many templates call checkout on generated source; DAO/service generators can update project files through `GeneratorHelper.AddFileToProject()`. That helper locates projects using `BLGeneratorUI.PathTfsRepos`, with fixed project paths for Configuration, RemotingServiceInterface, and Server, then inserts sorted `<None Include=...>` or `<Compile Include=...>` entries. It is not a general project-system API.

These are important effects to isolate in a port. A non-TFS generation mode should not silently attempt workspace initialization, and filesystem generation should be testable without check-in/project-file modification.

## Packaged generator DLL versions

`BusinesslogicGeneratorUI/GeneratorDll/` contains version folders `Version2.0.0` through `Version2.7.0`, then `Version3.0.0` and `Version3.1.0`. Older folders may also include metadata/settings XML; later folders primarily include DLLs. `AutoUpdate/CurrentVersion.txt` contains `3.1.0.28308`; the current source snapshot under `_MyGeneration13_2025/` includes a `ZESABLGeneratorUI.dll`. The `BusinesslogicGeneratorUI.csproj` itself references `GeneratorDll/Version3.1.0/MyMeta.dll` and outputs release assembly to `lib`.

`MyGenerationTemplates/lib/` contains `MyMeta.dll`, `ZESABLGeneratorUI.dll`, and the three Team Foundation client/version-control assemblies referenced by the UI project. The repository includes binaries; their internal behavior is not established by source file names alone. For source-level behavior, prefer the C# in `BusinesslogicGeneratorUI/` and use version directories as deployment history.

## `BlGeneratorAutoUpdate`

`BlGeneratorAutoUpdate/Program.cs` expects a source directory and a MyGeneration destination directory. It supports arguments as two values or as a comma-split first argument to accommodate paths with spaces. It waits ten seconds, looks for a process named `MyGeneration.exe` and kills any matches, copies every `*.dll` from the source directory to destination (overwriting and clearing destination read-only flags), launches `destination/MyGeneration.exe`, and reports success/failure on the console. The bundled `AutoUpdate/CurrentVersion.txt` is a version marker; the program shown does not parse it or perform version comparison. It copies DLLs, not metadata/config files.

## Related pages

- [Generator pipeline](generator-pipeline.md)
- [Generator UI](generator-ui.md)
- [Metadata XML schema](metadata-xml-schema.md)
- [Config migration tools](config-migration.md)
- [.NET 10 port notes](dotnet10-port-notes.md)
