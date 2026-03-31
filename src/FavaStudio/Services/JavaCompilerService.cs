using System.Diagnostics;

namespace FavaStudio.Services;

public class JavaCompilerService
{
    private readonly SettingsService _settings;

    public JavaCompilerService(SettingsService settings)
    {
        _settings = settings;
    }

    public async Task<(bool Success, string Output)> RunFileAsync(string filePath)
    {
        var result = await EnsureCompiledAsync();
        if (!result.Success) return result;

        var classesDir = Path.Combine(_settings.CompilerRoot, "build", "classes");
        var classpath = $"{classesDir};{_settings.AntlrJar}";

        var psi = new ProcessStartInfo
        {
            FileName = _settings.JavaPath,
            Arguments = $"-cp \"{classpath}\" FavaCompileAndRun \"{filePath}\"",
            WorkingDirectory = Path.Combine(_settings.CompilerRoot, "src"),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var proc = Process.Start(psi);
        if (proc is null)
            return (false, "Failed to start Java process. Check that Java is installed and the path is correct.");

        var stdout = await proc.StandardOutput.ReadToEndAsync();
        var stderr = await proc.StandardError.ReadToEndAsync();
        await proc.WaitForExitAsync();

        var output = stdout + (string.IsNullOrWhiteSpace(stderr) ? "" : "\n" + stderr);
        return (proc.ExitCode == 0, output);
    }

    private async Task<(bool Success, string Output)> EnsureCompiledAsync()
    {
        var classesDir = Path.Combine(_settings.CompilerRoot, "build", "classes");
        if (Directory.Exists(classesDir) && Directory.GetFiles(classesDir, "*.class", SearchOption.AllDirectories).Any())
            return (true, "Already compiled");

        Directory.CreateDirectory(classesDir);

        var javaFiles = Directory.GetFiles(Path.Combine(_settings.CompilerRoot, "src"), "*.java", SearchOption.AllDirectories);
        var filesArg = string.Join(" ", javaFiles.Select(f => $"\"{f}\""));

        var psi = new ProcessStartInfo
        {
            FileName = "javac",
            Arguments = $"-cp \"{_settings.AntlrJar}\" -d \"{classesDir}\" {filesArg}",
            WorkingDirectory = Path.Combine(_settings.CompilerRoot, "src"),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var proc = Process.Start(psi);
        if (proc is null)
            return (false, "Failed to start javac. Check that the JDK is installed.");

        var stdout = await proc.StandardOutput.ReadToEndAsync();
        var stderr = await proc.StandardError.ReadToEndAsync();
        await proc.WaitForExitAsync();

        var output = stdout + (string.IsNullOrWhiteSpace(stderr) ? "" : "\n" + stderr);
        return (proc.ExitCode == 0, output);
    }
}
