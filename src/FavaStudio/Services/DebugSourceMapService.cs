using System.Text.RegularExpressions;
using FavaStudio.Models;

namespace FavaStudio.Services;

public static class DebugSourceMapService
{
    private static readonly Regex DeclarationRegex = new(@"^(integer|real|bool|string)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex PrintRegex = new(@"^print\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex ReturnRegex = new(@"^return\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex IfRegex = new(@"^if\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex WhileRegex = new(@"^while\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex FunctionCallRegex = new(@"^[A-Za-z_][A-Za-z0-9_]*\s*\(", RegexOptions.Compiled);

    public static IReadOnlyList<(int Line, string Text)> GetExecutableLines(string sourceText)
    {
        var lines = sourceText.Replace("\r\n", "\n").Split('\n');
        var executableLines = new List<(int Line, string Text)>();
        for (var i = 0; i < lines.Length; i++)
        {
            var lineNumber = i + 1;
            var line = StripComment(lines[i]).Trim();
            if (!IsExecutableLine(line))
                continue;

            executableLines.Add((lineNumber, line));
        }

        return executableLines;
    }

    public static Dictionary<int, List<int>> BuildLineToInstructionPositions(string sourceText, IReadOnlyList<VisualizerInstruction> instructions)
    {
        var map = new Dictionary<int, List<int>>();
        if (instructions.Count == 0)
            return map;

        var statements = GetExecutableStatements(sourceText);

        if (statements.Count == 0)
            return map;

        var currentInstructionPosition = 0;
        foreach (var statement in statements)
        {
            if (currentInstructionPosition >= instructions.Count)
                break;

            var positions = MapStatementToInstructionPositions(
                statement.Text,
                instructions,
                ref currentInstructionPosition);

            AddPositions(map, statement.Line, positions);
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

    private static bool IsExecutableLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line) || line is "{" or "}")
            return false;
        if (line.StartsWith("function ", StringComparison.OrdinalIgnoreCase))
            return false;
        return true;
    }

    private static IReadOnlyList<(int Line, string Text)> GetExecutableStatements(string sourceText)
    {
        var statements = new List<(int Line, string Text)>();
        foreach (var (line, text) in GetExecutableLines(sourceText))
        {
            foreach (var statement in SplitStatements(text))
            {
                if (!string.IsNullOrWhiteSpace(statement))
                    statements.Add((line, statement.Trim()));
            }
        }

        return statements;
    }

    private static IReadOnlyList<string> SplitStatements(string line)
    {
        var statements = new List<string>();
        var start = 0;
        var inString = false;
        var escaped = false;

        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (inString)
            {
                escaped = !escaped && ch == '\\';
                if (ch == '"' && !escaped)
                    inString = false;
                if (ch != '\\')
                    escaped = false;
                continue;
            }

            if (ch == '"')
            {
                inString = true;
                continue;
            }

            if (ch == ';')
            {
                statements.Add(line[start..i]);
                start = i + 1;
            }
        }

        if (start < line.Length)
            statements.Add(line[start..]);

        return statements;
    }

    private static IReadOnlyList<int> MapStatementToInstructionPositions(
        string statement,
        IReadOnlyList<VisualizerInstruction> instructions,
        ref int cursor)
    {
        if (statement == "}")
            return MapNextSingle(instructions, ref cursor, IsPop);

        if (PrintRegex.IsMatch(statement))
            return MapNextSingle(instructions, ref cursor, IsPrint);

        if (DeclarationRegex.IsMatch(statement))
            return MapDeclaration(statement, instructions, ref cursor);

        if (IsAssignment(statement))
            return MapThroughRepeatedTargets(statement, instructions, ref cursor, IsStore, CountAssignments(statement));

        if (ReturnRegex.IsMatch(statement))
            return MapNextSingle(instructions, ref cursor, i => i.Opcode is "ret" or "retval");

        if (IfRegex.IsMatch(statement) || WhileRegex.IsMatch(statement))
            return MapRangeToNext(instructions, ref cursor, i => i.Opcode == "jumpf");

        if (FunctionCallRegex.IsMatch(statement))
            return MapRangeToNext(instructions, ref cursor, i => i.Opcode == "call");

        return MapNextNonStructural(instructions, ref cursor);
    }

    private static IReadOnlyList<int> MapDeclaration(string statement, IReadOnlyList<VisualizerInstruction> instructions, ref int cursor)
    {
        var start = FindNext(instructions, cursor, i => i.Opcode is "galloc" or "lalloc");
        if (start < 0)
            return MapThroughRepeatedTargets(statement, instructions, ref cursor, IsStore, CountInitializers(statement));

        var initializerCount = CountInitializers(statement);
        if (initializerCount == 0)
        {
            cursor = start + 1;
            return [start];
        }

        var positions = new List<int>();
        var searchFrom = start;
        var lastStore = -1;
        for (var i = 0; i < initializerCount; i++)
        {
            lastStore = FindNext(instructions, searchFrom, IsStore);
            if (lastStore < 0)
                break;
            searchFrom = lastStore + 1;
        }

        if (lastStore < 0)
        {
            cursor = start + 1;
            return [start];
        }

        for (var i = start; i <= lastStore; i++)
            positions.Add(i);

        cursor = lastStore + 1;
        return positions;
    }

    private static IReadOnlyList<int> MapThroughRepeatedTargets(
        string statement,
        IReadOnlyList<VisualizerInstruction> instructions,
        ref int cursor,
        Func<VisualizerInstruction, bool> predicate,
        int count)
    {
        if (count <= 0)
            return MapNextNonStructural(instructions, ref cursor);

        var positions = new List<int>();
        var start = -1;
        var searchFrom = cursor;
        var last = -1;
        for (var i = 0; i < count; i++)
        {
            last = FindNext(instructions, searchFrom, predicate);
            if (last < 0)
                break;
            start = start < 0 ? searchFrom : start;
            searchFrom = last + 1;
        }

        if (last < 0)
            return MapNextNonStructural(instructions, ref cursor);

        for (var i = Math.Max(0, start); i <= last; i++)
            positions.Add(i);

        cursor = last + 1;
        return positions;
    }

    private static IReadOnlyList<int> MapNextSingle(
        IReadOnlyList<VisualizerInstruction> instructions,
        ref int cursor,
        Func<VisualizerInstruction, bool> predicate)
    {
        var position = FindNext(instructions, cursor, predicate);
        if (position < 0)
            return MapNextNonStructural(instructions, ref cursor);

        cursor = position + 1;
        return [position];
    }

    private static IReadOnlyList<int> MapRangeToNext(
        IReadOnlyList<VisualizerInstruction> instructions,
        ref int cursor,
        Func<VisualizerInstruction, bool> predicate)
    {
        var end = FindNext(instructions, cursor, predicate);
        if (end < 0)
            return MapNextNonStructural(instructions, ref cursor);

        var positions = Enumerable.Range(cursor, end - cursor + 1).ToList();
        cursor = end + 1;
        return positions;
    }

    private static IReadOnlyList<int> MapNextNonStructural(IReadOnlyList<VisualizerInstruction> instructions, ref int cursor)
    {
        var position = FindNext(instructions, cursor, i => !IsStructural(i));
        if (position < 0)
            return [];

        cursor = position + 1;
        return [position];
    }

    private static int FindNext(IReadOnlyList<VisualizerInstruction> instructions, int start, Func<VisualizerInstruction, bool> predicate)
    {
        for (var i = Math.Max(0, start); i < instructions.Count; i++)
        {
            if (predicate(instructions[i]))
                return i;
        }

        return -1;
    }

    private static void AddPositions(Dictionary<int, List<int>> map, int line, IReadOnlyList<int> positions)
    {
        if (positions.Count == 0)
            return;

        if (!map.TryGetValue(line, out var mapped))
        {
            mapped = [];
            map[line] = mapped;
        }

        foreach (var position in positions)
        {
            if (!mapped.Contains(position))
                mapped.Add(position);
        }
    }

    private static bool IsAssignment(string statement) =>
        statement.Contains(":=", StringComparison.Ordinal);

    private static int CountAssignments(string statement) =>
        Regex.Matches(statement, @":=").Count;

    private static int CountInitializers(string statement) =>
        CountAssignments(statement);

    private static bool IsPrint(VisualizerInstruction instruction) =>
        instruction.Opcode.EndsWith("print", StringComparison.OrdinalIgnoreCase);

    private static bool IsStore(VisualizerInstruction instruction) =>
        instruction.Opcode is "gstore" or "lstore";

    private static bool IsPop(VisualizerInstruction instruction) =>
        instruction.Opcode == "pop";

    private static bool IsStructural(VisualizerInstruction instruction) =>
        instruction.Opcode is "call" or "halt" or "ret" or "retval" or "jump" or "jumpf" or "pop";
}
