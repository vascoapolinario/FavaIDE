package VM;

import VM.Instruction.Instruction;
import VM.Instruction.Instruction1Arg;

import java.io.ByteArrayInputStream;
import java.io.BufferedReader;
import java.io.DataInputStream;
import java.io.EOFException;
import java.io.IOException;
import java.io.InputStreamReader;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.InvalidPathException;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.nio.file.StandardOpenOption;
import java.time.Instant;
import java.time.ZoneOffset;
import java.time.ZonedDateTime;
import java.time.format.DateTimeFormatter;
import java.util.ArrayList;
import java.util.List;
import java.util.Locale;
import java.util.Stack;
import java.util.concurrent.ThreadLocalRandom;

public class vm {
    private record ExceptionHandler(int catchAddress, int stackSize, int framePointer) {}
    private static class FavaRuntimeException extends RuntimeException {
        FavaRuntimeException(String message) {
            super(message);
        }
    }

    private final boolean trace;
    private final byte[] bytecodes;

    private Instruction[] code;
    private int IP;
    private int FP = -1;

    private final Stack<Object> stack = new Stack<>();
    private final List<Object> constantPool = new ArrayList<>();
    private final List<Object> globals = new ArrayList<>();
    private final List<String> traceBuffer = new ArrayList<>();
    private final Stack<ExceptionHandler> exceptionHandlers = new Stack<>();
    private final BufferedReader input = new BufferedReader(new InputStreamReader(System.in));
    private final Path fileRoot;

    private static final Object NULL_VALUE = new Object() {
        @Override
        public String toString() {
            return "NULL";
        }
    };

    public vm(byte[] bytecodes, boolean trace) {
        this(bytecodes, trace, Paths.get("").toAbsolutePath().normalize().toString());
    }

    public vm(byte[] bytecodes, boolean trace, String fileRoot) {
        this.trace = trace;
        this.bytecodes = bytecodes;
        this.fileRoot = Paths.get(fileRoot).toAbsolutePath().normalize();
        decode(bytecodes);
        this.IP = 0;
    }

    private void decode(byte[] bytecodes) {
        ArrayList<Instruction> inst = new ArrayList<>();

        try (DataInputStream din = new DataInputStream(new ByteArrayInputStream(bytecodes))) {

            int poolSize = din.readInt();

            for (int i = 0; i < poolSize; i++) {
                byte tag = din.readByte();

                if (tag == 1) {
                    constantPool.add(din.readDouble());
                } else if (tag == 3) {
                    int len = din.readInt();
                    StringBuilder sb = new StringBuilder();
                    for (int j = 0; j < len; j++) {
                        sb.append(din.readChar());
                    }
                    constantPool.add(sb.toString());
                } else {
                    throw new RuntimeException("Unknown constant pool tag: " + tag);
                }
            }

            while (true) {
                byte b = din.readByte();
                OpCode opc = OpCode.convert(b);

                if (opc.nArgs() == 0) {
                    inst.add(new Instruction(opc));
                } else {
                    inst.add(new Instruction1Arg(opc, din.readInt()));
                }
            }

        } catch (EOFException e) {
            code = inst.toArray(new Instruction[0]);
        } catch (IOException e) {
            throw new RuntimeException(e);
        }
    }

    private void runtime_error(String msg) {
        throw new FavaRuntimeException(msg);
    }

    private Object constAt(int idx) {
        if (idx < 0 || idx >= constantPool.size()) {
            runtime_error("constant pool index out of bounds: " + idx);
        }
        return constantPool.get(idx);
    }

    private Object stackAt(int index) {
        if (index < 0 || index >= stack.size()) {
            runtime_error("invalid frame address");
        }
        return stack.get(index);
    }

    private int popInt() {
        Object v = stack.pop();
        if (!(v instanceof Integer)) {
            runtime_error("expected int on stack");
        }
        return (Integer) v;
    }

    private double popDouble() {
        Object v = stack.pop();
        if (!(v instanceof Double)) {
            runtime_error("expected real on stack");
        }
        return (Double) v;
    }

