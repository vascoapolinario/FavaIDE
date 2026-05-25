namespace FavaStudio.Models;

using System.IO;

public class TestResult
{
    public string Name { get; set; } = "";
    public string InputFile { get; set; } = "";
    public string ExpectedOutputFile { get; set; } = "";
    public string InputFileName => string.IsNullOrWhiteSpace(InputFile) ? "(missing input)" : Path.GetFileName(InputFile);
    public string ExpectedOutputFileName => string.IsNullOrWhiteSpace(ExpectedOutputFile) ? "(missing output)" : Path.GetFileName(ExpectedOutputFile);
    public bool HasRun { get; set; }
    public string Status => !HasRun ? "NEW" : Passed ? "PASS" : "FAIL";
    public bool Passed { get; set; }
    public string Message { get; set; } = "";
    public string ExpectedOutput { get; set; } = "";
    public string ActualOutput { get; set; } = "";
    public string DiffOutput { get; set; } = "";
    public TimeSpan Duration { get; set; }
    public string DurationText => HasRun ? $"{Duration.TotalMilliseconds:0} ms" : "--";
}
