using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FavaStudio.Models;

public sealed class VisualizerInstruction : INotifyPropertyChanged
{
    private bool _isCurrent;

    public int Index { get; init; }
    public string Opcode { get; init; } = "";
    public int? Argument { get; init; }
    public string Raw { get; init; } = "";
    public string Description { get; init; } = "";
    public string Display => Argument.HasValue ? $"{Index}: {Opcode} {Argument.Value}" : $"{Index}: {Opcode}";

    public bool IsCurrent
    {
        get => _isCurrent;
        set
        {
            if (_isCurrent == value) return;
            _isCurrent = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public sealed class VisualizerTimelineEntry
{
    public int Step { get; init; }
    public string Instruction { get; init; } = "";
    public string StackBefore { get; init; } = "";
    public string StackAfter { get; init; } = "";
    public string Note { get; init; } = "";
}

public sealed class VisualizerStackEntry
{
    public int Depth { get; init; }
    public string Type { get; init; } = "";
    public string Value { get; init; } = "";
}

public sealed class VisualizerValue
{
    public string Type { get; init; } = "";
    public object Value { get; init; } = "";
}

public sealed class OpcodeReferenceItem
{
    public int Opcode { get; init; }
    public string Name { get; init; } = "";
    public string Summary { get; init; } = "";
    public string Signature => $"{Opcode} • {Name}";
}
