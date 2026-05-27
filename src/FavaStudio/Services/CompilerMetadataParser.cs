using System.Globalization;
using FavaStudio.Models;

namespace FavaStudio.Services;

public static class CompilerMetadataParser
{
    public static IReadOnlyList<FavaHoverInfo> ParseTypeInfo(string section)
    {
        var result = new List<FavaHoverInfo>();
        foreach (var rawLine in section.Replace("\r\n", "\n").Split('\n'))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var parts = line.Split('|');
            if (parts.Length < 6)
                continue;

            if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var sourceLine) ||
                !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var startColumn) ||
                !int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var endColumn))
                continue;

            var address = -1;
            if (parts.Length >= 7)
                int.TryParse(parts[6], NumberStyles.Integer, CultureInfo.InvariantCulture, out address);

            result.Add(new FavaHoverInfo
            {
                Line = sourceLine,
                StartColumn = startColumn,
                EndColumn = Math.Max(startColumn, endColumn),
                Kind = parts[3],
                Name = parts[4],
                Type = parts[5],
                Address = address
            });
        }

        return result
            .OrderBy(item => item.Line)
            .ThenBy(item => item.StartColumn)
            .ThenBy(item => item.EndColumn - item.StartColumn)
            .ToList();
    }

    public static Dictionary<int, List<int>> ParseSourceMap(string section, IReadOnlyList<VisualizerInstruction> instructions)
    {
        var addressToPosition = instructions
            .Select((instruction, position) => new { instruction.Index, position })
            .ToDictionary(item => item.Index, item => item.position);
        var result = new Dictionary<int, List<int>>();

        foreach (var rawLine in section.Replace("\r\n", "\n").Split('\n'))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var parts = line.Split('|');
            if (parts.Length < 2 ||
                !int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var address) ||
                !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var sourceLine))
                continue;

            if (sourceLine <= 0 || !addressToPosition.TryGetValue(address, out var position))
                continue;

            if (!result.TryGetValue(sourceLine, out var positions))
            {
                positions = [];
                result[sourceLine] = positions;
            }

            if (!positions.Contains(position))
                positions.Add(position);
        }

        return result;
    }
}
