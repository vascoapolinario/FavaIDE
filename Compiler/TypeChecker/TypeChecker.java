package TypeChecker;

import Fava.FavaBaseVisitor;
import Fava.FavaLexer;
import Fava.FavaParser;
import SymbolTable.FavaType;
import SymbolTable.Symbol;
import SymbolTable.SymbolTable;
import org.antlr.v4.runtime.ParserRuleContext;
import org.antlr.v4.runtime.tree.ParseTree;

import java.util.ArrayList;
import java.util.Comparator;
import java.util.IdentityHashMap;
import java.util.List;
import java.util.Map;

public class TypeChecker extends FavaBaseVisitor<FavaType> {

    private record SemanticError(int line, int column, int order, String message) {}

    private final Map<FavaParser.ExprContext, FavaType> inferredTypes = new IdentityHashMap<>();
    private final Map<ParseTree, Symbol> resolvedSymbols = new IdentityHashMap<>();
    private final List<SemanticError> semanticErrors = new ArrayList<>();
    private final SymbolTable symbolTable = new SymbolTable();

    private int errorOrder = 0;
    private Symbol currentFunction;
    private int nextLocalAddress = 2;

    public Map<FavaParser.ExprContext, FavaType> getInferredTypes() {
        return inferredTypes;
    }

    public SymbolTable getSymbolTable() {
        return symbolTable;
    }

    public Symbol getResolvedSymbol(ParseTree node) {
        return resolvedSymbols.get(node);
    }

    public Map<ParseTree, Symbol> getResolvedSymbols() {
        return resolvedSymbols;
    }

    public List<String> getSemanticErrors() {
        return semanticErrors.stream()
                .sorted(Comparator.comparingInt(SemanticError::line)
                        .thenComparingInt(SemanticError::column)
                        .thenComparingInt(SemanticError::order))
                .map(SemanticError::message)
                .toList();
    }

    public boolean foundErrors() {
        return !semanticErrors.isEmpty();
    }

    private String readableType(FavaType type) {
        return type == null ? "void" : type.readableName();
    }

    private void saveType(FavaParser.ExprContext ctx, FavaType type) {
        inferredTypes.put(ctx, type);
    }

    private void remember(ParseTree node, Symbol symbol) {
        if (symbol != null) {
            resolvedSymbols.put(node, symbol);
        }
    }

    private void addError(int line, String message) {
        addError(line, 1, message);
    }

    private void addError(ParserRuleContext ctx, String message) {
        addError(ctx.start.getLine(), ctx.start.getCharPositionInLine() + 1, message);
    }

    private void addError(int line, int column, String message) {
        semanticErrors.add(new SemanticError(
                line,
                column,
                errorOrder++,
                "semantic error at line " + line + ":" + column + " - " + message
        ));
    }

    private void addBinaryError(ParserRuleContext ctx, String operator, FavaType leftType, FavaType rightType) {
        addError(ctx, "operator " + operator + " is invalid between " + readableType(leftType) + " and " + readableType(rightType));
    }

    private void addUnaryError(ParserRuleContext ctx, String operator, FavaType operandType) {
        addError(ctx, "operator " + operator + " is invalid for " + readableType(operandType));
    }

    private int scalarBaseType(FavaParser.BaseTypeContext ctx) {
        if (ctx.TYPEINTEGER() != null) return FavaLexer.INT;
        if (ctx.TYPEREAL() != null) return FavaLexer.REAL;
        if (ctx.TYPESTRING() != null) return FavaLexer.STRING;
        if (ctx.TYPEBOOL() != null) return FavaLexer.BOOL;
        throw new IllegalArgumentException("unknown declared type: " + ctx.getText());
    }

    private int scalarBaseType(FavaParser.ExprContext ctx) {
        if (ctx.baseType() != null) return scalarBaseType(ctx.baseType());
        throw new IllegalArgumentException("unknown array element type: " + ctx.getText());
    }

    private FavaType declaredTypeToExprType(FavaParser.TypeContext ctx) {
        return FavaType.of(scalarBaseType(ctx.baseType()), ctx.arraySuffix().size());
    }

    private boolean assignmentCompatible(FavaType targetType, FavaType sourceType) {
        if (targetType.equals(sourceType)) return true;
        return targetType.isReal() && sourceType.isInteger();
    }

    private boolean isReadCall(String name) {
        return name.equalsIgnoreCase("Read");
    }

    private boolean isLengthCall(String name) {
        return name.equalsIgnoreCase("Length");
    }

    private boolean isFileCall(String name) {
        return name.equalsIgnoreCase("CreateFile")
                || name.equalsIgnoreCase("ReadFile")
                || name.equalsIgnoreCase("WriteFile")
                || name.equalsIgnoreCase("AppendFile")
                || name.equalsIgnoreCase("FileExists")
                || name.equalsIgnoreCase("DeleteFile");
    }

