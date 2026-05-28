using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace FavaStudio.Editor;

public sealed class FavaSyntaxColorizer : DocumentColorizingTransformer
{
    private static readonly Regex StringRegex = new("\"(?:\\\\.|[^\"\\\\])*\"?", RegexOptions.Compiled);
    private static readonly Regex CommentRegex = new("//.*$", RegexOptions.Compiled);
    private static readonly Regex TypeRegex = new(@"\b(integer|real|bool|string)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex KeywordRegex = new(@"\b(function|return|if|else|while|for|in|print|true|false|new)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex FunctionDeclarationRegex = new(@"\bfunction\s+([A-Za-z_][A-Za-z0-9_]*)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex ForEachHeaderRegex = new(@"\bfor\s+([A-Za-z_][A-Za-z0-9_]*)\s+in\s+(.+?)(?=\s*(?:\{|$))", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex IdentifierRegex = new(@"\b[A-Za-z_][A-Za-z0-9_]*\b", RegexOptions.Compiled);
    private static readonly Regex FunctionCallRegex = new(@"\b([A-Za-z_][A-Za-z0-9_]*)\s*\(", RegexOptions.Compiled);

    private static readonly HashSet<string> NonCallIdentifiers = new(StringComparer.OrdinalIgnoreCase)
    {
        "if", "while", "for", "in", "print", "function", "return", "integer", "real", "bool", "string", "true", "false", "else", "new"
    };

    protected override void ColorizeLine(DocumentLine line)
    {
        var text = CurrentContext.Document.GetText(line);
        if (string.IsNullOrWhiteSpace(text))
            return;

        ColorizeByRegex(line, text, StringRegex, new SolidColorBrush(Color.FromRgb(0xE3, 0xC7, 0x5F)));
        ColorizeByRegex(line, text, CommentRegex, new SolidColorBrush(Color.FromRgb(0x6A, 0x73, 0x7D)));
        ColorizeByRegex(line, text, TypeRegex, new SolidColorBrush(Color.FromRgb(0x5A, 0xD1, 0x8A)), FontWeights.SemiBold);
        ColorizeByRegex(line, text, KeywordRegex, new SolidColorBrush(Color.FromRgb(0x7B, 0xC1, 0xFF)), FontWeights.SemiBold);

        foreach (Match match in FunctionDeclarationRegex.Matches(text))
        {
            if (!match.Success || match.Groups.Count < 2) continue;
            var name = match.Groups[1];
            ApplyStyle(line, name.Index, name.Length, new SolidColorBrush(Color.FromRgb(0xFF, 0x9A, 0x3D)), FontWeights.Bold);
        }

        foreach (Match match in ForEachHeaderRegex.Matches(text))
        {
            if (!match.Success || match.Groups.Count < 3) continue;
            var loopVariable = match.Groups[1];
            ApplyStyle(line, loopVariable.Index, loopVariable.Length, new SolidColorBrush(Color.FromRgb(0xC5, 0x9B, 0xFF)), FontWeights.SemiBold);

            var sourceExpression = match.Groups[2];
            foreach (Match identifier in IdentifierRegex.Matches(sourceExpression.Value))
            {
                if (!identifier.Success || NonCallIdentifiers.Contains(identifier.Value)) continue;
                ApplyStyle(line, sourceExpression.Index + identifier.Index, identifier.Length, new SolidColorBrush(Color.FromRgb(0xC5, 0x9B, 0xFF)));
            }
        }

        foreach (Match match in FunctionCallRegex.Matches(text))
        {
            if (!match.Success || match.Groups.Count < 2) continue;
            var name = match.Groups[1];
            if (NonCallIdentifiers.Contains(name.Value)) continue;
            ApplyStyle(line, name.Index, name.Length, new SolidColorBrush(Color.FromRgb(0xC5, 0x9B, 0xFF)));
        }
    }

    private void ColorizeByRegex(DocumentLine line, string text, Regex regex, Brush foreground, FontWeight? weight = null)
    {
        foreach (Match match in regex.Matches(text))
        {
            if (!match.Success || match.Length == 0) continue;
            ApplyStyle(line, match.Index, match.Length, foreground, weight);
        }
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
