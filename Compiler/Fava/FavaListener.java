// Generated from Fava.g4 by ANTLR 4.13.2
package Fava;
import org.antlr.v4.runtime.tree.ParseTreeListener;

/**
 * This interface defines a complete listener for a parse tree produced by
 * {@link FavaParser}.
 */
public interface FavaListener extends ParseTreeListener {
	/**
	 * Enter a parse tree produced by {@link FavaParser#prog}.
	 * @param ctx the parse tree
	 */
	void enterProg(FavaParser.ProgContext ctx);
	/**
	 * Exit a parse tree produced by {@link FavaParser#prog}.
	 * @param ctx the parse tree
	 */
	void exitProg(FavaParser.ProgContext ctx);
	/**
	 * Enter a parse tree produced by {@link FavaParser#funcDecl}.
	 * @param ctx the parse tree
	 */
	void enterFuncDecl(FavaParser.FuncDeclContext ctx);
	/**
	 * Exit a parse tree produced by {@link FavaParser#funcDecl}.
	 * @param ctx the parse tree
	 */
	void exitFuncDecl(FavaParser.FuncDeclContext ctx);
	/**
	 * Enter a parse tree produced by {@link FavaParser#decl}.
	 * @param ctx the parse tree
	 */
	void enterDecl(FavaParser.DeclContext ctx);
	/**
	 * Exit a parse tree produced by {@link FavaParser#decl}.
	 * @param ctx the parse tree
	 */
	void exitDecl(FavaParser.DeclContext ctx);
	/**
	 * Enter a parse tree produced by {@link FavaParser#varDecl}.
	 * @param ctx the parse tree
	 */
	void enterVarDecl(FavaParser.VarDeclContext ctx);
	/**
	 * Exit a parse tree produced by {@link FavaParser#varDecl}.
	 * @param ctx the parse tree
	 */
	void exitVarDecl(FavaParser.VarDeclContext ctx);
	/**
	 * Enter a parse tree produced by {@link FavaParser#type}.
	 * @param ctx the parse tree
	 */
	void enterType(FavaParser.TypeContext ctx);
	/**
	 * Exit a parse tree produced by {@link FavaParser#type}.
	 * @param ctx the parse tree
	 */
	void exitType(FavaParser.TypeContext ctx);
	/**
	 * Enter a parse tree produced by {@link FavaParser#baseType}.
	 * @param ctx the parse tree
	 */
	void enterBaseType(FavaParser.BaseTypeContext ctx);
	/**
	 * Exit a parse tree produced by {@link FavaParser#baseType}.
	 * @param ctx the parse tree
	 */
	void exitBaseType(FavaParser.BaseTypeContext ctx);
	/**
	 * Enter a parse tree produced by {@link FavaParser#arraySuffix}.
	 * @param ctx the parse tree
	 */
	void enterArraySuffix(FavaParser.ArraySuffixContext ctx);
	/**
	 * Exit a parse tree produced by {@link FavaParser#arraySuffix}.
	 * @param ctx the parse tree
	 */
	void exitArraySuffix(FavaParser.ArraySuffixContext ctx);
	/**
	 * Enter a parse tree produced by {@link FavaParser#block}.
	 * @param ctx the parse tree
	 */
	void enterBlock(FavaParser.BlockContext ctx);
	/**
	 * Exit a parse tree produced by {@link FavaParser#block}.
	 * @param ctx the parse tree
	 */
	void exitBlock(FavaParser.BlockContext ctx);
	/**
	 * Enter a parse tree produced by the {@code PrintStmt}
	 * labeled alternative in {@link FavaParser#stmt}.
	 * @param ctx the parse tree
	 */
	void enterPrintStmt(FavaParser.PrintStmtContext ctx);
	/**
	 * Exit a parse tree produced by the {@code PrintStmt}
	 * labeled alternative in {@link FavaParser#stmt}.
	 * @param ctx the parse tree
	 */
	void exitPrintStmt(FavaParser.PrintStmtContext ctx);
	/**
	 * Enter a parse tree produced by the {@code AssignStmt}
	 * labeled alternative in {@link FavaParser#stmt}.
	 * @param ctx the parse tree
	 */
	void enterAssignStmt(FavaParser.AssignStmtContext ctx);
	/**
	 * Exit a parse tree produced by the {@code AssignStmt}
	 * labeled alternative in {@link FavaParser#stmt}.
	 * @param ctx the parse tree
	 */
	void exitAssignStmt(FavaParser.AssignStmtContext ctx);
	/**
	 * Enter a parse tree produced by the {@code CallStmt}
	 * labeled alternative in {@link FavaParser#stmt}.
	 * @param ctx the parse tree
	 */
	void enterCallStmt(FavaParser.CallStmtContext ctx);
	/**
	 * Exit a parse tree produced by the {@code CallStmt}
	 * labeled alternative in {@link FavaParser#stmt}.
	 * @param ctx the parse tree
	 */
	void exitCallStmt(FavaParser.CallStmtContext ctx);
	/**
	 * Enter a parse tree produced by the {@code ReturnStmt}
	 * labeled alternative in {@link FavaParser#stmt}.
	 * @param ctx the parse tree
	 */
	void enterReturnStmt(FavaParser.ReturnStmtContext ctx);
	/**
	 * Exit a parse tree produced by the {@code ReturnStmt}
	 * labeled alternative in {@link FavaParser#stmt}.
	 * @param ctx the parse tree
	 */
	void exitReturnStmt(FavaParser.ReturnStmtContext ctx);
	/**
	 * Enter a parse tree produced by the {@code BlockStmt}
	 * labeled alternative in {@link FavaParser#stmt}.
	 * @param ctx the parse tree
	 */
	void enterBlockStmt(FavaParser.BlockStmtContext ctx);
	/**
	 * Exit a parse tree produced by the {@code BlockStmt}
	 * labeled alternative in {@link FavaParser#stmt}.
	 * @param ctx the parse tree
	 */
	void exitBlockStmt(FavaParser.BlockStmtContext ctx);
	/**
	 * Enter a parse tree produced by the {@code WhileStmt}
	 * labeled alternative in {@link FavaParser#stmt}.
	 * @param ctx the parse tree
	 */
	void enterWhileStmt(FavaParser.WhileStmtContext ctx);
	/**
	 * Exit a parse tree produced by the {@code WhileStmt}
	 * labeled alternative in {@link FavaParser#stmt}.
	 * @param ctx the parse tree
	 */
	void exitWhileStmt(FavaParser.WhileStmtContext ctx);
	/**
	 * Enter a parse tree produced by the {@code ForStmt}
	 * labeled alternative in {@link FavaParser#stmt}.
	 * @param ctx the parse tree
	 */
	void enterForStmt(FavaParser.ForStmtContext ctx);
	/**
	 * Exit a parse tree produced by the {@code ForStmt}
	 * labeled alternative in {@link FavaParser#stmt}.
	 * @param ctx the parse tree
	 */
	void exitForStmt(FavaParser.ForStmtContext ctx);
	/**
	 * Enter a parse tree produced by the {@code ForEachStmt}
	 * labeled alternative in {@link FavaParser#stmt}.
	 * @param ctx the parse tree
	 */
	void enterForEachStmt(FavaParser.ForEachStmtContext ctx);
	/**
	 * Exit a parse tree produced by the {@code ForEachStmt}
	 * labeled alternative in {@link FavaParser#stmt}.
	 * @param ctx the parse tree
	 */
	void exitForEachStmt(FavaParser.ForEachStmtContext ctx);
	/**
	 * Enter a parse tree produced by the {@code IfElseStmt}
	 * labeled alternative in {@link FavaParser#stmt}.
	 * @param ctx the parse tree
	 */
	void enterIfElseStmt(FavaParser.IfElseStmtContext ctx);
	/**
	 * Exit a parse tree produced by the {@code IfElseStmt}
	 * labeled alternative in {@link FavaParser#stmt}.
	 * @param ctx the parse tree
	 */
	void exitIfElseStmt(FavaParser.IfElseStmtContext ctx);
	/**
	 * Enter a parse tree produced by the {@code IfStmt}
	 * labeled alternative in {@link FavaParser#stmt}.
	 * @param ctx the parse tree
	 */
	void enterIfStmt(FavaParser.IfStmtContext ctx);
	/**
	 * Exit a parse tree produced by the {@code IfStmt}
	 * labeled alternative in {@link FavaParser#stmt}.
	 * @param ctx the parse tree
	 */
	void exitIfStmt(FavaParser.IfStmtContext ctx);
	/**
	 * Enter a parse tree produced by the {@code EmptyStmt}
	 * labeled alternative in {@link FavaParser#stmt}.
	 * @param ctx the parse tree
	 */
	void enterEmptyStmt(FavaParser.EmptyStmtContext ctx);
	/**
	 * Exit a parse tree produced by the {@code EmptyStmt}
	 * labeled alternative in {@link FavaParser#stmt}.
	 * @param ctx the parse tree
	 */
	void exitEmptyStmt(FavaParser.EmptyStmtContext ctx);
	/**
	 * Enter a parse tree produced by {@link FavaParser#lvalue}.
	 * @param ctx the parse tree
	 */
	void enterLvalue(FavaParser.LvalueContext ctx);
	/**
	 * Exit a parse tree produced by {@link FavaParser#lvalue}.
	 * @param ctx the parse tree
	 */
	void exitLvalue(FavaParser.LvalueContext ctx);
	/**
	 * Enter a parse tree produced by {@link FavaParser#call}.
	 * @param ctx the parse tree
	 */
	void enterCall(FavaParser.CallContext ctx);
	/**
	 * Exit a parse tree produced by {@link FavaParser#call}.
	 * @param ctx the parse tree
	 */
	void exitCall(FavaParser.CallContext ctx);
	/**
	 * Enter a parse tree produced by {@link FavaParser#argList}.
	 * @param ctx the parse tree
	 */
	void enterArgList(FavaParser.ArgListContext ctx);
	/**
	 * Exit a parse tree produced by {@link FavaParser#argList}.
	 * @param ctx the parse tree
	 */
	void exitArgList(FavaParser.ArgListContext ctx);
	/**
	 * Enter a parse tree produced by {@link FavaParser#expr}.
	 * @param ctx the parse tree
	 */
	void enterExpr(FavaParser.ExprContext ctx);
	/**
	 * Exit a parse tree produced by {@link FavaParser#expr}.
	 * @param ctx the parse tree
	 */
	void exitExpr(FavaParser.ExprContext ctx);
}