    private boolean isRandomCall(String name) {
        return name.equalsIgnoreCase("RandomInt")
                || name.equalsIgnoreCase("RandomReal");
    }

    private boolean isTimeCall(String name) {
        return name.equalsIgnoreCase("Now")
                || name.equalsIgnoreCase("Sleep");
    }

    private boolean isTextCall(String name) {
        return name.equalsIgnoreCase("Upper")
                || name.equalsIgnoreCase("Lower")
                || name.equalsIgnoreCase("Trim")
                || name.equalsIgnoreCase("Substring")
                || name.equalsIgnoreCase("Contains")
                || name.equalsIgnoreCase("Replace");
    }

    private boolean isCastCall(String name) {
        return name.equalsIgnoreCase("ToInteger")
                || name.equalsIgnoreCase("ToReal")
                || name.equalsIgnoreCase("ToString")
                || name.equalsIgnoreCase("ToBool");
    }

    private FavaType validateFileCall(FavaParser.CallContext ctx, boolean usedAsStatement) {
        String name = ctx.ID().getText();
        List<FavaParser.ExprContext> arguments = ctx.argList() == null ? List.of() : ctx.argList().expr();
        int expectedCount = name.equalsIgnoreCase("WriteFile") || name.equalsIgnoreCase("AppendFile") ? 2 : 1;
        if (arguments.size() != expectedCount) {
            addError(ctx, "function " + name + " expects " + expectedCount + " argument" + (expectedCount == 1 ? "" : "s"));
            for (FavaParser.ExprContext expr : arguments) {
                visit(expr);
            }
            return null;
        }

        boolean valid = true;
        for (FavaParser.ExprContext argument : arguments) {
            FavaType argumentType = visit(argument);
            if (argumentType != null && !argumentType.isString()) {
                addError(argument, "function " + name + " expects string arguments");
                valid = false;
            }
        }

        FavaType returnType;
        if (name.equalsIgnoreCase("ReadFile")) {
            returnType = FavaType.scalar(FavaLexer.STRING);
        } else if (name.equalsIgnoreCase("FileExists")) {
            returnType = FavaType.scalar(FavaLexer.BOOL);
        } else {
            returnType = null;
        }

        if (usedAsStatement) {
            if (returnType != null) {
                addError(ctx, "value of function " + name + " must be assigned to a variable");
            }
            return null;
        }

        if (returnType == null) {
            addError(ctx, "function " + name + " does not return a value");
            return null;
        }

        return valid ? returnType : null;
    }

    private FavaType validateRandomCall(FavaParser.CallContext ctx, boolean usedAsStatement) {
        String name = ctx.ID().getText();
        List<FavaParser.ExprContext> arguments = ctx.argList() == null ? List.of() : ctx.argList().expr();
        int expectedCount = name.equalsIgnoreCase("RandomInt") ? 2 : 0;
        FavaType returnType = name.equalsIgnoreCase("RandomInt")
                ? FavaType.scalar(FavaLexer.INT)
                : FavaType.scalar(FavaLexer.REAL);

        if (arguments.size() != expectedCount) {
            addError(ctx, "function " + name + " expects " + expectedCount + " argument" + (expectedCount == 1 ? "" : "s"));
            for (FavaParser.ExprContext expr : arguments) {
                visit(expr);
            }
            return null;
        }

        boolean valid = true;
        for (FavaParser.ExprContext argument : arguments) {
            FavaType argumentType = visit(argument);
            if (argumentType != null && !argumentType.isInteger()) {
                addError(argument, "function " + name + " expects integer arguments");
                valid = false;
            }
        }

        if (usedAsStatement) {
            addError(ctx, "value of function " + name + " must be assigned to a variable");
            return null;
        }

        return valid ? returnType : null;
    }

    private FavaType validateTimeCall(FavaParser.CallContext ctx, boolean usedAsStatement) {
        String name = ctx.ID().getText();
        List<FavaParser.ExprContext> arguments = ctx.argList() == null ? List.of() : ctx.argList().expr();
        int expectedCount = 1;
        FavaType returnType = name.equalsIgnoreCase("Now") ? FavaType.scalar(FavaLexer.STRING) : null;

        if (arguments.size() != expectedCount) {
            addError(ctx, "function " + name + " expects " + expectedCount + " argument" + (expectedCount == 1 ? "" : "s"));
            for (FavaParser.ExprContext expr : arguments) {
                visit(expr);
            }
            return null;
        }

        boolean valid = true;
        for (FavaParser.ExprContext argument : arguments) {
            FavaType argumentType = visit(argument);
            boolean validArgument = name.equalsIgnoreCase("Now")
                    ? argumentType != null && argumentType.isString()
                    : argumentType != null && argumentType.isInteger();
            if (argumentType != null && !validArgument) {
                addError(argument, "function " + name + " expects " + (name.equalsIgnoreCase("Now") ? "a string argument" : "integer arguments"));
                valid = false;
            }
        }

        if (usedAsStatement) {
            if (returnType != null) {
                addError(ctx, "value of function " + name + " must be assigned to a variable");
            }
            return null;
        }

        if (returnType == null) {
            addError(ctx, "function " + name + " does not return a value");
            return null;
        }

        return valid ? returnType : null;
    }

