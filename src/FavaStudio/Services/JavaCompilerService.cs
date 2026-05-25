using System.Diagnostics;
using System.IO;

namespace FavaStudio.Services;

public class JavaCompilerService
{
    private readonly SettingsService _settings;

    public JavaCompilerService(SettingsService settings)
    {
        _settings = settings;
    }

    public async Task<(bool Success, string Output)> RunFileAsync(string filePath, bool includeTrace = false)
    {
        var result = await EnsureCompiledAsync();
        if (!result.Success) return result;

        var layout = ResolveCompilerLayout();
        var classpath = string.Join(Path.PathSeparator, layout.ClassesDir, _settings.AntlrJar);

        var psi = new ProcessStartInfo
        {
            FileName = _settings.JavaPath,
            Arguments = includeTrace
                ? $"-cp \"{classpath}\" FavaCompileAndRun \"{filePath}\" -trace"
                : $"-cp \"{classpath}\" FavaCompileAndRun \"{filePath}\"",
            WorkingDirectory = layout.SourceDir,
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
        var layout = ResolveCompilerLayout();
        var javaFiles = Directory.GetFiles(layout.SourceDir, "*.java", SearchOption.AllDirectories);
        var classesDir = layout.ClassesDir;
        if (!NeedsCompile(classesDir, javaFiles))
            return (true, "Already compiled");

        Directory.CreateDirectory(classesDir);
        var filesArg = string.Join(" ", javaFiles.Select(f => $"\"{f}\""));

        var psi = new ProcessStartInfo
        {
            FileName = "javac",
            Arguments = $"-cp \"{_settings.AntlrJar}\" -d \"{classesDir}\" {filesArg}",
            WorkingDirectory = layout.SourceDir,
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

    private (string SourceDir, string ClassesDir) ResolveCompilerLayout()
    {
        var sourceDir = Path.Combine(_settings.CompilerRoot, "src");
        if (!File.Exists(Path.Combine(sourceDir, "FavaCompileAndRun.java")))
            sourceDir = _settings.CompilerRoot;

        return (sourceDir, Path.Combine(_settings.CompilerRoot, "build", "classes"));
    }

    private static bool NeedsCompile(string classesDir, IReadOnlyCollection<string> javaFiles)
    {
        if (!Directory.Exists(classesDir))
            return true;

        var classFiles = Directory.GetFiles(classesDir, "*.class", SearchOption.AllDirectories);
        if (classFiles.Length == 0)
            return true;

        var newestJava = javaFiles.Select(File.GetLastWriteTimeUtc).DefaultIfEmpty(DateTime.MinValue).Max();
        var newestClass = classFiles.Select(File.GetLastWriteTimeUtc).DefaultIfEmpty(DateTime.MinValue).Max();
        return newestJava > newestClass;
    }
}
