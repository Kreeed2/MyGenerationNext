using MyGenerationNext.Cli;
using EHA.ZESA.Util.GeneratorUI;
using MyGenerationNext.Core;
using MyGenerationNext.Core.Zeus;
using Spectre.Console;

var settingsPath = AppSettings.DefaultPath;
var settings = AppSettings.Load(settingsPath);
DatabaseInfo? db = null;

while (true)
{
    AnsiConsole.Clear();
    AnsiConsole.Write(new FigletText("MyGeneration").Color(Color.Green));
    AnsiConsole.MarkupLine($"[grey]Selected tables:[/] {settings.SelectedTables.Count}   [grey]Settings:[/] {Markup.Escape(settingsPath)}");

    var choice = AnsiConsole.Prompt(new SelectionPrompt<string>()
        .Title("What do you want to do?")
        .AddChoices("Select tables", "Edit settings", "Generate", "Quit"));

    switch (choice)
    {
        case "Select tables": await SelectTables(); break;
        case "Edit settings": EditSettings(); break;
        case "Generate": await Generate(); break;
        default: return;
    }
}

async Task<bool> EnsureDb()
{
    if (db is not null) return true;
    try
    {
        db = await AnsiConsole.Status().StartAsync("Reading database metadata...",
            _ => SqlServerMetadataReader.ReadAsync(settings.ConnectionString));
        return true;
    }
    catch (Exception ex)
    {
        AnsiConsole.MarkupLine($"[red]Connection failed:[/] {Markup.Escape(ex.Message)}");
        Pause();
        return false;
    }
}

