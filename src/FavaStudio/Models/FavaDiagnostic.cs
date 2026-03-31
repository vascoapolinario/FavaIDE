namespace FavaStudio.Models;

public class FavaDiagnostic
{
    public string Severity { get; set; } = "";
    public int Line { get; set; }
    public int Column { get; set; }
    public int UnderlineLength { get; set; } = 1;
    public string Message { get; set; } = "";

    public string Display => $"{Severity} at {Line}:{Column} — {Message}";
}