    private boolean popBool() {
        Object v = stack.pop();
        if (!(v instanceof Boolean)) {
            runtime_error("expected bool on stack");
        }
        return (Boolean) v;
    }

    private String popString() {
        Object v = stack.pop();
        if (!(v instanceof String)) {
            runtime_error("expected string on stack");
        }
        return (String) v;
    }

    private Object[] popArray() {
        Object v = stack.pop();
        if (!(v instanceof Object[])) {
            runtime_error("expected array on stack");
        }
        return (Object[]) v;
    }

    private Object popScalar() {
        Object value = stack.pop();
        if (value instanceof Object[] || value == NULL_VALUE) {
            runtime_error("expected scalar on stack");
        }
        return value;
    }

    private void checkArrayIndex(Object[] array, int index) {
        if (index < 0 || index >= array.length) {
            runtime_error("array index out of bounds: " + index);
        }
    }

    private void exec_iconst(int v) {
        stack.push(v);
    }

    private void exec_dconst(int idx) {
        stack.push(constAt(idx));
    }

    private void exec_sconst(int idx) {
        stack.push(constAt(idx));
    }


    private void exec_iuminus() {
        int a = popInt();
        stack.push(-a);
    }

    private void exec_iadd() {
        int b = popInt();
        int a = popInt();
        stack.push(a + b);
    }

    private void exec_isub() {
        int b = popInt();
        int a = popInt();
        stack.push(a - b);
    }

    private void exec_imult() {
        int b = popInt();
        int a = popInt();
        stack.push(a * b);
    }

    private void exec_idiv() {
        int b = popInt();
        int a = popInt();
        if (b == 0) runtime_error("division by 0");
        stack.push(a / b);
    }

    private void exec_imod() {
        int b = popInt();
        int a = popInt();
        if (b == 0) runtime_error("division by 0");
        stack.push(a % b);
    }

    private void exec_ieq() {
        int b = popInt();
        int a = popInt();
        stack.push(a == b);
    }

    private void exec_ineq() {
        int b = popInt();
        int a = popInt();
        stack.push(a != b);
    }

    private void exec_ilt() {
        int b = popInt();
        int a = popInt();
        stack.push(a < b);
    }

    private void exec_ileq() {
        int b = popInt();
        int a = popInt();
        stack.push(a <= b);
    }

    private void exec_itod() {
        int a = popInt();
        stack.push((double) a);
    }

    private void exec_itos() {
        int a = popInt();
        stack.push(String.valueOf(a));
    }

    private void exec_iprint() {
        System.out.println(popInt());
        System.out.flush();
    }

    private void exec_dprint() {
        System.out.println(popDouble());
        System.out.flush();
    }

    private void exec_duminus() {
        double a = popDouble();
        stack.push(-a);
    }

    private void exec_dadd() {
        double b = popDouble();
        double a = popDouble();
        stack.push(a + b);
    }

    private void exec_dsub() {
        double b = popDouble();
        double a = popDouble();
        stack.push(a - b);
    }

    private void exec_dmult() {
        double b = popDouble();
        double a = popDouble();
        stack.push(a * b);
    }

    private void exec_ddiv() {
        double b = popDouble();
        double a = popDouble();
        stack.push(a / b);
    }

    private void exec_deq() {
        double b = popDouble();
        double a = popDouble();
        stack.push(a == b);
    }

    private void exec_dneq() {
        double b = popDouble();
        double a = popDouble();
        stack.push(a != b);
    }

    private void exec_dlt() {
        double b = popDouble();
        double a = popDouble();
        stack.push(a < b);
    }

    private void exec_dleq() {
        double b = popDouble();
        double a = popDouble();
        stack.push(a <= b);
    }

    private void exec_dtos() {
        double a = popDouble();
        stack.push(String.valueOf(a));
    }


    private void exec_sprint() {
        System.out.println(popString());
        System.out.flush();
    }

