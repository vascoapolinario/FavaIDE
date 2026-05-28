using System.IO;
using System.Text.Json;

namespace FavaStudio.Services;

public class SettingsService
{
    public string JavaPath { get; set; } = "java";
    public string CompilerRoot { get; set; } = "";
    public string AntlrJar { get; set; } = "";
    public string InputsDir { get; set; } = "";
    public string OutputsDir { get; set; } = "";
    public string ProjectRoot { get; set; } = "";
    public bool ShowTestOutput { get; set; } = false;
    public List<string> RecentProjects { get; set; } = [];
    public List<string> RecentFiles { get; set; } = [];

    private static string SettingsPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                     "FavaStudio", "settings.json");

    public static SettingsService Load()
    {
        SettingsService settings;
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                settings = JsonSerializer.Deserialize<SettingsService>(json) ?? new SettingsService();
                ApplyBundledCompilerDefaults(settings);
                return settings;
            }
        }
        catch { }
        settings = new SettingsService();
        ApplyBundledCompilerDefaults(settings);
        return settings;
    }

    public void Save()
    {
        RecentProjects = NormalizeMostRecentList(RecentProjects, 10);
        RecentFiles = NormalizeMostRecentList(RecentFiles, 20);
        var dir = Path.GetDirectoryName(SettingsPath) ?? Path.GetTempPath();
        Directory.CreateDirectory(dir);
        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static List<string> NormalizeMostRecentList(IEnumerable<string>? items, int maxItems)
    {
        if (items is null) return [];
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var normalized = new List<string>();
        foreach (var item in items)
        {
            var path = item?.Trim();
            if (string.IsNullOrWhiteSpace(path)) continue;
            if (seen.Add(path))
                normalized.Add(path);
            if (normalized.Count >= maxItems)
                break;
        }

        return normalized;
    }

    private static void ApplyBundledCompilerDefaults(SettingsService settings)
    {
        var compilerRoot = FindBundledCompilerRoot();
        if (string.IsNullOrWhiteSpace(compilerRoot))
            return;

        if (string.IsNullOrWhiteSpace(settings.CompilerRoot) || !IsCompilerRoot(settings.CompilerRoot))
            settings.CompilerRoot = compilerRoot;

        var antlrJar = Directory.GetFiles(compilerRoot, "antlr-*-complete.jar").FirstOrDefault();
        if ((string.IsNullOrWhiteSpace(settings.AntlrJar) || !File.Exists(settings.AntlrJar)) && !string.IsNullOrWhiteSpace(antlrJar))
            settings.AntlrJar = antlrJar;
    }

    private static string FindBundledCompilerRoot()
    {
        var candidates = new List<string>();
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            candidates.Add(Path.Combine(current.FullName, "Compiler"));
            candidates.Add(current.FullName);
            current = current.Parent;
        }

        candidates.Add(Path.Combine(Directory.GetCurrentDirectory(), "Compiler"));

        return candidates
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(IsCompilerRoot) ?? "";
    }

    private static bool IsCompilerRoot(string path)
    {
        return File.Exists(Path.Combine(path, "FavaCompileAndRun.java"))
            || File.Exists(Path.Combine(path, "build", "classes", "FavaCompileAndRun.class"));
    }
}
