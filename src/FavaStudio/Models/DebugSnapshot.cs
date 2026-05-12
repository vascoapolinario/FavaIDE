namespace FavaStudio.Models;

internal sealed class DebugSnapshot
{
    public List<VisualizerValue> Stack { get; init; } = [];
    public List<VisualizerValue?> Globals { get; init; } = [];
    public List<VisualizerFrameState> Frames { get; init; } = [];
    public int FramePointer { get; init; }
    public int StepIndex { get; init; }
    public bool Halted { get; init; }
    public string RunOutput { get; init; } = "";
}