    private void exec_sconcat() {
        String b = popString();
        String a = popString();
        stack.push(a + b);
    }

    private void exec_seq() {
        String b = popString();
        String a = popString();
        stack.push(a.equals(b));
    }

    private void exec_sneq() {
        String b = popString();
        String a = popString();
        stack.push(!a.equals(b));
    }


    private void exec_tconst() {
        stack.push(true);
    }

    private void exec_fconst() {
        stack.push(false);
    }

    private void exec_bprint() {
        System.out.println(popBool());
        System.out.flush();
    }

    private void exec_beq() {
        boolean b = popBool();
        boolean a = popBool();
        stack.push(a == b);
    }

    private void exec_bneq() {
        boolean b = popBool();
        boolean a = popBool();
        stack.push(a != b);
    }

    private void exec_and() {
        boolean b = popBool();
        boolean a = popBool();
        stack.push(a && b);
    }

    private void exec_or() {
        boolean b = popBool();
        boolean a = popBool();
        stack.push(a || b);
    }

    private void exec_not() {
        boolean a = popBool();
        stack.push(!a);
    }

    private void exec_btos() {
        boolean a = popBool();
        stack.push(String.valueOf(a));
    }

    private void exec_jump(int addr) {
        IP = addr - 1;
    }

    private void exec_jumpf(int addr) {
        boolean cond = popBool();
        if (!cond) {
            IP = addr - 1;
        }
    }

    private void exec_galloc(int n) {
        for (int i = 0; i < n; i++) {
            globals.add(NULL_VALUE);
        }
    }

    private void exec_gload(int addr) {
        if (addr < 0 || addr >= globals.size()) {
            runtime_error("invalid global address");
        }

        Object value = globals.get(addr);
        if (value == NULL_VALUE) {
            runtime_error("accessing a NULL value");
        }

        stack.push(value);
    }

    private void exec_gstore(int addr) {
        if (addr < 0 || addr >= globals.size()) {
            runtime_error("invalid global address");
        }

        globals.set(addr, stack.pop());
    }

    private void exec_lalloc(int n) {
        for (int i = 0; i < n; i++) {
            stack.push(NULL_VALUE);
        }
    }

    private void exec_lload(int addr) {
        Object value = stackAt(FP + addr);
        if (value == NULL_VALUE) {
            runtime_error("accessing a NULL value");
        }
        stack.push(value);
    }

    private void exec_lstore(int addr) {
        int index = FP + addr;
        if (index < 0 || index >= stack.size()) {
            runtime_error("invalid frame address");
        }
        stack.set(index, stack.pop());
    }

    private void exec_pop(int n) {
        for (int i = 0; i < n; i++) {
            stack.pop();
        }
    }

    private void exec_call(int addr) {
        stack.push(FP);
        FP = stack.size() - 1;
        stack.push(IP + 1);
        IP = addr - 1;
    }

    private void exec_retval(int n) {
        Object returnValue = stack.pop();
        int returnAddress = (Integer) stackAt(FP + 1);
        int previousFP = (Integer) stackAt(FP);
        discardHandlersForCurrentFrame();
        stack.setSize(FP);
        stack.setSize(stack.size() - n);
        stack.push(returnValue);
        FP = previousFP;
        IP = returnAddress - 1;
    }

    private void exec_ret(int n) {
        int returnAddress = (Integer) stackAt(FP + 1);
        int previousFP = (Integer) stackAt(FP);
        discardHandlersForCurrentFrame();
        stack.setSize(FP);
        stack.setSize(stack.size() - n);
        FP = previousFP;
        IP = returnAddress - 1;
    }

    private void exec_aalloc() {
        int size = popInt();
        if (size < 0) {
            runtime_error("negative array size: " + size);
        }
        Object[] array = new Object[size];
        for (int i = 0; i < size; i++) {
            array[i] = NULL_VALUE;
        }
        stack.push(array);
    }

