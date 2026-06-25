package CodeGenerator;

import Fava.FavaBaseVisitor;
import Fava.FavaLexer;
import Fava.FavaParser;
import SymbolTable.FavaType;
import SymbolTable.Symbol;
import SymbolTable.SymbolTable;
import TypeChecker.TypeChecker;
import VM.Instruction.Instruction;
import VM.Instruction.Instruction1Arg;
import VM.OpCode;
import org.antlr.v4.runtime.ParserRuleContext;
import org.antlr.v4.runtime.Token;
import org.antlr.v4.runtime.tree.ParseTree;
import org.antlr.v4.runtime.tree.TerminalNode;

import java.io.DataOutputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Comparator;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;

public class CodeGen extends FavaBaseVisitor<Void> {

    private record CallPatch(int instructionIndex, Symbol function) {}
    private record SourceMapEntry(int instructionAddress, int line) {}
    private record TypeInfoEntry(int line, int startColumn, int endColumn, String kind, String name, String type, int address) {}

    private final List<Instruction> instructions = new ArrayList<>();
    private final List<Object> constantPool = new ArrayList<>();
    private final Map<Double, Integer> doubleIndexes = new LinkedHashMap<>();
    private final Map<String, Integer> stringIndexes = new LinkedHashMap<>();

    private final Map<FavaParser.ExprContext, FavaType> inferredTypes;
    private final Map<ParseTree, Symbol> resolvedSymbols;
    private final SymbolTable symbolTable;
    private final List<CallPatch> callPatches = new ArrayList<>();
    private final List<SourceMapEntry> sourceMap = new ArrayList<>();

    private Symbol currentFunction;
    private int currentSourceLine = -1;

    public CodeGen(TypeChecker checker) {
        this.inferredTypes = checker.getInferredTypes();
        this.resolvedSymbols = checker.getResolvedSymbols();
        this.symbolTable = checker.getSymbolTable();
    }

    private void emit(OpCode opcode) {
        instructions.add(new Instruction(opcode));
        recordSourceMap();
    }

    private void emit(OpCode opcode, int arg) {
        instructions.add(new Instruction1Arg(opcode, arg));
        recordSourceMap();
    }

    private int currentAddress() {
        return instructions.size();
    }

    private int enterSource(ParserRuleContext ctx) {
        int previous = currentSourceLine;
        currentSourceLine = ctx.start.getLine();
        return previous;
    }

    private void exitSource(int previous) {
        currentSourceLine = previous;
    }

    private void recordSourceMap() {
        if (currentSourceLine > 0) {
            sourceMap.add(new SourceMapEntry(instructions.size() - 1, currentSourceLine));
        }
    }

    private void patchJump(int instructionIndex, int targetAddress) {
        ((Instruction1Arg) instructions.get(instructionIndex)).setArg(targetAddress);
    }

    private FavaType exprType(FavaParser.ExprContext ctx) {
        FavaType type = inferredTypes.get(ctx);
        if (type == null) {
            throw new IllegalStateException("Missing inferred type for expression: " + ctx.getText());
        }
        return type;
    }

    private FavaType indexedType(FavaType type, int indexes) {
        FavaType result = type;
        for (int i = 0; i < indexes; i++) {
            result = result.elementType();
        }
        return result;
    }

    private int saveDouble(String text) {
        double value = Double.parseDouble(text);
        Integer existing = doubleIndexes.get(value);
        if (existing != null) {
            return existing;
        }
        int index = constantPool.size();
        constantPool.add(value);
        doubleIndexes.put(value, index);
        return index;
    }

    private int saveString(String text) {
        String value = text.substring(1, text.length() - 1);
        Integer existing = stringIndexes.get(value);
        if (existing != null) {
            return existing;
        }
        int index = constantPool.size();
        constantPool.add(value);
        stringIndexes.put(value, index);
        return index;
    }

    private void emitPrintForType(FavaType type) {
        if (type.isInteger()) {
            emit(OpCode.iprint);
        } else if (type.isReal()) {
            emit(OpCode.dprint);
        } else if (type.isString()) {
            emit(OpCode.sprint);
        } else if (type.isBool()) {
            emit(OpCode.bprint);
        } else {
            throw new IllegalArgumentException("Unsupported print type: " + type.readableName());
        }
    }

    private boolean isReadCall(FavaParser.CallContext ctx) {
        return ctx.ID().getText().equalsIgnoreCase("Read");
    }

    private boolean isLengthCall(FavaParser.CallContext ctx) {
        return ctx.ID().getText().equalsIgnoreCase("Length");
    }

