using System.Text.RegularExpressions;
using FavaStudio.Models;

namespace FavaStudio.Services;

public static class DeadCodeAnalyzer
{
    public static IReadOnlyList<FavaDiagnostic> Analyze(string source, IReadOnlyList<FavaHoverInfo> hoverInfos)
    {
        var diagnostics = new List<FavaDiagnostic>();
        AddUnreachableCodeDiagnostics(source, diagnostics);
        AddUnusedSymbolDiagnostics(source, hoverInfos, diagnostics);
        return diagnostics
            .GroupBy(d => new { d.Line, d.Column, d.Message })
            .Select(g => g.First())
            .OrderBy(d => d.Line)
            .ThenBy(d => d.Column)
            .ToList();
    }

    private static void AddUnreachableCodeDiagnostics(string source, List<FavaDiagnostic> diagnostics)
    {
        var lines = source.Replace("\r\n", "\n").Split('\n');
        var unreachableDepths = new Stack<int>();
        var depth = 0;

        for (var i = 0; i < lines.Length; i++)
        {
            var lineNumber = i + 1;
            var line = lines[i];
            var code = StripLineComment(line);
            var trimmed = code.Trim();

            while (unreachableDepths.Count > 0 && depth < unreachableDepths.Peek())
                unreachableDepths.Pop();

            if (unreachableDepths.Count > 0 && IsExecutable(trimmed))
            {
                diagnostics.Add(BuildWarning(
                    lineNumber,
                    FirstNonWhitespaceColumn(line),
                    Math.Max(1, trimmed.Length),
                    "Unreachable code after return.",
                    line,
                    "This statement cannot execute because a previous return exits the current function or block."));
            }

            if (Regex.IsMatch(trimmed, @"^return\b", RegexOptions.IgnoreCase))
                unreachableDepths.Push(depth + 1);

            foreach (var ch in code)
            {
                if (ch == '{')
                    depth++;
                else if (ch == '}')
                    depth = Math.Max(0, depth - 1);
            }
        }
    }

    private static void AddUnusedSymbolDiagnostics(string source, IReadOnlyList<FavaHoverInfo> hoverInfos, List<FavaDiagnostic> diagnostics)
    {
        var lines = source.Replace("\r\n", "\n").Split('\n');
        var definitions = hoverInfos
            .Where(info => info.IsDefinition && !string.IsNullOrWhiteSpace(info.Name))
            .Where(info => !info.Kind.Contains("function", StringComparison.OrdinalIgnoreCase) ||
                           !string.Equals(info.Name, "main", StringComparison.Ordinal))
            .ToList();

        foreach (var definition in definitions)
        {
            var hasReference = hoverInfos.Any(info =>
                !info.IsDefinition &&
                string.Equals(info.Name, definition.Name, StringComparison.Ordinal) &&
                IsSameSymbolKind(definition, info));

            if (hasReference)
                continue;

            var line = definition.Line > 0 && definition.Line <= lines.Length ? lines[definition.Line - 1] : "";
            var label = definition.Kind.Contains("function", StringComparison.OrdinalIgnoreCase) ? "Function" : "Variable";
            diagnostics.Add(BuildWarning(
                definition.Line,
                definition.StartColumn,
                Math.Max(1, definition.EndColumn - definition.StartColumn),
                $"{label} '{definition.Name}' is never used.",
                line,
                "The compiler accepted this symbol, but no expression or call currently references it."));
        }
    }

    private static bool IsSameSymbolKind(FavaHoverInfo definition, FavaHoverInfo reference)
    {
        if (definition.Kind.Contains("function", StringComparison.OrdinalIgnoreCase))
            return reference.Kind.Contains("function", StringComparison.OrdinalIgnoreCase) ||
                   reference.Kind.Contains("call", StringComparison.OrdinalIgnoreCase);

        return !reference.Kind.Contains("function", StringComparison.OrdinalIgnoreCase) &&
               !reference.Kind.Contains("call", StringComparison.OrdinalIgnoreCase);
    }

    private static FavaDiagnostic BuildWarning(int line, int column, int length, string message, string sourceLine, string explanation) =>
        new()
        {
            Severity = "Warning",
            Line = line,
            Column = Math.Max(1, column),
            UnderlineLength = Math.Max(1, length),
            Message = message,
            SourceLine = sourceLine,
            Explanation = explanation
        };

    private static string StripLineComment(string line)
    {
        var index = line.IndexOf("//", StringComparison.Ordinal);
        return index < 0 ? line : line[..index];
    }

    private static int FirstNonWhitespaceColumn(string line)
    {
        for (var i = 0; i < line.Length; i++)
        {
            if (!char.IsWhiteSpace(line[i]))
                return i + 1;
        }
        return 1;
    }

    private static bool IsExecutable(string trimmed) =>
        !string.IsNullOrWhiteSpace(trimmed) &&
        trimmed != "{" &&
        trimmed != "}" &&
        !trimmed.StartsWith("//", StringComparison.Ordinal);
}