    private FavaType validateTextCall(FavaParser.CallContext ctx, boolean usedAsStatement) {
        String name = ctx.ID().getText();
        List<FavaParser.ExprContext> arguments = ctx.argList() == null ? List.of() : ctx.argList().expr();
        int expectedCount;
        if (name.equalsIgnoreCase("Substring") || name.equalsIgnoreCase("Replace")) {
            expectedCount = 3;
        } else if (name.equalsIgnoreCase("Contains")) {
            expectedCount = 2;
        } else {
            expectedCount = 1;
        }

        if (arguments.size() != expectedCount) {
            addError(ctx, "function " + name + " expects " + expectedCount + " argument" + (expectedCount == 1 ? "" : "s"));
            for (FavaParser.ExprContext expr : arguments) {
                visit(expr);
            }
            return null;
        }

        boolean valid = true;
        for (int i = 0; i < arguments.size(); i++) {
            FavaType argumentType = visit(arguments.get(i));
            boolean expectsInt = name.equalsIgnoreCase("Substring") && (i == 1 || i == 2);
            if (argumentType == null) {
                continue;
            }
            if (expectsInt) {
                if (!argumentType.isInteger()) {
                    addError(arguments.get(i), "function " + name + " expects integer start and length arguments");
                    valid = false;
                }
            } else if (!argumentType.isString()) {
                addError(arguments.get(i), "function " + name + " expects string arguments");
                valid = false;
            }
        }

        if (usedAsStatement) {
            addError(ctx, "value of function " + name + " must be assigned to a variable");
            return null;
        }

        FavaType returnType = name.equalsIgnoreCase("Contains")
                ? FavaType.scalar(FavaLexer.BOOL)
                : FavaType.scalar(FavaLexer.STRING);

        return valid ? returnType : null;
    }

    private FavaType validateCastCall(FavaParser.CallContext ctx, boolean usedAsStatement) {
        String name = ctx.ID().getText();
        List<FavaParser.ExprContext> arguments = ctx.argList() == null ? List.of() : ctx.argList().expr();
        if (arguments.size() != 1) {
            addError(ctx, "function " + name + " expects 1 argument");
            for (FavaParser.ExprContext expr : arguments) {
                visit(expr);
            }
            return null;
        }

        FavaType argumentType = visit(arguments.get(0));
        if (argumentType == null) {
            return null;
        }
        if (!argumentType.isScalar()) {
            addError(ctx, "function " + name + " expects a scalar argument");
            return null;
        }
        if (usedAsStatement) {
            addError(ctx, "value of function " + name + " must be assigned to a variable");
            return null;
        }

        if (name.equalsIgnoreCase("ToInteger")) {
            return FavaType.scalar(FavaLexer.INT);
        }
        if (name.equalsIgnoreCase("ToReal")) {
            return FavaType.scalar(FavaLexer.REAL);
        }
        if (name.equalsIgnoreCase("ToString")) {
            return FavaType.scalar(FavaLexer.STRING);
        }
        return FavaType.scalar(FavaLexer.BOOL);
    }

    private void collectGlobalDeclarations(List<FavaParser.DeclContext> declarations) {
        for (FavaParser.DeclContext decl : declarations) {
            FavaType declaredType = declaredTypeToExprType(decl.type());
            for (FavaParser.VarDeclContext varDecl : decl.varDecl()) {
                String name = varDecl.ID().getText();
                if (symbolTable.hasGlobalName(name)) {
                    addError(varDecl, name + " already declared");
                    continue;
                }
                Symbol symbol = symbolTable.declareGlobalVariable(name, declaredType, varDecl.start.getLine());
                remember(varDecl, symbol);
            }
        }
    }

    private Symbol buildTemporaryFunction(FavaParser.FuncDeclContext ctx) {
        List<FavaType> parameterTypes = new ArrayList<>();
        int paramTypeCount = ctx.ARROW() == null ? ctx.type().size() : ctx.type().size() - 1;
        for (int i = 0; i < paramTypeCount; i++) {
            parameterTypes.add(declaredTypeToExprType(ctx.type(i)));
        }
        FavaType returnType = ctx.ARROW() == null ? null : declaredTypeToExprType(ctx.type(ctx.type().size() - 1));
        return new Symbol(ctx.ID(0).getText(), Symbol.Kind.FUNCTION, returnType, -1, ctx.start.getLine(), parameterTypes);
    }