    private boolean isFileCall(FavaParser.CallContext ctx) {
        String name = ctx.ID().getText();
        return name.equalsIgnoreCase("CreateFile")
                || name.equalsIgnoreCase("ReadFile")
                || name.equalsIgnoreCase("WriteFile")
                || name.equalsIgnoreCase("AppendFile")
                || name.equalsIgnoreCase("FileExists")
                || name.equalsIgnoreCase("DeleteFile");
    }

    private boolean isRandomCall(FavaParser.CallContext ctx) {
        String name = ctx.ID().getText();
        return name.equalsIgnoreCase("RandomInt")
                || name.equalsIgnoreCase("RandomReal");
    }

    private boolean isTimeCall(FavaParser.CallContext ctx) {
        String name = ctx.ID().getText();
        return name.equalsIgnoreCase("Now")
                || name.equalsIgnoreCase("Sleep");
    }

    private boolean isTextCall(FavaParser.CallContext ctx) {
        String name = ctx.ID().getText();
        return name.equalsIgnoreCase("Upper")
                || name.equalsIgnoreCase("Lower")
                || name.equalsIgnoreCase("Trim")
                || name.equalsIgnoreCase("Substring")
                || name.equalsIgnoreCase("Contains")
                || name.equalsIgnoreCase("Replace");
    }

    private boolean isCastCall(FavaParser.CallContext ctx) {
        String name = ctx.ID().getText();
        return name.equalsIgnoreCase("ToInteger")
                || name.equalsIgnoreCase("ToReal")
                || name.equalsIgnoreCase("ToString")
                || name.equalsIgnoreCase("ToBool");
    }

    private void emitReadForType(FavaType type) {
        if (type.isInteger()) {
            emit(OpCode.iread);
        } else if (type.isReal()) {
            emit(OpCode.dread);
        } else if (type.isString()) {
            emit(OpCode.sread);
        } else if (type.isBool()) {
            emit(OpCode.bread);
        } else {
            throw new IllegalArgumentException("Unsupported read type: " + type.readableName());
        }
    }

    private void emitLengthForType(FavaType type) {
        if (type.isArray()) {
            emit(OpCode.alength);
        } else if (type.isString()) {
            emit(OpCode.slength);
        } else {
            throw new IllegalArgumentException("Unsupported length type: " + type.readableName());
        }
    }

    private void emitFileCall(FavaParser.CallContext ctx) {
        String name = ctx.ID().getText();
        List<FavaParser.ExprContext> arguments = ctx.argList() == null ? List.of() : ctx.argList().expr();
        for (FavaParser.ExprContext argument : arguments) {
            visit(argument);
        }

        if (name.equalsIgnoreCase("CreateFile")) {
            emit(OpCode.fcreate);
        } else if (name.equalsIgnoreCase("ReadFile")) {
            emit(OpCode.fread);
        } else if (name.equalsIgnoreCase("WriteFile")) {
            emit(OpCode.fwrite);
        } else if (name.equalsIgnoreCase("AppendFile")) {
            emit(OpCode.fappend);
        } else if (name.equalsIgnoreCase("FileExists")) {
            emit(OpCode.fexists);
        } else if (name.equalsIgnoreCase("DeleteFile")) {
            emit(OpCode.fdelete);
        } else {
            throw new IllegalArgumentException("Unsupported file call: " + name);
        }
    }

    private void emitRandomCall(FavaParser.CallContext ctx) {
        String name = ctx.ID().getText();
        List<FavaParser.ExprContext> arguments = ctx.argList() == null ? List.of() : ctx.argList().expr();
        for (FavaParser.ExprContext argument : arguments) {
            visit(argument);
        }

        if (name.equalsIgnoreCase("RandomInt")) {
            emit(OpCode.randint);
        } else if (name.equalsIgnoreCase("RandomReal")) {
            emit(OpCode.randreal);
        } else {
            throw new IllegalArgumentException("Unsupported random call: " + name);
        }
    }

    private void emitTimeCall(FavaParser.CallContext ctx) {
        String name = ctx.ID().getText();
        List<FavaParser.ExprContext> arguments = ctx.argList() == null ? List.of() : ctx.argList().expr();
        for (FavaParser.ExprContext argument : arguments) {
            visit(argument);
        }

        if (name.equalsIgnoreCase("Now")) {
            emit(OpCode.nowutc);
        } else if (name.equalsIgnoreCase("Sleep")) {
            emit(OpCode.sleepms);
        } else {
            throw new IllegalArgumentException("Unsupported time call: " + name);
        }
    }

