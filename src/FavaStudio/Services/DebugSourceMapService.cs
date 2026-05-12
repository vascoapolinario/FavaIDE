using System.Text.RegularExpressions;
using FavaStudio.Models;

namespace FavaStudio.Services;

public static class DebugSourceMapService
{
    private static readonly Regex ExecutableTokenRegex = new(@"[A-Za-z_][A-Za-z0-9_]*|\d+|""(?:\\.|[^""\\])*""", RegexOptions.Compiled);

    public static Dictionary<int, List<int>> BuildLineToInstructionPositions(string sourceText, IReadOnlyList<VisualizerInstruction> instructions)
    {
        var map = new Dictionary<int, List<int>>();
        if (instructions.Count == 0)
            return map;

        var lines = sourceText.Replace("\r\n", "\n").Split('\n');
        var executableLines = new List<(int Line, string Text)>();
        for (var i = 0; i < lines.Length; i++)
        {
            var lineNumber = i + 1;
            var line = StripComment(lines[i]).Trim();
            if (string.IsNullOrWhiteSpace(line) || line is "{" or "}")
                continue;
            if (line.StartsWith("function ", StringComparison.OrdinalIgnoreCase))
                continue;
            executableLines.Add((lineNumber, line));
        }

        if (executableLines.Count == 0)
            return map;

        var currentInstructionPosition = 0;
        for (var i = 0; i < executableLines.Count && currentInstructionPosition < instructions.Count; i++)
        {
            var (lineNumber, text) = executableLines[i];
            var remainingLines = executableLines.Count - i;
            var remainingInstructions = instructions.Count - currentInstructionPosition;
            var estimatedForLine = EstimateInstructionCount(text);
            var minimumForRemaining = Math.Max(remainingLines - 1, 0);
            var maxForCurrent = Math.Max(remainingInstructions - minimumForRemaining, 1);
            var assigned = Math.Clamp(estimatedForLine, 1, maxForCurrent);

            if (!map.TryGetValue(lineNumber, out var positions))
            {
                positions = [];
                map[lineNumber] = positions;
            }

            for (var j = 0; j < assigned && currentInstructionPosition < instructions.Count; j++)
            {
                positions.Add(currentInstructionPosition);
                currentInstructionPosition++;
            }
        }

        if (currentInstructionPosition < instructions.Count)
        {
            var fallbackLine = executableLines[^1].Line;
            if (!map.TryGetValue(fallbackLine, out var positions))
            {
                positions = [];
                map[fallbackLine] = positions;
            }

            while (currentInstructionPosition < instructions.Count)
            {
                positions.Add(currentInstructionPosition);
                currentInstructionPosition++;
            }
        }

        return map;
    }

    public static int? FindNextInstructionPositionForBreakpoints(
        IReadOnlyCollection<int> breakpointLines,
        IReadOnlyDictionary<int, List<int>> lineToInstructionPositions,
        int currentInstructionPosition)
    {
        if (breakpointLines.Count == 0 || lineToInstructionPositions.Count == 0)
            return null;

        var candidatePositions = new HashSet<int>();
        foreach (var line in breakpointLines)
        {
            if (TryGetMappedPositions(line, lineToInstructionPositions, out var mapped))
            {
                foreach (var position in mapped)
                    candidatePositions.Add(position);
            }
        }

        if (candidatePositions.Count == 0)
            return null;

        var next = candidatePositions.Where(p => p >= currentInstructionPosition).OrderBy(p => p).FirstOrDefault(-1);
        return next >= 0 ? next : null;
    }

    private static bool TryGetMappedPositions(int line, IReadOnlyDictionary<int, List<int>> map, out IReadOnlyList<int> positions)
    {
        if (map.TryGetValue(line, out var direct) && direct.Count > 0)
        {
            positions = direct;
            return true;
        }

        var forward = map.Keys.Where(k => k > line).OrderBy(k => k).FirstOrDefault(-1);
        if (forward >= 0 && map.TryGetValue(forward, out var forwardPositions) && forwardPositions.Count > 0)
        {
            positions = forwardPositions;
            return true;
        }

        var backward = map.Keys.Where(k => k < line).OrderByDescending(k => k).FirstOrDefault(-1);
        if (backward >= 0 && map.TryGetValue(backward, out var backwardPositions) && backwardPositions.Count > 0)
        {
            positions = backwardPositions;
            return true;
        }

        positions = [];
        return false;
    }

    private static string StripComment(string line)
    {
        var index = line.IndexOf("//", StringComparison.Ordinal);
        return index < 0 ? line : line[..index];
    }

    private static int EstimateInstructionCount(string line)
    {
        var tokenCount = ExecutableTokenRegex.Matches(line).Count;
        if (tokenCount <= 1) return 1;

        var budget = 1;
        if (line.Contains('='))
            budget += 1;
        if (line.Contains('+') || line.Contains('-') || line.Contains('*') || line.Contains('/') || line.Contains('%'))
            budget += 2;
        if (line.Contains("if", StringComparison.OrdinalIgnoreCase) || line.Contains("while", StringComparison.OrdinalIgnoreCase))
            budget += 2;
        if (line.Contains("print", StringComparison.OrdinalIgnoreCase))
            budget += 1;
        if (line.Contains("return", StringComparison.OrdinalIgnoreCase))
            budget += 1;
        if (line.Contains('(') && line.Contains(')') && !line.StartsWith("if ", StringComparison.OrdinalIgnoreCase) && !line.StartsWith("while ", StringComparison.OrdinalIgnoreCase))
            budget += 2;

        return Math.Max(1, Math.Min(6, budget));
    }
}
