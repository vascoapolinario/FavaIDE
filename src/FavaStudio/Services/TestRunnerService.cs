using FavaStudio.Models;

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
            return new TestResult { Name = name, Passed = false, Message = $"Test '{name}' not found." };
        return await RunTestAsync(tc);
    }

    private List<TestCase> DiscoverTests()
    {
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
        var runner = new JavaCompilerService(_settings);
        var result = await runner.RunFileAsync(tc.InputFile);

        if (!result.Success)
        {
            return new TestResult
            {
                Name = tc.Name,
                Passed = false,
                Message = "Compiler/runtime error:\n" + result.Output
            };
        }

        var expected = File.Exists(tc.ExpectedOutputFile) ? File.ReadAllText(tc.ExpectedOutputFile) : "";
        var actual = Normalize(result.Output);
        var exp = Normalize(expected);

        var passed = actual == exp;

        return new TestResult
        {
            Name = tc.Name,
            Passed = passed,
            Message = passed
                ? "Matched expected output."
                : _settings.ShowTestOutput
                    ? $"Expected:\n{expected}\n\nGot:\n{result.Output}"
                    : "Output mismatch."
        };
    }

    private static string Normalize(string s) =>
        s.Replace("\r\n", "\n").Trim();
}
