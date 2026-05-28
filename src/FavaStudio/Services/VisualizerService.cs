using System.Globalization;
using System.Text.RegularExpressions;
using FavaStudio.Models;

namespace FavaStudio.Services;

public static class VisualizerService
{
    private const double DoubleComparisonTolerance = 1e-9;
    private static readonly Regex TraceLineRegex = new(
        @"^\s*(?<ip>\d+)\s*:\s*(?<opcode>[A-Za-z_][A-Za-z0-9_]*)\b",
        RegexOptions.Compiled);

    public static List<string> ParseConstantPool(string constantPoolSection)
    {
        var lines = constantPoolSection.Replace("\r\n", "\n").Split('\n');
        var constants = new List<string>();
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) continue;
            var colonIndex = trimmed.IndexOf(':');
            if (colonIndex < 0 || colonIndex >= trimmed.Length - 1) continue;
            constants.Add(trimmed[(colonIndex + 1)..].Trim());
        }
        return constants;
    }

    public static List<VisualizerInstruction> ParseInstructions(string instructionsSection)
    {
        var lines = instructionsSection.Replace("\r\n", "\n").Split('\n');
        var result = new List<VisualizerInstruction>();
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) continue;
            var colonIndex = trimmed.IndexOf(':');
            if (colonIndex <= 0 || colonIndex >= trimmed.Length - 1) continue;
            if (!int.TryParse(trimmed[..colonIndex].Trim(), out var index)) continue;
            var remainder = trimmed[(colonIndex + 1)..].Trim();
            if (string.IsNullOrWhiteSpace(remainder)) continue;
            var parts = remainder.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var opcode = parts[0].Trim().ToLowerInvariant();
            int? arg = null;
            if (parts.Length > 1 && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedArg))
                arg = parsedArg;

            result.Add(new VisualizerInstruction
            {
                Index = index,
                Opcode = opcode,
                Argument = arg,
                Raw = remainder,
                Description = GetInstructionDescription(opcode)
            });
        }
        return result.OrderBy(i => i.Index).ToList();
    }

    public static List<int> ParseTraceInstructionAddresses(string traceSection)
    {
        var lines = traceSection.Replace("\r\n", "\n").Split('\n');
        var result = new List<int>();
        foreach (var line in lines)
        {
            var match = TraceLineRegex.Match(line);
            if (!match.Success)
                continue;
            if (string.IsNullOrWhiteSpace(match.Groups["opcode"].Value))
                continue;

            if (int.TryParse(match.Groups["ip"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ip))
                result.Add(ip);
        }

        return result;
    }

    public static IReadOnlyList<OpcodeReferenceItem> BuildReference() =>
        OpcodeDescriptions
            .Select(kvp => new OpcodeReferenceItem { Opcode = kvp.Key, Name = kvp.Value.Name, Summary = kvp.Value.Description })
            .OrderBy(item => item.Opcode)
            .ToList();

    public static string GetInstructionDescription(string opcode)
    {
        var match = OpcodeDescriptions.Values.FirstOrDefault(v => v.Name == opcode);
        return string.IsNullOrWhiteSpace(match.Name) ? "Performs stack operation." : match.Description;
    }

    public static bool ApplyInstruction(
        VisualizerInstruction instruction,
        List<VisualizerValue> stack,
        List<VisualizerValue?> globals,
        List<VisualizerFrameState> frames,
        ref int currentFramePointer,
        int currentInstructionAddress,
        IReadOnlyList<string> constantPool,
        out string note,
        out string outputLine,
        out bool halted,
        out int? newIp)
    {
        note = instruction.Description;
        outputLine = "";
        halted = false;
        newIp = null;
        try
        {
            switch (instruction.Opcode)
            {
                case "iconst":
                    if (!instruction.Argument.HasValue) return Fail("Missing iconst argument.", out note);
                    stack.Add(new VisualizerValue { Type = "int", Value = instruction.Argument.Value });
                    return true;
                case "dconst":
                    if (!TryGetConst(instruction.Argument, constantPool, out var dConst)) return Fail("Invalid dconst index.", out note);
                    if (!double.TryParse(Unquote(dConst), NumberStyles.Any, CultureInfo.InvariantCulture, out var dValue))
                        return Fail($"dconst expects double at index {instruction.Argument}.", out note);
                    stack.Add(new VisualizerValue { Type = "double", Value = dValue });
                    return true;
                case "sconst":
                    if (!TryGetConst(instruction.Argument, constantPool, out var sConst)) return Fail("Invalid sconst index.", out note);
                    stack.Add(new VisualizerValue { Type = "string", Value = Unquote(sConst) });
                    return true;
                case "tconst":
                    stack.Add(new VisualizerValue { Type = "boolean", Value = true });
                    return true;
                case "fconst":
                    stack.Add(new VisualizerValue { Type = "boolean", Value = false });
                    return true;
                case "itod":
                    if (!TryPopInt(stack, out var intForDouble, out note)) return false;
                    stack.Add(new VisualizerValue { Type = "double", Value = (double)intForDouble });
                    note = $"Converted int {intForDouble} to double.";
                    return true;
                case "itos":
                    if (!TryPopInt(stack, out var intForString, out note)) return false;
                    stack.Add(new VisualizerValue { Type = "string", Value = intForString.ToString(CultureInfo.InvariantCulture) });
                    note = $"Converted int {intForString} to string.";
                    return true;
                case "dtos":
                    if (!TryPopDouble(stack, out var doubleForString, out note)) return false;
                    stack.Add(new VisualizerValue { Type = "string", Value = doubleForString.ToString(CultureInfo.InvariantCulture) });
                    note = $"Converted double {doubleForString.ToString(CultureInfo.InvariantCulture)} to string.";
                    return true;
                case "btos":
                    if (!TryPopBool(stack, out var boolForString, out note)) return false;
                    stack.Add(new VisualizerValue { Type = "string", Value = boolForString ? "true" : "false" });
                    note = $"Converted boolean {(boolForString ? "true" : "false")} to string.";
                    return true;
                case "iuminus":
                    if (!TryPopInt(stack, out var intNeg, out note)) return false;
                    stack.Add(new VisualizerValue { Type = "int", Value = -intNeg });
                    return true;
                case "duminus":
                    if (!TryPopDouble(stack, out var doubleNeg, out note)) return false;
                    stack.Add(new VisualizerValue { Type = "double", Value = -doubleNeg });
                    return true;
                case "iadd":
                    return ApplyIntBinary(stack, (a, b) => a + b, out note);
                case "isub":
                    return ApplyIntBinary(stack, (a, b) => a - b, out note);
                case "imult":
                    return ApplyIntBinary(stack, (a, b) => a * b, out note);
                case "idiv":
                    return ApplyIntBinary(stack, (a, b) => b == 0 ? throw new DivideByZeroException() : a / b, out note);
                case "imod":
                    return ApplyIntBinary(stack, (a, b) => b == 0 ? throw new DivideByZeroException() : a % b, out note);
                case "dadd":
                    return ApplyDoubleBinary(stack, (a, b) => a + b, out note);
                case "dsub":
                    return ApplyDoubleBinary(stack, (a, b) => a - b, out note);
                case "dmult":
                    return ApplyDoubleBinary(stack, (a, b) => a * b, out note);
                case "ddiv":
                    return ApplyDoubleBinary(stack, SafeDivideDouble, out note);
                case "sconcat":
                    if (!TryPopString(stack, out var rightString, out note)) return false;
                    if (!TryPopString(stack, out var leftString, out note)) return false;
                    stack.Add(new VisualizerValue { Type = "string", Value = leftString + rightString });
                    return true;
                case "and":
                    return ApplyBoolBinary(stack, (a, b) => a && b, out note);
                case "or":
                    return ApplyBoolBinary(stack, (a, b) => a || b, out note);
                case "not":
                    if (!TryPopBool(stack, out var boolVal, out note)) return false;
                    stack.Add(new VisualizerValue { Type = "boolean", Value = !boolVal });
                    return true;
                case "aalloc":
                    if (!TryPopInt(stack, out var arraySize, out note)) return false;
                    if (arraySize < 0) return Fail("aalloc size cannot be negative.", out note);
                    stack.Add(new VisualizerValue
                    {
                        Type = "array",
                        Value = Enumerable.Repeat<VisualizerValue?>(null, arraySize).ToList()
                    });
                    note = $"Allocated array with {arraySize} element slot(s).";
                    return true;
                case "aload":
                    if (!TryPopInt(stack, out var loadIndex, out note)) return false;
                    if (!TryPopArray(stack, out var loadArray, out note)) return false;
                    if (!TryGetArrayElement(loadArray, loadIndex, out var loadedElement, out note)) return false;
                    stack.Add(CloneValue(loadedElement));
                    note = $"Loaded array[{loadIndex}] = {FormatValue(loadedElement)}.";
                    return true;
                case "astore":
                    if (stack.Count == 0) return Fail("Stack underflow on astore value.", out note);
                    var arrayStoreValue = CloneValue(stack[^1]);
                    stack.RemoveAt(stack.Count - 1);
                    if (!TryPopInt(stack, out var storeIndex, out note)) return false;
                    if (!TryPopArray(stack, out var storeArray, out note)) return false;
                    if (!TrySetArrayElement(storeArray, storeIndex, arrayStoreValue, out note)) return false;
                    note = $"Stored {FormatValue(arrayStoreValue)} into array[{storeIndex}].";
                    return true;
                case "alength":
                    if (!TryPopArray(stack, out var lengthArray, out note)) return false;
                    stack.Add(new VisualizerValue { Type = "int", Value = lengthArray.Count });
                    note = $"Pushed array length {lengthArray.Count}.";
                    return true;
                case "slength":
                    if (!TryPopString(stack, out var lengthString, out note)) return false;
                    stack.Add(new VisualizerValue { Type = "int", Value = lengthString.Length });
                    note = $"Pushed string length {lengthString.Length}.";
                    return true;
                case "iread":
                    if (!TryPopAny(stack, out var intPrompt, out note)) return false;
                    stack.Add(new VisualizerValue { Type = "int", Value = 0 });
                    note = $"Read integer input after prompt {FormatValue(intPrompt)}. Visualizer uses 0.";
                    return true;
                case "dread":
                    if (!TryPopAny(stack, out var realPrompt, out note)) return false;
                    stack.Add(new VisualizerValue { Type = "double", Value = 0.0 });
                    note = $"Read real input after prompt {FormatValue(realPrompt)}. Visualizer uses 0.0.";
                    return true;
                case "sread":
                    if (!TryPopAny(stack, out var stringPrompt, out note)) return false;
                    stack.Add(new VisualizerValue { Type = "string", Value = "" });
                    note = $"Read string input after prompt {FormatValue(stringPrompt)}. Visualizer uses empty string.";
                    return true;
                case "bread":
                    if (!TryPopAny(stack, out var boolPrompt, out note)) return false;
                    stack.Add(new VisualizerValue { Type = "boolean", Value = false });
                    note = $"Read bool input after prompt {FormatValue(boolPrompt)}. Visualizer uses false.";
                    return true;
                case "ieq":
                    return ApplyIntCompare(stack, (a, b) => a == b, out note);
                case "ineq":
                    return ApplyIntCompare(stack, (a, b) => a != b, out note);
                case "ilt":
                    return ApplyIntCompare(stack, (a, b) => a < b, out note);
                case "ileq":
                    return ApplyIntCompare(stack, (a, b) => a <= b, out note);
                case "deq":
                    return ApplyDoubleCompare(stack, (a, b) => Math.Abs(a - b) < DoubleComparisonTolerance, out note);
                case "dneq":
                    return ApplyDoubleCompare(stack, (a, b) => Math.Abs(a - b) >= DoubleComparisonTolerance, out note);
                case "dlt":
                    return ApplyDoubleCompare(stack, (a, b) => a < b, out note);
                case "dleq":
                    return ApplyDoubleCompare(stack, (a, b) => a <= b, out note);
                case "seq":
                    return ApplyStringCompare(stack, (a, b) => a == b, out note);
                case "sneq":
                    return ApplyStringCompare(stack, (a, b) => a != b, out note);
                case "beq":
                    return ApplyBoolCompare(stack, (a, b) => a == b, out note);
                case "bneq":
                    return ApplyBoolCompare(stack, (a, b) => a != b, out note);
                case "iprint":
                    if (!TryPopInt(stack, out var intPrint, out note)) return false;
                    outputLine = intPrint.ToString(CultureInfo.InvariantCulture);
                    note = $"Output: {outputLine}";
                    return true;
                case "dprint":
                    if (!TryPopDouble(stack, out var doublePrint, out note)) return false;
                    outputLine = doublePrint.ToString(CultureInfo.InvariantCulture);
                    note = $"Output: {outputLine}";
                    return true;
                case "sprint":
                    if (!TryPopString(stack, out var stringPrint, out note)) return false;
                    outputLine = stringPrint;
                    note = $"Output: {outputLine}";
                    return true;
                case "bprint":
                    if (!TryPopBool(stack, out var boolPrint, out note)) return false;
                    outputLine = boolPrint ? "true" : "false";
                    note = $"Output: {outputLine}";
                    return true;
                case "halt":
                    halted = true;
                    note = "Execution halted.";
                    return true;
                case "jump":
                    if (!instruction.Argument.HasValue) return Fail("Missing jump argument.", out note);
                    newIp = instruction.Argument.Value;
                    note = $"Unconditional jump to instruction {instruction.Argument.Value}.";
                    return true;
                case "jumpf":
                    if (!instruction.Argument.HasValue) return Fail("Missing jumpf argument.", out note);
                    if (!TryPopBool(stack, out var jumpCondition, out note)) return false;
                    if (!jumpCondition)
                    {
                        newIp = instruction.Argument.Value;
                        note = $"Condition false: jump to instruction {instruction.Argument.Value}.";
                    }
                    else
                    {
                        note = "Condition true: no jump.";
                    }
                    return true;
                case "galloc":
                    if (!instruction.Argument.HasValue) return Fail("Missing galloc count.", out note);
                    for (var i = 0; i < instruction.Argument.Value; i++)
                        globals.Add(null);
                    note = $"Allocated {instruction.Argument.Value} global slot(s). Total: {globals.Count}.";
                    return true;
                case "gload":
                    if (!instruction.Argument.HasValue) return Fail("Missing gload address.", out note);
                    var gloadAddr = instruction.Argument.Value;
                    if (gloadAddr < 0 || gloadAddr >= globals.Count)
                        return Fail($"gload address {gloadAddr} out of range (0..{globals.Count - 1}).", out note);
                    var gloadVal = globals[gloadAddr];
                    if (gloadVal is null)
                        return Fail($"gload: globals[{gloadAddr}] is NULL (uninitialized).", out note);
                    stack.Add(gloadVal);
                    note = $"Pushed globals[{gloadAddr}] = {FormatValue(gloadVal)}.";
                    return true;
                case "gstore":
                    if (!instruction.Argument.HasValue) return Fail("Missing gstore address.", out note);
                    var gstoreAddr = instruction.Argument.Value;
                    if (gstoreAddr < 0 || gstoreAddr >= globals.Count)
                        return Fail($"gstore address {gstoreAddr} out of range (0..{globals.Count - 1}).", out note);
                    if (stack.Count == 0) return Fail("Stack underflow on gstore.", out note);
                    var storeVal = stack[^1];
                    stack.RemoveAt(stack.Count - 1);
                    globals[gstoreAddr] = storeVal;
                    note = $"Stored {FormatValue(storeVal)} into globals[{gstoreAddr}].";
                    return true;
                case "lalloc":
                    if (!instruction.Argument.HasValue) return Fail("Missing lalloc count.", out note);
                    if (instruction.Argument.Value < 0) return Fail("lalloc count cannot be negative.", out note);
                    if (frames.Count == 0) return Fail("lalloc requires an active call frame.", out note);
                    for (var i = 0; i < instruction.Argument.Value; i++)
                        stack.Add(new VisualizerValue { Type = "null", Value = "NULL" });
                    frames[^1].LocalCount += instruction.Argument.Value;
                    note = $"Allocated {instruction.Argument.Value} local slot(s).";
                    return true;
                case "lload":
                    if (!instruction.Argument.HasValue) return Fail("Missing lload address.", out note);
                    if (!TryResolveFrameAddress(currentFramePointer, instruction.Argument.Value, stack, out var lloadIndex, out note))
                        return false;
                    var lloadValue = stack[lloadIndex];
                    if (string.Equals(lloadValue.Type, "null", StringComparison.OrdinalIgnoreCase))
                        return Fail($"lload: stack[{lloadIndex}] is NULL (uninitialized).", out note);
                    stack.Add(CloneValue(lloadValue));
                    note = $"Loaded local/arg at FP{FormatOffset(instruction.Argument.Value)}.";
                    return true;
                case "lstore":
                    if (!instruction.Argument.HasValue) return Fail("Missing lstore address.", out note);
                    if (!TryResolveFrameAddress(currentFramePointer, instruction.Argument.Value, stack, out var lstoreIndex, out note))
                        return false;
                    if (stack.Count == 0) return Fail("Stack underflow on lstore.", out note);
                    var lstoreValue = stack[^1];
                    stack.RemoveAt(stack.Count - 1);
                    stack[lstoreIndex] = CloneValue(lstoreValue);
                    note = $"Stored value into FP{FormatOffset(instruction.Argument.Value)}.";
                    return true;
                case "pop":
                    if (!instruction.Argument.HasValue) return Fail("Missing pop count.", out note);
                    if (instruction.Argument.Value < 0) return Fail("pop count cannot be negative.", out note);
                    if (stack.Count < instruction.Argument.Value) return Fail("Stack underflow on pop.", out note);
                    stack.RemoveRange(stack.Count - instruction.Argument.Value, instruction.Argument.Value);
                    note = $"Popped {instruction.Argument.Value} value(s).";
                    return true;
                case "call":
                    if (!instruction.Argument.HasValue) return Fail("Missing call target address.", out note);
                    var previousFp = currentFramePointer;
                    stack.Add(new VisualizerValue { Type = "frameptr", Value = previousFp });
                    currentFramePointer = stack.Count - 1;
                    stack.Add(new VisualizerValue { Type = "retaddr", Value = currentInstructionAddress + 1 });
                    frames.Add(new VisualizerFrameState { FramePointer = currentFramePointer, LocalCount = 0 });
                    newIp = instruction.Argument.Value;
                    note = $"Called function at instruction {instruction.Argument.Value}.";
                    return true;
                case "retval":
                    if (!instruction.Argument.HasValue) return Fail("Missing retval argument count.", out note);
                    if (!TryReturnWithValue(stack, frames, ref currentFramePointer, instruction.Argument.Value, out var returnValue, out var returnAddress, out note))
                        return false;
                    stack.Add(returnValue);
                    newIp = returnAddress;
                    note = $"Returned value to instruction {returnAddress}.";
                    return true;
                case "ret":
                    if (!instruction.Argument.HasValue) return Fail("Missing ret argument count.", out note);
                    if (!TryReturnVoid(stack, frames, ref currentFramePointer, instruction.Argument.Value, out var retAddress, out note))
                        return false;
                    newIp = retAddress;
                    note = $"Returned to instruction {retAddress}.";
                    return true;
                default:
                    note = $"Instruction '{instruction.Opcode}' is not supported in visualizer simulation.";
                    return false;
            }
        }
        catch (DivideByZeroException)
        {
            note = "Division by zero.";
            return false;
        }
    }

    public static string StackToText(IReadOnlyList<VisualizerValue> stack)
    {
        if (stack.Count == 0) return "(empty)";
        return string.Join(" | ", stack.Select(v => $"{v.Type}:{FormatValue(v)}"));
    }

    public static IReadOnlyList<VisualizerStackEntry> StackToEntries(IReadOnlyList<VisualizerValue> stack)
    {
        var rows = new List<VisualizerStackEntry>();
        for (var i = stack.Count - 1; i >= 0; i--)
        {
            rows.Add(new VisualizerStackEntry
            {
                Depth = stack.Count - i,
                Type = stack[i].Type,
                Value = FormatValue(stack[i])
            });
        }
        return rows;
    }

    public static IReadOnlyList<VisualizerGlobalEntry> GlobalsToEntries(IReadOnlyList<VisualizerValue?> globals)
    {
        var rows = new List<VisualizerGlobalEntry>();
        for (var i = 0; i < globals.Count; i++)
        {
            var g = globals[i];
            rows.Add(new VisualizerGlobalEntry
            {
                Address = i,
                Type = g?.Type ?? "null",
                Value = g is null ? "NULL" : FormatValue(g)
            });
        }
        return rows;
    }

    private static bool TryResolveFrameAddress(int framePointer, int offset, IReadOnlyList<VisualizerValue> stack, out int index, out string note)
    {
        index = framePointer + offset;
        if (framePointer < 0)
        {
            note = "No active frame pointer for local access.";
            return false;
        }

        if (index < 0 || index >= stack.Count)
        {
            note = $"Frame address FP{FormatOffset(offset)} resolved to invalid stack index {index}.";
            return false;
        }

        note = "";
        return true;
    }

    private static bool TryReturnWithValue(
        List<VisualizerValue> stack,
        List<VisualizerFrameState> frames,
        ref int currentFramePointer,
        int argumentCount,
        out VisualizerValue returnValue,
        out int returnAddress,
        out string note)
    {
        returnValue = new VisualizerValue();
        returnAddress = -1;

        if (argumentCount < 0)
        {
            note = "retval argument count cannot be negative.";
            return false;
        }

        if (stack.Count == 0)
        {
            note = "Stack underflow on retval.";
            return false;
        }

        returnValue = CloneValue(stack[^1]);
        stack.RemoveAt(stack.Count - 1);
        if (!TryUnwindFrame(stack, frames, ref currentFramePointer, argumentCount, out returnAddress, out note))
            return false;

        return true;
    }

    private static bool TryReturnVoid(
        List<VisualizerValue> stack,
        List<VisualizerFrameState> frames,
        ref int currentFramePointer,
        int argumentCount,
        out int returnAddress,
        out string note)
    {
        returnAddress = -1;

        if (argumentCount < 0)
        {
            note = "ret argument count cannot be negative.";
            return false;
        }

        return TryUnwindFrame(stack, frames, ref currentFramePointer, argumentCount, out returnAddress, out note);
    }

    private static bool TryUnwindFrame(
        List<VisualizerValue> stack,
        List<VisualizerFrameState> frames,
        ref int currentFramePointer,
        int argumentCount,
        out int returnAddress,
        out string note)
    {
        returnAddress = -1;
        if (frames.Count == 0)
        {
            note = "ret/retval requires an active call frame.";
            return false;
        }

        var frame = frames[^1];
        if (frame.FramePointer < 0 || frame.FramePointer + 1 >= stack.Count)
        {
            note = "Current frame metadata is invalid.";
            return false;
        }

        if (!TryGetIntAt(stack, frame.FramePointer, out var previousFramePointer))
        {
            note = "Stored frame pointer is invalid.";
            return false;
        }

        if (!TryGetIntAt(stack, frame.FramePointer + 1, out returnAddress))
        {
            note = "Stored return address is invalid.";
            return false;
        }

        var framePayloadCount = stack.Count - frame.FramePointer;
        if (framePayloadCount < 2)
        {
            note = "Corrupted frame stack layout.";
            return false;
        }

        stack.RemoveRange(frame.FramePointer, framePayloadCount);
        frames.RemoveAt(frames.Count - 1);
        currentFramePointer = previousFramePointer;

        if (stack.Count < argumentCount)
        {
            note = "Stack underflow while removing call arguments.";
            return false;
        }

        if (argumentCount > 0)
            stack.RemoveRange(stack.Count - argumentCount, argumentCount);

        note = "";
        return true;
    }

    private static bool TryGetIntAt(IReadOnlyList<VisualizerValue> stack, int index, out int value)
    {
        value = 0;
        if (index < 0 || index >= stack.Count) return false;
        try
        {
            value = Convert.ToInt32(stack[index].Value, CultureInfo.InvariantCulture);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static VisualizerValue CloneValue(VisualizerValue value) =>
        new()
        {
            Type = value.Type,
            Value = value.Value
        };

    private static bool TryPopArray(List<VisualizerValue> stack, out List<VisualizerValue?> array, out string note)
    {
        array = [];
        if (stack.Count == 0)
        {
            note = "Stack underflow.";
            return false;
        }

        var value = stack[^1];
        stack.RemoveAt(stack.Count - 1);
        if (!string.Equals(value.Type, "array", StringComparison.OrdinalIgnoreCase) ||
            value.Value is not List<VisualizerValue?> arrayValue)
        {
            note = $"Type mismatch: expected array, got {value.Type}.";
            return false;
        }

        array = arrayValue;
        note = "";
        return true;
    }

    private static bool TryGetArrayElement(
        IReadOnlyList<VisualizerValue?> array,
        int index,
        out VisualizerValue value,
        out string note)
    {
        value = new VisualizerValue();
        if (index < 0 || index >= array.Count)
        {
            note = $"Array index {index} out of range (0..{array.Count - 1}).";
            return false;
        }

        var element = array[index];
        if (element is null || string.Equals(element.Type, "null", StringComparison.OrdinalIgnoreCase))
        {
            note = $"aload: array[{index}] is NULL (uninitialized).";
            return false;
        }

        value = element;
        note = "";
        return true;
    }

    private static bool TrySetArrayElement(List<VisualizerValue?> array, int index, VisualizerValue value, out string note)
    {
        if (index < 0 || index >= array.Count)
        {
            note = $"Array index {index} out of range (0..{array.Count - 1}).";
            return false;
        }

        array[index] = value;
        note = "";
        return true;
    }

    private static string FormatOffset(int offset) => offset >= 0 ? $"+{offset}" : offset.ToString(CultureInfo.InvariantCulture);

    private static bool TryGetConst(int? index, IReadOnlyList<string> constantPool, out string value)
    {
        value = "";
        if (!index.HasValue || index.Value < 0 || index.Value >= constantPool.Count) return false;
        value = constantPool[index.Value];
        return true;
    }

    private static bool ApplyIntBinary(List<VisualizerValue> stack, Func<int, int, int> op, out string note)
    {
        if (!TryPopInt(stack, out var right, out note)) return false;
        if (!TryPopInt(stack, out var left, out note)) return false;
        stack.Add(new VisualizerValue { Type = "int", Value = op(left, right) });
        note = $"Applied int op to {left} and {right}.";
        return true;
    }

    private static bool ApplyDoubleBinary(List<VisualizerValue> stack, Func<double, double, double> op, out string note)
    {
        if (!TryPopDouble(stack, out var right, out note)) return false;
        if (!TryPopDouble(stack, out var left, out note)) return false;
        stack.Add(new VisualizerValue { Type = "double", Value = op(left, right) });
        note = $"Applied double op to {left} and {right}.";
        return true;
    }

    private static bool ApplyBoolBinary(List<VisualizerValue> stack, Func<bool, bool, bool> op, out string note)
    {
        if (!TryPopBool(stack, out var right, out note)) return false;
        if (!TryPopBool(stack, out var left, out note)) return false;
        stack.Add(new VisualizerValue { Type = "boolean", Value = op(left, right) });
        note = $"Applied boolean op to {left} and {right}.";
        return true;
    }

    private static bool ApplyIntCompare(List<VisualizerValue> stack, Func<int, int, bool> compare, out string note)
    {
        if (!TryPopInt(stack, out var right, out note)) return false;
        if (!TryPopInt(stack, out var left, out note)) return false;
        stack.Add(new VisualizerValue { Type = "boolean", Value = compare(left, right) });
        note = $"Compared ints {left} and {right}.";
        return true;
    }

    private static bool ApplyDoubleCompare(List<VisualizerValue> stack, Func<double, double, bool> compare, out string note)
    {
        if (!TryPopDouble(stack, out var right, out note)) return false;
        if (!TryPopDouble(stack, out var left, out note)) return false;
        stack.Add(new VisualizerValue { Type = "boolean", Value = compare(left, right) });
        note = $"Compared doubles {left} and {right}.";
        return true;
    }

    private static double SafeDivideDouble(double left, double right)
    {
        if (Math.Abs(right) < DoubleComparisonTolerance)
            throw new DivideByZeroException();
        return left / right;
    }

    private static bool ApplyStringCompare(List<VisualizerValue> stack, Func<string, string, bool> compare, out string note)
    {
        if (!TryPopString(stack, out var right, out note)) return false;
        if (!TryPopString(stack, out var left, out note)) return false;
        stack.Add(new VisualizerValue { Type = "boolean", Value = compare(left, right) });
        note = $"Compared strings \"{left}\" and \"{right}\".";
        return true;
    }

    private static bool ApplyBoolCompare(List<VisualizerValue> stack, Func<bool, bool, bool> compare, out string note)
    {
        if (!TryPopBool(stack, out var right, out note)) return false;
        if (!TryPopBool(stack, out var left, out note)) return false;
        stack.Add(new VisualizerValue { Type = "boolean", Value = compare(left, right) });
        note = $"Compared booleans {left} and {right}.";
        return true;
    }

    private static bool TryPopInt(List<VisualizerValue> stack, out int value, out string note)
    {
        value = 0;
        if (!TryPop(stack, "int", out var popped, out note)) return false;
        value = Convert.ToInt32(popped.Value, CultureInfo.InvariantCulture);
        return true;
    }

    private static bool TryPopDouble(List<VisualizerValue> stack, out double value, out string note)
    {
        value = 0;
        if (!TryPop(stack, "double", out var popped, out note)) return false;
        value = Convert.ToDouble(popped.Value, CultureInfo.InvariantCulture);
        return true;
    }

    private static bool TryPopString(List<VisualizerValue> stack, out string value, out string note)
    {
        value = "";
        if (!TryPop(stack, "string", out var popped, out note)) return false;
        value = Convert.ToString(popped.Value, CultureInfo.InvariantCulture) ?? "";
        return true;
    }

    private static bool TryPopBool(List<VisualizerValue> stack, out bool value, out string note)
    {
        value = false;
        if (!TryPop(stack, "boolean", out var popped, out note)) return false;
        value = Convert.ToBoolean(popped.Value, CultureInfo.InvariantCulture);
        return true;
    }

    private static bool TryPop(List<VisualizerValue> stack, string expectedType, out VisualizerValue value, out string note)
    {
        value = new VisualizerValue();
        if (stack.Count == 0)
        {
            note = "Stack underflow.";
            return false;
        }

        value = stack[^1];
        stack.RemoveAt(stack.Count - 1);
        if (!string.Equals(value.Type, expectedType, StringComparison.OrdinalIgnoreCase))
        {
            note = $"Type mismatch: expected {expectedType}, got {value.Type}.";
            return false;
        }

        note = "";
        return true;
    }

    private static bool Fail(string message, out string note)
    {
        note = message;
        return false;
    }

    private static bool TryPopAny(List<VisualizerValue> stack, out VisualizerValue value, out string note)
    {
        value = new VisualizerValue();
        if (stack.Count == 0)
        {
            note = "Stack underflow.";
            return false;
        }

        value = stack[^1];
        stack.RemoveAt(stack.Count - 1);
        note = "";
        return true;
    }

    private static string Unquote(string value)
    {
        var trimmed = value.Trim();
        return trimmed.Length >= 2 && trimmed.StartsWith('"') && trimmed.EndsWith('"')
            ? trimmed[1..^1]
            : trimmed;
    }

    private static string FormatValue(VisualizerValue value)
    {
        if (value.Type == "array" && value.Value is IReadOnlyList<VisualizerValue?> array)
        {
            var preview = string.Join(", ", array.Take(6).Select(element => element is null ? "NULL" : FormatValue(element)));
            if (array.Count > 6)
                preview += ", ...";
            return $"[{preview}] ({array.Count})";
        }

        return value.Type switch
        {
            "double" => Convert.ToDouble(value.Value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture),
            "boolean" => Convert.ToBoolean(value.Value, CultureInfo.InvariantCulture) ? "true" : "false",
            _ => Convert.ToString(value.Value, CultureInfo.InvariantCulture) ?? ""
        };
    }

    private static readonly Dictionary<int, (string Name, string Description)> OpcodeDescriptions = new()
    {
        { 0, ("iconst", "Pushes an integer literal argument onto the stack.") },
        { 1, ("dconst", "Pushes a double constant from the constant pool by index.") },
        { 2, ("sconst", "Pushes a string constant from the constant pool by index.") },
        { 3, ("iprint", "Pops an int and prints it.") },
        { 4, ("iuminus", "Negates the top int.") },
        { 5, ("iadd", "Pops two ints and pushes their sum.") },
        { 6, ("isub", "Pops two ints and pushes left minus right.") },
        { 7, ("imult", "Pops two ints and pushes their product.") },
        { 8, ("idiv", "Pops two ints and pushes left divided by right.") },
        { 9, ("imod", "Pops two ints and pushes left modulo right.") },
        { 10, ("ieq", "Pops two ints and pushes whether they are equal.") },
        { 11, ("ineq", "Pops two ints and pushes whether they are different.") },
        { 12, ("ilt", "Pops two ints and pushes left < right.") },
        { 13, ("ileq", "Pops two ints and pushes left <= right.") },
        { 14, ("itod", "Converts top int to double.") },
        { 15, ("itos", "Converts top int to string.") },
        { 16, ("dprint", "Pops a double and prints it.") },
        { 17, ("duminus", "Negates the top double.") },
        { 18, ("dadd", "Pops two doubles and pushes their sum.") },
        { 19, ("dsub", "Pops two doubles and pushes left minus right.") },
        { 20, ("dmult", "Pops two doubles and pushes their product.") },
        { 21, ("ddiv", "Pops two doubles and pushes left divided by right.") },
        { 22, ("deq", "Pops two doubles and pushes equality result.") },
        { 23, ("dneq", "Pops two doubles and pushes non-equality result.") },
        { 24, ("dlt", "Pops two doubles and pushes left < right.") },
        { 25, ("dleq", "Pops two doubles and pushes left <= right.") },
        { 26, ("dtos", "Converts top double to string.") },
        { 27, ("sprint", "Pops a string and prints it.") },
        { 28, ("sconcat", "Pops two strings and pushes concatenation.") },
        { 29, ("seq", "Pops two strings and pushes equality result.") },
        { 30, ("sneq", "Pops two strings and pushes non-equality result.") },
        { 31, ("tconst", "Pushes boolean true.") },
        { 32, ("fconst", "Pushes boolean false.") },
        { 33, ("bprint", "Pops a boolean and prints true/false.") },
        { 34, ("beq", "Pops two booleans and pushes equality result.") },
        { 35, ("bneq", "Pops two booleans and pushes non-equality result.") },
        { 36, ("and", "Pops two booleans and pushes logical and.") },
        { 37, ("or", "Pops two booleans and pushes logical or.") },
        { 38, ("not", "Pops one boolean and pushes logical not.") },
        { 39, ("btos", "Converts top boolean to string.") },
        { 40, ("halt", "Stops program execution.") },
        { 41, ("jump", "Unconditional jump to the given instruction address.") },
        { 42, ("jumpf", "Pops a boolean; jumps to address if false, otherwise continues.") },
        { 43, ("galloc", "Allocates n NULL-initialized slots in the global variable array.") },
        { 44, ("gload", "Pushes the global variable at the given address onto the stack.") },
        { 45, ("gstore", "Pops the top of stack and stores it in the global variable at the given address.") },
        { 46, ("lalloc", "Allocates n NULL-initialized local slots in the current call frame.") },
        { 47, ("lload", "Pushes Stack[FP + addr] onto the stack.") },
        { 48, ("lstore", "Pops a value and stores it in Stack[FP + addr].") },
        { 49, ("pop", "Pops n values from the top of the runtime stack.") },
        { 50, ("call", "Creates a new call frame, saves FP/return address, and jumps to addr.") },
        { 51, ("retval", "Returns from non-void function: keeps return value, restores frame, pops n args.") },
        { 52, ("ret", "Returns from void function: restores frame and pops n args.") },
        { 53, ("aalloc", "Pops an int size and pushes a NULL-initialized array reference.") },
        { 54, ("aload", "Pops array reference and index, then pushes the initialized element.") },
        { 55, ("astore", "Pops value, index, and array reference, then stores the element.") },
        { 56, ("alength", "Pops an array reference and pushes its integer length.") },
        { 57, ("iread", "Pops a prompt value, reads a line, parses it as integer, and pushes it.") },
        { 58, ("dread", "Pops a prompt value, reads a line, parses it as real, and pushes it.") },
        { 59, ("sread", "Pops a prompt value, reads a line, and pushes it as string.") },
        { 60, ("bread", "Pops a prompt value, reads true or false, and pushes it as bool.") },
        { 61, ("slength", "Pops a string and pushes its character length.") }
    };
}