    private void collectFunctionDeclarations(List<FavaParser.FuncDeclContext> functions) {
        for (FavaParser.FuncDeclContext function : functions) {
            String name = function.ID(0).getText();
            Symbol symbol;
            if (isReadCall(name)) {
                addError(function, "Read is a built-in function");
                symbol = buildTemporaryFunction(function);
            } else if (symbolTable.hasGlobalName(name)) {
                addError(function, name + " already declared");
                symbol = buildTemporaryFunction(function);
            } else {
                List<FavaType> parameterTypes = new ArrayList<>();
                int paramTypeCount = function.ARROW() == null ? function.type().size() : function.type().size() - 1;
                for (int i = 0; i < paramTypeCount; i++) {
                    parameterTypes.add(declaredTypeToExprType(function.type(i)));
                }
                FavaType returnType = function.ARROW() == null ? null : declaredTypeToExprType(function.type(function.type().size() - 1));
                symbol = symbolTable.declareFunction(name, returnType, parameterTypes, function.start.getLine());
            }
            remember(function, symbol);
        }
    }

    private void checkGlobalInitializers(List<FavaParser.DeclContext> declarations) {
        for (FavaParser.DeclContext decl : declarations) {
            FavaType declaredType = declaredTypeToExprType(decl.type());
            for (FavaParser.VarDeclContext varDecl : decl.varDecl()) {
                if (varDecl.expr() == null) {
                    continue;
                }
                FavaType exprType = visit(varDecl.expr());
                if (exprType != null && !assignmentCompatible(declaredType, exprType)) {
                    addError(varDecl, "operator := is invalid between " + readableType(declaredType) + " and " + readableType(exprType));
                }
            }
        }
    }

    private void declareParameters(Symbol functionSymbol, FavaParser.FuncDeclContext ctx) {
        int parameterCount = functionSymbol.getParameterCount();
        if (parameterCount == 0) {
            return;
        }

        for (int i = 0; i < parameterCount; i++) {
            String name = ctx.ID(i + 1).getText();
            FavaType declaredType = declaredTypeToExprType(ctx.type(i));
            int address = i - parameterCount;
            int line = ctx.ID(i + 1).getSymbol().getLine();

            if (symbolTable.containsInCurrentScope(name)) {
                addError(line, name + " already declared");
                continue;
            }

            Symbol symbol = symbolTable.declareScopedVariable(name, declaredType, Symbol.Kind.ARGUMENT, address, line);
            remember(ctx.ID(i + 1), symbol);
        }
    }

    private void analyzeBlock(FavaParser.BlockContext ctx, boolean createScope) {
        if (createScope) {
            symbolTable.enterScope();
        }

        int blockLocals = 0;

        for (ParseTree child : ctx.children) {
            if (child instanceof FavaParser.DeclContext decl) {
                FavaType declaredType = declaredTypeToExprType(decl.type());
                for (FavaParser.VarDeclContext varDecl : decl.varDecl()) {
                    String name = varDecl.ID().getText();
                    if (symbolTable.containsInCurrentScope(name)) {
                        addError(varDecl, name + " already declared");
                    } else {
                        Symbol symbol = symbolTable.declareScopedVariable(name, declaredType, Symbol.Kind.LOCAL_VARIABLE, nextLocalAddress++, varDecl.start.getLine());
                        remember(varDecl, symbol);
                        blockLocals++;
                    }

                    if (varDecl.expr() != null) {
                        FavaType exprType = visit(varDecl.expr());
                        if (exprType != null && !assignmentCompatible(declaredType, exprType)) {
                            addError(varDecl, "operator := is invalid between " + readableType(declaredType) + " and " + readableType(exprType));
                        }
                    }
                }
            } else if (child instanceof FavaParser.StmtContext stmt) {
                visit(stmt);
            }
        }

        nextLocalAddress -= blockLocals;
        if (createScope) {
            symbolTable.exitScope();
        }
    }

    private boolean alwaysReturns(FavaParser.BlockContext block) {
        for (FavaParser.StmtContext stmt : block.stmt()) {
            if (alwaysReturns(stmt)) {
                return true;
            }
        }
        return false;
    }

    private boolean alwaysReturns(FavaParser.StmtContext stmt) {
        if (stmt instanceof FavaParser.ReturnStmtContext) {
            return true;
        }
        if (stmt instanceof FavaParser.BlockStmtContext blockStmt) {
            return alwaysReturns(blockStmt.block());
        }
        if (stmt instanceof FavaParser.IfElseStmtContext ifElseStmt) {
            return alwaysReturns(ifElseStmt.stmt(0)) && alwaysReturns(ifElseStmt.stmt(1));
        }
        return false;
    }

