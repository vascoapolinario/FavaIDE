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

    private static string SettingsPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                     "FavaStudio", "settings.json");

    public static SettingsService Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<SettingsService>(json) ?? new SettingsService();
            }
        }
        catch { }
        return new SettingsService();
    }

    public void Save()
    {
        var dir = Path.GetDirectoryName(SettingsPath) ?? Path.GetTempPath();
        Directory.CreateDirectory(dir);
        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }
}
