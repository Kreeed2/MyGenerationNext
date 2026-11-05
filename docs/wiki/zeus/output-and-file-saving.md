---
title: Output object and saving files (ZeusOutput)
summary: Documents the `output` intrinsic (write/writeln/tabs/save/append/preserve regions), backup/overwrite semantics and encodings.
keywords: [ZeusOutput, output.write, save, saveEnc, backup, preserve, autoTab, SavedFiles]
source_paths:
  - mygeneration-legacy-code/mygeneration/Zeus/ZeusOutput.cs
  - mygeneration-legacy-code/mygeneration/EngineInterface/ZeusInterfaces/IZeusOutput.cs
  - mygeneration-legacy-code/templates/Tutorials/Chapter1(C#).zeus
---

# ZeusOutput (`output`)

Methods are lowercase (template-facing). Each `ZeusContext` has one; nested `Copy()` contexts get fresh outputs.

| Member | Behavior |
|---|---|
| `write(s)`, `writeln(s)` | append (writeln adds `\r\n`) |
| `autoTab(s)`, `autoTabLn(s)` | prefix with tabs by `tabLevel` |
| `incTab()`, `decTab()`, `tabLevel` | indentation |
| `text` (get/set), `clear()`, `rollback(n)` | buffer access |
| `append(path)` | append buffer to file |
| `save(path, action)` | write buffer to file |
| `saveEnc(path, action, encoding)` | same with encoding |
| `SavedFiles` | list of paths saved (read by `SavedTemplateInput.Execute`, ZeusCmd `[GENERATED_FILE]`) |

## save semantics
`action` is a bool (`true` = backup "b", `false` = overwrite "o") or a string whose first letter selects `b`/`o`/`a`.
- Backup: existing file moved to `path.NNN.bak` (001, 002, ...), then new file written.
- Overwrite: delete and recreate. Append: append. Missing directories are created.
- `saveEnc` encoding names: ascii, unicode, bigendianunicode, utf7, utf8; otherwise default.

## Preserve regions
Keeps hand-edited code across regeneration. `setPreserveSource(path, prefix, suffix)` scans an existing file for `prefix + PRESERVE_BEGIN key + suffix ... prefix + PRESERVE_END`; defaults prefix `//::`, suffix `:://`. Then `preserve(key)` emits the block (old content if present), `getPreservedData(key)`, `getPreserveBlock(key)`.

## Group templates
`ZeusExecutioner.ExecuteCode` calls `context.Output.clear()` before each child body of a group; templates normally call `output.save(...)` per file.

## Related pages
- [context-and-intrinsic-objects.md](context-and-intrinsic-objects.md)
- [execution-flow.md](execution-flow.md)
- [zeuscmd.md](zeuscmd.md)
- [scriban-dotnet10-mapping.md](scriban-dotnet10-mapping.md)
- [INDEX.md](INDEX.md)