    private void exec_aload() {
        int index = popInt();
        Object[] array = popArray();
        checkArrayIndex(array, index);
        Object value = array[index];
        if (value == NULL_VALUE) {
            runtime_error("accessing a NULL value");
        }
        stack.push(value);
    }

    private void exec_astore() {
        Object value = stack.pop();
        int index = popInt();
        Object[] array = popArray();
        checkArrayIndex(array, index);
        array[index] = value;
    }

    private void exec_alength() {
        Object[] array = popArray();
        stack.push(array.length);
    }

    private void exec_slength() {
        String value = popString();
        stack.push(value.length());
    }

    private Path resolveFilePath(String pathText) {
        try {
            Path rawPath = Paths.get(pathText);
            Path resolved = rawPath.isAbsolute()
                    ? rawPath.toAbsolutePath().normalize()
                    : fileRoot.resolve(rawPath).normalize();

            if (!resolved.startsWith(fileRoot)) {
                runtime_error("file path escapes project root: " + pathText);
            }
            return resolved;
        } catch (InvalidPathException e) {
            runtime_error("invalid file path: " + pathText);
            return fileRoot;
        }
    }

    private void ensureParentDirectory(Path path) {
        Path parent = path.getParent();
        if (parent == null) {
            return;
        }
        try {
            Files.createDirectories(parent);
        } catch (IOException e) {
            runtime_error("failed to create parent directory: " + e.getMessage());
        }
    }

    private void exec_fcreate() {
        Path path = resolveFilePath(popString());
        ensureParentDirectory(path);
        try {
            if (!Files.exists(path)) {
                Files.createFile(path);
            }
        } catch (IOException e) {
            runtime_error("failed to create file: " + e.getMessage());
        }
    }

    private void exec_fread() {
        Path path = resolveFilePath(popString());
        try {
            stack.push(Files.readString(path, StandardCharsets.UTF_8));
        } catch (IOException e) {
            runtime_error("failed to read file: " + e.getMessage());
        }
    }

    private void exec_fwrite() {
        String content = popString();
        Path path = resolveFilePath(popString());
        ensureParentDirectory(path);
        try {
            Files.writeString(path, content, StandardCharsets.UTF_8, StandardOpenOption.CREATE, StandardOpenOption.TRUNCATE_EXISTING);
        } catch (IOException e) {
            runtime_error("failed to write file: " + e.getMessage());
        }
    }

    private void exec_fappend() {
        String content = popString();
        Path path = resolveFilePath(popString());
        ensureParentDirectory(path);
        try {
            Files.writeString(path, content, StandardCharsets.UTF_8, StandardOpenOption.CREATE, StandardOpenOption.APPEND);
        } catch (IOException e) {
            runtime_error("failed to append file: " + e.getMessage());
        }
    }

    private void exec_fexists() {
        Path path = resolveFilePath(popString());
        stack.push(Files.exists(path));
    }

    private void exec_fdelete() {
        Path path = resolveFilePath(popString());
        try {
            Files.deleteIfExists(path);
        } catch (IOException e) {
            runtime_error("failed to delete file: " + e.getMessage());
        }
    }

    private void exec_randint() {
        int max = popInt();
        int min = popInt();
        if (min > max) {
            runtime_error("RandomInt min cannot be greater than max");
        }
        stack.push((int) ThreadLocalRandom.current().nextLong(min, (long) max + 1L));
    }

    private void exec_randreal() {
        stack.push(ThreadLocalRandom.current().nextDouble());
    }

