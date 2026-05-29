namespace FavaStudio.Models;

public class FavaDiagnostic
{
    public string Severity { get; set; } = "";
    public int Line { get; set; }
    public int Column { get; set; }
    public int UnderlineLength { get; set; } = 1;
    public string Message { get; set; } = "";
    public string SourceLine { get; set; } = "";
    public string Explanation { get; set; } = "";

    public string Display => $"{Severity} at {Line}:{Column} - {Message}";
    public string Tooltip =>
        string.IsNullOrWhiteSpace(SourceLine)
            ? BuildTooltip(includeSource: false)
            : BuildTooltip(includeSource: true);    

    private string BuildTooltip(bool includeSource)
    {
        var tooltip = $"{Severity}\nLine {Line}, column {Column}\n\n{Message}";
        if (!string.IsNullOrWhiteSpace(Explanation))
            tooltip += $"\n\n{Explanation}";
        if (includeSource)
            tooltip += $"\n\n{SourceLine.TrimEnd()}";
        return tooltip;
    }
}
