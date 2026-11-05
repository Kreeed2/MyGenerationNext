---
title: Scintilla editor and syntax highlighting
summary: The legacy source editor wraps native Scintilla through ScintillaNET, applies XML lexer/style configuration, and adds Zeus-specific language behavior.
keywords: [Scintilla, ScintillaNET, editor, syntax highlighting, autocomplete]
source_paths: [mygeneration-legacy-code/scintilla/ScintillaNET/ScintillaControl.cs, mygeneration-legacy-code/scintilla/ScintillaNET/Configuration, mygeneration-legacy-code/mygeneration/MyGenUtility/Forms/TemplateEditor/ZeusScintillaControl.cs, mygeneration-legacy-code/mygeneration/MyGeneration/ZeusScintillaControl.cs, mygeneration-legacy-code/mygeneration/MyGeneration/Settings/ScintillaNET.xml]
---

# Scintilla editor

## Control stack

`scintilla/ScintillaNET/ScintillaControl.cs` is a WinForms `Control` that hosts the native Scintilla lexer component (`SciLexer.dll`, named by `DefaultDllName`). It marshals native notifications through its window procedure, exposes .NET events and editing APIs, and supports initialization/configuration, printing, find/replace, and range/indexer helpers. The native DLL is shipped separately; `lib/thirdparty/ScintillaNET.dll` is the compiled library corresponding to the in-repository source.

The app wraps it as `ZeusScintillaControl` in `MyGenUtility/Forms/TemplateEditor/ZeusScintillaControl.cs`. That wrapper implements `IEditControl` and `IScintillaNet`, tracks template `Mode` and `Language`, initializes find/replace on focus, offers font/code-page overrides, and updates styles when language or mode changes. It wires `AutoCompleteHelper.CharAdded` and drag/drop in the shared editor wrapper. A second `MyGeneration/ZeusScintillaControl.cs` variant exists and should not be assumed identical; the MyGeneration project currently uses shared `MyGenUtility` forms.

## Configuration and language behavior

`MyGenerationMDI_Load` loads the Scintilla configuration via `ConfigurationUtility` from its `SCINTILLA_CONFIG_FILE` under the application settings directory, retries on parse/read failures, sets `configureDelegate` from `configFile.MasterScintilla.Configure`, and assigns it to `ZeusScintillaControl.StaticConfigure`. The template editor selects the lexer language/mode based on template properties. `Settings/ScintillaNET.xml` contains lexer keyword classes and style settings (including C#, JScript, VBScript, VB.Net and SQL keywords); the standalone ScintillaNET project also has per-language XML resources under `scintilla/ScintillaNET/resources`.

Editor behavior includes smart indentation, brace matching inherited from ScintillaNET, per-language keyword/style configuration, optional autocomplete in the shared Zeus wrapper, and search/replace and go-to-line commands. The `.NET 10` rewrite should choose a terminal-native editor widget or deliberately offer simpler line-based editing; Scintilla's native Win32 control and XML styling protocol do not map directly to Terminal.Gui.

## Preserve concepts, not implementation

Template editing, syntax-aware navigation, diagnostics with line/column, and language/mode selection can remain valuable. The control's Windows message handling, native DLL deployment, WinForms font/color widgets, and shell-wide `StaticConfigure` delegate are implementation details to replace. SQL syntax highlighting can be retained for SQL Server template snippets without porting the old language set.

## Related pages

- [Template browser and editor](template-browser-and-editor.md)
- [Main application](main-application.md)
- [Utilities](utilities.md)
- [TUI mapping](tui-mapping.md)
