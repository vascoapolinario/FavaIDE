using System.Collections.Generic;
using System.Text.RegularExpressions;
using FavaStudio.Models;

namespace FavaStudio.Services;

public static class DiagnosticsParser
{
    // Matches: "parser error at line 1:8 - extraneous input ';' expecting {"
    //          "lexer error at line 2:3 - token recognition error at: '$'"
    private static readonly Regex _pattern = new(
        @"^(lexer|parser) error at line (\d+):(\d+) - (.+)$",
        RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static IReadOnlyList<FavaDiagnostic> Parse(string output)
    {
        var results = new List<FavaDiagnostic>();
        foreach (Match m in _pattern.Matches(output))
        {
            if (!int.TryParse(m.Groups[2].Value, out var line) ||
                !int.TryParse(m.Groups[3].Value, out var column))
                continue;

            results.Add(new FavaDiagnostic
            {
                Severity = m.Groups[1].Value.ToLower() == "lexer" ? "Lexer Error" : "Parser Error",
                Line = line,
                Column = column,
                UnderlineLength = 1,
                Message = m.Groups[4].Value.Trim()
            });
        }
        return results;
    }
}