    private boolean addLoopVariable(FavaParser.StmtContext ctx, ParseTree idNode, String name, FavaType type) {
        if (symbolTable.containsInCurrentScope(name)) {
            addError(ctx, name + " already declared");
            return false;
        }
        Symbol symbol = symbolTable.declareScopedVariable(name, type, Symbol.Kind.LOCAL_VARIABLE, nextLocalAddress++, ctx.start.getLine());
        remember(idNode, symbol);
        return true;
    }

    private Symbol resolveVariable(String name) {
        return symbolTable.lookupVisibleVariable(name);
    }

    private Symbol resolveFunction(String name) {
        return symbolTable.lookupFunction(name);
    }

    private FavaType validateLvalue(FavaParser.LvalueContext ctx) {
        String name = ctx.ID().getText();
        Symbol variable = resolveVariable(name);

        if (variable == null) {
            if (resolveFunction(name) != null) {
                addError(ctx, name + " is not a variable");
            } else {
                addError(ctx, name + " not declared");
            }
            for (FavaParser.ExprContext index : ctx.expr()) {
                visit(index);
            }
            return null;
        }

        remember(ctx.ID(), variable);

        if (ctx.expr().isEmpty()) {
            return variable.getType();
        }

        FavaType targetType = variable.getType();
        for (FavaParser.ExprContext index : ctx.expr()) {
            FavaType indexType = visit(index);
            if (indexType != null && !indexType.isInteger()) {
                addError(ctx, "array index must be of type integer");
            }
            if (targetType.isString()) {
                addError(ctx, "cannot assign to a string character");
                return null;
            }
            if (!targetType.isArray()) {
                addError(ctx, name + " is not an array");
                return null;
            }
            targetType = targetType.elementType();
        }

        return targetType;
    }

    private FavaType validateCall(FavaParser.CallContext ctx, boolean usedAsStatement) {
        String name = ctx.ID().getText();
        Symbol variable = resolveVariable(name);
        if (variable != null) {
            addError(ctx, name + " is not a function");
            if (ctx.argList() != null) {
                for (FavaParser.ExprContext expr : ctx.argList().expr()) {
                    visit(expr);
                }
            }
            return null;
        }

        if (isReadCall(name)) {
            List<FavaParser.ExprContext> arguments = ctx.argList() == null ? List.of() : ctx.argList().expr();
            if (arguments.size() != 1) {
                addError(ctx, "function Read expects 1 argument");
                for (FavaParser.ExprContext expr : arguments) {
                    visit(expr);
                }
                return null;
            }

            FavaType promptType = visit(arguments.get(0));
            if (promptType == null) {
                return null;
            }
            if (!promptType.isScalar()) {
                addError(ctx, "Read expects a scalar prompt expression");
                return null;
            }
            if (usedAsStatement) {
                addError(ctx, "value of function Read must be assigned to a variable");
                return null;
            }

            return promptType;
        }

        if (isLengthCall(name)) {
            List<FavaParser.ExprContext> arguments = ctx.argList() == null ? List.of() : ctx.argList().expr();
            if (arguments.size() != 1) {
                addError(ctx, "function Length expects 1 argument");
                for (FavaParser.ExprContext expr : arguments) {
                    visit(expr);
                }
                return null;
            }

            FavaType argumentType = visit(arguments.get(0));
            if (argumentType == null) {
                return null;
            }
            if (!argumentType.isArray() && !argumentType.isString()) {
                addError(ctx, "Length expects an array or string, got " + readableType(argumentType));
                return null;
            }
            if (usedAsStatement) {
                addError(ctx, "value of function Length must be assigned to a variable");
                return null;
            }

            return FavaType.scalar(FavaLexer.INT);
        }

        if (isFileCall(name)) {
            return validateFileCall(ctx, usedAsStatement);
        }

        if (isRandomCall(name)) {
            return validateRandomCall(ctx, usedAsStatement);
        }

        if (isTimeCall(name)) {
            return validateTimeCall(ctx, usedAsStatement);
        }

        if (isTextCall(name)) {
            return validateTextCall(ctx, usedAsStatement);
        }

        if (isCastCall(name)) {
            return validateCastCall(ctx, usedAsStatement);
        }

        Symbol function = resolveFunction(name);
        if (function == null) {
            addError(ctx, name + " not declared");
            if (ctx.argList() != null) {
                for (FavaParser.ExprContext expr : ctx.argList().expr()) {
                    visit(expr);
                }
            }
            return null;
        }

        remember(ctx, function);

        List<FavaParser.ExprContext> arguments = ctx.argList() == null ? List.of() : ctx.argList().expr();
        if (arguments.size() != function.getParameterCount()) {
            addError(ctx, "function " + name + " expects " + function.getParameterCount() + " arguments");
            for (FavaParser.ExprContext expr : arguments) {
                visit(expr);
            }
            return null;
        }

        boolean valid = true;
        for (int i = 0; i < arguments.size(); i++) {
            FavaType argumentType = visit(arguments.get(i));
            FavaType parameterType = function.getParameterTypes().get(i);
            if (argumentType != null && !assignmentCompatible(parameterType, argumentType)) {
                addError(ctx, "expecting an expression of type " + readableType(parameterType) + " for argument of function " + name);
                valid = false;
            }
        }

        if (usedAsStatement) {
            if (function.returnsValue()) {
                addError(ctx, "value of function " + name + " must be assigned to a variable");
                valid = false;
            }
            return null;
        }

        if (!function.returnsValue()) {
            addError(ctx, "function " + name + " does not return a value");
            return null;
        }

        if (!valid) {
            return null;
        }

        saveType((FavaParser.ExprContext) ctx.getParent(), function.getType());
        return function.getType();
    }

