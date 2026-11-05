---
title: Tutorial Templates
summary: The eight Chapter1/Chapter2 tutorial templates (C#, VB.NET, VBScript, JScript) and the template-authoring concepts they demonstrate.
keywords: [tutorials, Chapter1, Chapter2, zeus, DotNetScriptGui, DotNetScriptTemplate, output, WinForms]
source_paths: [mygeneration-legacy-code/templates/Tutorials]
---

# Tutorials

Folder `mygeneration-legacy-code/templates/Tutorials/` has 8 `.zeus` files: `Chapter1` and `Chapter2` x C#, VB.NET, VBScript, JScript. Namespaces `Tutorials.<Lang>.Chapter N`.

## Chapter 1 - "Input / Output"
- Shows `output.write/writeln`, three tabbing techniques (manual tabs, `autoTabLn` with `tabLevel++`, `incTab/decTab`), literal markup outside `<% %>`, and the `output.text +=` anti-pattern.
- Prints the `input` keys `__defaultOutputPath`, `__dbDriver`, `__dbTarget`, `__language`, `__dbTargetMappingFileName`, `__dbLanguageMappingFileName`, `__userMetaDataFileName`.
- Saves `Chapter1.txt` with `output.save(path,false)`. GUI is empty.

## Chapter 2 - "Column Properties"
- GUI is a custom WinForms form (`MyForm : Form`, referencing System.Windows.Forms/System.Drawing; takes `dbRoot MyMeta` and `IZeusInput`): database combo (`MyMeta.Databases`, `DefaultDatabase`) and table listbox (`IDatabase.Tables`). On OK sets `zeusInput["databaseName"]` and `["tableName"]`, otherwise `ui.IsCanceled=true`.
- Body lists `column.Name : column.DataTypeName` for `ITable.Columns`.
- The .Net Script versions are `class GeneratedGui : DotNetScriptGui` and `class GeneratedTemplate : DotNetScriptTemplate` with `Setup()` / `Render()`. VBScript/JScript versions use the Microsoft Script engine and `ui.Add*`.

## Port relevance
Only the concept "pick database + table, iterate `Table.Columns`, print name/type" carries over; it is the minimal Scriban model smoke test.

## Related pages
- [zeus-api.md](zeus-api.md)
- [template-catalog.md](template-catalog.md)
- [overview.md](overview.md)
- [INDEX.md](INDEX.md)
