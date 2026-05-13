namespace FavaStudio.Models;

public class TestResult
{
    public string Name { get; set; } = "";
    public string InputFile { get; set; } = "";
    public string ExpectedOutputFile { get; set; } = "";
    public bool HasRun { get; set; }
    public string Status => !HasRun ? "NEW" : Passed ? "PASS" : "FAIL";
    public bool Passed { get; set; }
    public string Message { get; set; } = "";
}
