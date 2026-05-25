package SymbolTable;

import java.util.ArrayList;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;

public class SymbolTable {

    private final List<Map<String, Symbol>> variableScopes = new ArrayList<>();
    private final Map<String, Symbol> functions = new LinkedHashMap<>();
    private int nextGlobalAddress = 0;

    public SymbolTable() {
        variableScopes.add(new LinkedHashMap<>());
    }

    private String normalize(String name) {
        return name.toLowerCase();
    }

    private Map<String, Symbol> currentScope() {
        return variableScopes.get(variableScopes.size() - 1);
    }

    private Map<String, Symbol> globalScope() {
        return variableScopes.get(0);
    }

    public void enterScope() {
        variableScopes.add(new LinkedHashMap<>());
    }

    public void exitScope() {
        if (variableScopes.size() == 1) {
            throw new IllegalStateException("cannot exit global scope");
        }
        variableScopes.remove(variableScopes.size() - 1);
    }

    public boolean containsInCurrentScope(String name) {
        return currentScope().containsKey(normalize(name));
    }

    public boolean hasGlobalName(String name) {
        String key = normalize(name);
        return globalScope().containsKey(key) || functions.containsKey(key);
    }

    public Symbol declareGlobalVariable(String name, FavaType type, int line) {
        String key = normalize(name);
        Symbol symbol = new Symbol(name, Symbol.Kind.GLOBAL_VARIABLE, type, nextGlobalAddress++, line);
        globalScope().put(key, symbol);
        return symbol;
    }

    public Symbol declareScopedVariable(String name, FavaType type, Symbol.Kind kind, int address, int line) {
        if (kind == Symbol.Kind.GLOBAL_VARIABLE || kind == Symbol.Kind.FUNCTION) {
            throw new IllegalArgumentException("invalid scoped symbol kind: " + kind);
        }

        String key = normalize(name);
        Symbol symbol = new Symbol(name, kind, type, address, line);
        currentScope().put(key, symbol);
        return symbol;
    }

    public Symbol declareFunction(String name, FavaType returnType, List<FavaType> parameterTypes, int line) {
        String key = normalize(name);
        Symbol symbol = new Symbol(name, Symbol.Kind.FUNCTION, returnType, -1, line, parameterTypes);
        functions.put(key, symbol);
        return symbol;
    }

    public Symbol lookupVisibleVariable(String name) {
        String key = normalize(name);
        for (int i = variableScopes.size() - 1; i >= 0; i--) {
            Symbol symbol = variableScopes.get(i).get(key);
            if (symbol != null) {
                return symbol;
            }
        }
        return null;
    }

    public Symbol lookupFunction(String name) {
        return functions.get(normalize(name));
    }
}
