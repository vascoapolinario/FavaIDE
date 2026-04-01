using System.Globalization;
using FavaStudio.Models;

namespace FavaStudio.Services;

public static class VisualizerService
{
    private const double DoubleComparisonTolerance = 1e-9;

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
        IReadOnlyList<string> constantPool,
        out string note,
        out string outputLine,
        out bool halted)
    {
        note = instruction.Description;
        outputLine = "";
        halted = false;
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
                    outputLine = boolPrint ? "verdadeiro" : "falso";
                    note = $"Output: {outputLine}";
                    return true;
                case "halt":
                    halted = true;
                    note = "Execution halted.";
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

    private static string Unquote(string value)
    {
        var trimmed = value.Trim();
        return trimmed.Length >= 2 && trimmed.StartsWith('"') && trimmed.EndsWith('"')
            ? trimmed[1..^1]
            : trimmed;
    }

    private static string FormatValue(VisualizerValue value)
    {
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
        { 33, ("bprint", "Pops a boolean and prints verdadeiro/falso.") },
        { 34, ("beq", "Pops two booleans and pushes equality result.") },
        { 35, ("bneq", "Pops two booleans and pushes non-equality result.") },
        { 36, ("and", "Pops two booleans and pushes logical and.") },
        { 37, ("or", "Pops two booleans and pushes logical or.") },
        { 38, ("not", "Pops one boolean and pushes logical not.") },
        { 39, ("btos", "Converts top boolean to string.") },
        { 40, ("halt", "Stops program execution.") }
    };
}
