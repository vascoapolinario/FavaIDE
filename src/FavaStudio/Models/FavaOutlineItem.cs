namespace FavaStudio.Models;

public sealed class FavaOutlineItem
{
    public string Kind { get; init; } = "";
    public string Name { get; init; } = "";
    public string Type { get; init; } = "";
    public int Line { get; init; }
    public int Column { get; init; }

    public string Badge => Kind.Contains("function", StringComparison.OrdinalIgnoreCase) ? "fn" : "var";
    public string DisplayKind => Kind.Replace(" definition", "", StringComparison.OrdinalIgnoreCase).Replace('_', ' ');
    public string Detail => string.IsNullOrWhiteSpace(Type) ? $"line {Line}" : $"{Type} - line {Line}";
}
