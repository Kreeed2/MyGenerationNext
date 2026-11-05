---
title: End-to-end template execution flow
summary: Step-by-step trace, with method names, of loading, parsing, GUI, body execution, group handling, collect and saved-input runs.
keywords: [ZeusExecutioner, ZeusTemplate.Execute, ExecuteCodeSegment, PopulateContextObjects, Collect, ExecuteAndCollect, group, skipGui]
source_paths:
  - mygeneration-legacy-code/mygeneration/Zeus/ZeusExecutioner.cs
  - mygeneration-legacy-code/mygeneration/Zeus/ZeusTemplate.cs
  - mygeneration-legacy-code/mygeneration/Zeus/ZeusContext.cs
  - mygeneration-legacy-code/mygeneration/Zeus/SavedInput/SavedTemplateInput.cs
  - mygeneration-legacy-code/mygeneration/ZeusCmd/ZeusCmd.cs
---

# Execution flow

1. **Entry** (ZeusCmd `_ProcessTemplate`, IDE, or `SavedTemplateInput.Execute`): create `ZeusContext` (sets `objects["ui"]`), set `Log`, fill `Input` (`DefaultSettings.PopulateZeusContext` or saved items).
2. **Load**: `new ZeusTemplate(path)` -> `ZeusTemplateParser.LoadTemplate` (see [template-file-format.md](template-file-format.md)); parse errors -> `LegacyTemplateParser`.
3. `ZeusTemplate.Execute(context, timeout, skipGui)` -> `new ZeusExecutioner(context.Log).Execute(template, context, timeout, collectedInput, eventHandler, skipGui)`.
4. **GUI phase**
   - `skipGui == true`: `PopulateContextObjects(context)` and run every `ZeusFactory.Preprocessors[i].Process(context)`.
   - else: GUI engine helper gets `Timeout` and `SetShowGuiHandler(DynamicGUI_Display)`; `GuiSegment.Execute(context)` -> `ExecuteCodeSegment`; `Cleanup()`; `collectedInput.Add(context.Input)`.
5. `ExecuteCodeSegment(segment, ...)`:
   - `ZeusCodeParser.ParseCode` if not already parsed (see [template-parsing.md](template-parsing.md)).
   - `PopulateContextObjects`: each `ZeusConfig.IntrinsicObjects` -> `context.Objects[VariableName]`.
   - push template on `ZeusContext.TemplateStack`.
   - GUI segment: run preprocessors, `ExecuteGuiCode` (skipped if `context.Input.Contains(reqVars)`), `context.Input.AddItems(context.Gui)`; returns `!Gui.IsCanceled`.
   - Body segment: `ExecuteCode` -> engine `EngineExecuteCode` (compile/instantiate `Render()` or run script, see [script-engines.md](script-engines.md)).
6. If GUI result true, **body phase** with the body engine's helper: `BodySegment.Execute(context)`.
7. **Group templates** (`Type == Group`): after the group body, `IncludedTemplates` (loaded from `IncludedTemplatePaths` via `FileTools.MakeAbsolute`) each: `context.Output.clear()`, push template, run child body with child's engine. A `templateGroupIds` list (UniqueID) prevents recursion.
8. **Errors**: `helper.HasErrors` -> `ZeusExecutionException`; other exceptions -> `ZeusRuntimeException`; outer `Execute` catches, `context.Log.Write(ex)`, logs "Successfully rendered Template" or "Canceled Template execution".
9. **Files**: template code calls `output.save(...)`; paths appear in `context.Output.SavedFiles`.

## Variants
- `Collect(context, timeout, collectedInput)`: copies `collectedInput` into `context.Gui.Defaults`, `ForceDisplay = true`, runs GUI only, stores result back.
- `ExecuteAndCollect`: GUI + body and collects input.
- Nested: `ZeusTemplateContext.Execute(path, copyContext)` -> `new ZeusTemplate(abs).Execute(ctx, 0, true)`.
- Saved input: `SavedTemplateInput.Execute` -> skipGui run, preprocessor reconnects MyMeta from `__db*` input keys.
- Project: `ZeusProject.Execute` -> each saved object -> above.

## Related pages
- [template-parsing.md](template-parsing.md)
- [script-engines.md](script-engines.md)
- [context-and-intrinsic-objects.md](context-and-intrinsic-objects.md)
- [saved-input-and-projects.md](saved-input-and-projects.md)
- [scriban-dotnet10-mapping.md](scriban-dotnet10-mapping.md)
- [INDEX.md](INDEX.md)
