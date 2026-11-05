namespace MyGenerationNext.Core.Zeus.WinPath;

/// <summary>Normalizes Windows-style separators for the file system calls templates make.</summary>
public static class Paths
{
    public static string Normalize(string path) =>
        Path.DirectorySeparatorChar == '\\' ? path : path.Replace('\\', Path.DirectorySeparatorChar);
}

public static class Directory
{
    public static DirectoryInfo CreateDirectory(string path) => System.IO.Directory.CreateDirectory(Paths.Normalize(path));
    public static bool Exists(string? path) => path is not null && System.IO.Directory.Exists(Paths.Normalize(path));
    public static string[] GetFiles(string path) => System.IO.Directory.GetFiles(Paths.Normalize(path));
    public static string[] GetFiles(string path, string pattern) => System.IO.Directory.GetFiles(Paths.Normalize(path), pattern);
    public static void Delete(string path, bool recursive = false) => System.IO.Directory.Delete(Paths.Normalize(path), recursive);
}

public static class File
{
    public static bool Exists(string? path) => path is not null && System.IO.File.Exists(Paths.Normalize(path));
    public static string ReadAllText(string path) => System.IO.File.ReadAllText(Paths.Normalize(path));
    public static string[] ReadAllLines(string path) => System.IO.File.ReadAllLines(Paths.Normalize(path));
    public static void WriteAllText(string path, string contents) => System.IO.File.WriteAllText(Paths.Normalize(path), contents);
    public static void Delete(string path) => System.IO.File.Delete(Paths.Normalize(path));
}
