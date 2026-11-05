---
title: GUI input controls (ui.* / GuiController)
summary: Catalogs the `ui` object API and Gui* control classes templates use to collect input, including MyMeta binders, events and the WinForms renderer.
keywords: [GuiController, ui, AddTextBox, AddComboBox, AddListBox, AddFilePicker, DynamicForm, FormBuilder, GuiDataBinder, AttachEvent]
source_paths:
  - mygeneration-legacy-code/mygeneration/Zeus/UserInterface/GuiController.cs
  - mygeneration-legacy-code/mygeneration/Zeus/UserInterface/GuiControl.cs
  - mygeneration-legacy-code/mygeneration/Zeus/UserInterface/GuiTextBox.cs
  - mygeneration-legacy-code/mygeneration/Zeus/UserInterface/GuiDataBinder.cs
  - mygeneration-legacy-code/mygeneration/Zeus/UserInterface/WinForms/DynamicForm.cs
  - mygeneration-legacy-code/mygeneration/Zeus/UserInterface/WinForms/FormBuilder.cs
  - mygeneration-legacy-code/templates/Other/TemplateGroupExample.jgen
---

# GUI controls

The GUI segment of a template builds a form by calling `ui.*` (the `GuiController` intrinsic) inside `setup()` (JScript/VBScript) or `Setup()` (C#/VB). Values entered become `input[ID]` for the body.

## GuiController
Defaults: Width 800, Height 600, Title "Zeus Dynamic Form", StartPosition "centerparent". Auto-layout: Left 10, Top = previous Top+Height+10, Width = form width - 30 (-15 more if tabs).
- Adders: `AddLabel(id,text,tooltip)`, `AddTextBox(id,text,tooltip)`, `AddCheckBox(id,text,isChecked,tooltip)`, `AddButton`, `AddOkButton`, `AddOkButtonIfNonExistant` (id `ButtonOk__internal`), `DoesOkButtonExist`, `AddFilePicker(id,text,tooltip,targetControl,picksFolder)`, `AddComboBox(id,tooltip)`, `AddListBox(id,tooltip)`, `AddCheckBoxList(id,tooltip)`, `AddGrid(id,tooltip)`, `AddTab(text)`.
- MyMeta binders (with `parentId`): `AddDatabaseSelector`, `AddTableSelector`, `AddViewSelector`, `AddProcedureSelector` (combo) and `Add*MultiSelector` (listbox). `BindMyMetaControl` uses `BindingTag` and `MyMeta.dbRoot` (`Databases`/`DefaultDatabase`/`Tables`/`Views`/`Procedures`); `UpdateAutoBinding` rebinds children when a parent changes.
- State: `Title`, `Width`, `Height`, `StartPosition`, `ShowGui` (must be true to display), `IsCanceled`, `ForceDisplay`, `Defaults` (Hashtable pre-seeding values, applied by `FormBuilder.InitializeControlData`), indexer `ui[id]`, `TabNames`, enumerable in order, `AttachEvent("onclose", fn)`. Duplicate ids throw.

## Control classes (base `GuiControl`)
Base: `ID`, `IsDataControl` (false for Button, Label), `ToolTip`, `Visible`, `Enabled`, `ForeColor` ("Black"), `BackColor`, `Width` (300), `Height` (16), `Top`, `Left`, abstract `Value`, `AttachEvent`/`HasEventHandlers`/`GetEventHandlers` (`onblur`, `onfocus`), `AutoBindingChildControls`.

| Control | Value | Extras | Events |
|---|---|---|---|
| `GuiLabel` | - | Text, Bold, Underline, Italic, Strikeout | - |
| `GuiTextBox` | Text | WordWrap, Multiline, VerticalScroll, HorizontalScroll | onkeypress |
| `GuiCheckBox` | Checked | Text | onclick |
| `GuiButton` | Text | ClosesForm, CancelGeneration | onclick |
| `GuiFilePicker` | ItemData | Text, TargetControl, PicksFolder | onselect |
| `GuiComboBox` | SelectedValue | Items, `Add(val,text)`, SelectedIndex/Text, `Select`, `BindData`, `BindDataFromTable`, `AutoBindingControlParentID`, `BindingTag` | onchange |
| `GuiListBox` | selected items ArrayList | `IsMultiSelect`, `SelectAll`, `ClearSelected`, `SelectedItems`, list API | onchange |
| `GuiCheckBoxList` | selected items ArrayList | list API (`IGuiListControl`) | onchange |
| `GuiGrid` | DataSource (`SimpleTable`) | `BindData` | - |

`IGuiListControl`: Items, Clear, indexer, Sorted, Count, SelectedIndex(es), IndexOf, GetValueAtIndex/GetTextAtIndex, IsSelectedAtIndex, SelectAtIndex, Add, Contains, Select, BindData, BindDataFromTable.
`GuiDataBinder` (internal) binds DataTable (value/text fields), Hashtable, NameValueCollection, ArrayList, string[], object[], IDictionary, `Scripting.Dictionary` (COM), IEnumerable.

## Events and rendering
Event handlers are script function names (strings). `WinForms/DynamicForm` calls `executioner.ExecuteFunction(name, control)`. `ZeusExecutioner.DynamicGUI_Display` shows the form; Cancel sets `gui.IsCanceled`. After the GUI, `context.Input.AddItems(context.Gui)` copies data controls to input. Template example: `Other/TemplateGroupExample.jgen` (label, textbox, file picker, combo + listbox with `AttachEvent("onchange", ...)`, `ui.ShowGUI = true`).

WinForms rendering (`DynamicForm`, `FormBuilder`, `ZeusDisplayError`) is discard material; the control model is what a TUI prompt layer should reproduce (see [scriban-dotnet10-mapping.md](scriban-dotnet10-mapping.md)).

## Related pages
- [context-and-intrinsic-objects.md](context-and-intrinsic-objects.md)
- [execution-flow.md](execution-flow.md)
- [saved-input-and-projects.md](saved-input-and-projects.md)
- [scriban-dotnet10-mapping.md](scriban-dotnet10-mapping.md)
- [INDEX.md](INDEX.md)
