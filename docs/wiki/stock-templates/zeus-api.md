---
title: Zeus Template API (ui, input, output)
summary: Reference for the objects a Zeus template sees - GUI builder, input dictionary and ZeusOutput text buffer - as used by stock templates.
keywords: [zeus, ZeusOutput, ZeusInput, GuiController, preserve, save, autoTab, input keys]
source_paths: [mygeneration-legacy-code/documentation/help/zeus/ZeusClassReference/ZeusOutput.cs, mygeneration-legacy-code/documentation/help/zeus/ZeusClassReference/ZeusInput.cs, MyGenerationTemplates/_MyGeneration13_2025/Settings/ZeusConfig.xml]
---

# Zeus API

Class sources are copied for Sandcastle into `documentation/help/zeus/ZeusClassReference/` (`ZeusInput.cs`, `ZeusOutput.cs`, `Data/Simple*.cs`, `UserInterface/Gui*.cs`, `Interfaces/IGui*.cs`).

## `output` (`ZeusOutput`)
- Writing: `write`, `writeln`/`writeLn`, `text` (the buffer), `clear`.
- Indentation: `autoTab`, `autoTabLn`, `incTab`, `decTab`, `tabLevel`.
- `save(file, mode)`: modes `"o"` overwrite, `"b"` backup, `"a"` append, `"d"` don't overwrite if file exists. Stock templates call `output.save(file,false)` or `output.save(file,"d")`.
- Preserve regions: `setPreserveSource(file,"<!--::","::-->")`, `preserve(name)`, `getPreserveBlock(name)`, `getPreservedData(key)`. Used by `HTML_DatabaseReport.csgen` (`customHeaders`).
- Multi-table idiom: per table build text, `output.save(perTableFile)`, append to a buffer, `output.clear`, and finally write the combined buffer (e.g. `MicrosoftSQL_ALL.SQL`).

## `input` (`ZeusInput`, `IZeusInput`)
An indexer dictionary (`this[key]`, `Contains`, `Remove`, `AddItems`, `Keys`, `Values`). GUI controls write their values here by control name. Predefined keys: `__defaultOutputPath`, `__dbDriver`, `__dbTarget`, `__language`, `__dbTargetMappingFileName`, `__dbLanguageMappingFileName`, `__userMetaDataFileName`, `__version`, `defaultOutputPath`.

## `ui` (`GuiController`)
`AddLabel`, `AddTextBox`, `AddFilePicker`, `AddComboBox`, `AddListBox`, `AddCheckBox`, `AddGrid`; `BindData(MyMeta.Databases)`; `AttachEvent("onchange", ...)`; `ShowGUI`, `IsCanceled`, `Title`, `Width`, `Height`. Standard stock control names: `txtPath`, `txtNamespace`, `cmbDatabase`, `lstTables` / `lstViews` / `lstProcs`, check boxes.

## Port note
A TUI replaces `ui`; `input` becomes a typed options record (CLI args/config); `output` becomes Scriban render + file writer with overwrite policy (`Overwrite` / `SkipIfExists`).

## Related pages
- [overview.md](overview.md)
- [template-catalog.md](template-catalog.md)
- [tutorials.md](tutorials.md)
- [scriban-port-notes.md](scriban-port-notes.md)
- [INDEX.md](INDEX.md)
