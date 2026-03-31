namespace FavaStudio.Models;

public class TestResult
{
    public string Name { get; set; } = "";
    public string Status => Passed ? "PASS" : "FAIL";
    public bool Passed { get; set; }
    public string Message { get; set; } = "";
}
