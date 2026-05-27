namespace FavaStudio.Models;

public sealed class FavaReferenceItem
{
    public string Name { get; init; } = "";
    public string Kind { get; init; } = "";
    public string Type { get; init; } = "";
    public int Line { get; init; }
    public int Column { get; init; }
    public string Preview { get; init; } = "";
    public bool IsDefinition { get; init; }

    public string Badge => IsDefinition ? "decl" : "ref";
    public string DisplayKind => Kind.Replace(" definition", "", StringComparison.OrdinalIgnoreCase).Replace('_', ' ');
    public string Location => $"{Line}:{Column}";
    public string Detail => string.IsNullOrWhiteSpace(Type) ? DisplayKind : $"{DisplayKind} | {Type}";
}
