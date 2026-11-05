using System.Text.Json;

namespace MyGenerationNext.Cli;

public sealed class AppSettings
{
    public string ConnectionString { get; set; } = "Server=localhost;Database=master;Integrated Security=true;TrustServerCertificate=true";
    public string TemplateDirectory { get; set; } = "templates";
    public string SourceDirectory { get; set; } = "generated";
    public List<string> SelectedTables { get; set; } = [];

    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    public static string DefaultPath => Path.Combine(Directory.GetCurrentDirectory(), "mygen.settings.json");

    public static AppSettings Load(string path) =>
        File.Exists(path) ? JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path)) ?? new() : new();

    public void Save(string path) => File.WriteAllText(path, JsonSerializer.Serialize(this, Json));
}