    private void exec_nowutc() {
        String part = popString().trim().toLowerCase(Locale.ROOT);
        ZonedDateTime now = Instant.now().atZone(ZoneOffset.UTC);

        switch (part) {
            case "datetime", "date-time", "full" ->
                    stack.push(now.format(DateTimeFormatter.ofPattern("yyyy-MM-dd HH:mm:ss.SSS 'UTC'")));
            case "date" -> stack.push(now.format(DateTimeFormatter.ISO_LOCAL_DATE));
            case "time" -> stack.push(now.format(DateTimeFormatter.ofPattern("HH:mm:ss.SSS")));
            case "year" -> stack.push(String.valueOf(now.getYear()));
            case "month" -> stack.push(String.format(Locale.ROOT, "%02d", now.getMonthValue()));
            case "day" -> stack.push(String.format(Locale.ROOT, "%02d", now.getDayOfMonth()));
            case "hour" -> stack.push(String.format(Locale.ROOT, "%02d", now.getHour()));
            case "minute" -> stack.push(String.format(Locale.ROOT, "%02d", now.getMinute()));
            case "second" -> stack.push(String.format(Locale.ROOT, "%02d", now.getSecond()));
            case "millisecond", "millis", "ms" -> stack.push(String.format(Locale.ROOT, "%03d", now.getNano() / 1_000_000));
            default -> runtime_error("unknown Now part: " + part);
        }
    }

    private void exec_sleepms() {
        int milliseconds = popInt();
        if (milliseconds < 0) {
            runtime_error("Sleep milliseconds cannot be negative");
        }
        try {
            Thread.sleep(milliseconds);
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
            runtime_error("sleep interrupted");
        }
    }

    private void exec_supper() {
        stack.push(popString().toUpperCase(Locale.ROOT));
    }

    private void exec_slower() {
        stack.push(popString().toLowerCase(Locale.ROOT));
    }

    private void exec_strim() {
        stack.push(popString().trim());
    }

    private void exec_ssubstr() {
        int length = popInt();
        int start = popInt();
        String source = popString();
        if (start < 0 || length < 0 || start + length > source.length()) {
            runtime_error("Substring range out of bounds");
        }
        stack.push(source.substring(start, start + length));
    }

    private void exec_scontains() {
        String needle = popString();
        String source = popString();
        stack.push(source.contains(needle));
    }

    private void exec_sreplace() {
        String replacement = popString();
        String target = popString();
        String source = popString();
        stack.push(source.replace(target, replacement));
    }

    private void exec_sget() {
        int index = popInt();
        String source = popString();
        if (index < 0 || index >= source.length()) {
            runtime_error("string index out of bounds: " + index);
        }
        stack.push(source.substring(index, index + 1));
    }

    private void exec_toint() {
        Object value = popScalar();
        if (value instanceof Integer integer) {
            stack.push(integer);
        } else if (value instanceof Double real) {
            stack.push(real.intValue());
        } else if (value instanceof String text) {
            try {
                stack.push(Integer.parseInt(text.trim()));
            } catch (NumberFormatException e) {
                runtime_error("cannot convert string to integer: " + text);
            }
        } else if (value instanceof Boolean bool) {
            stack.push(bool ? 1 : 0);
        } else {
            runtime_error("cannot convert value to integer");
        }
    }

    private void exec_toreal() {
        Object value = popScalar();
        if (value instanceof Integer integer) {
            stack.push((double) integer);
        } else if (value instanceof Double real) {
            stack.push(real);
        } else if (value instanceof String text) {
            try {
                stack.push(Double.parseDouble(text.trim()));
            } catch (NumberFormatException e) {
                runtime_error("cannot convert string to real: " + text);
            }
        } else if (value instanceof Boolean bool) {
            stack.push(bool ? 1.0 : 0.0);
        } else {
            runtime_error("cannot convert value to real");
        }
    }

    private void exec_tostr() {
        Object value = popScalar();
        stack.push(String.valueOf(value));
    }

    private void exec_tobool() {
        Object value = popScalar();
        if (value instanceof Boolean bool) {
            stack.push(bool);
        } else if (value instanceof Integer integer) {
            stack.push(integer != 0);
        } else if (value instanceof Double real) {
            stack.push(real != 0.0);
        } else if (value instanceof String text) {
            String normalized = text.trim();
            if (normalized.equalsIgnoreCase("true")) {
                stack.push(true);
            } else if (normalized.equalsIgnoreCase("false")) {
                stack.push(false);
            } else {
                runtime_error("cannot convert string to bool: " + text);
            }
        } else {
            runtime_error("cannot convert value to bool");
        }
    }

