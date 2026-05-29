using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace FavaStudio.Services;

public class JavaCompilerService
{
    private const int MaxCapturedOutputChars = 1_000_000;
    private readonly SettingsService _settings;

    public JavaCompilerService(SettingsService settings)
    {
        _settings = settings;
    }

    public async Task<(bool Success, string Output)> RunFileAsync(
        string filePath,
        bool includeTrace = false,
        bool checkOnly = false,
        Action<string>? onOutputChanged = null,
        Action<Func<string, Task>?>? onInputWriterChanged = null,
        CancellationToken cancellationToken = default)
    {
        var result = await EnsureCompiledAsync(cancellationToken);
        if (!result.Success) return result;

        var layout = ResolveCompilerLayout();
        var classpath = string.Join(Path.PathSeparator, layout.ClassesDir, _settings.AntlrJar);

        var psi = new ProcessStartInfo
        {
            FileName = _settings.JavaPath,
            Arguments = BuildRunArguments(classpath, filePath, ResolveProjectRoot(filePath), includeTrace, checkOnly),
            WorkingDirectory = GetRuntimeWorkingDirectory(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var proc = Process.Start(psi);
        if (proc is null)
            return (false, "Failed to start Java process. Check that Java is installed and the path is correct.");

        var outputBuilder = new StringBuilder();
        var inputEchoTruncated = false;
        void AppendInputEcho(string line)
        {
            string snapshot;
            lock (outputBuilder)
            {
                AppendCapturedText(outputBuilder, line + Environment.NewLine, ref inputEchoTruncated);
                snapshot = outputBuilder.ToString();
            }
            onOutputChanged?.Invoke(snapshot);
        }

        onInputWriterChanged?.Invoke(async line =>
        {
            if (!proc.HasExited)
            {
                AppendInputEcho(line);
                await proc.StandardInput.WriteLineAsync(line);
                await proc.StandardInput.FlushAsync();
            }
        });

        var stdoutTask = ReadStreamAsync(proc.StandardOutput, outputBuilder, onOutputChanged);
        var stderrTask = ReadStreamAsync(proc.StandardError, outputBuilder, onOutputChanged);
        try
        {
            await proc.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            KillProcessTree(proc);
            await proc.WaitForExitAsync();
            throw;
        }
        await Task.WhenAll(stdoutTask, stderrTask);
        onInputWriterChanged?.Invoke(null);

        return (proc.ExitCode == 0, outputBuilder.ToString());
    }

    private string ResolveProjectRoot(string filePath)
    {
        if (!string.IsNullOrWhiteSpace(_settings.ProjectRoot) && Directory.Exists(_settings.ProjectRoot))
            return _settings.ProjectRoot;

        return Path.GetDirectoryName(Path.GetFullPath(filePath)) ?? Directory.GetCurrentDirectory();
    }

    private static string BuildRunArguments(string classpath, string filePath, string projectRoot, bool includeTrace, bool checkOnly)
    {
        var args = new StringBuilder($"-cp \"{classpath}\" FavaCompileAndRun \"{filePath}\" -root \"{projectRoot}\"");
        if (includeTrace)
            args.Append(" -trace");
        if (checkOnly)
            args.Append(" -check");
        return args.ToString();
    }

    private static async Task ReadStreamAsync(StreamReader reader, StringBuilder outputBuilder, Action<string>? onOutputChanged)
    {
        var buffer = new char[4096];
        var truncated = false;
        while (true)
        {
            var read = await reader.ReadAsync(buffer, 0, buffer.Length);
            if (read == 0)
                break;

            string snapshot;
            lock (outputBuilder)
            {
                AppendCapturedText(outputBuilder, new string(buffer, 0, read), ref truncated);
                snapshot = outputBuilder.ToString();
            }
            onOutputChanged?.Invoke(snapshot);
        }
    }

    private static void AppendCapturedText(StringBuilder outputBuilder, string text, ref bool truncated)
    {
        if (outputBuilder.Length < MaxCapturedOutputChars)
        {
            var remaining = MaxCapturedOutputChars - outputBuilder.Length;
            if (text.Length <= remaining)
            {
                outputBuilder.Append(text);
            }
            else
            {
                outputBuilder.Append(text.AsSpan(0, remaining));
            }
        }
        else if (!truncated)
        {
            outputBuilder.AppendLine();
            outputBuilder.Append("[output truncated after 1000000 characters]");
            truncated = true;
        }
    }

    private async Task<(bool Success, string Output)> EnsureCompiledAsync(CancellationToken cancellationToken = default)
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

        var outputBuilder = new StringBuilder();
        var stdoutTask = ReadStreamAsync(proc.StandardOutput, outputBuilder, null);
        var stderrTask = ReadStreamAsync(proc.StandardError, outputBuilder, null);
        try
        {
            await proc.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            KillProcessTree(proc);
            await proc.WaitForExitAsync();
            throw;
        }
        await Task.WhenAll(stdoutTask, stderrTask);

        return (proc.ExitCode == 0, outputBuilder.ToString());
    }

    private static void KillProcessTree(Process process)
    {
        try
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
        }
        catch (InvalidOperationException)
        {
        }
    }

    private (string SourceDir, string ClassesDir) ResolveCompilerLayout()
    {
        var sourceDir = Path.Combine(_settings.CompilerRoot, "src");
        if (!File.Exists(Path.Combine(sourceDir, "FavaCompileAndRun.java")))
            sourceDir = _settings.CompilerRoot;

        return (sourceDir, Path.Combine(_settings.CompilerRoot, "build", "classes"));
    }

    private static string GetRuntimeWorkingDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), "FavaStudio", "runtime");
        Directory.CreateDirectory(directory);
        return directory;
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