    private void emitTextCall(FavaParser.CallContext ctx) {
        String name = ctx.ID().getText();
        List<FavaParser.ExprContext> arguments = ctx.argList() == null ? List.of() : ctx.argList().expr();
        for (FavaParser.ExprContext argument : arguments) {
            visit(argument);
        }

        if (name.equalsIgnoreCase("Upper")) {
            emit(OpCode.supper);
        } else if (name.equalsIgnoreCase("Lower")) {
            emit(OpCode.slower);
        } else if (name.equalsIgnoreCase("Trim")) {
            emit(OpCode.strim);
        } else if (name.equalsIgnoreCase("Substring")) {
            emit(OpCode.ssubstr);
        } else if (name.equalsIgnoreCase("Contains")) {
            emit(OpCode.scontains);
        } else if (name.equalsIgnoreCase("Replace")) {
            emit(OpCode.sreplace);
        } else {
            throw new IllegalArgumentException("Unsupported text call: " + name);
        }
    }

    private void emitCastCall(FavaParser.CallContext ctx) {
        String name = ctx.ID().getText();
        FavaParser.ExprContext argument = ctx.argList().expr(0);
        visit(argument);

        if (name.equalsIgnoreCase("ToInteger")) {
            emit(OpCode.toint);
        } else if (name.equalsIgnoreCase("ToReal")) {
            emit(OpCode.toreal);
        } else if (name.equalsIgnoreCase("ToString")) {
            emit(OpCode.tostr);
        } else if (name.equalsIgnoreCase("ToBool")) {
            emit(OpCode.tobool);
        } else {
            throw new IllegalArgumentException("Unsupported cast call: " + name);
        }
    }

    private void emitToStringConversion(FavaType type) {
        if (type.isInteger()) {
            emit(OpCode.itos);
        } else if (type.isReal()) {
            emit(OpCode.dtos);
        } else if (type.isBool()) {
            emit(OpCode.btos);
        }
    }

    private void emitLoad(Symbol symbol) {
        if (symbol.getKind() == Symbol.Kind.GLOBAL_VARIABLE) {
            emit(OpCode.gload, symbol.getAddress());
        } else {
            emit(OpCode.lload, symbol.getAddress());
        }
    }

    private void emitStore(Symbol symbol) {
        if (symbol.getKind() == Symbol.Kind.GLOBAL_VARIABLE) {
            emit(OpCode.gstore, symbol.getAddress());
        } else {
            emit(OpCode.lstore, symbol.getAddress());
        }
    }

    private void emitIncrement(Symbol symbol) {
        emitLoad(symbol);
        emit(OpCode.iconst, 1);
        emit(OpCode.iadd);
        emitStore(symbol);
    }

    private void emitIntToRealIfNeeded(FavaType targetType, FavaType sourceType) {
        if (targetType != null && sourceType != null && targetType.isReal() && sourceType.isInteger()) {
            emit(OpCode.itod);
        }
    }

    private int blockLocalCount(FavaParser.BlockContext ctx) {
        int total = 0;
        for (FavaParser.DeclContext decl : ctx.decl()) {
            total += decl.varDecl().size();
        }
        return total;
    }

    private int declarationLocalCount(FavaParser.DeclContext ctx) {
        return ctx.varDecl().size();
    }

