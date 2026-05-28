package VM;

import VM.Instruction.Instruction;
import VM.Instruction.Instruction1Arg;

import java.io.ByteArrayInputStream;
import java.io.BufferedReader;
import java.io.DataInputStream;
import java.io.EOFException;
import java.io.IOException;
import java.io.InputStreamReader;
import java.util.ArrayList;
import java.util.List;
import java.util.Stack;

public class vm {
    private final boolean trace;
    private final byte[] bytecodes;

    private Instruction[] code;
    private int IP;
    private int FP = -1;

    private final Stack<Object> stack = new Stack<>();
    private final List<Object> constantPool = new ArrayList<>();
    private final List<Object> globals = new ArrayList<>();
    private final List<String> traceBuffer = new ArrayList<>();
    private final BufferedReader input = new BufferedReader(new InputStreamReader(System.in));

    private static final Object NULL_VALUE = new Object() {
        @Override
        public String toString() {
            return "NULL";
        }
    };

    public vm(byte[] bytecodes, boolean trace) {
        this.trace = trace;
        this.bytecodes = bytecodes;
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
        System.out.println("runtime error: " + msg);
        System.exit(0);
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
        stack.setSize(FP);
        stack.setSize(stack.size() - n);
        stack.push(returnValue);
        FP = previousFP;
        IP = returnAddress - 1;
    }

    private void exec_ret(int n) {
        int returnAddress = (Integer) stackAt(FP + 1);
        int previousFP = (Integer) stackAt(FP);
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
        }
    }

    public void run() {
        System.out.println("*** VM output ***");

        while (IP < code.length) {
            Instruction inst = code[IP];

            if (inst.getOpCode() == OpCode.halt) {
                break;
            }

            exec_inst(inst);
            IP++;
        }

        if (trace && !traceBuffer.isEmpty()) {
            System.out.println("*** VM trace ***");
            for (String line : traceBuffer) {
                System.out.println(line);
            }
        }
    }
}
