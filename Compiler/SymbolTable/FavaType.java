package SymbolTable;

import Fava.FavaLexer;

public record FavaType(int baseType, int dimensions) {

    public static FavaType scalar(int baseType) {
        return new FavaType(baseType, 0);
    }

    public static FavaType array(int baseType) {
        return new FavaType(baseType, 1);
    }

    public static FavaType of(int baseType, int dimensions) {
        return new FavaType(baseType, dimensions);
    }

    public boolean isArray() {
        return dimensions > 0;
    }

    public boolean isScalar() {
        return dimensions == 0;
    }

    public boolean isInteger() {
        return isScalar() && baseType == FavaLexer.INT;
    }

    public boolean isReal() {
        return isScalar() && baseType == FavaLexer.REAL;
    }

    public boolean isString() {
        return isScalar() && baseType == FavaLexer.STRING;
    }

    public boolean isBool() {
        return isScalar() && baseType == FavaLexer.BOOL;
    }

    public boolean isNumeric() {
        return isInteger() || isReal();
    }

    public FavaType elementType() {
        if (!isArray()) {
            throw new IllegalStateException("scalar type has no element type");
        }
        return new FavaType(baseType, dimensions - 1);
    }

    public String readableName() {
        String name;
        if (baseType == FavaLexer.INT) {
            name = "integer";
        } else if (baseType == FavaLexer.REAL) {
            name = "real";
        } else if (baseType == FavaLexer.STRING) {
            name = "string";
        } else if (baseType == FavaLexer.BOOL) {
            name = "bool";
        } else {
            throw new IllegalArgumentException("unknown type: " + baseType);
        }

        return name + "[]".repeat(dimensions);
    }
}
