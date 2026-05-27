namespace FavaStudio.Models;

public sealed class FavaHoverInfo
{
    public int Line { get; init; }
    public int StartColumn { get; init; }
    public int EndColumn { get; init; }
    public string Kind { get; init; } = "";
    public string Name { get; init; } = "";
    public string Type { get; init; } = "";
    public int Address { get; init; } = -1;
    public bool IsDefinition => Kind.Contains("definition", StringComparison.OrdinalIgnoreCase);

    public string Tooltip
    {
        get
        {
            var subject = string.IsNullOrWhiteSpace(Name) ? DisplayKind : $"{DisplayKind} {Name}";
            return $"{subject}\nType: {Type}";
        }
    }

    public string DisplayKind => Kind.Replace(" definition", "", StringComparison.OrdinalIgnoreCase).Replace('_', ' ');
}
