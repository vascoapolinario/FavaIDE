using System.IO;
using System.Text.Json;
using FavaStudio.Models;

namespace FavaStudio.Services;

public class SettingsService
{
    public string JavaPath { get; set; } = "java";
    public string CompilerRoot { get; set; } = "";
    public string AntlrJar { get; set; } = "";
    public string InputsDir { get; set; } = "";
    public string OutputsDir { get; set; } = "";
    public string ProjectRoot { get; set; } = "";
    public string InterpreterEntryFile { get; set; } = "";
    public bool ShowTestOutput { get; set; } = false;
    public List<string> RecentProjects { get; set; } = [];
    public List<string> RecentFiles { get; set; } = [];
    public Dictionary<string, ProjectMetadata> ProjectMetadata { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public string UiBackgroundColor { get; set; } = "#141C1B";
    public string UiPanelColor { get; set; } = "#20302D";
    public string UiPanelAltColor { get; set; } = "#1A2927";
    public string UiTextColor { get; set; } = "#E7F5F1";
    public string UiMutedTextColor { get; set; } = "#9DB8B0";
    public string UiAccentColor { get; set; } = "#56D6A3";
    public string UiBorderColor { get; set; } = "#314B46";
    public string EditorBackgroundColor { get; set; } = "#111817";
    public string ConsoleBackgroundColor { get; set; } = "#101615";
    public string EditorFontFamily { get; set; } = "Cascadia Mono";
    public double EditorFontSize { get; set; } = 15;
    public double ConsoleFontSize { get; set; } = 15;
    public string SyntaxColorMode { get; set; } = "default";
    public bool DiscordPresenceEnabled { get; set; } = false;
    public string DiscordDetailsMode { get; set; } = "file";
    public string DiscordStateMode { get; set; } = "projectStatus";
    public string DiscordCustomDetails { get; set; } = "Editing {file}";
    public string DiscordCustomState { get; set; } = "{project} - {status}";

    public string ResolvedAntlrJar => ResolveAntlrJar();

    public static string DataDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FavaStudio");

    public static string SettingsPath => Path.Combine(DataDirectory, "settings.json");

    public static SettingsService Load()
    {
        SettingsService settings;
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                settings = JsonSerializer.Deserialize<SettingsService>(json) ?? new SettingsService();
                settings.ProjectMetadata = new Dictionary<string, ProjectMetadata>(settings.ProjectMetadata ?? [], StringComparer.OrdinalIgnoreCase);
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
        ProjectMetadata = NormalizeProjectMetadata(ProjectMetadata);
        var dir = Path.GetDirectoryName(SettingsPath) ?? Path.GetTempPath();
        Directory.CreateDirectory(dir);
        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }

    public void ResetToDefaults()
    {
        var defaults = new SettingsService();
        ApplyBundledCompilerDefaults(defaults);

        JavaPath = defaults.JavaPath;
        CompilerRoot = defaults.CompilerRoot;
        AntlrJar = defaults.AntlrJar;
        InputsDir = "";
        OutputsDir = "";
        ProjectRoot = "";
        InterpreterEntryFile = "";
        ShowTestOutput = defaults.ShowTestOutput;
        RecentProjects.Clear();
        RecentFiles.Clear();
        ProjectMetadata.Clear();
        UiBackgroundColor = defaults.UiBackgroundColor;
        UiPanelColor = defaults.UiPanelColor;
        UiPanelAltColor = defaults.UiPanelAltColor;
        UiTextColor = defaults.UiTextColor;
        UiMutedTextColor = defaults.UiMutedTextColor;
        UiAccentColor = defaults.UiAccentColor;
        UiBorderColor = defaults.UiBorderColor;
        EditorBackgroundColor = defaults.EditorBackgroundColor;
        ConsoleBackgroundColor = defaults.ConsoleBackgroundColor;
        EditorFontFamily = defaults.EditorFontFamily;
        EditorFontSize = defaults.EditorFontSize;
        ConsoleFontSize = defaults.ConsoleFontSize;
        SyntaxColorMode = defaults.SyntaxColorMode;
        DiscordPresenceEnabled = defaults.DiscordPresenceEnabled;
        DiscordDetailsMode = defaults.DiscordDetailsMode;
        DiscordStateMode = defaults.DiscordStateMode;
        DiscordCustomDetails = defaults.DiscordCustomDetails;
        DiscordCustomState = defaults.DiscordCustomState;
    }

    public string ResolveAntlrJar()
    {
        var compilerJar = FindAntlrJarInCompilerRoot(CompilerRoot);
        if (!string.IsNullOrWhiteSpace(compilerJar))
            return compilerJar;

        return File.Exists(AntlrJar) ? AntlrJar : "";
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

    private static Dictionary<string, ProjectMetadata> NormalizeProjectMetadata(Dictionary<string, ProjectMetadata>? metadata)
    {
        var normalized = new Dictionary<string, ProjectMetadata>(StringComparer.OrdinalIgnoreCase);
        if (metadata is null)
            return normalized;

        foreach (var (path, value) in metadata)
        {
            if (string.IsNullOrWhiteSpace(path) || value is null)
                continue;
            normalized[path.Trim()] = value;
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

        var antlrJar = FindAntlrJarInCompilerRoot(compilerRoot);
        if ((string.IsNullOrWhiteSpace(settings.AntlrJar) || !File.Exists(settings.AntlrJar)) && !string.IsNullOrWhiteSpace(antlrJar))
            settings.AntlrJar = antlrJar;
    }

    private static string FindAntlrJarInCompilerRoot(string compilerRoot)
    {
        if (string.IsNullOrWhiteSpace(compilerRoot) || !Directory.Exists(compilerRoot))
            return "";

        return Directory.GetFiles(compilerRoot, "antlr-*-complete.jar", SearchOption.TopDirectoryOnly)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault() ?? "";
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
