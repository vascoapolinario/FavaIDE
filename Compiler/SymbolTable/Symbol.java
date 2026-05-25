package SymbolTable;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

public class Symbol {

    public enum Kind {
        GLOBAL_VARIABLE,
        LOCAL_VARIABLE,
        ARGUMENT,
        FUNCTION
    }

    private final String name;
    private final Kind kind;
    private final FavaType type;
    private final int address;
    private final int line;
    private final List<FavaType> parameterTypes;
    private int codeAddress = -1;

    public Symbol(String name, Kind kind, FavaType type, int address, int line) {
        this(name, kind, type, address, line, Collections.emptyList());
    }

    public Symbol(String name, Kind kind, FavaType type, int address, int line, List<FavaType> parameterTypes) {
        this.name = name;
        this.kind = kind;
        this.type = type;
        this.address = address;
        this.line = line;
        this.parameterTypes = Collections.unmodifiableList(new ArrayList<>(parameterTypes));
    }

    public String getName() {
        return name;
    }

    public Kind getKind() {
        return kind;
    }

    public FavaType getType() {
        return type;
    }

    public int getAddress() {
        return address;
    }

    public int getLine() {
        return line;
    }

    public List<FavaType> getParameterTypes() {
        return parameterTypes;
    }

    public int getParameterCount() {
        return parameterTypes.size();
    }

    public boolean isVariable() {
        return kind != Kind.FUNCTION;
    }

    public boolean returnsValue() {
        return kind == Kind.FUNCTION && type != null;
    }

    public int getCodeAddress() {
        return codeAddress;
    }

    public void setCodeAddress(int codeAddress) {
        this.codeAddress = codeAddress;
    }
}
