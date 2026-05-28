// Generated from Fava.g4 by ANTLR 4.13.2
package Fava;
import org.antlr.v4.runtime.atn.*;
import org.antlr.v4.runtime.dfa.DFA;
import org.antlr.v4.runtime.*;
import org.antlr.v4.runtime.misc.*;
import org.antlr.v4.runtime.tree.*;
import java.util.List;
import java.util.Iterator;
import java.util.ArrayList;

@SuppressWarnings({"all", "warnings", "unchecked", "unused", "cast", "CheckReturnValue", "this-escape"})
public class FavaParser extends Parser {
	static { RuntimeMetaData.checkVersion("4.13.2", RuntimeMetaData.VERSION); }

	protected static final DFA[] _decisionToDFA;
	protected static final PredictionContextCache _sharedContextCache =
		new PredictionContextCache();
	public static final int
		T__0=1, T__1=2, ASSIGN=3, ARROW=4, EQUAL=5, NEQUAL=6, CONCAT=7, INC=8, 
		LPAREN=9, RPAREN=10, LBRACE=11, RBRACE=12, LBRACK=13, RBRACK=14, PLUS=15, 
		MINUS=16, TIMES=17, DIV=18, SMALLER=19, LARGER=20, SMALLEROREQUAL=21, 
		LARGEROREQUAL=22, INT=23, REAL=24, BOOL=25, STRING=26, FUNCTION=27, PRINT=28, 
		RETURN=29, WHILE=30, FOR=31, IN=32, IF=33, ELSE=34, NEW=35, TYPEBOOL=36, 
		TYPEINTEGER=37, TYPEREAL=38, TYPESTRING=39, MOD=40, NOT=41, AND=42, OR=43, 
		TRUE=44, FALSE=45, ID=46, WS=47, SL_COMMENT=48, ML_COMMENT=49;
	public static final int
		RULE_prog = 0, RULE_funcDecl = 1, RULE_decl = 2, RULE_varDecl = 3, RULE_type = 4, 
		RULE_baseType = 5, RULE_arraySuffix = 6, RULE_block = 7, RULE_stmt = 8, 
		RULE_lvalue = 9, RULE_call = 10, RULE_argList = 11, RULE_expr = 12;
	private static String[] makeRuleNames() {
		return new String[] {
			"prog", "funcDecl", "decl", "varDecl", "type", "baseType", "arraySuffix", 
			"block", "stmt", "lvalue", "call", "argList", "expr"
		};
	}
	public static final String[] ruleNames = makeRuleNames();

	private static String[] makeLiteralNames() {
		return new String[] {
			null, "','", "';'", "':='", "'->'", "'='", "'<>'", "'||'", "'++'", "'('", 
			"')'", "'{'", "'}'", "'['", "']'", "'+'", "'-'", "'*'", "'/'", "'<'", 
			"'>'", "'<='", "'>='"
		};
	}
	private static final String[] _LITERAL_NAMES = makeLiteralNames();
	private static String[] makeSymbolicNames() {
		return new String[] {
			null, null, null, "ASSIGN", "ARROW", "EQUAL", "NEQUAL", "CONCAT", "INC", 
			"LPAREN", "RPAREN", "LBRACE", "RBRACE", "LBRACK", "RBRACK", "PLUS", "MINUS", 
			"TIMES", "DIV", "SMALLER", "LARGER", "SMALLEROREQUAL", "LARGEROREQUAL", 
			"INT", "REAL", "BOOL", "STRING", "FUNCTION", "PRINT", "RETURN", "WHILE", 
			"FOR", "IN", "IF", "ELSE", "NEW", "TYPEBOOL", "TYPEINTEGER", "TYPEREAL", 
			"TYPESTRING", "MOD", "NOT", "AND", "OR", "TRUE", "FALSE", "ID", "WS", 
			"SL_COMMENT", "ML_COMMENT"
		};
	}
	private static final String[] _SYMBOLIC_NAMES = makeSymbolicNames();
	public static final Vocabulary VOCABULARY = new VocabularyImpl(_LITERAL_NAMES, _SYMBOLIC_NAMES);

	/**
	 * @deprecated Use {@link #VOCABULARY} instead.
	 */
	@Deprecated
	public static final String[] tokenNames;
	static {
		tokenNames = new String[_SYMBOLIC_NAMES.length];
		for (int i = 0; i < tokenNames.length; i++) {
			tokenNames[i] = VOCABULARY.getLiteralName(i);
			if (tokenNames[i] == null) {
				tokenNames[i] = VOCABULARY.getSymbolicName(i);
			}

			if (tokenNames[i] == null) {
				tokenNames[i] = "<INVALID>";
			}
		}
	}

	@Override
	@Deprecated
	public String[] getTokenNames() {
		return tokenNames;
	}

	@Override

	public Vocabulary getVocabulary() {
		return VOCABULARY;
	}

	@Override
	public String getGrammarFileName() { return "Fava.g4"; }

	@Override
	public String[] getRuleNames() { return ruleNames; }

	@Override
	public String getSerializedATN() { return _serializedATN; }

	@Override
	public ATN getATN() { return _ATN; }

	public FavaParser(TokenStream input) {
		super(input);
		_interp = new ParserATNSimulator(this,_ATN,_decisionToDFA,_sharedContextCache);
	}