    private String readLine(String typeName) {
        try {
            String line = input.readLine();
            if (line == null) {
                runtime_error("expected " + typeName + " input, got end of input");
            }
            return line;
        } catch (IOException e) {
            runtime_error("failed to read input: " + e.getMessage());
            return "";
        }
    }

    private void printPrompt(Object prompt) {
        System.out.print(String.valueOf(prompt));
        System.out.flush();
    }

    private void exec_iread() {
        printPrompt(stack.pop());
        String line = readLine("integer").trim();
        try {
            stack.push(Integer.parseInt(line));
        } catch (NumberFormatException e) {
            runtime_error("invalid integer input: " + line);
        }
    }

    private void exec_dread() {
        printPrompt(stack.pop());
        String line = readLine("real").trim();
        try {
            stack.push(Double.parseDouble(line));
        } catch (NumberFormatException e) {
            runtime_error("invalid real input: " + line);
        }
    }

    private void exec_sread() {
        printPrompt(stack.pop());
        stack.push(readLine("string"));
    }

    private void exec_bread() {
        printPrompt(stack.pop());
        String line = readLine("bool").trim();
        if (line.equalsIgnoreCase("true")) {
            stack.push(true);
        } else if (line.equalsIgnoreCase("false")) {
            stack.push(false);
        } else {
            runtime_error("invalid bool input: " + line);
        }
    }

    private void exec_pushexh(int catchAddress) {
        exceptionHandlers.push(new ExceptionHandler(catchAddress, stack.size(), FP));
    }

    private void exec_popexh() {
        if (exceptionHandlers.isEmpty()) {
            runtime_error("exception handler stack underflow");
        }
        exceptionHandlers.pop();
    }

    private void discardHandlersForCurrentFrame() {
        while (!exceptionHandlers.isEmpty() && exceptionHandlers.peek().framePointer() == FP) {
            exceptionHandlers.pop();
        }
    }

    private boolean handleRuntimeError(FavaRuntimeException error) {
        if (exceptionHandlers.isEmpty()) {
            System.out.println("runtime error: " + error.getMessage());
            System.exit(1);
            return false;
        }

        ExceptionHandler handler = exceptionHandlers.pop();
        stack.setSize(handler.stackSize());
        FP = handler.framePointer();
        stack.push(error.getMessage());
        IP = handler.catchAddress();
        return true;
    }

