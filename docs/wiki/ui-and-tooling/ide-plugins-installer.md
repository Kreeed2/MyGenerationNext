---
title: IDE plugins, converter, installer, and help
summary: Legacy distribution includes Visual Studio and SharpDevelop integrations, a CodeSmith conversion utility, NSIS packaging, third-party binaries, and compiled/help-authoring assets.
keywords: [IDE, Visual Studio, SharpDevelop, installer, CodeSmith, help]
source_paths: [mygeneration-legacy-code/ideplugins/visualstudio2005, mygeneration-legacy-code/ideplugins/sharpdevelop, mygeneration-legacy-code/codesmith2mygen, mygeneration-legacy-code/installer, mygeneration-legacy-code/lib/thirdparty, mygeneration-legacy-code/documentation/help]
---

# IDE plugins, installer, and help

## Visual Studio 2005 add-in

`ideplugins/visualstudio2005/MyGenVS2005/Connect.cs` implements Visual Studio's `IDTExtensibility2` and `IDTCommandTarget`. On connection, it registers Tools-menu commands for “MyGeneration Template Browser” and “MyGeneration Project Editor”; command execution opens `AddInTemplateBrowser` or `AddInProjectBrowser`. Those forms use shared `MyGenUtility` browser controls and Visual Studio's DTE automation: the project browser can locate/open a `.zprj` associated with Solution Explorer, and template execution can open generated files in the IDE and write status to the Output window. The add-in reads the installation path from the `Software\MyGeneration13` registry key.

## SharpDevelop add-in

The SharpDevelop source is a legacy add-in for old SharpDevelop versions, with separate 2.2/3.0 projects and `.addin` manifests. `Src/TestPad.cs` implements `AbstractPadContent` and embeds the shared `TemplateBrowserControl`; it initializes application paths from the same registry install key. `Src/MyUserControl.cs` is a minimal sample control demonstrating XML-form loading and a button. It is not a modern IDE plugin contract and has no relevance to a standalone TUI.

## CodeSmith conversion

There are two conversion entry routes: standalone `codesmith2mygen/CodeSmith2MyGeneration` and integrated `plugins/MyGeneration.UI.Plugins.CodeSmith2MyGen`. Both provide `FormConvertCodeSmith`, with the integrated form receiving `IMyGenerationMDI`. The form selects a CodeSmith template folder, CodeSmith application location, output folder, and selected templates; it parses via `CstParser`, chooses a `LanguageHelper`, generates `.zeus` template files, logs errors, and optionally opens outputs in MyGeneration. The UI stores paths and launch preference through `Config`. The conversion library contains parser/template models, language generation, mappings, and processor plugins. Treat conversion as an optional one-time migration aid, not a core generator screen.

## Installer and bundled dependencies

`installer/mygeneration.nsi` is an NSIS script for MyGeneration 1.3. It installs to `$PROGRAMFILES\MyGeneration13`, detects/installs .NET Framework 4, carries legacy .NET/MDAC/Script Control checks, copies `MyGeneration.exe`, `ZeusCmd.exe`, `MyGenUtility.dll`, `PluginInterfaces.dll`, `Zeus.dll`, MyMeta, ScintillaNET/SciLexer, and DockPanel-related libraries. Optional plugins are marked `/nonfatal`; the script therefore does not imply every provider/plugin is always shipped. It also installs templates, architectures, help files, icons, and registry/shell integration. `installer/BuildAll.bat` and related `.nsi` scripts document the historical packaging approach.

`lib/thirdparty` contains compiled dependencies including ScintillaNET/SciLexer, docking/splitter controls, COM interop libraries, and database provider libraries. The source also exists for some dependencies (notably ScintillaNET). The rewrite should replace only dependencies required by its target stack; it should not bundle obsolete WinForms/native binaries by habit.

## Documentation and help assets

`documentation/help/mygeneration/mygeneration.hnd` is a HelpNDoc project. `documentation/help/zeus/ZeusClassReference` contains the Zeus API reference project/source; the `tools` directory records legacy help-authoring/build utilities. The installer copies compiled `.chm` files, while the repository retains authoring inputs and update scripts. These are legacy WinHelp/CHM delivery assets, not the Markdown agent wiki being authored here.

## Related pages

- [Plugins](plugins.md)
- [Utilities](utilities.md)
- [Overview](overview.md)
- [TUI mapping](tui-mapping.md)