    @Override
    public FavaType visitProg(FavaParser.ProgContext ctx) {
        collectGlobalDeclarations(ctx.decl());
        collectFunctionDeclarations(ctx.funcDecl());

        if (resolveFunction("main") == null) {
            addError(ctx, "missing main()");
        }

        currentFunction = null;
        checkGlobalInitializers(ctx.decl());

        for (FavaParser.FuncDeclContext function : ctx.funcDecl()) {
            visit(function);
        }

        return null;
    }

    @Override
    public FavaType visitFuncDecl(FavaParser.FuncDeclContext ctx) {
        Symbol previousFunction = currentFunction;
        int previousNextLocalAddress = nextLocalAddress;

        currentFunction = getResolvedSymbol(ctx);
        nextLocalAddress = 2;

        symbolTable.enterScope();
        declareParameters(currentFunction, ctx);
        analyzeBlock(ctx.block(), false);

        if (currentFunction.returnsValue() && !alwaysReturns(ctx.block())) {
            addError(ctx.block(), "missing return in function " + currentFunction.getName());
        }

        symbolTable.exitScope();
        currentFunction = previousFunction;
        nextLocalAddress = previousNextLocalAddress;
        return null;
    }

    @Override
    public FavaType visitPrintStmt(FavaParser.PrintStmtContext ctx) {
        FavaType type = visit(ctx.expr());
        if (type != null && type.isArray()) {
            addError(ctx, "cannot print expression of type " + readableType(type));
        }
        return null;
    }

    @Override
    public FavaType visitAssignStmt(FavaParser.AssignStmtContext ctx) {
        FavaType targetType = validateLvalue(ctx.lvalue());
        FavaType exprType = visit(ctx.expr());
        if (targetType != null && exprType != null && !assignmentCompatible(targetType, exprType)) {
            addError(ctx, "operator := is invalid between " + readableType(targetType) + " and " + readableType(exprType));
        }
        return null;
    }

    @Override
    public FavaType visitCallStmt(FavaParser.CallStmtContext ctx) {
        validateCall(ctx.call(), true);
        return null;
    }

    @Override
    public FavaType visitReturnStmt(FavaParser.ReturnStmtContext ctx) {
        if (currentFunction == null) {
            return null;
        }

        FavaType returnType = currentFunction.getType();
        if (returnType == null) {
            if (ctx.expr() != null) {
                visit(ctx.expr());
                addError(ctx, "function " + currentFunction.getName() + " does not return a value");
            }
            return null;
        }

        if (ctx.expr() == null) {
            addError(ctx, "function " + currentFunction.getName() + " must return a value of type " + readableType(returnType));
            return null;
        }

        FavaType exprType = visit(ctx.expr());
        if (exprType != null && !assignmentCompatible(returnType, exprType)) {
            addError(ctx, "function " + currentFunction.getName() + " must return a value of type " + readableType(returnType));
        }
        return null;
    }

    @Override
    public FavaType visitBlockStmt(FavaParser.BlockStmtContext ctx) {
        analyzeBlock(ctx.block(), true);
        return null;
    }

    @Override
    public FavaType visitWhileStmt(FavaParser.WhileStmtContext ctx) {
        FavaType conditionType = visit(ctx.expr());
        if (conditionType != null && !conditionType.isBool()) {
            addError(ctx, "while expression must be of type bool");
        }
        visit(ctx.stmt());
        return null;
    }

