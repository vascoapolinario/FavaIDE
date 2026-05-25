using System.Collections.Generic;
using System.Text.RegularExpressions;
using FavaStudio.Models;

namespace FavaStudio.Services;

public static class DiagnosticsParser
{
    // Matches: "parser error at line 1:8 - extraneous input ';' expecting {"
    //          "lexer error at line 2:3 - token recognition error at: '$'"
    private static readonly Regex _lexerParserPattern = new(
        @"^(lexer|parser) error at line (\d+):(\d+) - (.+)$",
        RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Matches ANTLR's default form: "line 1:8 error: extraneous input ';' expecting {"
    private static readonly Regex _antlrPattern = new(
        @"^line\s+(\d+):(\d+)\s+error:\s*(.+)$",
        RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Matches: "error in line 2: x already declared"
    private static readonly Regex _semanticPattern = new(
        @"^error in line (\d+):\s*(.+)$",
        RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Matches: "semantic error at line 2:7 - x already declared"
    private static readonly Regex _semanticWithColumnPattern = new(
        @"^semantic error at line (\d+):(\d+) - (.+)$",
        RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static IReadOnlyList<FavaDiagnostic> Parse(string output, string sourceText = "")
    {
        var results = new List<FavaDiagnostic>();
        var sourceLines = sourceText.Replace("\r\n", "\n").Split('\n');

        foreach (Match m in _lexerParserPattern.Matches(output))
        {
            if (!int.TryParse(m.Groups[2].Value, out var line) ||
                !int.TryParse(m.Groups[3].Value, out var column))
                continue;

            results.Add(Enrich(new FavaDiagnostic
            {
                Severity = m.Groups[1].Value.ToLower() == "lexer" ? "Lexer Error" : "Parser Error",
                Line = line,
                Column = column,
                UnderlineLength = 1,
                Message = m.Groups[4].Value.Trim()
            }, sourceLines));
        }

        foreach (Match m in _antlrPattern.Matches(output))
        {
            if (!int.TryParse(m.Groups[1].Value, out var line) ||
                !int.TryParse(m.Groups[2].Value, out var column))
                continue;

            results.Add(Enrich(new FavaDiagnostic
            {
                Severity = "Parser Error",
                Line = line,
                Column = column + 1,
                UnderlineLength = 1,
                Message = m.Groups[3].Value.Trim()
            }, sourceLines));
        }

        foreach (Match m in _semanticWithColumnPattern.Matches(output))
        {
            if (!int.TryParse(m.Groups[1].Value, out var line) ||
                !int.TryParse(m.Groups[2].Value, out var column))
                continue;

            results.Add(Enrich(new FavaDiagnostic
            {
                Severity = "Semantic Error",
                Line = line,
                Column = column,
                UnderlineLength = 1,
                Message = m.Groups[3].Value.Trim()
            }, sourceLines));
        }

        foreach (Match m in _semanticPattern.Matches(output))
        {
            if (!int.TryParse(m.Groups[1].Value, out var line))
                continue;

            results.Add(Enrich(new FavaDiagnostic
            {
                Severity = "Semantic Error",
                Line = line,
                Column = 1,
                UnderlineLength = 1,
                Message = m.Groups[2].Value.Trim()
            }, sourceLines));
        }

        return results.OrderBy(d => d.Line).ThenBy(d => d.Column).ToList();
    }

    private static FavaDiagnostic Enrich(FavaDiagnostic diagnostic, IReadOnlyList<string> sourceLines)
    {
        if (diagnostic.Line <= 0 || diagnostic.Line > sourceLines.Count)
            return diagnostic;

        var sourceLine = sourceLines[diagnostic.Line - 1];
        diagnostic.SourceLine = sourceLine;

        if (diagnostic.Severity == "Semantic Error")
            InferSemanticRange(diagnostic, sourceLine);
        else
            ClampRange(diagnostic, sourceLine);

        return diagnostic;
    }

    private static void InferSemanticRange(FavaDiagnostic diagnostic, string sourceLine)
    {
        var message = diagnostic.Message;
        diagnostic.Explanation = Explain(message);

        var operatorError = Regex.Match(message, @"^operator\s+(.+?)\s+is invalid\b", RegexOptions.IgnoreCase);
        if (operatorError.Success && TrySetRangeForToken(diagnostic, sourceLine, operatorError.Groups[1].Value))
            return;

        if (message.Contains("operator :=", StringComparison.OrdinalIgnoreCase) &&
            TrySetAssignmentRange(diagnostic, sourceLine))
            return;

        var namedFunction = Regex.Match(message, @"^function\s+([A-Za-z_][A-Za-z0-9_]*)\b", RegexOptions.IgnoreCase);
        if (namedFunction.Success && TrySetRangeForToken(diagnostic, sourceLine, namedFunction.Groups[1].Value))
            return;

        var target = Regex.Match(message, @"^([A-Za-z_][A-Za-z0-9_]*)\s+(already declared|not declared|is not)", RegexOptions.IgnoreCase);
        if (target.Success && TrySetRangeForIdentifier(diagnostic, sourceLine, target.Groups[1].Value))
            return;

        if (message.Contains("cannot print", StringComparison.OrdinalIgnoreCase) &&
            TrySetAfterKeywordRange(diagnostic, sourceLine, "print"))
            return;

        if (message.Contains("array size", StringComparison.OrdinalIgnoreCase) &&
            TrySetBracketContentRange(diagnostic, sourceLine, preferLast: true))
            return;

        if (message.Contains("array index", StringComparison.OrdinalIgnoreCase) &&
            TrySetBracketContentRange(diagnostic, sourceLine, preferLast: false))
            return;

        if (message.Contains("while expression", StringComparison.OrdinalIgnoreCase) &&
            TrySetConditionRange(diagnostic, sourceLine, "while"))
            return;

        if (message.Contains("if expression", StringComparison.OrdinalIgnoreCase) &&
            TrySetConditionRange(diagnostic, sourceLine, "if"))
            return;

        if (message.Contains("must return", StringComparison.OrdinalIgnoreCase) &&
            TrySetRangeForToken(diagnostic, sourceLine, "return"))
            return;

        if (message.Contains("missing return", StringComparison.OrdinalIgnoreCase) &&
            TrySetRangeForToken(diagnostic, sourceLine, "}"))
            return;

        var firstToken = Regex.Match(sourceLine, @"\S+");
        if (firstToken.Success)
        {
            diagnostic.Column = firstToken.Index + 1;
            diagnostic.UnderlineLength = Math.Max(1, firstToken.Length);
            return;
        }

        diagnostic.Column = 1;
        diagnostic.UnderlineLength = 1;
    }

    private static string Explain(string message)
    {
        if (message.Contains("not declared", StringComparison.OrdinalIgnoreCase))
            return "This name is used before the compiler can find a matching variable or function declaration.";
        if (message.Contains("already declared", StringComparison.OrdinalIgnoreCase))
            return "This scope already has a symbol with that name.";
        if (message.Contains("operator :=", StringComparison.OrdinalIgnoreCase))
            return "The expression on the right side cannot be assigned to the target type.";
        if (message.StartsWith("operator ", StringComparison.OrdinalIgnoreCase))
            return "The operand types do not support this operator.";
        if (message.Contains("array index", StringComparison.OrdinalIgnoreCase))
            return "Array indexes must be integer expressions.";
        if (message.Contains("array size", StringComparison.OrdinalIgnoreCase))
            return "Array allocation sizes must be integer expressions.";
        if (message.Contains("cannot print", StringComparison.OrdinalIgnoreCase))
            return "Only scalar values can be printed directly.";
        if (message.Contains("expects", StringComparison.OrdinalIgnoreCase) && message.Contains("arguments", StringComparison.OrdinalIgnoreCase))
            return "The call has a different number of arguments than the function declaration.";
        if (message.Contains("argument of function", StringComparison.OrdinalIgnoreCase))
            return "One of the call arguments does not match the parameter type.";
        if (message.Contains("while expression", StringComparison.OrdinalIgnoreCase) || message.Contains("if expression", StringComparison.OrdinalIgnoreCase))
            return "Control-flow conditions must evaluate to bool.";
        if (message.Contains("return", StringComparison.OrdinalIgnoreCase))
            return "The return statement must match the function's declared return behavior.";
        return "";
    }

    private static bool TrySetRangeForIdentifier(FavaDiagnostic diagnostic, string sourceLine, string identifier)
    {
        var match = Regex.Matches(sourceLine, $@"\b{Regex.Escape(identifier)}\b")
            .Cast<Match>()
            .FirstOrDefault();
        if (match is null)
            return false;

        diagnostic.Column = match.Index + 1;
        diagnostic.UnderlineLength = match.Length;
        return true;
    }

    private static bool TrySetAssignmentRange(FavaDiagnostic diagnostic, string sourceLine)
    {
        var assignIndex = sourceLine.IndexOf(":=", StringComparison.Ordinal);
        if (assignIndex < 0)
            return false;

        var rightSide = sourceLine[(assignIndex + 2)..];
        var rightToken = Regex.Match(rightSide, @"\S+");
        if (rightToken.Success)
        {
            diagnostic.Column = assignIndex + 3 + rightToken.Index;
            diagnostic.UnderlineLength = rightToken.Length;
            return true;
        }

        diagnostic.Column = assignIndex + 1;
        diagnostic.UnderlineLength = 2;
        return true;
    }

    private static bool TrySetAfterKeywordRange(FavaDiagnostic diagnostic, string sourceLine, string keyword)
    {
        var keywordMatch = Regex.Match(sourceLine, $@"\b{Regex.Escape(keyword)}\b", RegexOptions.IgnoreCase);
        if (!keywordMatch.Success)
            return false;

        var afterKeyword = sourceLine[(keywordMatch.Index + keywordMatch.Length)..];
        var expression = Regex.Match(afterKeyword, @"\S+");
        if (!expression.Success)
        {
            diagnostic.Column = keywordMatch.Index + 1;
            diagnostic.UnderlineLength = keywordMatch.Length;
            return true;
        }

        diagnostic.Column = keywordMatch.Index + keywordMatch.Length + expression.Index + 1;
        diagnostic.UnderlineLength = TrimTrailingStatementPunctuation(expression.Value).Length;
        return true;
    }

    private static bool TrySetBracketContentRange(FavaDiagnostic diagnostic, string sourceLine, bool preferLast)
    {
        var matches = Regex.Matches(sourceLine, @"\[(?<expr>[^\]]*)\]").Cast<Match>().ToList();
        if (matches.Count == 0)
            return false;

        var match = preferLast ? matches[^1] : matches[0];
        var expression = match.Groups["expr"];
        var trimmed = expression.Value.Trim();
        diagnostic.Column = expression.Index + expression.Value.IndexOf(trimmed, StringComparison.Ordinal) + 1;
        diagnostic.UnderlineLength = Math.Max(1, trimmed.Length);
        return true;
    }

    private static bool TrySetConditionRange(FavaDiagnostic diagnostic, string sourceLine, string keyword)
    {
        var keywordIndex = sourceLine.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
        if (keywordIndex < 0)
            return false;

        var open = sourceLine.IndexOf('(', keywordIndex);
        var close = open >= 0 ? sourceLine.IndexOf(')', open + 1) : -1;
        if (open < 0 || close <= open)
            return TrySetAfterKeywordRange(diagnostic, sourceLine, keyword);

        var expression = sourceLine.Substring(open + 1, close - open - 1).Trim();
        diagnostic.Column = open + 2 + sourceLine.Substring(open + 1, close - open - 1).IndexOf(expression, StringComparison.Ordinal);
        diagnostic.UnderlineLength = Math.Max(1, expression.Length);
        return true;
    }

    private static string TrimTrailingStatementPunctuation(string value) =>
        value.TrimEnd(';', ',', ')', ']');

    private static bool TrySetRangeForToken(FavaDiagnostic diagnostic, string sourceLine, string token)
    {
        var index = sourceLine.IndexOf(token, StringComparison.Ordinal);
        if (index < 0)
            return false;

        diagnostic.Column = index + 1;
        diagnostic.UnderlineLength = Math.Max(1, token.Length);
        return true;
    }

    private static void ClampRange(FavaDiagnostic diagnostic, string sourceLine)
    {
        diagnostic.Column = Math.Clamp(diagnostic.Column, 1, Math.Max(1, sourceLine.Length + 1));
        diagnostic.UnderlineLength = Math.Clamp(diagnostic.UnderlineLength, 1, Math.Max(1, sourceLine.Length - diagnostic.Column + 2));
    }
}