	@SuppressWarnings("CheckReturnValue")
	public static class ProgContext extends ParserRuleContext {
		public TerminalNode EOF() { return getToken(FavaParser.EOF, 0); }
		public List<DeclContext> decl() {
			return getRuleContexts(DeclContext.class);
		}
		public DeclContext decl(int i) {
			return getRuleContext(DeclContext.class,i);
		}
		public List<FuncDeclContext> funcDecl() {
			return getRuleContexts(FuncDeclContext.class);
		}
		public FuncDeclContext funcDecl(int i) {
			return getRuleContext(FuncDeclContext.class,i);
		}
		public ProgContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_prog; }
		@Override
		public void enterRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).enterProg(this);
		}
		@Override
		public void exitRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).exitProg(this);
		}
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof FavaVisitor ) return ((FavaVisitor<? extends T>)visitor).visitProg(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ProgContext prog() throws RecognitionException {
		ProgContext _localctx = new ProgContext(_ctx, getState());
		enterRule(_localctx, 0, RULE_prog);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(29);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 1030792151040L) != 0)) {
				{
				{
				setState(26);
				decl();
				}
				}
				setState(31);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(33); 
			_errHandler.sync(this);
			_la = _input.LA(1);
			do {
				{
				{
				setState(32);
				funcDecl();
				}
				}
				setState(35); 
				_errHandler.sync(this);
				_la = _input.LA(1);
			} while ( _la==FUNCTION );
			setState(37);
			match(EOF);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class FuncDeclContext extends ParserRuleContext {
		public TerminalNode FUNCTION() { return getToken(FavaParser.FUNCTION, 0); }
		public List<TerminalNode> ID() { return getTokens(FavaParser.ID); }
		public TerminalNode ID(int i) {
			return getToken(FavaParser.ID, i);
		}
		public TerminalNode LPAREN() { return getToken(FavaParser.LPAREN, 0); }
		public TerminalNode RPAREN() { return getToken(FavaParser.RPAREN, 0); }
		public BlockContext block() {
			return getRuleContext(BlockContext.class,0);
		}
		public List<TypeContext> type() {
			return getRuleContexts(TypeContext.class);
		}
		public TypeContext type(int i) {
			return getRuleContext(TypeContext.class,i);
		}
		public TerminalNode ARROW() { return getToken(FavaParser.ARROW, 0); }
		public FuncDeclContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_funcDecl; }
		@Override
		public void enterRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).enterFuncDecl(this);
		}
		@Override
		public void exitRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).exitFuncDecl(this);
		}
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof FavaVisitor ) return ((FavaVisitor<? extends T>)visitor).visitFuncDecl(this);
			else return visitor.visitChildren(this);
		}
	}

	public final FuncDeclContext funcDecl() throws RecognitionException {
		FuncDeclContext _localctx = new FuncDeclContext(_ctx, getState());
		enterRule(_localctx, 2, RULE_funcDecl);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(39);
			match(FUNCTION);
			setState(40);
			match(ID);
			setState(41);
			match(LPAREN);
			setState(53);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if ((((_la) & ~0x3f) == 0 && ((1L << _la) & 1030792151040L) != 0)) {
				{
				setState(42);
				type();
				setState(43);
				match(ID);
				setState(50);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while (_la==T__0) {
					{
					{
					setState(44);
					match(T__0);
					setState(45);
					type();
					setState(46);
					match(ID);
					}
					}
					setState(52);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				}
			}

			setState(55);
			match(RPAREN);
			setState(58);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==ARROW) {
				{
				setState(56);
				match(ARROW);
				setState(57);
				type();
				}
			}

			setState(60);
			block();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class DeclContext extends ParserRuleContext {
		public TypeContext type() {
			return getRuleContext(TypeContext.class,0);
		}
		public List<VarDeclContext> varDecl() {
			return getRuleContexts(VarDeclContext.class);
		}
		public VarDeclContext varDecl(int i) {
			return getRuleContext(VarDeclContext.class,i);
		}
		public DeclContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_decl; }
		@Override
		public void enterRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).enterDecl(this);
		}
		@Override
		public void exitRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).exitDecl(this);
		}
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof FavaVisitor ) return ((FavaVisitor<? extends T>)visitor).visitDecl(this);
			else return visitor.visitChildren(this);
		}
	}

	public final DeclContext decl() throws RecognitionException {
		DeclContext _localctx = new DeclContext(_ctx, getState());
		enterRule(_localctx, 4, RULE_decl);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(62);
			type();
			setState(63);
			varDecl();
			setState(68);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==T__0) {
				{
				{
				setState(64);
				match(T__0);
				setState(65);
				varDecl();
				}
				}
				setState(70);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(71);
			match(T__1);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class VarDeclContext extends ParserRuleContext {
		public TerminalNode ID() { return getToken(FavaParser.ID, 0); }
		public TerminalNode ASSIGN() { return getToken(FavaParser.ASSIGN, 0); }
		public ExprContext expr() {
			return getRuleContext(ExprContext.class,0);
		}
		public VarDeclContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_varDecl; }
		@Override
		public void enterRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).enterVarDecl(this);
		}
		@Override
		public void exitRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).exitVarDecl(this);
		}
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof FavaVisitor ) return ((FavaVisitor<? extends T>)visitor).visitVarDecl(this);
			else return visitor.visitChildren(this);
		}
	}

	public final VarDeclContext varDecl() throws RecognitionException {
		VarDeclContext _localctx = new VarDeclContext(_ctx, getState());
		enterRule(_localctx, 6, RULE_varDecl);
		try {
			setState(77);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,6,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(73);
				match(ID);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(74);
				match(ID);
				setState(75);
				match(ASSIGN);
				setState(76);
				expr(0);
				}
				break;
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class TypeContext extends ParserRuleContext {
		public BaseTypeContext baseType() {
			return getRuleContext(BaseTypeContext.class,0);
		}
		public List<ArraySuffixContext> arraySuffix() {
			return getRuleContexts(ArraySuffixContext.class);
		}
		public ArraySuffixContext arraySuffix(int i) {
			return getRuleContext(ArraySuffixContext.class,i);
		}
		public TypeContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_type; }
		@Override
		public void enterRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).enterType(this);
		}
		@Override
		public void exitRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).exitType(this);
		}
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof FavaVisitor ) return ((FavaVisitor<? extends T>)visitor).visitType(this);
			else return visitor.visitChildren(this);
		}
	}

	public final TypeContext type() throws RecognitionException {
		TypeContext _localctx = new TypeContext(_ctx, getState());
		enterRule(_localctx, 8, RULE_type);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(79);
			baseType();
			setState(83);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==LBRACK) {
				{
				{
				setState(80);
				arraySuffix();
				}
				}
				setState(85);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class BaseTypeContext extends ParserRuleContext {
		public TerminalNode TYPEBOOL() { return getToken(FavaParser.TYPEBOOL, 0); }
		public TerminalNode TYPEINTEGER() { return getToken(FavaParser.TYPEINTEGER, 0); }
		public TerminalNode TYPEREAL() { return getToken(FavaParser.TYPEREAL, 0); }
		public TerminalNode TYPESTRING() { return getToken(FavaParser.TYPESTRING, 0); }
		public BaseTypeContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_baseType; }
		@Override
		public void enterRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).enterBaseType(this);
		}
		@Override
		public void exitRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).exitBaseType(this);
		}
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof FavaVisitor ) return ((FavaVisitor<? extends T>)visitor).visitBaseType(this);
			else return visitor.visitChildren(this);
		}
	}

	public final BaseTypeContext baseType() throws RecognitionException {
		BaseTypeContext _localctx = new BaseTypeContext(_ctx, getState());
		enterRule(_localctx, 10, RULE_baseType);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(86);
			_la = _input.LA(1);
			if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & 1030792151040L) != 0)) ) {
			_errHandler.recoverInline(this);
			}
			else {
				if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
				_errHandler.reportMatch(this);
				consume();
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class ArraySuffixContext extends ParserRuleContext {
		public TerminalNode LBRACK() { return getToken(FavaParser.LBRACK, 0); }
		public TerminalNode RBRACK() { return getToken(FavaParser.RBRACK, 0); }
		public ArraySuffixContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_arraySuffix; }
		@Override
		public void enterRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).enterArraySuffix(this);
		}
		@Override
		public void exitRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).exitArraySuffix(this);
		}
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof FavaVisitor ) return ((FavaVisitor<? extends T>)visitor).visitArraySuffix(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ArraySuffixContext arraySuffix() throws RecognitionException {
		ArraySuffixContext _localctx = new ArraySuffixContext(_ctx, getState());
		enterRule(_localctx, 12, RULE_arraySuffix);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(88);
			match(LBRACK);
			setState(89);
			match(RBRACK);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class BlockContext extends ParserRuleContext {
		public TerminalNode LBRACE() { return getToken(FavaParser.LBRACE, 0); }
		public TerminalNode RBRACE() { return getToken(FavaParser.RBRACE, 0); }
		public List<DeclContext> decl() {
			return getRuleContexts(DeclContext.class);
		}
		public DeclContext decl(int i) {
			return getRuleContext(DeclContext.class,i);
		}
		public List<StmtContext> stmt() {
			return getRuleContexts(StmtContext.class);
		}
		public StmtContext stmt(int i) {
			return getRuleContext(StmtContext.class,i);
		}
		public BlockContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_block; }
		@Override
		public void enterRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).enterBlock(this);
		}
		@Override
		public void exitRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).exitBlock(this);
		}
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof FavaVisitor ) return ((FavaVisitor<? extends T>)visitor).visitBlock(this);
			else return visitor.visitChildren(this);
		}
	}

	public final BlockContext block() throws RecognitionException {
		BlockContext _localctx = new BlockContext(_ctx, getState());
		enterRule(_localctx, 14, RULE_block);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(91);
			match(LBRACE);
			setState(96);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 71412152797188L) != 0)) {
				{
				setState(94);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case TYPEBOOL:
				case TYPEINTEGER:
				case TYPEREAL:
				case TYPESTRING:
					{
					setState(92);
					decl();
					}
					break;
				case T__1:
				case LBRACE:
				case PRINT:
				case RETURN:
				case WHILE:
				case FOR:
				case IF:
				case ID:
					{
					setState(93);
					stmt();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(98);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(99);
			match(RBRACE);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class StmtContext extends ParserRuleContext {
		public StmtContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_stmt; }
	 
		public StmtContext() { }
		public void copyFrom(StmtContext ctx) {
			super.copyFrom(ctx);
		}
	}
	@SuppressWarnings("CheckReturnValue")
	public static class PrintStmtContext extends StmtContext {
		public TerminalNode PRINT() { return getToken(FavaParser.PRINT, 0); }
		public ExprContext expr() {
			return getRuleContext(ExprContext.class,0);
		}
		public PrintStmtContext(StmtContext ctx) { copyFrom(ctx); }
		@Override
		public void enterRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).enterPrintStmt(this);
		}
		@Override
		public void exitRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).exitPrintStmt(this);
		}
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof FavaVisitor ) return ((FavaVisitor<? extends T>)visitor).visitPrintStmt(this);
			else return visitor.visitChildren(this);
		}
	}
	@SuppressWarnings("CheckReturnValue")
	public static class IfStmtContext extends StmtContext {
		public TerminalNode IF() { return getToken(FavaParser.IF, 0); }
		public TerminalNode LPAREN() { return getToken(FavaParser.LPAREN, 0); }
		public ExprContext expr() {
			return getRuleContext(ExprContext.class,0);
		}
		public TerminalNode RPAREN() { return getToken(FavaParser.RPAREN, 0); }
		public StmtContext stmt() {
			return getRuleContext(StmtContext.class,0);
		}
		public IfStmtContext(StmtContext ctx) { copyFrom(ctx); }
		@Override
		public void enterRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).enterIfStmt(this);
		}
		@Override
		public void exitRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).exitIfStmt(this);
		}
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof FavaVisitor ) return ((FavaVisitor<? extends T>)visitor).visitIfStmt(this);
			else return visitor.visitChildren(this);
		}
	}
	@SuppressWarnings("CheckReturnValue")
	public static class WhileStmtContext extends StmtContext {
		public TerminalNode WHILE() { return getToken(FavaParser.WHILE, 0); }
		public TerminalNode LPAREN() { return getToken(FavaParser.LPAREN, 0); }
		public ExprContext expr() {
			return getRuleContext(ExprContext.class,0);
		}
		public TerminalNode RPAREN() { return getToken(FavaParser.RPAREN, 0); }
		public StmtContext stmt() {
			return getRuleContext(StmtContext.class,0);
		}
		public WhileStmtContext(StmtContext ctx) { copyFrom(ctx); }
		@Override
		public void enterRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).enterWhileStmt(this);
		}
		@Override
		public void exitRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).exitWhileStmt(this);
		}
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof FavaVisitor ) return ((FavaVisitor<? extends T>)visitor).visitWhileStmt(this);
			else return visitor.visitChildren(this);
		}
	}
	@SuppressWarnings("CheckReturnValue")
	public static class IfElseStmtContext extends StmtContext {
		public TerminalNode IF() { return getToken(FavaParser.IF, 0); }
		public TerminalNode LPAREN() { return getToken(FavaParser.LPAREN, 0); }
		public ExprContext expr() {
			return getRuleContext(ExprContext.class,0);
		}
		public TerminalNode RPAREN() { return getToken(FavaParser.RPAREN, 0); }
		public List<StmtContext> stmt() {
			return getRuleContexts(StmtContext.class);
		}
		public StmtContext stmt(int i) {
			return getRuleContext(StmtContext.class,i);
		}
		public TerminalNode ELSE() { return getToken(FavaParser.ELSE, 0); }
		public IfElseStmtContext(StmtContext ctx) { copyFrom(ctx); }
		@Override
		public void enterRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).enterIfElseStmt(this);
		}
		@Override
		public void exitRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).exitIfElseStmt(this);
		}
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof FavaVisitor ) return ((FavaVisitor<? extends T>)visitor).visitIfElseStmt(this);
			else return visitor.visitChildren(this);
		}
	}
	@SuppressWarnings("CheckReturnValue")
	public static class AssignStmtContext extends StmtContext {
		public LvalueContext lvalue() {
			return getRuleContext(LvalueContext.class,0);
		}
		public TerminalNode ASSIGN() { return getToken(FavaParser.ASSIGN, 0); }
		public ExprContext expr() {
			return getRuleContext(ExprContext.class,0);
		}
		public AssignStmtContext(StmtContext ctx) { copyFrom(ctx); }
		@Override
		public void enterRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).enterAssignStmt(this);
		}
		@Override
		public void exitRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).exitAssignStmt(this);
		}
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof FavaVisitor ) return ((FavaVisitor<? extends T>)visitor).visitAssignStmt(this);
			else return visitor.visitChildren(this);
		}
	}
	@SuppressWarnings("CheckReturnValue")
	public static class BlockStmtContext extends StmtContext {
		public BlockContext block() {
			return getRuleContext(BlockContext.class,0);
		}
		public BlockStmtContext(StmtContext ctx) { copyFrom(ctx); }
		@Override
		public void enterRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).enterBlockStmt(this);
		}
		@Override
		public void exitRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).exitBlockStmt(this);
		}
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof FavaVisitor ) return ((FavaVisitor<? extends T>)visitor).visitBlockStmt(this);
			else return visitor.visitChildren(this);
		}
	}
	@SuppressWarnings("CheckReturnValue")
	public static class CallStmtContext extends StmtContext {
		public CallContext call() {
			return getRuleContext(CallContext.class,0);
		}
		public CallStmtContext(StmtContext ctx) { copyFrom(ctx); }
		@Override
		public void enterRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).enterCallStmt(this);
		}
		@Override
		public void exitRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).exitCallStmt(this);
		}
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof FavaVisitor ) return ((FavaVisitor<? extends T>)visitor).visitCallStmt(this);
			else return visitor.visitChildren(this);
		}
	}
	@SuppressWarnings("CheckReturnValue")
	public static class ForEachStmtContext extends StmtContext {
		public TerminalNode FOR() { return getToken(FavaParser.FOR, 0); }
		public TerminalNode ID() { return getToken(FavaParser.ID, 0); }
		public TerminalNode IN() { return getToken(FavaParser.IN, 0); }
		public ExprContext expr() {
			return getRuleContext(ExprContext.class,0);
		}
		public StmtContext stmt() {
			return getRuleContext(StmtContext.class,0);
		}
		public ForEachStmtContext(StmtContext ctx) { copyFrom(ctx); }
		@Override
		public void enterRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).enterForEachStmt(this);
		}
		@Override
		public void exitRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).exitForEachStmt(this);
		}
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof FavaVisitor ) return ((FavaVisitor<? extends T>)visitor).visitForEachStmt(this);
			else return visitor.visitChildren(this);
		}
	}
	@SuppressWarnings("CheckReturnValue")
	public static class EmptyStmtContext extends StmtContext {
		public EmptyStmtContext(StmtContext ctx) { copyFrom(ctx); }
		@Override
		public void enterRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).enterEmptyStmt(this);
		}
		@Override
		public void exitRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).exitEmptyStmt(this);
		}
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof FavaVisitor ) return ((FavaVisitor<? extends T>)visitor).visitEmptyStmt(this);
			else return visitor.visitChildren(this);
		}
	}
	@SuppressWarnings("CheckReturnValue")
	public static class ReturnStmtContext extends StmtContext {
		public TerminalNode RETURN() { return getToken(FavaParser.RETURN, 0); }
		public ExprContext expr() {
			return getRuleContext(ExprContext.class,0);
		}
		public ReturnStmtContext(StmtContext ctx) { copyFrom(ctx); }
		@Override
		public void enterRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).enterReturnStmt(this);
		}
		@Override
		public void exitRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).exitReturnStmt(this);
		}
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof FavaVisitor ) return ((FavaVisitor<? extends T>)visitor).visitReturnStmt(this);
			else return visitor.visitChildren(this);
		}
	}
	@SuppressWarnings("CheckReturnValue")
	public static class ForStmtContext extends StmtContext {
		public TerminalNode FOR() { return getToken(FavaParser.FOR, 0); }
		public TerminalNode LPAREN() { return getToken(FavaParser.LPAREN, 0); }
		public TypeContext type() {
			return getRuleContext(TypeContext.class,0);
		}
		public List<TerminalNode> ID() { return getTokens(FavaParser.ID); }
		public TerminalNode ID(int i) {
			return getToken(FavaParser.ID, i);
		}
		public TerminalNode ASSIGN() { return getToken(FavaParser.ASSIGN, 0); }
		public List<ExprContext> expr() {
			return getRuleContexts(ExprContext.class);
		}
		public ExprContext expr(int i) {
			return getRuleContext(ExprContext.class,i);
		}
		public TerminalNode INC() { return getToken(FavaParser.INC, 0); }
		public TerminalNode RPAREN() { return getToken(FavaParser.RPAREN, 0); }
		public StmtContext stmt() {
			return getRuleContext(StmtContext.class,0);
		}
		public ForStmtContext(StmtContext ctx) { copyFrom(ctx); }
		@Override
		public void enterRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).enterForStmt(this);
		}
		@Override
		public void exitRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).exitForStmt(this);
		}
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof FavaVisitor ) return ((FavaVisitor<? extends T>)visitor).visitForStmt(this);
			else return visitor.visitChildren(this);
		}
	}

	public final StmtContext stmt() throws RecognitionException {
		StmtContext _localctx = new StmtContext(_ctx, getState());
		enterRule(_localctx, 16, RULE_stmt);
		int _la;
		try {
			setState(160);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,11,_ctx) ) {
			case 1:
				_localctx = new PrintStmtContext(_localctx);
				enterOuterAlt(_localctx, 1);
				{
				setState(101);
				match(PRINT);
				setState(102);
				expr(0);
				setState(103);
				match(T__1);
				}
				break;
			case 2:
				_localctx = new AssignStmtContext(_localctx);
				enterOuterAlt(_localctx, 2);
				{
				setState(105);
				lvalue();
				setState(106);
				match(ASSIGN);
				setState(107);
				expr(0);
				setState(108);
				match(T__1);
				}
				break;
			case 3:
				_localctx = new CallStmtContext(_localctx);
				enterOuterAlt(_localctx, 3);
				{
				setState(110);
				call();
				setState(111);
				match(T__1);
				}
				break;
			case 4:
				_localctx = new ReturnStmtContext(_localctx);
				enterOuterAlt(_localctx, 4);
				{
				setState(113);
				match(RETURN);
				setState(115);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if ((((_la) & ~0x3f) == 0 && ((1L << _la) & 72602253066752L) != 0)) {
					{
					setState(114);
					expr(0);
					}
				}

				setState(117);
				match(T__1);
				}
				break;
			case 5:
				_localctx = new BlockStmtContext(_localctx);
				enterOuterAlt(_localctx, 5);
				{
				setState(118);
				block();
				}
				break;
			case 6:
				_localctx = new WhileStmtContext(_localctx);
				enterOuterAlt(_localctx, 6);
				{
				setState(119);
				match(WHILE);
				setState(120);
				match(LPAREN);
				setState(121);
				expr(0);
				setState(122);
				match(RPAREN);
				setState(123);
				stmt();
				}
				break;
			case 7:
				_localctx = new ForStmtContext(_localctx);
				enterOuterAlt(_localctx, 7);
				{
				setState(125);
				match(FOR);
				setState(126);
				match(LPAREN);
				setState(127);
				type();
				setState(128);
				match(ID);
				setState(129);
				match(ASSIGN);
				setState(130);
				expr(0);
				setState(131);
				match(T__1);
				setState(132);
				expr(0);
				setState(133);
				match(T__1);
				setState(134);
				match(ID);
				setState(135);
				match(INC);
				setState(136);
				match(RPAREN);
				setState(137);
				stmt();
				}
				break;
			case 8:
				_localctx = new ForEachStmtContext(_localctx);
				enterOuterAlt(_localctx, 8);
				{
				setState(139);
				match(FOR);
				setState(140);
				match(ID);
				setState(141);
				match(IN);
				setState(142);
				expr(0);
				setState(143);
				stmt();
				}
				break;
			case 9:
				_localctx = new IfElseStmtContext(_localctx);
				enterOuterAlt(_localctx, 9);
				{
				setState(145);
				match(IF);
				setState(146);
				match(LPAREN);
				setState(147);
				expr(0);
				setState(148);
				match(RPAREN);
				setState(149);
				stmt();
				setState(150);
				match(ELSE);
				setState(151);
				stmt();
				}
				break;
			case 10:
				_localctx = new IfStmtContext(_localctx);
				enterOuterAlt(_localctx, 10);
				{
				setState(153);
				match(IF);
				setState(154);
				match(LPAREN);
				setState(155);
				expr(0);
				setState(156);
				match(RPAREN);
				setState(157);
				stmt();
				}
				break;
			case 11:
				_localctx = new EmptyStmtContext(_localctx);
				enterOuterAlt(_localctx, 11);
				{
				setState(159);
				match(T__1);
				}
				break;
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class LvalueContext extends ParserRuleContext {
		public TerminalNode ID() { return getToken(FavaParser.ID, 0); }
		public List<TerminalNode> LBRACK() { return getTokens(FavaParser.LBRACK); }
		public TerminalNode LBRACK(int i) {
			return getToken(FavaParser.LBRACK, i);
		}
		public List<ExprContext> expr() {
			return getRuleContexts(ExprContext.class);
		}
		public ExprContext expr(int i) {
			return getRuleContext(ExprContext.class,i);
		}
		public List<TerminalNode> RBRACK() { return getTokens(FavaParser.RBRACK); }
		public TerminalNode RBRACK(int i) {
			return getToken(FavaParser.RBRACK, i);
		}
		public LvalueContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_lvalue; }
		@Override
		public void enterRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).enterLvalue(this);
		}
		@Override
		public void exitRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).exitLvalue(this);
		}
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof FavaVisitor ) return ((FavaVisitor<? extends T>)visitor).visitLvalue(this);
			else return visitor.visitChildren(this);
		}
	}

	public final LvalueContext lvalue() throws RecognitionException {
		LvalueContext _localctx = new LvalueContext(_ctx, getState());
		enterRule(_localctx, 18, RULE_lvalue);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(162);
			match(ID);
			setState(169);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==LBRACK) {
				{
				{
				setState(163);
				match(LBRACK);
				setState(164);
				expr(0);
				setState(165);
				match(RBRACK);
				}
				}
				setState(171);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class CallContext extends ParserRuleContext {
		public TerminalNode ID() { return getToken(FavaParser.ID, 0); }
		public TerminalNode LPAREN() { return getToken(FavaParser.LPAREN, 0); }
		public TerminalNode RPAREN() { return getToken(FavaParser.RPAREN, 0); }
		public ArgListContext argList() {
			return getRuleContext(ArgListContext.class,0);
		}
		public CallContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_call; }
		@Override
		public void enterRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).enterCall(this);
		}
		@Override
		public void exitRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).exitCall(this);
		}
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof FavaVisitor ) return ((FavaVisitor<? extends T>)visitor).visitCall(this);
			else return visitor.visitChildren(this);
		}
	}

	public final CallContext call() throws RecognitionException {
		CallContext _localctx = new CallContext(_ctx, getState());
		enterRule(_localctx, 20, RULE_call);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(172);
			match(ID);
			setState(173);
			match(LPAREN);
			setState(175);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if ((((_la) & ~0x3f) == 0 && ((1L << _la) & 72602253066752L) != 0)) {
				{
				setState(174);
				argList();
				}
			}

			setState(177);
			match(RPAREN);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class ArgListContext extends ParserRuleContext {
		public List<ExprContext> expr() {
			return getRuleContexts(ExprContext.class);
		}
		public ExprContext expr(int i) {
			return getRuleContext(ExprContext.class,i);
		}
		public ArgListContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_argList; }
		@Override
		public void enterRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).enterArgList(this);
		}
		@Override
		public void exitRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).exitArgList(this);
		}
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof FavaVisitor ) return ((FavaVisitor<? extends T>)visitor).visitArgList(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ArgListContext argList() throws RecognitionException {
		ArgListContext _localctx = new ArgListContext(_ctx, getState());
		enterRule(_localctx, 22, RULE_argList);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(179);
			expr(0);
			setState(184);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==T__0) {
				{
				{
				setState(180);
				match(T__0);
				setState(181);
				expr(0);
				}
				}
				setState(186);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class ExprContext extends ParserRuleContext {
		public TerminalNode LPAREN() { return getToken(FavaParser.LPAREN, 0); }
		public List<ExprContext> expr() {
			return getRuleContexts(ExprContext.class);
		}
		public ExprContext expr(int i) {
			return getRuleContext(ExprContext.class,i);
		}
		public TerminalNode RPAREN() { return getToken(FavaParser.RPAREN, 0); }
		public TerminalNode NOT() { return getToken(FavaParser.NOT, 0); }
		public TerminalNode MINUS() { return getToken(FavaParser.MINUS, 0); }
		public TerminalNode NEW() { return getToken(FavaParser.NEW, 0); }
		public BaseTypeContext baseType() {
			return getRuleContext(BaseTypeContext.class,0);
		}
		public TerminalNode LBRACK() { return getToken(FavaParser.LBRACK, 0); }
		public TerminalNode RBRACK() { return getToken(FavaParser.RBRACK, 0); }
		public List<ArraySuffixContext> arraySuffix() {
			return getRuleContexts(ArraySuffixContext.class);
		}
		public ArraySuffixContext arraySuffix(int i) {
			return getRuleContext(ArraySuffixContext.class,i);
		}
		public CallContext call() {
			return getRuleContext(CallContext.class,0);
		}
		public TerminalNode INT() { return getToken(FavaParser.INT, 0); }
		public TerminalNode REAL() { return getToken(FavaParser.REAL, 0); }
		public TerminalNode STRING() { return getToken(FavaParser.STRING, 0); }
		public TerminalNode BOOL() { return getToken(FavaParser.BOOL, 0); }
		public TerminalNode ID() { return getToken(FavaParser.ID, 0); }
		public TerminalNode TIMES() { return getToken(FavaParser.TIMES, 0); }
		public TerminalNode DIV() { return getToken(FavaParser.DIV, 0); }
		public TerminalNode MOD() { return getToken(FavaParser.MOD, 0); }
		public TerminalNode PLUS() { return getToken(FavaParser.PLUS, 0); }
		public TerminalNode CONCAT() { return getToken(FavaParser.CONCAT, 0); }
		public TerminalNode SMALLER() { return getToken(FavaParser.SMALLER, 0); }
		public TerminalNode LARGER() { return getToken(FavaParser.LARGER, 0); }
		public TerminalNode SMALLEROREQUAL() { return getToken(FavaParser.SMALLEROREQUAL, 0); }
		public TerminalNode LARGEROREQUAL() { return getToken(FavaParser.LARGEROREQUAL, 0); }
		public TerminalNode EQUAL() { return getToken(FavaParser.EQUAL, 0); }
		public TerminalNode NEQUAL() { return getToken(FavaParser.NEQUAL, 0); }
		public TerminalNode AND() { return getToken(FavaParser.AND, 0); }
		public TerminalNode OR() { return getToken(FavaParser.OR, 0); }
		public ExprContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_expr; }
		@Override
		public void enterRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).enterExpr(this);
		}
		@Override
		public void exitRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).exitExpr(this);
		}
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof FavaVisitor ) return ((FavaVisitor<? extends T>)visitor).visitExpr(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ExprContext expr() throws RecognitionException {
		return expr(0);
	}

	private ExprContext expr(int _p) throws RecognitionException {
		ParserRuleContext _parentctx = _ctx;
		int _parentState = getState();
		ExprContext _localctx = new ExprContext(_ctx, _parentState);
		ExprContext _prevctx = _localctx;
		int _startState = 24;
		enterRecursionRule(_localctx, 24, RULE_expr, _p);
		int _la;
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(212);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,16,_ctx) ) {
			case 1:
				{
				setState(188);
				match(LPAREN);
				setState(189);
				expr(0);
				setState(190);
				match(RPAREN);
				}
				break;
			case 2:
				{
				setState(192);
				_la = _input.LA(1);
				if ( !(_la==MINUS || _la==NOT) ) {
				_errHandler.recoverInline(this);
				}
				else {
					if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
					_errHandler.reportMatch(this);
					consume();
				}
				setState(193);
				expr(16);
				}
				break;
			case 3:
				{
				setState(194);
				match(NEW);
				setState(195);
				baseType();
				setState(199);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,15,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(196);
						arraySuffix();
						}
						} 
					}
					setState(201);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,15,_ctx);
				}
				setState(202);
				match(LBRACK);
				setState(203);
				expr(0);
				setState(204);
				match(RBRACK);
				}
				break;
			case 4:
				{
				setState(206);
				call();
				}
				break;
			case 5:
				{
				setState(207);
				match(INT);
				}
				break;
			case 6:
				{
				setState(208);
				match(REAL);
				}
				break;
			case 7:
				{
				setState(209);
				match(STRING);
				}
				break;
			case 8:
				{
				setState(210);
				match(BOOL);
				}
				break;
			case 9:
				{
				setState(211);
				match(ID);
				}
				break;
			}
			_ctx.stop = _input.LT(-1);
			setState(242);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,18,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					if ( _parseListeners!=null ) triggerExitRuleEvent();
					_prevctx = _localctx;
					{
					setState(240);
					_errHandler.sync(this);
					switch ( getInterpreter().adaptivePredict(_input,17,_ctx) ) {
					case 1:
						{
						_localctx = new ExprContext(_parentctx, _parentState);
						pushNewRecursionContext(_localctx, _startState, RULE_expr);
						setState(214);
						if (!(precpred(_ctx, 13))) throw new FailedPredicateException(this, "precpred(_ctx, 13)");
						setState(215);
						_la = _input.LA(1);
						if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & 1099512020992L) != 0)) ) {
						_errHandler.recoverInline(this);
						}
						else {
							if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
							_errHandler.reportMatch(this);
							consume();
						}
						setState(216);
						expr(14);
						}
						break;
					case 2:
						{
						_localctx = new ExprContext(_parentctx, _parentState);
						pushNewRecursionContext(_localctx, _startState, RULE_expr);
						setState(217);
						if (!(precpred(_ctx, 12))) throw new FailedPredicateException(this, "precpred(_ctx, 12)");
						setState(218);
						_la = _input.LA(1);
						if ( !(_la==PLUS || _la==MINUS) ) {
						_errHandler.recoverInline(this);
						}
						else {
							if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
							_errHandler.reportMatch(this);
							consume();
						}
						setState(219);
						expr(13);
						}
						break;
					case 3:
						{
						_localctx = new ExprContext(_parentctx, _parentState);
						pushNewRecursionContext(_localctx, _startState, RULE_expr);
						setState(220);
						if (!(precpred(_ctx, 11))) throw new FailedPredicateException(this, "precpred(_ctx, 11)");
						setState(221);
						match(CONCAT);
						setState(222);
						expr(12);
						}
						break;
					case 4:
						{
						_localctx = new ExprContext(_parentctx, _parentState);
						pushNewRecursionContext(_localctx, _startState, RULE_expr);
						setState(223);
						if (!(precpred(_ctx, 10))) throw new FailedPredicateException(this, "precpred(_ctx, 10)");
						setState(224);
						_la = _input.LA(1);
						if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & 7864320L) != 0)) ) {
						_errHandler.recoverInline(this);
						}
						else {
							if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
							_errHandler.reportMatch(this);
							consume();
						}
						setState(225);
						expr(11);
						}
						break;
					case 5:
						{
						_localctx = new ExprContext(_parentctx, _parentState);
						pushNewRecursionContext(_localctx, _startState, RULE_expr);
						setState(226);
						if (!(precpred(_ctx, 9))) throw new FailedPredicateException(this, "precpred(_ctx, 9)");
						setState(227);
						_la = _input.LA(1);
						if ( !(_la==EQUAL || _la==NEQUAL) ) {
						_errHandler.recoverInline(this);
						}
						else {
							if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
							_errHandler.reportMatch(this);
							consume();
						}
						setState(228);
						expr(10);
						}
						break;
					case 6:
						{
						_localctx = new ExprContext(_parentctx, _parentState);
						pushNewRecursionContext(_localctx, _startState, RULE_expr);
						setState(229);
						if (!(precpred(_ctx, 8))) throw new FailedPredicateException(this, "precpred(_ctx, 8)");
						setState(230);
						match(AND);
						setState(231);
						expr(9);
						}
						break;
					case 7:
						{
						_localctx = new ExprContext(_parentctx, _parentState);
						pushNewRecursionContext(_localctx, _startState, RULE_expr);
						setState(232);
						if (!(precpred(_ctx, 7))) throw new FailedPredicateException(this, "precpred(_ctx, 7)");
						setState(233);
						match(OR);
						setState(234);
						expr(8);
						}
						break;
					case 8:
						{
						_localctx = new ExprContext(_parentctx, _parentState);
						pushNewRecursionContext(_localctx, _startState, RULE_expr);
						setState(235);
						if (!(precpred(_ctx, 14))) throw new FailedPredicateException(this, "precpred(_ctx, 14)");
						setState(236);
						match(LBRACK);
						setState(237);
						expr(0);
						setState(238);
						match(RBRACK);
						}
						break;
					}
					} 
				}
				setState(244);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,18,_ctx);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			unrollRecursionContexts(_parentctx);
		}
		return _localctx;
	}

	public boolean sempred(RuleContext _localctx, int ruleIndex, int predIndex) {
		switch (ruleIndex) {
		case 12:
			return expr_sempred((ExprContext)_localctx, predIndex);
		}
		return true;
	}
	private boolean expr_sempred(ExprContext _localctx, int predIndex) {
		switch (predIndex) {
		case 0:
			return precpred(_ctx, 13);
		case 1:
			return precpred(_ctx, 12);
		case 2:
			return precpred(_ctx, 11);
		case 3:
			return precpred(_ctx, 10);
		case 4:
			return precpred(_ctx, 9);
		case 5:
			return precpred(_ctx, 8);
		case 6:
			return precpred(_ctx, 7);
		case 7:
			return precpred(_ctx, 14);
		}
		return true;
	}

	public static final String _serializedATN =
		"\u0004\u00011\u00f6\u0002\u0000\u0007\u0000\u0002\u0001\u0007\u0001\u0002"+
		"\u0002\u0007\u0002\u0002\u0003\u0007\u0003\u0002\u0004\u0007\u0004\u0002"+
		"\u0005\u0007\u0005\u0002\u0006\u0007\u0006\u0002\u0007\u0007\u0007\u0002"+
		"\b\u0007\b\u0002\t\u0007\t\u0002\n\u0007\n\u0002\u000b\u0007\u000b\u0002"+
		"\f\u0007\f\u0001\u0000\u0005\u0000\u001c\b\u0000\n\u0000\f\u0000\u001f"+
		"\t\u0000\u0001\u0000\u0004\u0000\"\b\u0000\u000b\u0000\f\u0000#\u0001"+
		"\u0000\u0001\u0000\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001"+
		"\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0005\u00011\b"+
		"\u0001\n\u0001\f\u00014\t\u0001\u0003\u00016\b\u0001\u0001\u0001\u0001"+
		"\u0001\u0001\u0001\u0003\u0001;\b\u0001\u0001\u0001\u0001\u0001\u0001"+
		"\u0002\u0001\u0002\u0001\u0002\u0001\u0002\u0005\u0002C\b\u0002\n\u0002"+
		"\f\u0002F\t\u0002\u0001\u0002\u0001\u0002\u0001\u0003\u0001\u0003\u0001"+
		"\u0003\u0001\u0003\u0003\u0003N\b\u0003\u0001\u0004\u0001\u0004\u0005"+
		"\u0004R\b\u0004\n\u0004\f\u0004U\t\u0004\u0001\u0005\u0001\u0005\u0001"+
		"\u0006\u0001\u0006\u0001\u0006\u0001\u0007\u0001\u0007\u0001\u0007\u0005"+
		"\u0007_\b\u0007\n\u0007\f\u0007b\t\u0007\u0001\u0007\u0001\u0007\u0001"+
		"\b\u0001\b\u0001\b\u0001\b\u0001\b\u0001\b\u0001\b\u0001\b\u0001\b\u0001"+
		"\b\u0001\b\u0001\b\u0001\b\u0001\b\u0003\bt\b\b\u0001\b\u0001\b\u0001"+
		"\b\u0001\b\u0001\b\u0001\b\u0001\b\u0001\b\u0001\b\u0001\b\u0001\b\u0001"+
		"\b\u0001\b\u0001\b\u0001\b\u0001\b\u0001\b\u0001\b\u0001\b\u0001\b\u0001"+
		"\b\u0001\b\u0001\b\u0001\b\u0001\b\u0001\b\u0001\b\u0001\b\u0001\b\u0001"+
		"\b\u0001\b\u0001\b\u0001\b\u0001\b\u0001\b\u0001\b\u0001\b\u0001\b\u0001"+
		"\b\u0001\b\u0001\b\u0001\b\u0001\b\u0003\b\u00a1\b\b\u0001\t\u0001\t\u0001"+
		"\t\u0001\t\u0001\t\u0005\t\u00a8\b\t\n\t\f\t\u00ab\t\t\u0001\n\u0001\n"+
		"\u0001\n\u0003\n\u00b0\b\n\u0001\n\u0001\n\u0001\u000b\u0001\u000b\u0001"+
		"\u000b\u0005\u000b\u00b7\b\u000b\n\u000b\f\u000b\u00ba\t\u000b\u0001\f"+
		"\u0001\f\u0001\f\u0001\f\u0001\f\u0001\f\u0001\f\u0001\f\u0001\f\u0001"+
		"\f\u0005\f\u00c6\b\f\n\f\f\f\u00c9\t\f\u0001\f\u0001\f\u0001\f\u0001\f"+
		"\u0001\f\u0001\f\u0001\f\u0001\f\u0001\f\u0001\f\u0003\f\u00d5\b\f\u0001"+
		"\f\u0001\f\u0001\f\u0001\f\u0001\f\u0001\f\u0001\f\u0001\f\u0001\f\u0001"+
		"\f\u0001\f\u0001\f\u0001\f\u0001\f\u0001\f\u0001\f\u0001\f\u0001\f\u0001"+
		"\f\u0001\f\u0001\f\u0001\f\u0001\f\u0001\f\u0001\f\u0001\f\u0005\f\u00f1"+
		"\b\f\n\f\f\f\u00f4\t\f\u0001\f\u0000\u0001\u0018\r\u0000\u0002\u0004\u0006"+
		"\b\n\f\u000e\u0010\u0012\u0014\u0016\u0018\u0000\u0006\u0001\u0000$\'"+
		"\u0002\u0000\u0010\u0010))\u0002\u0000\u0011\u0012((\u0001\u0000\u000f"+
		"\u0010\u0001\u0000\u0013\u0016\u0001\u0000\u0005\u0006\u0111\u0000\u001d"+
		"\u0001\u0000\u0000\u0000\u0002\'\u0001\u0000\u0000\u0000\u0004>\u0001"+
		"\u0000\u0000\u0000\u0006M\u0001\u0000\u0000\u0000\bO\u0001\u0000\u0000"+
		"\u0000\nV\u0001\u0000\u0000\u0000\fX\u0001\u0000\u0000\u0000\u000e[\u0001"+
		"\u0000\u0000\u0000\u0010\u00a0\u0001\u0000\u0000\u0000\u0012\u00a2\u0001"+
		"\u0000\u0000\u0000\u0014\u00ac\u0001\u0000\u0000\u0000\u0016\u00b3\u0001"+
		"\u0000\u0000\u0000\u0018\u00d4\u0001\u0000\u0000\u0000\u001a\u001c\u0003"+
		"\u0004\u0002\u0000\u001b\u001a\u0001\u0000\u0000\u0000\u001c\u001f\u0001"+
		"\u0000\u0000\u0000\u001d\u001b\u0001\u0000\u0000\u0000\u001d\u001e\u0001"+
		"\u0000\u0000\u0000\u001e!\u0001\u0000\u0000\u0000\u001f\u001d\u0001\u0000"+
		"\u0000\u0000 \"\u0003\u0002\u0001\u0000! \u0001\u0000\u0000\u0000\"#\u0001"+
		"\u0000\u0000\u0000#!\u0001\u0000\u0000\u0000#$\u0001\u0000\u0000\u0000"+
		"$%\u0001\u0000\u0000\u0000%&\u0005\u0000\u0000\u0001&\u0001\u0001\u0000"+
		"\u0000\u0000\'(\u0005\u001b\u0000\u0000()\u0005.\u0000\u0000)5\u0005\t"+
		"\u0000\u0000*+\u0003\b\u0004\u0000+2\u0005.\u0000\u0000,-\u0005\u0001"+
		"\u0000\u0000-.\u0003\b\u0004\u0000./\u0005.\u0000\u0000/1\u0001\u0000"+
		"\u0000\u00000,\u0001\u0000\u0000\u000014\u0001\u0000\u0000\u000020\u0001"+
		"\u0000\u0000\u000023\u0001\u0000\u0000\u000036\u0001\u0000\u0000\u0000"+
		"42\u0001\u0000\u0000\u00005*\u0001\u0000\u0000\u000056\u0001\u0000\u0000"+
		"\u000067\u0001\u0000\u0000\u00007:\u0005\n\u0000\u000089\u0005\u0004\u0000"+
		"\u00009;\u0003\b\u0004\u0000:8\u0001\u0000\u0000\u0000:;\u0001\u0000\u0000"+
		"\u0000;<\u0001\u0000\u0000\u0000<=\u0003\u000e\u0007\u0000=\u0003\u0001"+
		"\u0000\u0000\u0000>?\u0003\b\u0004\u0000?D\u0003\u0006\u0003\u0000@A\u0005"+
		"\u0001\u0000\u0000AC\u0003\u0006\u0003\u0000B@\u0001\u0000\u0000\u0000"+
		"CF\u0001\u0000\u0000\u0000DB\u0001\u0000\u0000\u0000DE\u0001\u0000\u0000"+
		"\u0000EG\u0001\u0000\u0000\u0000FD\u0001\u0000\u0000\u0000GH\u0005\u0002"+
		"\u0000\u0000H\u0005\u0001\u0000\u0000\u0000IN\u0005.\u0000\u0000JK\u0005"+
		".\u0000\u0000KL\u0005\u0003\u0000\u0000LN\u0003\u0018\f\u0000MI\u0001"+
		"\u0000\u0000\u0000MJ\u0001\u0000\u0000\u0000N\u0007\u0001\u0000\u0000"+
		"\u0000OS\u0003\n\u0005\u0000PR\u0003\f\u0006\u0000QP\u0001\u0000\u0000"+
		"\u0000RU\u0001\u0000\u0000\u0000SQ\u0001\u0000\u0000\u0000ST\u0001\u0000"+
		"\u0000\u0000T\t\u0001\u0000\u0000\u0000US\u0001\u0000\u0000\u0000VW\u0007"+
		"\u0000\u0000\u0000W\u000b\u0001\u0000\u0000\u0000XY\u0005\r\u0000\u0000"+
		"YZ\u0005\u000e\u0000\u0000Z\r\u0001\u0000\u0000\u0000[`\u0005\u000b\u0000"+
		"\u0000\\_\u0003\u0004\u0002\u0000]_\u0003\u0010\b\u0000^\\\u0001\u0000"+
		"\u0000\u0000^]\u0001\u0000\u0000\u0000_b\u0001\u0000\u0000\u0000`^\u0001"+
		"\u0000\u0000\u0000`a\u0001\u0000\u0000\u0000ac\u0001\u0000\u0000\u0000"+
		"b`\u0001\u0000\u0000\u0000cd\u0005\f\u0000\u0000d\u000f\u0001\u0000\u0000"+
		"\u0000ef\u0005\u001c\u0000\u0000fg\u0003\u0018\f\u0000gh\u0005\u0002\u0000"+
		"\u0000h\u00a1\u0001\u0000\u0000\u0000ij\u0003\u0012\t\u0000jk\u0005\u0003"+
		"\u0000\u0000kl\u0003\u0018\f\u0000lm\u0005\u0002\u0000\u0000m\u00a1\u0001"+
		"\u0000\u0000\u0000no\u0003\u0014\n\u0000op\u0005\u0002\u0000\u0000p\u00a1"+
		"\u0001\u0000\u0000\u0000qs\u0005\u001d\u0000\u0000rt\u0003\u0018\f\u0000"+
		"sr\u0001\u0000\u0000\u0000st\u0001\u0000\u0000\u0000tu\u0001\u0000\u0000"+
		"\u0000u\u00a1\u0005\u0002\u0000\u0000v\u00a1\u0003\u000e\u0007\u0000w"+
		"x\u0005\u001e\u0000\u0000xy\u0005\t\u0000\u0000yz\u0003\u0018\f\u0000"+
		"z{\u0005\n\u0000\u0000{|\u0003\u0010\b\u0000|\u00a1\u0001\u0000\u0000"+
		"\u0000}~\u0005\u001f\u0000\u0000~\u007f\u0005\t\u0000\u0000\u007f\u0080"+
		"\u0003\b\u0004\u0000\u0080\u0081\u0005.\u0000\u0000\u0081\u0082\u0005"+
		"\u0003\u0000\u0000\u0082\u0083\u0003\u0018\f\u0000\u0083\u0084\u0005\u0002"+
		"\u0000\u0000\u0084\u0085\u0003\u0018\f\u0000\u0085\u0086\u0005\u0002\u0000"+
		"\u0000\u0086\u0087\u0005.\u0000\u0000\u0087\u0088\u0005\b\u0000\u0000"+
		"\u0088\u0089\u0005\n\u0000\u0000\u0089\u008a\u0003\u0010\b\u0000\u008a"+
		"\u00a1\u0001\u0000\u0000\u0000\u008b\u008c\u0005\u001f\u0000\u0000\u008c"+
		"\u008d\u0005.\u0000\u0000\u008d\u008e\u0005 \u0000\u0000\u008e\u008f\u0003"+
		"\u0018\f\u0000\u008f\u0090\u0003\u0010\b\u0000\u0090\u00a1\u0001\u0000"+
		"\u0000\u0000\u0091\u0092\u0005!\u0000\u0000\u0092\u0093\u0005\t\u0000"+
		"\u0000\u0093\u0094\u0003\u0018\f\u0000\u0094\u0095\u0005\n\u0000\u0000"+
		"\u0095\u0096\u0003\u0010\b\u0000\u0096\u0097\u0005\"\u0000\u0000\u0097"+
		"\u0098\u0003\u0010\b\u0000\u0098\u00a1\u0001\u0000\u0000\u0000\u0099\u009a"+
		"\u0005!\u0000\u0000\u009a\u009b\u0005\t\u0000\u0000\u009b\u009c\u0003"+
		"\u0018\f\u0000\u009c\u009d\u0005\n\u0000\u0000\u009d\u009e\u0003\u0010"+
		"\b\u0000\u009e\u00a1\u0001\u0000\u0000\u0000\u009f\u00a1\u0005\u0002\u0000"+
		"\u0000\u00a0e\u0001\u0000\u0000\u0000\u00a0i\u0001\u0000\u0000\u0000\u00a0"+
		"n\u0001\u0000\u0000\u0000\u00a0q\u0001\u0000\u0000\u0000\u00a0v\u0001"+
		"\u0000\u0000\u0000\u00a0w\u0001\u0000\u0000\u0000\u00a0}\u0001\u0000\u0000"+
		"\u0000\u00a0\u008b\u0001\u0000\u0000\u0000\u00a0\u0091\u0001\u0000\u0000"+
		"\u0000\u00a0\u0099\u0001\u0000\u0000\u0000\u00a0\u009f\u0001\u0000\u0000"+
		"\u0000\u00a1\u0011\u0001\u0000\u0000\u0000\u00a2\u00a9\u0005.\u0000\u0000"+
		"\u00a3\u00a4\u0005\r\u0000\u0000\u00a4\u00a5\u0003\u0018\f\u0000\u00a5"+
		"\u00a6\u0005\u000e\u0000\u0000\u00a6\u00a8\u0001\u0000\u0000\u0000\u00a7"+
		"\u00a3\u0001\u0000\u0000\u0000\u00a8\u00ab\u0001\u0000\u0000\u0000\u00a9"+
		"\u00a7\u0001\u0000\u0000\u0000\u00a9\u00aa\u0001\u0000\u0000\u0000\u00aa"+
		"\u0013\u0001\u0000\u0000\u0000\u00ab\u00a9\u0001\u0000\u0000\u0000\u00ac"+
		"\u00ad\u0005.\u0000\u0000\u00ad\u00af\u0005\t\u0000\u0000\u00ae\u00b0"+
		"\u0003\u0016\u000b\u0000\u00af\u00ae\u0001\u0000\u0000\u0000\u00af\u00b0"+
		"\u0001\u0000\u0000\u0000\u00b0\u00b1\u0001\u0000\u0000\u0000\u00b1\u00b2"+
		"\u0005\n\u0000\u0000\u00b2\u0015\u0001\u0000\u0000\u0000\u00b3\u00b8\u0003"+
		"\u0018\f\u0000\u00b4\u00b5\u0005\u0001\u0000\u0000\u00b5\u00b7\u0003\u0018"+
		"\f\u0000\u00b6\u00b4\u0001\u0000\u0000\u0000\u00b7\u00ba\u0001\u0000\u0000"+
		"\u0000\u00b8\u00b6\u0001\u0000\u0000\u0000\u00b8\u00b9\u0001\u0000\u0000"+
		"\u0000\u00b9\u0017\u0001\u0000\u0000\u0000\u00ba\u00b8\u0001\u0000\u0000"+
		"\u0000\u00bb\u00bc\u0006\f\uffff\uffff\u0000\u00bc\u00bd\u0005\t\u0000"+
		"\u0000\u00bd\u00be\u0003\u0018\f\u0000\u00be\u00bf\u0005\n\u0000\u0000"+
		"\u00bf\u00d5\u0001\u0000\u0000\u0000\u00c0\u00c1\u0007\u0001\u0000\u0000"+
		"\u00c1\u00d5\u0003\u0018\f\u0010\u00c2\u00c3\u0005#\u0000\u0000\u00c3"+
		"\u00c7\u0003\n\u0005\u0000\u00c4\u00c6\u0003\f\u0006\u0000\u00c5\u00c4"+
		"\u0001\u0000\u0000\u0000\u00c6\u00c9\u0001\u0000\u0000\u0000\u00c7\u00c5"+
		"\u0001\u0000\u0000\u0000\u00c7\u00c8\u0001\u0000\u0000\u0000\u00c8\u00ca"+
		"\u0001\u0000\u0000\u0000\u00c9\u00c7\u0001\u0000\u0000\u0000\u00ca\u00cb"+
		"\u0005\r\u0000\u0000\u00cb\u00cc\u0003\u0018\f\u0000\u00cc\u00cd\u0005"+
		"\u000e\u0000\u0000\u00cd\u00d5\u0001\u0000\u0000\u0000\u00ce\u00d5\u0003"+
		"\u0014\n\u0000\u00cf\u00d5\u0005\u0017\u0000\u0000\u00d0\u00d5\u0005\u0018"+
		"\u0000\u0000\u00d1\u00d5\u0005\u001a\u0000\u0000\u00d2\u00d5\u0005\u0019"+
		"\u0000\u0000\u00d3\u00d5\u0005.\u0000\u0000\u00d4\u00bb\u0001\u0000\u0000"+
		"\u0000\u00d4\u00c0\u0001\u0000\u0000\u0000\u00d4\u00c2\u0001\u0000\u0000"+
		"\u0000\u00d4\u00ce\u0001\u0000\u0000\u0000\u00d4\u00cf\u0001\u0000\u0000"+
		"\u0000\u00d4\u00d0\u0001\u0000\u0000\u0000\u00d4\u00d1\u0001\u0000\u0000"+
		"\u0000\u00d4\u00d2\u0001\u0000\u0000\u0000\u00d4\u00d3\u0001\u0000\u0000"+
		"\u0000\u00d5\u00f2\u0001\u0000\u0000\u0000\u00d6\u00d7\n\r\u0000\u0000"+
		"\u00d7\u00d8\u0007\u0002\u0000\u0000\u00d8\u00f1\u0003\u0018\f\u000e\u00d9"+
		"\u00da\n\f\u0000\u0000\u00da\u00db\u0007\u0003\u0000\u0000\u00db\u00f1"+
		"\u0003\u0018\f\r\u00dc\u00dd\n\u000b\u0000\u0000\u00dd\u00de\u0005\u0007"+
		"\u0000\u0000\u00de\u00f1\u0003\u0018\f\f\u00df\u00e0\n\n\u0000\u0000\u00e0"+
		"\u00e1\u0007\u0004\u0000\u0000\u00e1\u00f1\u0003\u0018\f\u000b\u00e2\u00e3"+
		"\n\t\u0000\u0000\u00e3\u00e4\u0007\u0005\u0000\u0000\u00e4\u00f1\u0003"+
		"\u0018\f\n\u00e5\u00e6\n\b\u0000\u0000\u00e6\u00e7\u0005*\u0000\u0000"+
		"\u00e7\u00f1\u0003\u0018\f\t\u00e8\u00e9\n\u0007\u0000\u0000\u00e9\u00ea"+
		"\u0005+\u0000\u0000\u00ea\u00f1\u0003\u0018\f\b\u00eb\u00ec\n\u000e\u0000"+
		"\u0000\u00ec\u00ed\u0005\r\u0000\u0000\u00ed\u00ee\u0003\u0018\f\u0000"+
		"\u00ee\u00ef\u0005\u000e\u0000\u0000\u00ef\u00f1\u0001\u0000\u0000\u0000"+
		"\u00f0\u00d6\u0001\u0000\u0000\u0000\u00f0\u00d9\u0001\u0000\u0000\u0000"+
		"\u00f0\u00dc\u0001\u0000\u0000\u0000\u00f0\u00df\u0001\u0000\u0000\u0000"+
		"\u00f0\u00e2\u0001\u0000\u0000\u0000\u00f0\u00e5\u0001\u0000\u0000\u0000"+
		"\u00f0\u00e8\u0001\u0000\u0000\u0000\u00f0\u00eb\u0001\u0000\u0000\u0000"+
		"\u00f1\u00f4\u0001\u0000\u0000\u0000\u00f2\u00f0\u0001\u0000\u0000\u0000"+
		"\u00f2\u00f3\u0001\u0000\u0000\u0000\u00f3\u0019\u0001\u0000\u0000\u0000"+
		"\u00f4\u00f2\u0001\u0000\u0000\u0000\u0013\u001d#25:DMS^`s\u00a0\u00a9"+
		"\u00af\u00b8\u00c7\u00d4\u00f0\u00f2";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}