    @Override
    public FavaType visitForStmt(FavaParser.ForStmtContext ctx) {
        symbolTable.enterScope();

        FavaType loopType = declaredTypeToExprType(ctx.type());
        String loopName = ctx.ID(0).getText();
        boolean declaredLoopVariable = addLoopVariable(ctx, ctx.ID(0), loopName, loopType);

        FavaType initType = visit(ctx.expr(0));
        if (initType != null && !assignmentCompatible(loopType, initType)) {
            addError(ctx, "operator := is invalid between " + readableType(loopType) + " and " + readableType(initType));
        }

        FavaType conditionType = visit(ctx.expr(1));
        if (conditionType != null && !conditionType.isBool()) {
            addError(ctx, "for expression must be of type bool");
        }

        Symbol incrementSymbol = resolveVariable(ctx.ID(1).getText());
        if (incrementSymbol == null) {
            addError(ctx, ctx.ID(1).getText() + " not declared");
        } else {
            remember(ctx.ID(1), incrementSymbol);
            if (!incrementSymbol.getType().isInteger()) {
                addError(ctx, "operator ++ is invalid for " + readableType(incrementSymbol.getType()));
            }
        }

        visit(ctx.stmt());

        if (declaredLoopVariable) {
            nextLocalAddress--;
        }
        symbolTable.exitScope();
        return null;
    }

    @Override
    public FavaType visitForEachStmt(FavaParser.ForEachStmtContext ctx) {
        FavaType collectionType = visit(ctx.expr());
        FavaType itemType = FavaType.scalar(FavaLexer.INT);
        if (collectionType != null) {
            if (collectionType.isArray()) {
                itemType = collectionType.elementType();
            } else if (collectionType.isString()) {
                itemType = FavaType.scalar(FavaLexer.STRING);
            } else {
                addError(ctx, "for-in expects an array or string expression");
            }
        }

        symbolTable.enterScope();
        boolean declaredLoopVariable = addLoopVariable(ctx, ctx.ID(), ctx.ID().getText(), itemType);
        nextLocalAddress += 2;
        visit(ctx.stmt());
        nextLocalAddress -= declaredLoopVariable ? 3 : 2;
        symbolTable.exitScope();
        return null;
    }

    @Override
    public FavaType visitIfStmt(FavaParser.IfStmtContext ctx) {
        FavaType conditionType = visit(ctx.expr());
        if (conditionType != null && !conditionType.isBool()) {
            addError(ctx, "if expression must be of type bool");
        }
        visit(ctx.stmt());
        return null;
    }

    @Override
    public FavaType visitIfElseStmt(FavaParser.IfElseStmtContext ctx) {
        FavaType conditionType = visit(ctx.expr());
        if (conditionType != null && !conditionType.isBool()) {
            addError(ctx, "if expression must be of type bool");
        }
        visit(ctx.stmt(0));
        visit(ctx.stmt(1));
        return null;
    }

    @Override
    public FavaType visitEmptyStmt(FavaParser.EmptyStmtContext ctx) {
        return null;
    }

