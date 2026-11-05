---
title: ZeusCmd command-line tool
summary: Documents ZeusCmd.exe switches, modes (template, project, MyMeta), argument validation and logging behavior.
keywords: [ZeusCmd, CmdInput, command line, switches, ProcessMode, -t, -p, -i, -c, internaluse, Log]
source_paths:
  - mygeneration-legacy-code/mygeneration/ZeusCmd/ZeusCmd.cs
  - mygeneration-legacy-code/mygeneration/ZeusCmd/CmdInput.cs
  - mygeneration-legacy-code/mygeneration/ZeusCmd/Log.cs
  - mygeneration-legacy-code/mygeneration/ZeusCmd/ZeusCmd.csproj
---

# ZeusCmd

.NET 4.0 console exe (`Main` is `[STAThread]`, returns `ReturnValue`: 0 or -1). References PluginInterfaces, MyMeta, MyGenUtility (for `DefaultSettings`) and Zeus.

## Flow
`ZeusCmd(args)`: `new Log()` -> `_ProcessArgs` (`new CmdInput(args)`; no args or `-?` prints `HELP_TEXT`; applies `-aio`/`-rio` to `ZeusConfig.Current` and `Save()`; enables log/console) -> switch on `CmdInput.Mode` (`ProcessMode`: Other, Template, Project, MyMeta) -> `_log.Close()`. Invalid input prints `ErrorMessage` and returns -1.

## Switches (`CmdInput.Parse`; long form in parentheses)
| Switch | Meaning |
|---|---|
| `-?`/`-h` (`-help`) | help |
| `-s` (`-silent`) | no console output |
| `-r` (`-relative`) | make paths absolute relative to `FileTools.ApplicationPath` |
| `-l <path>` (`-logfile`) | log file |
| `-e <int>` (`-timeout`) | timeout, default -1 |
| `-t <path>` (`-template`) | template file; mode Template |
| `-i <path>` (`-inputfile`) | saved input `.zinp`; template path taken from it if no `-t` |
| `-o <path>` (`-outfile`) | write `context.Output.text` to file, else console |
| `-c <path>` (`-collect`) | collect GUI input and save to file |
| `-p <path>` (`-project`) | project mode |
| `-pf`/`-m` (`-projectfolder`, `-module`) | run project module by `/Root/Module` path (repeatable) |
| `-ti` (`-templateinstance`) | run saved item `module/object` (repeatable) |
| `-rti` (`-recordtemplateinstance`) | with `-t`, collect input for a project item and print XML between `[BEGIN_RECORDING]`/`[END_RECORDING]` |
| `-aio dll classpath var`, `-rio var` | add/remove intrinsic object in ZeusConfig |
| `-tc type connstring` | test DB connection (`dbRoot.BuildConnection`) |
| `-mumd f1 db1 f2 db2 out` | `dbRoot.MergeUserMetaDataFiles` |
| `-internaluse`, `-installvs2005` | IDE integration (VS2005 add-in `.Addin` copy in `_InstallVS2005`) |

Note: `CmdInput._internalUse` defaults to `true`, so output markers (`[GENERATED_FILE]path`, `[PROJECT_GENERATION_COMPLETE]`) and timestamp-less log lines are always on; `ZeusProcessManager` in MyGenUtility parses them.

## Template mode (`_ProcessTemplate`)
- `-i`: `context.Input.AddItems(savedInput.InputData.InputItems)`; `template.Execute(context, timeout, true)` (no GUI).
- `-c`: `DefaultSettings.Instance.PopulateZeusContext`; `template.ExecuteAndCollect(context, timeout, items)`; `collectedInput.Save()`.
- otherwise: populate defaults; `template.Execute(context, timeout, false)` (GUI shown).
Then output text to `-o` file or console. Exceptions: `Log.Write(ex)` + "Template execution failed.".

## Project mode (`_ProcessProject`)
Loads `ZeusProject(path).Load()`. Priority: modules (`ExecuteModule` -> `FindModule` by case-insensitive `ProjectPath` -> `ZeusModule.Execute`); else record (`CollectProjectItem` -> `template.Collect`); else items (`ExecuteProjectItem` -> `SavedObjects[name].Execute`); else `proj.Execute(timeout, log)`.

## Log (`ZeusCmd/Log.cs`, `Zeus.Log : ILog`)
Properties `IsConsoleEnabled`, `IsLogEnabled`, `IsInternalUseMode`, `FileName`; `Write(string|format|Exception)` (exception: `ERROR: [type] - message`, stack only in DEBUG); appends to log file.

## Rewrite hints
Keep: template path, saved input, collect (as JSON), project run, timeout, output path, silent/log. Drop: `-aio/-rio`, VS2005, `-mumd`, COM-era `-tc` providers (replace by SQL Server connection test).

## Related pages
- [execution-flow.md](execution-flow.md)
- [saved-input-and-projects.md](saved-input-and-projects.md)
- [context-and-intrinsic-objects.md](context-and-intrinsic-objects.md)
- [scriban-dotnet10-mapping.md](scriban-dotnet10-mapping.md)
- [INDEX.md](INDEX.md)