    private boolean alwaysReturns(FavaParser.BlockContext ctx) {
        for (FavaParser.StmtContext stmt : ctx.stmt()) {
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
        if (stmt instanceof FavaParser.TryCatchStmtContext tryCatchStmt) {
            return alwaysReturns(tryCatchStmt.stmt(0)) && alwaysReturns(tryCatchStmt.stmt(1));
        }
        return false;
    }

    private int scalarBaseType(FavaParser.BaseTypeContext ctx) {
        if (ctx.TYPEINTEGER() != null) return FavaLexer.INT;
        if (ctx.TYPEREAL() != null) return FavaLexer.REAL;
        if (ctx.TYPESTRING() != null) return FavaLexer.STRING;
        if (ctx.TYPEBOOL() != null) return FavaLexer.BOOL;
        throw new IllegalArgumentException("unknown declared type: " + ctx.getText());
    }

    private FavaType declaredTypeToExprType(FavaParser.TypeContext ctx) {
        return FavaType.of(scalarBaseType(ctx.baseType()), ctx.arraySuffix().size());
    }

    private void emitBlock(FavaParser.BlockContext ctx, boolean emitFinalPop) {
        int localCount = blockLocalCount(ctx);

        for (ParseTree child : ctx.children) {
            if (child instanceof FavaParser.DeclContext decl) {
                int declarationLocalCount = declarationLocalCount(decl);
                if (declarationLocalCount > 0) {
                    emit(OpCode.lalloc, declarationLocalCount);
                }

                FavaType declaredType = declaredTypeToExprType(decl.type());
                for (FavaParser.VarDeclContext varDecl : decl.varDecl()) {
                    if (varDecl.expr() == null) {
                        continue;
                    }
                    Symbol symbol = resolvedSymbols.get(varDecl);
                    visit(varDecl.expr());
                    emitIntToRealIfNeeded(declaredType, exprType(varDecl.expr()));
                    emitStore(symbol);
                }
            } else if (child instanceof FavaParser.StmtContext stmt) {
                visit(stmt);
            }
        }

        if (emitFinalPop && localCount > 0) {
            emit(OpCode.pop, localCount);
        }
    }

    private void emitCall(FavaParser.CallContext ctx) {
        if (isReadCall(ctx)) {
            FavaParser.ExprContext prompt = ctx.argList().expr(0);
            visit(prompt);
            emitReadForType(exprType(prompt));
            return;
        }

        if (isLengthCall(ctx)) {
            FavaParser.ExprContext target = ctx.argList().expr(0);
            visit(target);
            emitLengthForType(exprType(target));
            return;
        }

        if (isFileCall(ctx)) {
            emitFileCall(ctx);
            return;
        }

        if (isRandomCall(ctx)) {
            emitRandomCall(ctx);
            return;
        }

        if (isTimeCall(ctx)) {
            emitTimeCall(ctx);
            return;
        }

        if (isTextCall(ctx)) {
            emitTextCall(ctx);
            return;
        }

        if (isCastCall(ctx)) {
            emitCastCall(ctx);
            return;
        }

        Symbol function = resolvedSymbols.get(ctx);
        List<FavaParser.ExprContext> arguments = ctx.argList() == null ? List.of() : ctx.argList().expr();
        for (int i = 0; i < arguments.size(); i++) {
            FavaParser.ExprContext argument = arguments.get(i);
            visit(argument);
            emitIntToRealIfNeeded(function.getParameterTypes().get(i), exprType(argument));
        }
        int callInstruction = currentAddress();
        emit(OpCode.call, -1);
        callPatches.add(new CallPatch(callInstruction, function));
    }

    private void emitIndexedLoad(FavaParser.ExprContext collectionExpr, FavaParser.ExprContext indexExpr) {
        visit(collectionExpr);
        visit(indexExpr);
        emitLoadElementForType(exprType(collectionExpr));
    }

    private void emitLoadElementForType(FavaType collectionType) {
        emit(collectionType.isString() ? OpCode.sget : OpCode.aload);
    }

    private void emitArrayStore(FavaParser.LvalueContext ctx, FavaType targetType, FavaParser.ExprContext expr) {
        emitLoad(resolvedSymbols.get(ctx.ID()));
        for (int i = 0; i < ctx.expr().size() - 1; i++) {
            visit(ctx.expr(i));
            emit(OpCode.aload);
        }
        visit(ctx.expr(ctx.expr().size() - 1));
        visit(expr);
        emitIntToRealIfNeeded(targetType, exprType(expr));
        emit(OpCode.astore);
    }

    private void patchCalls() {
        for (CallPatch patch : callPatches) {
            ((Instruction1Arg) instructions.get(patch.instructionIndex())).setArg(patch.function().getCodeAddress());
        }
    }

    @Override
    public Void visitProg(FavaParser.ProgContext ctx) {
        for (FavaParser.DeclContext decl : ctx.decl()) {
            visit(decl);
        }

        Symbol main = symbolTable.lookupFunction("main");
        int mainCall = currentAddress();
        emit(OpCode.call, -1);
        if (main != null && main.returnsValue()) {
            emit(OpCode.pop, 1);
        }
        emit(OpCode.halt);

        for (FavaParser.FuncDeclContext function : ctx.funcDecl()) {
            visit(function);
        }

        ((Instruction1Arg) instructions.get(mainCall)).setArg(main.getCodeAddress());
        patchCalls();
        return null;
    }

    @Override
    public Void visitDecl(FavaParser.DeclContext ctx) {
        int previous = enterSource(ctx);
        emit(OpCode.galloc, ctx.varDecl().size());
        FavaType declaredType = declaredTypeToExprType(ctx.type());
        for (FavaParser.VarDeclContext varDecl : ctx.varDecl()) {
            if (varDecl.expr() == null) {
                continue;
            }
            Symbol symbol = resolvedSymbols.get(varDecl);
            visit(varDecl.expr());
            emitIntToRealIfNeeded(declaredType, exprType(varDecl.expr()));
            emitStore(symbol);
        }
        exitSource(previous);
        return null;
    }

    @Override
    public Void visitFuncDecl(FavaParser.FuncDeclContext ctx) {
        int previous = enterSource(ctx);
        currentFunction = resolvedSymbols.get(ctx);
        currentFunction.setCodeAddress(currentAddress());

        emitBlock(ctx.block(), true);
        if (!currentFunction.returnsValue() && !alwaysReturns(ctx.block())) {
            emit(OpCode.ret, currentFunction.getParameterCount());
        }

        currentFunction = null;
        exitSource(previous);
        return null;
    }

    @Override
    public Void visitPrintStmt(FavaParser.PrintStmtContext ctx) {
        int previous = enterSource(ctx);
        visit(ctx.expr());
        emitPrintForType(exprType(ctx.expr()));
        exitSource(previous);
        return null;
    }

    @Override
    public Void visitAssignStmt(FavaParser.AssignStmtContext ctx) {
        int previous = enterSource(ctx);
        FavaParser.LvalueContext lvalue = ctx.lvalue();
        Symbol symbol = resolvedSymbols.get(lvalue.ID());

        if (!lvalue.expr().isEmpty()) {
            emitArrayStore(lvalue, indexedType(symbol.getType(), lvalue.expr().size()), ctx.expr());
            exitSource(previous);
            return null;
        }

        visit(ctx.expr());
        emitIntToRealIfNeeded(symbol.getType(), exprType(ctx.expr()));
        emitStore(symbol);
        exitSource(previous);
        return null;
    }

    @Override
    public Void visitCallStmt(FavaParser.CallStmtContext ctx) {
        int previous = enterSource(ctx);
        emitCall(ctx.call());
        exitSource(previous);
        return null;
    }

    @Override
    public Void visitReturnStmt(FavaParser.ReturnStmtContext ctx) {
        int previous = enterSource(ctx);
        if (ctx.expr() != null) {
            visit(ctx.expr());
            emitIntToRealIfNeeded(currentFunction.getType(), exprType(ctx.expr()));
        }

        if (ctx.expr() != null) {
            emit(OpCode.retval, currentFunction.getParameterCount());
        } else {
            emit(OpCode.ret, currentFunction.getParameterCount());
        }
        exitSource(previous);
        return null;
    }

    @Override
    public Void visitBlockStmt(FavaParser.BlockStmtContext ctx) {
        int previous = enterSource(ctx);
        emitBlock(ctx.block(), true);
        exitSource(previous);
        return null;
    }

    @Override
    public Void visitIfStmt(FavaParser.IfStmtContext ctx) {
        int previous = enterSource(ctx);
        visit(ctx.expr());
        int jumpFalse = currentAddress();
        emit(OpCode.jumpf, -1);
        visit(ctx.stmt());
        patchJump(jumpFalse, currentAddress());
        exitSource(previous);
        return null;
    }

    @Override
    public Void visitIfElseStmt(FavaParser.IfElseStmtContext ctx) {
        int previous = enterSource(ctx);
        visit(ctx.expr());
        int jumpFalse = currentAddress();
        emit(OpCode.jumpf, -1);
        visit(ctx.stmt(0));
        int jumpEnd = currentAddress();
        emit(OpCode.jump, -1);
        patchJump(jumpFalse, currentAddress());
        visit(ctx.stmt(1));
        patchJump(jumpEnd, currentAddress());
        exitSource(previous);
        return null;
    }

    @Override
    public Void visitWhileStmt(FavaParser.WhileStmtContext ctx) {
        int previous = enterSource(ctx);
        int loopStart = currentAddress();
        visit(ctx.expr());
        int jumpFalse = currentAddress();
        emit(OpCode.jumpf, -1);
        visit(ctx.stmt());
        emit(OpCode.jump, loopStart);
        patchJump(jumpFalse, currentAddress());
        exitSource(previous);
        return null;
    }

    @Override
    public Void visitForStmt(FavaParser.ForStmtContext ctx) {
        int previous = enterSource(ctx);
        Symbol loopSymbol = resolvedSymbols.get(ctx.ID(0));
        Symbol incrementSymbol = resolvedSymbols.get(ctx.ID(1));
        FavaType loopType = declaredTypeToExprType(ctx.type());

        emit(OpCode.lalloc, 1);
        visit(ctx.expr(0));
        emitIntToRealIfNeeded(loopType, exprType(ctx.expr(0)));
        emitStore(loopSymbol);

        int loopStart = currentAddress();
        visit(ctx.expr(1));
        int jumpFalse = currentAddress();
        emit(OpCode.jumpf, -1);
        visit(ctx.stmt());
        emitIncrement(incrementSymbol);
        emit(OpCode.jump, loopStart);
        patchJump(jumpFalse, currentAddress());
        emit(OpCode.pop, 1);

        exitSource(previous);
        return null;
    }

    @Override
    public Void visitForEachStmt(FavaParser.ForEachStmtContext ctx) {
        int previous = enterSource(ctx);
        Symbol loopSymbol = resolvedSymbols.get(ctx.ID());
        FavaType collectionType = exprType(ctx.expr());
        int indexAddress = loopSymbol.getAddress() + 1;
        int collectionAddress = loopSymbol.getAddress() + 2;
        Symbol indexSymbol = new Symbol("$" + loopSymbol.getName() + "_index", Symbol.Kind.LOCAL_VARIABLE, FavaType.scalar(FavaLexer.INT), indexAddress, ctx.start.getLine());
        Symbol collectionSymbol = new Symbol("$" + loopSymbol.getName() + "_collection", Symbol.Kind.LOCAL_VARIABLE, collectionType, collectionAddress, ctx.start.getLine());

        emit(OpCode.lalloc, 3);
        visit(ctx.expr());
        emitStore(collectionSymbol);
        emit(OpCode.iconst, 0);
        emitStore(indexSymbol);

        int loopStart = currentAddress();
        emitLoad(indexSymbol);
        emitLoad(collectionSymbol);
        emit(collectionType.isString() ? OpCode.slength : OpCode.alength);
        emit(OpCode.ilt);
        int jumpFalse = currentAddress();
        emit(OpCode.jumpf, -1);

        emitLoad(collectionSymbol);
        emitLoad(indexSymbol);
        emitLoadElementForType(collectionType);
        emitStore(loopSymbol);

        visit(ctx.stmt());
        emitIncrement(indexSymbol);
        emit(OpCode.jump, loopStart);
        patchJump(jumpFalse, currentAddress());
        emit(OpCode.pop, 3);

        exitSource(previous);
        return null;
    }

    @Override
    public Void visitTryCatchStmt(FavaParser.TryCatchStmtContext ctx) {
        int previous = enterSource(ctx);
        Symbol catchSymbol = ctx.ID() == null ? null : resolvedSymbols.get(ctx.ID());
        if (catchSymbol != null) {
            emit(OpCode.lalloc, 1);
        }

        int pushHandler = currentAddress();
        emit(OpCode.pushexh, -1);
        visit(ctx.stmt(0));
        emit(OpCode.popexh);
        int jumpEnd = currentAddress();
        emit(OpCode.jump, -1);

        int catchStart = currentAddress();
        patchJump(pushHandler, catchStart);
        if (catchSymbol != null) {
            emitStore(catchSymbol);
        } else {
            emit(OpCode.pop, 1);
        }
        visit(ctx.stmt(1));
        patchJump(jumpEnd, currentAddress());
        if (catchSymbol != null) {
            emit(OpCode.pop, 1);
        }

        exitSource(previous);
        return null;
    }

    @Override
    public Void visitEmptyStmt(FavaParser.EmptyStmtContext ctx) {
        return null;
    }

    @Override
    public Void visitExpr(FavaParser.ExprContext ctx) {
        int previous = enterSource(ctx);
        if (ctx.call() != null) {
            emitCall(ctx.call());
            exitSource(previous);
            return null;
        }

        if (ctx.NEW() != null) {
            visit(ctx.expr(0));
            emit(OpCode.aalloc);
            exitSource(previous);
            return null;
        }

        if (ctx.expr().size() == 2 && ctx.LBRACK() != null) {
            emitIndexedLoad(ctx.expr(0), ctx.expr(1));
            exitSource(previous);
            return null;
        }

        if (ctx.INT() != null) {
            emit(OpCode.iconst, Integer.parseInt(ctx.INT().getText()));
            exitSource(previous);
            return null;
        }

        if (ctx.REAL() != null) {
            emit(OpCode.dconst, saveDouble(ctx.REAL().getText()));
            exitSource(previous);
            return null;
        }

        if (ctx.STRING() != null) {
            emit(OpCode.sconst, saveString(ctx.STRING().getText()));
            exitSource(previous);
            return null;
        }

        if (ctx.BOOL() != null) {
            emit(ctx.BOOL().getText().equalsIgnoreCase("true") ? OpCode.tconst : OpCode.fconst);
            exitSource(previous);
            return null;
        }

        if (ctx.ID() != null) {
            emitLoad(resolvedSymbols.get(ctx.ID()));
            exitSource(previous);
            return null;
        }

        if (ctx.LPAREN() != null) {
            visit(ctx.expr(0));
            exitSource(previous);
            return null;
        }

        if (ctx.expr().size() == 1) {
            FavaParser.ExprContext child = ctx.expr(0);
            visit(child);
            FavaType childType = exprType(child);
            if (ctx.NOT() != null) {
                emit(OpCode.not);
            } else if (ctx.MINUS() != null) {
                emit(childType.isInteger() ? OpCode.iuminus : OpCode.duminus);
            }
            exitSource(previous);
            return null;
        }

        if (ctx.expr().size() == 2) {
            FavaParser.ExprContext left = ctx.expr(0);
            FavaParser.ExprContext right = ctx.expr(1);
            FavaType leftType = exprType(left);
            FavaType rightType = exprType(right);

            if (ctx.AND() != null || ctx.OR() != null) {
                visit(left);
                visit(right);
                emit(ctx.AND() != null ? OpCode.and : OpCode.or);
                exitSource(previous);
                return null;
            }

            if (ctx.EQUAL() != null || ctx.NEQUAL() != null) {
                boolean equal = ctx.EQUAL() != null;
                if (leftType.isInteger() && rightType.isInteger()) {
                    visit(left);
                    visit(right);
                    emit(equal ? OpCode.ieq : OpCode.ineq);
                    exitSource(previous);
                    return null;
                }
                if (leftType.isNumeric() && rightType.isNumeric()) {
                    visit(left);
                    emitIntToRealIfNeeded(FavaType.scalar(FavaLexer.REAL), leftType);
                    visit(right);
                    emitIntToRealIfNeeded(FavaType.scalar(FavaLexer.REAL), rightType);
                    emit(equal ? OpCode.deq : OpCode.dneq);
                    exitSource(previous);
                    return null;
                }
                if (leftType.isString()) {
                    visit(left);
                    visit(right);
                    emit(equal ? OpCode.seq : OpCode.sneq);
                    exitSource(previous);
                    return null;
                }
                visit(left);
                visit(right);
                emit(equal ? OpCode.beq : OpCode.bneq);
                exitSource(previous);
                return null;
            }

            if (ctx.SMALLER() != null || ctx.SMALLEROREQUAL() != null || ctx.LARGER() != null || ctx.LARGEROREQUAL() != null) {
                boolean intOnly = leftType.isInteger() && rightType.isInteger();
                FavaType realType = FavaType.scalar(FavaLexer.REAL);
                if (ctx.LARGER() != null || ctx.LARGEROREQUAL() != null) {
                    visit(right);
                    if (!intOnly) emitIntToRealIfNeeded(realType, rightType);
                    visit(left);
                    if (!intOnly) emitIntToRealIfNeeded(realType, leftType);
                    emit(ctx.LARGER() != null ? (intOnly ? OpCode.ilt : OpCode.dlt) : (intOnly ? OpCode.ileq : OpCode.dleq));
                    exitSource(previous);
                    return null;
                }
                visit(left);
                if (!intOnly) emitIntToRealIfNeeded(realType, leftType);
                visit(right);
                if (!intOnly) emitIntToRealIfNeeded(realType, rightType);
                emit(ctx.SMALLER() != null ? (intOnly ? OpCode.ilt : OpCode.dlt) : (intOnly ? OpCode.ileq : OpCode.dleq));
                exitSource(previous);
                return null;
            }

            if (ctx.CONCAT() != null) {
                visit(left);
                if (!leftType.isString()) {
                    emitToStringConversion(leftType);
                }
                visit(right);
                if (!rightType.isString()) {
                    emitToStringConversion(rightType);
                }
                emit(OpCode.sconcat);
                exitSource(previous);
                return null;
            }

            if (ctx.MOD() != null) {
                visit(left);
                visit(right);
                emit(OpCode.imod);
                exitSource(previous);
                return null;
            }

            boolean intOnly = leftType.isInteger() && rightType.isInteger();
            FavaType realType = FavaType.scalar(FavaLexer.REAL);
            visit(left);
            if (!intOnly) emitIntToRealIfNeeded(realType, leftType);
            visit(right);
            if (!intOnly) emitIntToRealIfNeeded(realType, rightType);

            if (ctx.PLUS() != null) {
                emit(intOnly ? OpCode.iadd : OpCode.dadd);
            } else if (ctx.MINUS() != null) {
                emit(intOnly ? OpCode.isub : OpCode.dsub);
            } else if (ctx.TIMES() != null) {
                emit(intOnly ? OpCode.imult : OpCode.dmult);
            } else {
                emit(intOnly ? OpCode.idiv : OpCode.ddiv);
            }
            exitSource(previous);
            return null;
        }

        throw new IllegalStateException("Unsupported expression: " + ctx.getText());
    }

    public void dumpConstantPool() {
        System.out.println("*** Constant pool ***");
        for (int i = 0; i < constantPool.size(); i++) {
            Object constant = constantPool.get(i);
            if (constant instanceof String) {
                System.out.println(i + ": \"" + constant + "\"");
            } else {
                System.out.println(i + ": " + constant);
            }
        }
    }

    public void dumpInstructions() {
        System.out.println("*** Instructions ***");
        for (int i = 0; i < instructions.size(); i++) {
            System.out.println(i + ": " + instructions.get(i));
        }
    }

    public void dumpTypeInfo() {
        System.out.println("*** Type info ***");
        collectTypeInfo().stream()
                .sorted(Comparator
                        .comparingInt(TypeInfoEntry::line)
                        .thenComparingInt(TypeInfoEntry::startColumn)
                        .thenComparingInt(TypeInfoEntry::endColumn))
                .forEach(info -> System.out.println(
                        info.line() + "|" +
                                info.startColumn() + "|" +
                                info.endColumn() + "|" +
                                info.kind() + "|" +
                                info.name() + "|" +
                                info.type() + "|" +
                                info.address()
                ));
    }

    public void dumpSourceMap() {
        System.out.println("*** Source map ***");
        for (SourceMapEntry entry : sourceMap) {
            System.out.println(entry.instructionAddress() + "|" + entry.line());
        }
    }

    private List<TypeInfoEntry> collectTypeInfo() {
        List<TypeInfoEntry> result = new ArrayList<>();
        for (Map.Entry<FavaParser.ExprContext, FavaType> entry : inferredTypes.entrySet()) {
            FavaParser.ExprContext ctx = entry.getKey();
            String name = ctx.getText();
            String kind = "expression";
            if (ctx.ID() != null) {
                kind = "variable";
            } else if (ctx.expr().size() == 2 && ctx.LBRACK() != null) {
                kind = "indexed element";
            } else if (ctx.call() != null) {
                kind = "call";
                name = ctx.call().ID().getText();
            }
            addTypeInfo(result, ctx.start, ctx.stop, kind, name, entry.getValue().readableName());
        }

        for (Map.Entry<ParseTree, Symbol> entry : resolvedSymbols.entrySet()) {
            if (entry.getKey() instanceof FavaParser.VarDeclContext varDecl) {
                Symbol symbol = entry.getValue();
                addTypeInfo(
                        result,
                        varDecl.ID().getSymbol(),
                        varDecl.ID().getSymbol(),
                        symbol.getKind().name().toLowerCase() + " definition",
                        symbol.getName(),
                        symbol.getType().readableName(),
                        symbol.getAddress());
                continue;
            }
            if (entry.getKey() instanceof FavaParser.FuncDeclContext functionDecl) {
                Symbol symbol = entry.getValue();
                addTypeInfo(
                        result,
                        functionDecl.ID(0).getSymbol(),
                        functionDecl.ID(0).getSymbol(),
                        "function definition",
                        symbol.getName(),
                        signature(symbol),
                        symbol.getAddress());
                continue;
            }
            if (entry.getKey() instanceof FavaParser.CallContext call) {
                Symbol symbol = entry.getValue();
                addTypeInfo(
                        result,
                        call.ID().getSymbol(),
                        call.ID().getSymbol(),
                        "call",
                        symbol.getName(),
                        signature(symbol),
                        symbol.getAddress());
                continue;
            }
            if (entry.getKey() instanceof TerminalNode node) {
                Symbol symbol = entry.getValue();
                String type = symbol.getKind() == Symbol.Kind.FUNCTION
                        ? signature(symbol)
                        : symbol.getType().readableName();
                String kind = symbol.getKind() == Symbol.Kind.FUNCTION
                        ? "call"
                        : symbol.getKind().name().toLowerCase();
                addTypeInfo(result, node.getSymbol(), node.getSymbol(), kind, symbol.getName(), type, symbol.getAddress());
            }
        }
        return result;
    }

    private void addTypeInfo(List<TypeInfoEntry> result, Token start, Token stop, String kind, String name, String type) {
        addTypeInfo(result, start, stop, kind, name, type, -1);
    }

    private void addTypeInfo(List<TypeInfoEntry> result, Token start, Token stop, String kind, String name, String type, int address) {
        if (start == null || stop == null || start.getLine() <= 0) {
            return;
        }
        int startColumn = start.getCharPositionInLine() + 1;
        int endColumn = stop.getCharPositionInLine() + stop.getText().length() + 1;
        result.add(new TypeInfoEntry(start.getLine(), startColumn, endColumn, kind, sanitize(name), sanitize(type), address));
    }

    private String signature(Symbol symbol) {
        String parameters = symbol.getParameterTypes().stream()
                .map(FavaType::readableName)
                .reduce((left, right) -> left + ", " + right)
                .orElse("");
        String returnType = symbol.returnsValue() ? symbol.getType().readableName() : "void";
        return "(" + parameters + ") -> " + returnType;
    }

    private String sanitize(String value) {
        return value.replace("|", "/").replace("\n", " ").replace("\r", " ");
    }

    public void saveBytecodes(String filename) throws IOException {
        try (DataOutputStream out = new DataOutputStream(new FileOutputStream(filename))) {
            out.writeInt(constantPool.size());
            for (Object constant : constantPool) {
                if (constant instanceof Double d) {
                    out.writeByte(1);
                    out.writeDouble(d);
                } else if (constant instanceof String s) {
                    out.writeByte(3);
                    out.writeInt(s.length());
                    for (int i = 0; i < s.length(); i++) {
                        out.writeChar(s.charAt(i));
                    }
                } else {
                    throw new IOException("Unsupported constant pool entry: " + constant);
                }
            }
            for (Instruction instruction : instructions) {
                instruction.writeTo(out);
            }
        }
    }
}