    private void exec_inst(Instruction inst) {
        if (trace) {
            String bytes = inst.nArgs() == 0
                    ? "[" + inst.getOpCode().code() + "]"
                    : "[" + inst.getOpCode().code() + ", " + ((Instruction1Arg) inst).getArg() + "]";
            traceBuffer.add(String.format("%5s: %-15s %-14s FP=%s Stack: %s", IP, inst, bytes, FP, stack));
        }

        OpCode opc = inst.getOpCode();
        int arg;

        switch (opc) {
            case iconst -> {
                arg = ((Instruction1Arg) inst).getArg();
                exec_iconst(arg);
            }
            case dconst -> {
                arg = ((Instruction1Arg) inst).getArg();
                exec_dconst(arg);
            }
            case sconst -> {
                arg = ((Instruction1Arg) inst).getArg();
                exec_sconst(arg);
            }

            case iprint -> exec_iprint();
            case iuminus -> exec_iuminus();
            case iadd -> exec_iadd();
            case isub -> exec_isub();
            case imult -> exec_imult();
            case idiv -> exec_idiv();
            case imod -> exec_imod();
            case ieq -> exec_ieq();
            case ineq -> exec_ineq();
            case ilt -> exec_ilt();
            case ileq -> exec_ileq();
            case itod -> exec_itod();
            case itos -> exec_itos();

            case dprint -> exec_dprint();
            case duminus -> exec_duminus();
            case dadd -> exec_dadd();
            case dsub -> exec_dsub();
            case dmult -> exec_dmult();
            case ddiv -> exec_ddiv();
            case deq -> exec_deq();
            case dneq -> exec_dneq();
            case dlt -> exec_dlt();
            case dleq -> exec_dleq();
            case dtos -> exec_dtos();

            case sprint -> exec_sprint();
            case sconcat -> exec_sconcat();
            case seq -> exec_seq();
            case sneq -> exec_sneq();

            case tconst -> exec_tconst();
            case fconst -> exec_fconst();
            case bprint -> exec_bprint();
            case beq -> exec_beq();
            case bneq -> exec_bneq();
            case and -> exec_and();
            case or -> exec_or();
            case not -> exec_not();
            case btos -> exec_btos();

            case halt -> {
            }

            case jump -> {
                arg = ((Instruction1Arg) inst).getArg();
                exec_jump(arg);
            }
            case jumpf -> {
                arg = ((Instruction1Arg) inst).getArg();
                exec_jumpf(arg);
            }
            case galloc -> {
                arg = ((Instruction1Arg) inst).getArg();
                exec_galloc(arg);
            }
            case gload -> {
                arg = ((Instruction1Arg) inst).getArg();
                exec_gload(arg);
            }
            case gstore -> {
                arg = ((Instruction1Arg) inst).getArg();
                exec_gstore(arg);
            }
            case lalloc -> {
                arg = ((Instruction1Arg) inst).getArg();
                exec_lalloc(arg);
            }
            case lload -> {
                arg = ((Instruction1Arg) inst).getArg();
                exec_lload(arg);
            }
            case lstore -> {
                arg = ((Instruction1Arg) inst).getArg();
                exec_lstore(arg);
            }
            case pop -> {
                arg = ((Instruction1Arg) inst).getArg();
                exec_pop(arg);
            }
            case call -> {
                arg = ((Instruction1Arg) inst).getArg();
                exec_call(arg);
            }
            case retval -> {
                arg = ((Instruction1Arg) inst).getArg();
                exec_retval(arg);
            }
            case ret -> {
                arg = ((Instruction1Arg) inst).getArg();
                exec_ret(arg);
            }

            case aalloc -> exec_aalloc();
            case aload -> exec_aload();
            case astore -> exec_astore();
            case alength -> exec_alength();

            case iread -> exec_iread();
            case dread -> exec_dread();
            case sread -> exec_sread();
            case bread -> exec_bread();
            case slength -> exec_slength();

            case fcreate -> exec_fcreate();
            case fread -> exec_fread();
            case fwrite -> exec_fwrite();
            case fappend -> exec_fappend();
            case fexists -> exec_fexists();
            case fdelete -> exec_fdelete();

            case randint -> exec_randint();
            case randreal -> exec_randreal();
            case nowutc -> exec_nowutc();
            case sleepms -> exec_sleepms();

            case supper -> exec_supper();
            case slower -> exec_slower();
            case strim -> exec_strim();
            case ssubstr -> exec_ssubstr();
            case scontains -> exec_scontains();
            case sreplace -> exec_sreplace();
            case sget -> exec_sget();

            case toint -> exec_toint();
            case toreal -> exec_toreal();
            case tostr -> exec_tostr();
            case tobool -> exec_tobool();

            case pushexh -> {
                arg = ((Instruction1Arg) inst).getArg();
                exec_pushexh(arg);
            }
            case popexh -> exec_popexh();
        }
    }

    public void run() {
        System.out.println("*** VM output ***");

        while (IP < code.length) {
            Instruction inst = code[IP];

            if (inst.getOpCode() == OpCode.halt) {
                break;
            }

            try {
                exec_inst(inst);
                IP++;
            } catch (FavaRuntimeException error) {
                if (!handleRuntimeError(error)) {
                    break;
                }
            }
        }

        if (trace && !traceBuffer.isEmpty()) {
            System.out.println("*** VM trace ***");
            for (String line : traceBuffer) {
                System.out.println(line);
            }
        }
    }
}
