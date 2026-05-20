using FavaStudio.Models;
using System.Diagnostics;
using System.IO;

namespace FavaStudio.Services;

public class TestRunnerService
{
    private readonly SettingsService _settings;

    public TestRunnerService(SettingsService settings)
    {
        _settings = settings;
    }

    public async Task<List<TestResult>> RunAllTestsAsync()
    {
        var cases = DiscoverTests();
        var results = new List<TestResult>();

        foreach (var tc in cases)
        {
            results.Add(await RunTestAsync(tc));
        }
        return results;
    }

    public async Task<TestResult> RunSingleTestAsync(string name)
    {
        var tc = DiscoverTests().FirstOrDefault(t => t.Name == name);
        if (tc is null)
            return new TestResult { Name = name, Passed = false, HasRun = true, Message = $"Test '{name}' not found." };
        return await RunTestAsync(tc);
    }

    private List<TestCase> DiscoverTests()
    {
        if (string.IsNullOrWhiteSpace(_settings.InputsDir) || !Directory.Exists(_settings.InputsDir))
            return [];

        if (string.IsNullOrWhiteSpace(_settings.OutputsDir))
            return [];

        var inputs = Directory.GetFiles(_settings.InputsDir, "*.fava");
        var cases = new List<TestCase>();

        foreach (var input in inputs)
        {
            var name = Path.GetFileNameWithoutExtension(input);
            var expected = Path.Combine(_settings.OutputsDir, $"{name}.txt");
            cases.Add(new TestCase { Name = name, InputFile = input, ExpectedOutputFile = expected });
        }
        return cases;
    }

    private async Task<TestResult> RunTestAsync(TestCase tc)
    {
        var stopwatch = Stopwatch.StartNew();
        var runner = new JavaCompilerService(_settings);
        var result = await runner.RunFileAsync(tc.InputFile);
        stopwatch.Stop();

        if (!result.Success)
        {
            return new TestResult
            {
                Name = tc.Name,
                InputFile = tc.InputFile,
                ExpectedOutputFile = tc.ExpectedOutputFile,
                HasRun = true,
                Passed = false,
                Duration = stopwatch.Elapsed,
                ActualOutput = result.Output,
                DiffOutput = "Compiler/runtime error:\n" + result.Output,
                Message = "Compiler/runtime error."
            };
        }

        var expected = File.Exists(tc.ExpectedOutputFile) ? File.ReadAllText(tc.ExpectedOutputFile) : "";
        var actual = Normalize(result.Output);
        var exp = Normalize(expected);

        var passed = actual == exp;
        var diff = passed ? "No differences." : BuildDiff(exp, actual);

        return new TestResult
        {
            Name = tc.Name,
            InputFile = tc.InputFile,
            ExpectedOutputFile = tc.ExpectedOutputFile,
            HasRun = true,
            Passed = passed,
            Duration = stopwatch.Elapsed,
            ExpectedOutput = exp,
            ActualOutput = actual,
            DiffOutput = diff,
            Message = passed
                ? "Matched expected output."
                : _settings.ShowTestOutput
                    ? $"Expected:\n{expected}\n\nGot:\n{result.Output}"
                    : "Output mismatch."
        };
    }

    private static string Normalize(string s) =>
        s.Replace("\r\n", "\n").Trim();

    private static string BuildDiff(string expected, string actual)
    {
        var expectedLines = expected.Split('\n');
        var actualLines = actual.Split('\n');
        var max = Math.Max(expectedLines.Length, actualLines.Length);
        var lines = new List<string>();

        for (var i = 0; i < max && lines.Count < 200; i++)
        {
            var exp = i < expectedLines.Length ? expectedLines[i] : "";
            var act = i < actualLines.Length ? actualLines[i] : "";
            if (exp == act)
                continue;

            lines.Add($"Line {i + 1}");
            lines.Add($"  expected: {exp}");
            lines.Add($"  actual:   {act}");
        }

        if (lines.Count == 0)
            return "Outputs differ only by normalization.";
        if (max > 200)
            lines.Add("Diff truncated.");
        return string.Join(Environment.NewLine, lines);
    }
}