    @Override
    public FavaType visitExpr(FavaParser.ExprContext ctx) {
        if (ctx.call() != null) {
            FavaType type = validateCall(ctx.call(), false);
            if (type != null) {
                saveType(ctx, type);
            }
            return type;
        }

        if (ctx.NEW() != null) {
            FavaType sizeType = visit(ctx.expr(0));
            if (sizeType != null && !sizeType.isInteger()) {
                addError(ctx, "array size must be of type integer");
            }
            FavaType type = FavaType.of(scalarBaseType(ctx), ctx.arraySuffix().size() + 1);
            saveType(ctx, type);
            return type;
        }

        if (ctx.expr().size() == 2 && ctx.LBRACK() != null) {
            FavaType arrayType = visit(ctx.expr(0));
            FavaType indexType = visit(ctx.expr(1));
            if (indexType != null && !indexType.isInteger()) {
                addError(ctx, "array index must be of type integer");
            }
            if (arrayType == null) {
                return null;
            }
            if (arrayType.isString()) {
                FavaType type = FavaType.scalar(FavaLexer.STRING);
                saveType(ctx, type);
                return type;
            }
            if (!arrayType.isArray()) {
                addError(ctx, "cannot index expression of type " + readableType(arrayType));
                return null;
            }
            FavaType type = arrayType.elementType();
            saveType(ctx, type);
            return type;
        }

        if (ctx.INT() != null) {
            FavaType type = FavaType.scalar(FavaLexer.INT);
            saveType(ctx, type);
            return type;
        }

        if (ctx.REAL() != null) {
            FavaType type = FavaType.scalar(FavaLexer.REAL);
            saveType(ctx, type);
            return type;
        }

        if (ctx.STRING() != null) {
            FavaType type = FavaType.scalar(FavaLexer.STRING);
            saveType(ctx, type);
            return type;
        }

        if (ctx.BOOL() != null) {
            FavaType type = FavaType.scalar(FavaLexer.BOOL);
            saveType(ctx, type);
            return type;
        }

        if (ctx.ID() != null) {
            String name = ctx.ID().getText();
            Symbol variable = resolveVariable(name);
            if (variable != null) {
                remember(ctx.ID(), variable);
                saveType(ctx, variable.getType());
                return variable.getType();
            }
            if (resolveFunction(name) != null) {
                addError(ctx, name + " is not a variable");
            } else {
                addError(ctx, name + " not declared");
            }
            return null;
        }

        if (ctx.LPAREN() != null) {
            FavaType innerType = visit(ctx.expr(0));
            if (innerType != null) {
                saveType(ctx, innerType);
            }
            return innerType;
        }

        if (ctx.expr().size() == 1) {
            FavaType operandType = visit(ctx.expr(0));
            if (operandType == null) {
                return null;
            }

            if (ctx.NOT() != null) {
                if (operandType.isBool()) {
                    FavaType type = FavaType.scalar(FavaLexer.BOOL);
                    saveType(ctx, type);
                    return type;
                }
                addUnaryError(ctx, "not", operandType);
                return null;
            }

            if (ctx.MINUS() != null) {
                if (operandType.isInteger() || operandType.isReal()) {
                    saveType(ctx, operandType);
                    return operandType;
                }
                addUnaryError(ctx, "-", operandType);
                return null;
            }
        }

        if (ctx.expr().size() == 2) {
            FavaType leftType = visit(ctx.expr(0));
            FavaType rightType = visit(ctx.expr(1));
            if (leftType == null || rightType == null) {
                return null;
            }

            String operatorText = ctx.getChild(1).getText().toLowerCase();

            if (ctx.AND() != null || ctx.OR() != null) {
                if (leftType.isBool() && rightType.isBool()) {
                    FavaType type = FavaType.scalar(FavaLexer.BOOL);
                    saveType(ctx, type);
                    return type;
                }
                addBinaryError(ctx, operatorText, leftType, rightType);
                return null;
            }

            if (ctx.EQUAL() != null || ctx.NEQUAL() != null) {
                boolean allowed =
                        (leftType.isBool() && rightType.isBool()) ||
                                (leftType.isInteger() && rightType.isInteger()) ||
                                (leftType.isInteger() && rightType.isReal()) ||
                                (leftType.isReal() && rightType.isInteger()) ||
                                (leftType.isReal() && rightType.isReal()) ||
                                (leftType.isString() && rightType.isString());
                if (allowed) {
                    FavaType type = FavaType.scalar(FavaLexer.BOOL);
                    saveType(ctx, type);
                    return type;
                }
                addBinaryError(ctx, operatorText, leftType, rightType);
                return null;
            }

            if (ctx.SMALLER() != null || ctx.LARGER() != null || ctx.SMALLEROREQUAL() != null || ctx.LARGEROREQUAL() != null) {
                if (leftType.isNumeric() && rightType.isNumeric()) {
                    FavaType type = FavaType.scalar(FavaLexer.BOOL);
                    saveType(ctx, type);
                    return type;
                }
                addBinaryError(ctx, operatorText, leftType, rightType);
                return null;
            }

            if (ctx.CONCAT() != null) {
                boolean allowed =
                        (leftType.isBool() && rightType.isString()) ||
                                (leftType.isString() && rightType.isBool()) ||
                                (leftType.isInteger() && rightType.isString()) ||
                                (leftType.isString() && rightType.isInteger()) ||
                                (leftType.isReal() && rightType.isString()) ||
                                (leftType.isString() && rightType.isReal()) ||
                                (leftType.isString() && rightType.isString());
                if (allowed) {
                    FavaType type = FavaType.scalar(FavaLexer.STRING);
                    saveType(ctx, type);
                    return type;
                }
                addBinaryError(ctx, "||", leftType, rightType);
                return null;
            }

            if (ctx.MOD() != null) {
                if (leftType.isInteger() && rightType.isInteger()) {
                    FavaType type = FavaType.scalar(FavaLexer.INT);
                    saveType(ctx, type);
                    return type;
                }
                addBinaryError(ctx, "mod", leftType, rightType);
                return null;
            }

            if (ctx.PLUS() != null || ctx.MINUS() != null || ctx.TIMES() != null || ctx.DIV() != null) {
                if (leftType.isInteger() && rightType.isInteger()) {
                    FavaType type = FavaType.scalar(FavaLexer.INT);
                    saveType(ctx, type);
                    return type;
                }
                if (leftType.isNumeric() && rightType.isNumeric()) {
                    FavaType type = FavaType.scalar(FavaLexer.REAL);
                    saveType(ctx, type);
                    return type;
                }
                addBinaryError(ctx, operatorText, leftType, rightType);
                return null;
            }
        }

        return null;
    }
}

