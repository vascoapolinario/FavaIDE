using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace FavaStudio.Editor;

public sealed record FavaSyntaxPalette(
    Color String,
    Color Comment,
    Color Type,
    Color Keyword,
    Color Function,
    Color Identifier)
{
    public static FavaSyntaxPalette Default { get; } = new(
        Color.FromRgb(0xE3, 0xC7, 0x5F),
        Color.FromRgb(0x6A, 0x73, 0x7D),
        Color.FromRgb(0x5A, 0xD1, 0x8A),
        Color.FromRgb(0x7B, 0xC1, 0xFF),
        Color.FromRgb(0xFF, 0x9A, 0x3D),
        Color.FromRgb(0xC5, 0x9B, 0xFF));
}

public sealed class FavaSyntaxColorizer : DocumentColorizingTransformer
{
    private readonly Func<FavaSyntaxPalette> _paletteProvider;
    private static readonly Regex StringRegex = new("\"(?:\\\\.|[^\"\\\\])*\"?", RegexOptions.Compiled);
    private static readonly Regex LineCommentRegex = new("//.*$", RegexOptions.Compiled);
    private static readonly Regex BlockCommentRegex = new(@"/\*.*?\*/", RegexOptions.Compiled);
    private static readonly Regex TypeRegex = new(@"\b(integer|real|bool|string)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex KeywordRegex = new(@"\b(function|module|import|return|if|else|while|for|in|try|catch|exception|as|print|true|false|new)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex FunctionDeclarationRegex = new(@"\bfunction\s+([A-Za-z_][A-Za-z0-9_]*)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex ForEachHeaderRegex = new(@"\bfor\s+([A-Za-z_][A-Za-z0-9_]*)\s+in\s+(.+?)(?=\s*(?:\{|$))", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex IdentifierRegex = new(@"\b[A-Za-z_][A-Za-z0-9_]*\b", RegexOptions.Compiled);
    private static readonly Regex FunctionCallRegex = new(@"\b([A-Za-z_][A-Za-z0-9_]*)\s*\(", RegexOptions.Compiled);

    private static readonly HashSet<string> NonCallIdentifiers = new(StringComparer.OrdinalIgnoreCase)
    {
        "if", "while", "for", "in", "try", "catch", "exception", "as", "print", "function", "module", "import", "return", "integer", "real", "bool", "string", "true", "false", "else", "new"
    };

    public FavaSyntaxColorizer(Func<FavaSyntaxPalette>? paletteProvider = null)
    {
        _paletteProvider = paletteProvider ?? (() => FavaSyntaxPalette.Default);
    }

    protected override void ColorizeLine(DocumentLine line)
    {
        var text = CurrentContext.Document.GetText(line);
        if (string.IsNullOrWhiteSpace(text))
            return;

        var palette = _paletteProvider();
        var commentRanges = FindCommentRanges(line, text);
        ColorizeByRegex(line, text, StringRegex, BrushOf(palette.String), excludedRanges: commentRanges);
        ColorizeByRegex(line, text, TypeRegex, BrushOf(palette.Type), FontWeights.SemiBold, commentRanges);
        ColorizeByRegex(line, text, KeywordRegex, BrushOf(palette.Keyword), FontWeights.SemiBold, commentRanges);

        foreach (Match match in FunctionDeclarationRegex.Matches(text))
        {
            if (!match.Success || match.Groups.Count < 2) continue;
            var name = match.Groups[1];
            if (IntersectsAny(name.Index, name.Length, commentRanges)) continue;
            ApplyStyle(line, name.Index, name.Length, BrushOf(palette.Function), FontWeights.Bold);
        }

        foreach (Match match in ForEachHeaderRegex.Matches(text))
        {
            if (!match.Success || match.Groups.Count < 3) continue;
            var loopVariable = match.Groups[1];
            if (IntersectsAny(loopVariable.Index, loopVariable.Length, commentRanges)) continue;
            ApplyStyle(line, loopVariable.Index, loopVariable.Length, BrushOf(palette.Identifier), FontWeights.SemiBold);

            var sourceExpression = match.Groups[2];
            foreach (Match identifier in IdentifierRegex.Matches(sourceExpression.Value))
            {
                if (!identifier.Success || NonCallIdentifiers.Contains(identifier.Value)) continue;
                if (IntersectsAny(sourceExpression.Index + identifier.Index, identifier.Length, commentRanges)) continue;
                ApplyStyle(line, sourceExpression.Index + identifier.Index, identifier.Length, BrushOf(palette.Identifier));
            }
        }

        foreach (Match match in FunctionCallRegex.Matches(text))
        {
            if (!match.Success || match.Groups.Count < 2) continue;
            var name = match.Groups[1];
            if (NonCallIdentifiers.Contains(name.Value)) continue;
            if (IntersectsAny(name.Index, name.Length, commentRanges)) continue;
            ApplyStyle(line, name.Index, name.Length, BrushOf(palette.Identifier));
        }

        var commentBrush = BrushOf(palette.Comment);
        foreach (var (start, length) in commentRanges)
            ApplyStyle(line, start, length, commentBrush);
    }

    private static SolidColorBrush BrushOf(Color color) => new(color);

    private void ColorizeByRegex(DocumentLine line, string text, Regex regex, Brush foreground, FontWeight? weight = null, IReadOnlyList<(int Start, int Length)>? excludedRanges = null)
    {
        foreach (Match match in regex.Matches(text))
        {
            if (!match.Success || match.Length == 0) continue;
            if (IntersectsAny(match.Index, match.Length, excludedRanges)) continue;
            ApplyStyle(line, match.Index, match.Length, foreground, weight);
        }
    }

    private IReadOnlyList<(int Start, int Length)> FindCommentRanges(DocumentLine line, string text)
    {
        var ranges = new List<(int Start, int Length)>();
        foreach (Match match in BlockCommentRegex.Matches(text))
            ranges.Add((match.Index, match.Length));
        foreach (Match match in LineCommentRegex.Matches(text))
            ranges.Add((match.Index, match.Length));

        if (IsInsideOpenBlockCommentBeforeLine(line))
            ranges.Add((0, text.Length));
        return ranges;
    }

    private bool IsInsideOpenBlockCommentBeforeLine(DocumentLine line)
    {
        var document = CurrentContext.Document;
        var beforeLine = document.GetText(0, line.Offset);
        var lastOpen = beforeLine.LastIndexOf("/*", StringComparison.Ordinal);
        if (lastOpen < 0)
            return false;
        var lastClose = beforeLine.LastIndexOf("*/", StringComparison.Ordinal);
        return lastClose < lastOpen;
    }

    private static bool IntersectsAny(int start, int length, IReadOnlyList<(int Start, int Length)>? ranges)
    {
        if (ranges is null || ranges.Count == 0)
            return false;

        var end = start + length;
        return ranges.Any(range => start < range.Start + range.Length && end > range.Start);
    }

    private void ApplyStyle(DocumentLine line, int indexInLine, int length, Brush foreground, FontWeight? weight = null)
    {
        ChangeLinePart(line.Offset + indexInLine, line.Offset + indexInLine + length, element =>
        {
            element.TextRunProperties.SetForegroundBrush(foreground);
            if (weight.HasValue)
                element.TextRunProperties.SetTypeface(new Typeface(
                    element.TextRunProperties.Typeface.FontFamily,
                    element.TextRunProperties.Typeface.Style,
                    weight.Value,
                    element.TextRunProperties.Typeface.Stretch));
        });
    }
}