async Task SelectTables()
{
    if (!await EnsureDb()) return;

    var categories = new (string Label, Func<IEnumerable<string>> Names)[]
    {
        ("Tables",    () => db!.TablesOnly.Select(t => $"{t.Schema}.{t.Name}")),
        ("Views",     () => db!.Views.Select(v => $"{v.Schema}.{v.Name}")),
        ("Functions", () => db!.Procedures.Where(p => p.IsFunction).Select(f => $"{f.Schema}.{f.Name}")),
    };

    var selected = new HashSet<string>(settings.SelectedTables, StringComparer.OrdinalIgnoreCase);

    while (true)
    {
        var tabs = categories.Select(c =>
        {
            var total = c.Names().Count();
            var picked = c.Names().Count(selected.Contains);
            return $"{c.Label} [grey]({picked}/{total})[/]";
        }).Append("Deselect all").Append("Done").ToArray();

        var tab = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Pick an object type [grey](selected/total shown per type)[/]")
            .AddChoices(tabs));

        if (tab == "Done") break;

        if (tab == "Deselect all")
        {
            selected.Clear();
            AnsiConsole.MarkupLine("[grey]All selected objects deselected.[/]");
            Pause();
            continue;
        }

        var category = categories[Array.IndexOf(tabs, tab)];
        var all = category.Names().OrderBy(n => n).ToList();

        var filterChoice = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title($"Filter {category.Label.ToLowerInvariant()} [grey]({all.Count(selected.Contains)} of {all.Count} selected)[/]")
            .AddChoices("Show all", "Show selected only", "Custom text filter..."));

        List<string> matches;
        string? filterText = null;
        if (filterChoice == "Show selected only")
        {
            matches = all.Where(selected.Contains).ToList();
        }
        else if (filterChoice == "Custom text filter...")
        {
            filterText = AnsiConsole.Prompt(
                new TextPrompt<string>($"Filter {category.Label.ToLowerInvariant()} [grey](empty = show all)[/]:")
                    .AllowEmpty());
            matches = string.IsNullOrWhiteSpace(filterText)
                ? all
                : all.Where(n => n.Contains(filterText, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        else
        {
            matches = all;
        }

        if (matches.Count == 0)
        {
            AnsiConsole.MarkupLine(filterChoice == "Show selected only"
                ? $"[yellow]No {category.Label.ToLowerInvariant()} selected yet.[/]"
                : $"[yellow]No {category.Label.ToLowerInvariant()} match '{Markup.Escape(filterText ?? string.Empty)}'.[/]");
            Pause();
            continue;
        }

        var prompt = new MultiSelectionPrompt<string>()
            .Title($"Tick the {category.Label.ToLowerInvariant()} to generate [grey](space = toggle, enter = accept)[/]")
            .PageSize(20)
            .NotRequired()
            .MoreChoicesText("[grey](Move up and down for more)[/]");
        foreach (var name in matches)
        {
            prompt.AddChoice(name);
            if (selected.Contains(name)) prompt.Select(name);
        }
        var picked = AnsiConsole.Prompt(prompt);
        foreach (var name in matches) selected.Remove(name);
        foreach (var name in picked) selected.Add(name);
    }

    settings.SelectedTables = selected.ToList();
    settings.Save(settingsPath);
}

void EditSettings()
{
    settings.ConnectionString = AnsiConsole.Prompt(
        new TextPrompt<string>("Connection string:").DefaultValue(settings.ConnectionString).AllowEmpty());
    settings.TemplateDirectory = AnsiConsole.Prompt(
        new TextPrompt<string>("Template directory:").DefaultValue(settings.TemplateDirectory));
    settings.SourceDirectory = AnsiConsole.Prompt(
        new TextPrompt<string>("Source directory:").DefaultValue(settings.SourceDirectory));
    settings.MetaDataFile = AnsiConsole.Prompt(
        new TextPrompt<string>("Metadata file (BLGeneratorUIMeta.xml) [grey](empty = <template dir>/BLGeneratorMetaData)[/]:")
            .DefaultValue(settings.MetaDataFile).AllowEmpty());
    settings.SaveMetaData = AnsiConsole.Prompt(
        new ConfirmationPrompt("Write metadata overrides back after generation?").DefaultValue(settings.SaveMetaData));
    settings.Save(settingsPath);
    db = null; // connection string may have changed
}

async Task Generate()
{
    if (settings.SelectedTables.Count == 0) { AnsiConsole.MarkupLine("[yellow]No tables selected.[/]"); Pause(); return; }
    if (!Directory.Exists(settings.TemplateDirectory)) { AnsiConsole.MarkupLine("[red]Template directory not found.[/]"); Pause(); return; }
    if (!await EnsureDb()) return;

    var templates = Directory.GetFiles(settings.TemplateDirectory, "*.csgen");
    if (templates.Length == 0) { AnsiConsole.MarkupLine("[yellow]No .csgen files found.[/]"); Pause(); return; }

    var chosen = AnsiConsole.Prompt(new MultiSelectionPrompt<string>()
        .Title("Templates to run [grey](group templates such as BL.csgen run all their included templates)[/]")
        .NotRequired()
        .AddChoices(templates.Select(p => Path.GetFileName(p)).Order()));
    Directory.CreateDirectory(settings.SourceDirectory);

    var meta = Path.Combine(settings.TemplateDirectory, "BLGeneratorMetaData");
    TfsHelper.Log = m => AnsiConsole.MarkupLine($"[grey]{Markup.Escape(m)}[/]");
    GeneratorEnvironment.Reset();
    GeneratorEnvironment.Database = db;
    GeneratorEnvironment.AutoSelectStammdatenDependencies = false;
    GeneratorEnvironment.TargetPath = Path.GetFullPath(settings.SourceDirectory);
    GeneratorEnvironment.SelectedObjects = settings.SelectedTables
        .Select(t => t[(t.IndexOf('.') + 1)..]).ToHashSet(StringComparer.OrdinalIgnoreCase);
    GeneratorEnvironment.MemberUseOldDataSetLogicFile = Path.Combine(meta, "MemberUseOldDataSetLogic.xml");
    GeneratorEnvironment.MetaDataFile = settings.ResolveMetaDataFile();
    GeneratorEnvironment.SaveMetaData = settings.SaveMetaData;
    await GeneratorEnvironment.LoadServerExtrasAsync(settings.ConnectionString);

    var files = 0; var failed = 0;
    foreach (var file in chosen)
    {
        try
        {
            var result = await Task.Run(() => ZeusExecutor.Run(Path.Combine(settings.TemplateDirectory, file),
                new ZeusRunOptions { MyMeta = new MyMeta.dbRoot(db!) }));
            files += result.SavedFiles.Count;
            AnsiConsole.MarkupLine($"[green]{Markup.Escape(file)}[/]: {result.RenderedTemplates.Count} template(s), {result.SavedFiles.Count} file(s) written");
        }
        catch (Exception ex)
        {
            failed++;
            AnsiConsole.MarkupLine($"[red]{Markup.Escape(file)}:[/] {Markup.Escape(ex.Message)}");
        }
    }
    AnsiConsole.MarkupLine($"[green]{files} file(s) written[/], [red]{failed} template run(s) failed[/].");
    Pause();
}

static void Pause()
{
    AnsiConsole.MarkupLine("[grey]Press any key...[/]");
    Console.ReadKey(true);
}
