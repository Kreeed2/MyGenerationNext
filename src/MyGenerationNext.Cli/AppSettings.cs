using System.Text.Json;

namespace MyGenerationNext.Cli;

public sealed class AppSettings
{
    public string ConnectionString { get; set; } = "Server=localhost;Database=master;Integrated Security=true;TrustServerCertificate=true";
    public string TemplateDirectory { get; set; } = "templates";
    public string SourceDirectory { get; set; } = "generated";
    public List<string> SelectedTables { get; set; } = [];
    /// <summary>Saved per-object overrides (BLGeneratorUIMeta.xml: namespaces for views/functions, Generate, LOV, ...). Empty = &lt;TemplateDirectory&gt;/BLGeneratorMetaData/BLGeneratorUIMeta.xml.</summary>
    public string MetaDataFile { get; set; } = "";
    /// <summary>Write the metadata file back after generation so manual overrides persist.</summary>
    public bool SaveMetaData { get; set; } = true;

    public string ResolveMetaDataFile() => string.IsNullOrWhiteSpace(MetaDataFile)
        ? Path.Combine(TemplateDirectory, "BLGeneratorMetaData", "BLGeneratorUIMeta.xml")
        : MetaDataFile;

    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    public static string DefaultPath => Path.Combine(Directory.GetCurrentDirectory(), "mygen.settings.json");

    public static AppSettings Load(string path) =>
        File.Exists(path) ? JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path)) ?? new() : new();

    public void Save(string path) => File.WriteAllText(path, JsonSerializer.Serialize(this, Json));
}
