// Generated from C:/Users/User/IdeaProjects/Compiladores/FavaCompiler/src/Fava.g4 by ANTLR 4.13.2
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
		T__0=1, T__1=2, ASSIGN=3, ARROW=4, EQUAL=5, NEQUAL=6, CONCAT=7, LPAREN=8, 
		RPAREN=9, LBRACE=10, RBRACE=11, LBRACK=12, RBRACK=13, PLUS=14, MINUS=15, 
		TIMES=16, DIV=17, SMALLER=18, LARGER=19, SMALLEROREQUAL=20, LARGEROREQUAL=21, 
		INT=22, REAL=23, BOOL=24, STRING=25, FUNCTION=26, PRINT=27, RETURN=28, 
		WHILE=29, IF=30, ELSE=31, NEW=32, LENGTH=33, TYPEBOOL=34, TYPEINTEGER=35, 
		TYPEREAL=36, TYPESTRING=37, MOD=38, NOT=39, AND=40, OR=41, TRUE=42, FALSE=43, 
		ID=44, WS=45, SL_COMMENT=46, ML_COMMENT=47;
	public static final int
		RULE_prog = 0, RULE_funcDecl = 1, RULE_decl = 2, RULE_varDecl = 3, RULE_type = 4, 
		RULE_block = 5, RULE_stmt = 6, RULE_lvalue = 7, RULE_call = 8, RULE_argList = 9, 
		RULE_expr = 10;
	private static String[] makeRuleNames() {
		return new String[] {
			"prog", "funcDecl", "decl", "varDecl", "type", "block", "stmt", "lvalue", 
			"call", "argList", "expr"
		};
	}
	public static final String[] ruleNames = makeRuleNames();

	private static String[] makeLiteralNames() {
		return new String[] {
			null, "','", "';'", "':='", "'->'", "'='", "'<>'", "'||'", "'('", "')'", 
			"'{'", "'}'", "'['", "']'", "'+'", "'-'", "'*'", "'/'", "'<'", "'>'", 
			"'<='", "'>='"
		};
	}
	private static final String[] _LITERAL_NAMES = makeLiteralNames();
	private static String[] makeSymbolicNames() {
		return new String[] {
			null, null, null, "ASSIGN", "ARROW", "EQUAL", "NEQUAL", "CONCAT", "LPAREN", 
			"RPAREN", "LBRACE", "RBRACE", "LBRACK", "RBRACK", "PLUS", "MINUS", "TIMES", 
			"DIV", "SMALLER", "LARGER", "SMALLEROREQUAL", "LARGEROREQUAL", "INT", 
			"REAL", "BOOL", "STRING", "FUNCTION", "PRINT", "RETURN", "WHILE", "IF", 
			"ELSE", "NEW", "LENGTH", "TYPEBOOL", "TYPEINTEGER", "TYPEREAL", "TYPESTRING", 
			"MOD", "NOT", "AND", "OR", "TRUE", "FALSE", "ID", "WS", "SL_COMMENT", 
			"ML_COMMENT"
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
			setState(25);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 257698037760L) != 0)) {
				{
				{
				setState(22);
				decl();
				}
				}
				setState(27);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(29); 
			_errHandler.sync(this);
			_la = _input.LA(1);
			do {
				{
				{
				setState(28);
				funcDecl();
				}
				}
				setState(31); 
				_errHandler.sync(this);
				_la = _input.LA(1);
			} while ( _la==FUNCTION );
			setState(33);
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
			setState(35);
			match(FUNCTION);
			setState(36);
			match(ID);
			setState(37);
			match(LPAREN);
			setState(49);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if ((((_la) & ~0x3f) == 0 && ((1L << _la) & 257698037760L) != 0)) {
				{
				setState(38);
				type();
				setState(39);
				match(ID);
				setState(46);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while (_la==T__0) {
					{
					{
					setState(40);
					match(T__0);
					setState(41);
					type();
					setState(42);
					match(ID);
					}
					}
					setState(48);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				}
			}

			setState(51);
			match(RPAREN);
			setState(54);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==ARROW) {
				{
				setState(52);
				match(ARROW);
				setState(53);
				type();
				}
			}

			setState(56);
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
			setState(58);
			type();
			setState(59);
			varDecl();
			setState(64);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==T__0) {
				{
				{
				setState(60);
				match(T__0);
				setState(61);
				varDecl();
				}
				}
				setState(66);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(67);
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
			setState(73);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,6,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(69);
				match(ID);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(70);
				match(ID);
				setState(71);
				match(ASSIGN);
				setState(72);
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
		public TerminalNode TYPEBOOL() { return getToken(FavaParser.TYPEBOOL, 0); }
		public TerminalNode LBRACK() { return getToken(FavaParser.LBRACK, 0); }
		public TerminalNode RBRACK() { return getToken(FavaParser.RBRACK, 0); }
		public TerminalNode TYPEINTEGER() { return getToken(FavaParser.TYPEINTEGER, 0); }
		public TerminalNode TYPEREAL() { return getToken(FavaParser.TYPEREAL, 0); }
		public TerminalNode TYPESTRING() { return getToken(FavaParser.TYPESTRING, 0); }
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
			setState(95);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case TYPEBOOL:
				enterOuterAlt(_localctx, 1);
				{
				setState(75);
				match(TYPEBOOL);
				setState(78);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==LBRACK) {
					{
					setState(76);
					match(LBRACK);
					setState(77);
					match(RBRACK);
					}
				}

				}
				break;
			case TYPEINTEGER:
				enterOuterAlt(_localctx, 2);
				{
				setState(80);
				match(TYPEINTEGER);
				setState(83);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==LBRACK) {
					{
					setState(81);
					match(LBRACK);
					setState(82);
					match(RBRACK);
					}
				}

				}
				break;
			case TYPEREAL:
				enterOuterAlt(_localctx, 3);
				{
				setState(85);
				match(TYPEREAL);
				setState(88);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==LBRACK) {
					{
					setState(86);
					match(LBRACK);
					setState(87);
					match(RBRACK);
					}
				}

				}
				break;
			case TYPESTRING:
				enterOuterAlt(_localctx, 4);
				{
				setState(90);
				match(TYPESTRING);
				setState(93);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==LBRACK) {
					{
					setState(91);
					match(LBRACK);
					setState(92);
					match(RBRACK);
					}
				}

				}
				break;
			default:
				throw new NoViableAltException(this);
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
		enterRule(_localctx, 10, RULE_block);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(97);
			match(LBRACE);
			setState(101);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 257698037760L) != 0)) {
				{
				{
				setState(98);
				decl();
				}
				}
				setState(103);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(107);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 17594199311364L) != 0)) {
				{
				{
				setState(104);
				stmt();
				}
				}
				setState(109);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(110);
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

	public final StmtContext stmt() throws RecognitionException {
		StmtContext _localctx = new StmtContext(_ctx, getState());
		enterRule(_localctx, 12, RULE_stmt);
		int _la;
		try {
			setState(151);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,15,_ctx) ) {
			case 1:
				_localctx = new PrintStmtContext(_localctx);
				enterOuterAlt(_localctx, 1);
				{
				setState(112);
				match(PRINT);
				setState(113);
				expr(0);
				setState(114);
				match(T__1);
				}
				break;
			case 2:
				_localctx = new AssignStmtContext(_localctx);
				enterOuterAlt(_localctx, 2);
				{
				setState(116);
				lvalue();
				setState(117);
				match(ASSIGN);
				setState(118);
				expr(0);
				setState(119);
				match(T__1);
				}
				break;
			case 3:
				_localctx = new CallStmtContext(_localctx);
				enterOuterAlt(_localctx, 3);
				{
				setState(121);
				call();
				setState(122);
				match(T__1);
				}
				break;
			case 4:
				_localctx = new ReturnStmtContext(_localctx);
				enterOuterAlt(_localctx, 4);
				{
				setState(124);
				match(RETURN);
				setState(126);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if ((((_la) & ~0x3f) == 0 && ((1L << _la) & 18154889707776L) != 0)) {
					{
					setState(125);
					expr(0);
					}
				}

				setState(128);
				match(T__1);
				}
				break;
			case 5:
				_localctx = new BlockStmtContext(_localctx);
				enterOuterAlt(_localctx, 5);
				{
				setState(129);
				block();
				}
				break;
			case 6:
				_localctx = new WhileStmtContext(_localctx);
				enterOuterAlt(_localctx, 6);
				{
				setState(130);
				match(WHILE);
				setState(131);
				match(LPAREN);
				setState(132);
				expr(0);
				setState(133);
				match(RPAREN);
				setState(134);
				stmt();
				}
				break;
			case 7:
				_localctx = new IfElseStmtContext(_localctx);
				enterOuterAlt(_localctx, 7);
				{
				setState(136);
				match(IF);
				setState(137);
				match(LPAREN);
				setState(138);
				expr(0);
				setState(139);
				match(RPAREN);
				setState(140);
				stmt();
				setState(141);
				match(ELSE);
				setState(142);
				stmt();
				}
				break;
			case 8:
				_localctx = new IfStmtContext(_localctx);
				enterOuterAlt(_localctx, 8);
				{
				setState(144);
				match(IF);
				setState(145);
				match(LPAREN);
				setState(146);
				expr(0);
				setState(147);
				match(RPAREN);
				setState(148);
				stmt();
				}
				break;
			case 9:
				_localctx = new EmptyStmtContext(_localctx);
				enterOuterAlt(_localctx, 9);
				{
				setState(150);
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
		public TerminalNode LBRACK() { return getToken(FavaParser.LBRACK, 0); }
		public ExprContext expr() {
			return getRuleContext(ExprContext.class,0);
		}
		public TerminalNode RBRACK() { return getToken(FavaParser.RBRACK, 0); }
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
		enterRule(_localctx, 14, RULE_lvalue);
		try {
			setState(159);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,16,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(153);
				match(ID);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(154);
				match(ID);
				setState(155);
				match(LBRACK);
				setState(156);
				expr(0);
				setState(157);
				match(RBRACK);
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
		enterRule(_localctx, 16, RULE_call);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(161);
			match(ID);
			setState(162);
			match(LPAREN);
			setState(164);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if ((((_la) & ~0x3f) == 0 && ((1L << _la) & 18154889707776L) != 0)) {
				{
				setState(163);
				argList();
				}
			}

			setState(166);
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
		enterRule(_localctx, 18, RULE_argList);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(168);
			expr(0);
			setState(173);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==T__0) {
				{
				{
				setState(169);
				match(T__0);
				setState(170);
				expr(0);
				}
				}
				setState(175);
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
		public TerminalNode LBRACK() { return getToken(FavaParser.LBRACK, 0); }
		public TerminalNode RBRACK() { return getToken(FavaParser.RBRACK, 0); }
		public TerminalNode TYPEBOOL() { return getToken(FavaParser.TYPEBOOL, 0); }
		public TerminalNode TYPEINTEGER() { return getToken(FavaParser.TYPEINTEGER, 0); }
		public TerminalNode TYPEREAL() { return getToken(FavaParser.TYPEREAL, 0); }
		public TerminalNode TYPESTRING() { return getToken(FavaParser.TYPESTRING, 0); }
		public TerminalNode LENGTH() { return getToken(FavaParser.LENGTH, 0); }
		public TerminalNode ID() { return getToken(FavaParser.ID, 0); }
		public CallContext call() {
			return getRuleContext(CallContext.class,0);
		}
		public TerminalNode INT() { return getToken(FavaParser.INT, 0); }
		public TerminalNode REAL() { return getToken(FavaParser.REAL, 0); }
		public TerminalNode STRING() { return getToken(FavaParser.STRING, 0); }
		public TerminalNode BOOL() { return getToken(FavaParser.BOOL, 0); }
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
		int _startState = 20;
		enterRecursionRule(_localctx, 20, RULE_expr, _p);
		int _la;
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(205);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,19,_ctx) ) {
			case 1:
				{
				setState(177);
				match(LPAREN);
				setState(178);
				expr(0);
				setState(179);
				match(RPAREN);
				}
				break;
			case 2:
				{
				setState(181);
				_la = _input.LA(1);
				if ( !(_la==MINUS || _la==NOT) ) {
				_errHandler.recoverInline(this);
				}
				else {
					if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
					_errHandler.reportMatch(this);
					consume();
				}
				setState(182);
				expr(17);
				}
				break;
			case 3:
				{
				setState(183);
				match(NEW);
				setState(184);
				_la = _input.LA(1);
				if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & 257698037760L) != 0)) ) {
				_errHandler.recoverInline(this);
				}
				else {
					if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
					_errHandler.reportMatch(this);
					consume();
				}
				setState(185);
				match(LBRACK);
				setState(186);
				expr(0);
				setState(187);
				match(RBRACK);
				}
				break;
			case 4:
				{
				setState(189);
				match(LENGTH);
				setState(190);
				match(LPAREN);
				setState(191);
				expr(0);
				setState(192);
				match(RPAREN);
				}
				break;
			case 5:
				{
				setState(194);
				match(ID);
				setState(195);
				match(LBRACK);
				setState(196);
				expr(0);
				setState(197);
				match(RBRACK);
				}
				break;
			case 6:
				{
				setState(199);
				call();
				}
				break;
			case 7:
				{
				setState(200);
				match(INT);
				}
				break;
			case 8:
				{
				setState(201);
				match(REAL);
				}
				break;
			case 9:
				{
				setState(202);
				match(STRING);
				}
				break;
			case 10:
				{
				setState(203);
				match(BOOL);
				}
				break;
			case 11:
				{
				setState(204);
				match(ID);
				}
				break;
			}
			_ctx.stop = _input.LT(-1);
			setState(230);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,21,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					if ( _parseListeners!=null ) triggerExitRuleEvent();
					_prevctx = _localctx;
					{
					setState(228);
					_errHandler.sync(this);
					switch ( getInterpreter().adaptivePredict(_input,20,_ctx) ) {
					case 1:
						{
						_localctx = new ExprContext(_parentctx, _parentState);
						pushNewRecursionContext(_localctx, _startState, RULE_expr);
						setState(207);
						if (!(precpred(_ctx, 13))) throw new FailedPredicateException(this, "precpred(_ctx, 13)");
						setState(208);
						_la = _input.LA(1);
						if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & 274878103552L) != 0)) ) {
						_errHandler.recoverInline(this);
						}
						else {
							if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
							_errHandler.reportMatch(this);
							consume();
						}
						setState(209);
						expr(14);
						}
						break;
					case 2:
						{
						_localctx = new ExprContext(_parentctx, _parentState);
						pushNewRecursionContext(_localctx, _startState, RULE_expr);
						setState(210);
						if (!(precpred(_ctx, 12))) throw new FailedPredicateException(this, "precpred(_ctx, 12)");
						setState(211);
						_la = _input.LA(1);
						if ( !(_la==PLUS || _la==MINUS) ) {
						_errHandler.recoverInline(this);
						}
						else {
							if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
							_errHandler.reportMatch(this);
							consume();
						}
						setState(212);
						expr(13);
						}
						break;
					case 3:
						{
						_localctx = new ExprContext(_parentctx, _parentState);
						pushNewRecursionContext(_localctx, _startState, RULE_expr);
						setState(213);
						if (!(precpred(_ctx, 11))) throw new FailedPredicateException(this, "precpred(_ctx, 11)");
						setState(214);
						match(CONCAT);
						setState(215);
						expr(12);
						}
						break;
					case 4:
						{
						_localctx = new ExprContext(_parentctx, _parentState);
						pushNewRecursionContext(_localctx, _startState, RULE_expr);
						setState(216);
						if (!(precpred(_ctx, 10))) throw new FailedPredicateException(this, "precpred(_ctx, 10)");
						setState(217);
						_la = _input.LA(1);
						if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & 3932160L) != 0)) ) {
						_errHandler.recoverInline(this);
						}
						else {
							if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
							_errHandler.reportMatch(this);
							consume();
						}
						setState(218);
						expr(11);
						}
						break;
					case 5:
						{
						_localctx = new ExprContext(_parentctx, _parentState);
						pushNewRecursionContext(_localctx, _startState, RULE_expr);
						setState(219);
						if (!(precpred(_ctx, 9))) throw new FailedPredicateException(this, "precpred(_ctx, 9)");
						setState(220);
						_la = _input.LA(1);
						if ( !(_la==EQUAL || _la==NEQUAL) ) {
						_errHandler.recoverInline(this);
						}
						else {
							if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
							_errHandler.reportMatch(this);
							consume();
						}
						setState(221);
						expr(10);
						}
						break;
					case 6:
						{
						_localctx = new ExprContext(_parentctx, _parentState);
						pushNewRecursionContext(_localctx, _startState, RULE_expr);
						setState(222);
						if (!(precpred(_ctx, 8))) throw new FailedPredicateException(this, "precpred(_ctx, 8)");
						setState(223);
						match(AND);
						setState(224);
						expr(9);
						}
						break;
					case 7:
						{
						_localctx = new ExprContext(_parentctx, _parentState);
						pushNewRecursionContext(_localctx, _startState, RULE_expr);
						setState(225);
						if (!(precpred(_ctx, 7))) throw new FailedPredicateException(this, "precpred(_ctx, 7)");
						setState(226);
						match(OR);
						setState(227);
						expr(8);
						}
						break;
					}
					} 
				}
				setState(232);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,21,_ctx);
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
		case 10:
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
		}
		return true;
	}

	public static final String _serializedATN =
		"\u0004\u0001/\u00ea\u0002\u0000\u0007\u0000\u0002\u0001\u0007\u0001\u0002"+
		"\u0002\u0007\u0002\u0002\u0003\u0007\u0003\u0002\u0004\u0007\u0004\u0002"+
		"\u0005\u0007\u0005\u0002\u0006\u0007\u0006\u0002\u0007\u0007\u0007\u0002"+
		"\b\u0007\b\u0002\t\u0007\t\u0002\n\u0007\n\u0001\u0000\u0005\u0000\u0018"+
		"\b\u0000\n\u0000\f\u0000\u001b\t\u0000\u0001\u0000\u0004\u0000\u001e\b"+
		"\u0000\u000b\u0000\f\u0000\u001f\u0001\u0000\u0001\u0000\u0001\u0001\u0001"+
		"\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001"+
		"\u0001\u0001\u0001\u0005\u0001-\b\u0001\n\u0001\f\u00010\t\u0001\u0003"+
		"\u00012\b\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0003\u00017\b\u0001"+
		"\u0001\u0001\u0001\u0001\u0001\u0002\u0001\u0002\u0001\u0002\u0001\u0002"+
		"\u0005\u0002?\b\u0002\n\u0002\f\u0002B\t\u0002\u0001\u0002\u0001\u0002"+
		"\u0001\u0003\u0001\u0003\u0001\u0003\u0001\u0003\u0003\u0003J\b\u0003"+
		"\u0001\u0004\u0001\u0004\u0001\u0004\u0003\u0004O\b\u0004\u0001\u0004"+
		"\u0001\u0004\u0001\u0004\u0003\u0004T\b\u0004\u0001\u0004\u0001\u0004"+
		"\u0001\u0004\u0003\u0004Y\b\u0004\u0001\u0004\u0001\u0004\u0001\u0004"+
		"\u0003\u0004^\b\u0004\u0003\u0004`\b\u0004\u0001\u0005\u0001\u0005\u0005"+
		"\u0005d\b\u0005\n\u0005\f\u0005g\t\u0005\u0001\u0005\u0005\u0005j\b\u0005"+
		"\n\u0005\f\u0005m\t\u0005\u0001\u0005\u0001\u0005\u0001\u0006\u0001\u0006"+
		"\u0001\u0006\u0001\u0006\u0001\u0006\u0001\u0006\u0001\u0006\u0001\u0006"+
		"\u0001\u0006\u0001\u0006\u0001\u0006\u0001\u0006\u0001\u0006\u0001\u0006"+
		"\u0003\u0006\u007f\b\u0006\u0001\u0006\u0001\u0006\u0001\u0006\u0001\u0006"+
		"\u0001\u0006\u0001\u0006\u0001\u0006\u0001\u0006\u0001\u0006\u0001\u0006"+
		"\u0001\u0006\u0001\u0006\u0001\u0006\u0001\u0006\u0001\u0006\u0001\u0006"+
		"\u0001\u0006\u0001\u0006\u0001\u0006\u0001\u0006\u0001\u0006\u0001\u0006"+
		"\u0001\u0006\u0003\u0006\u0098\b\u0006\u0001\u0007\u0001\u0007\u0001\u0007"+
		"\u0001\u0007\u0001\u0007\u0001\u0007\u0003\u0007\u00a0\b\u0007\u0001\b"+
		"\u0001\b\u0001\b\u0003\b\u00a5\b\b\u0001\b\u0001\b\u0001\t\u0001\t\u0001"+
		"\t\u0005\t\u00ac\b\t\n\t\f\t\u00af\t\t\u0001\n\u0001\n\u0001\n\u0001\n"+
		"\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001"+
		"\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001"+
		"\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0003\n\u00ce"+
		"\b\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001"+
		"\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001"+
		"\n\u0001\n\u0001\n\u0001\n\u0005\n\u00e5\b\n\n\n\f\n\u00e8\t\n\u0001\n"+
		"\u0000\u0001\u0014\u000b\u0000\u0002\u0004\u0006\b\n\f\u000e\u0010\u0012"+
		"\u0014\u0000\u0006\u0002\u0000\u000f\u000f\'\'\u0001\u0000\"%\u0002\u0000"+
		"\u0010\u0011&&\u0001\u0000\u000e\u000f\u0001\u0000\u0012\u0015\u0001\u0000"+
		"\u0005\u0006\u010b\u0000\u0019\u0001\u0000\u0000\u0000\u0002#\u0001\u0000"+
		"\u0000\u0000\u0004:\u0001\u0000\u0000\u0000\u0006I\u0001\u0000\u0000\u0000"+
		"\b_\u0001\u0000\u0000\u0000\na\u0001\u0000\u0000\u0000\f\u0097\u0001\u0000"+
		"\u0000\u0000\u000e\u009f\u0001\u0000\u0000\u0000\u0010\u00a1\u0001\u0000"+
		"\u0000\u0000\u0012\u00a8\u0001\u0000\u0000\u0000\u0014\u00cd\u0001\u0000"+
		"\u0000\u0000\u0016\u0018\u0003\u0004\u0002\u0000\u0017\u0016\u0001\u0000"+
		"\u0000\u0000\u0018\u001b\u0001\u0000\u0000\u0000\u0019\u0017\u0001\u0000"+
		"\u0000\u0000\u0019\u001a\u0001\u0000\u0000\u0000\u001a\u001d\u0001\u0000"+
		"\u0000\u0000\u001b\u0019\u0001\u0000\u0000\u0000\u001c\u001e\u0003\u0002"+
		"\u0001\u0000\u001d\u001c\u0001\u0000\u0000\u0000\u001e\u001f\u0001\u0000"+
		"\u0000\u0000\u001f\u001d\u0001\u0000\u0000\u0000\u001f \u0001\u0000\u0000"+
		"\u0000 !\u0001\u0000\u0000\u0000!\"\u0005\u0000\u0000\u0001\"\u0001\u0001"+
		"\u0000\u0000\u0000#$\u0005\u001a\u0000\u0000$%\u0005,\u0000\u0000%1\u0005"+
		"\b\u0000\u0000&\'\u0003\b\u0004\u0000\'.\u0005,\u0000\u0000()\u0005\u0001"+
		"\u0000\u0000)*\u0003\b\u0004\u0000*+\u0005,\u0000\u0000+-\u0001\u0000"+
		"\u0000\u0000,(\u0001\u0000\u0000\u0000-0\u0001\u0000\u0000\u0000.,\u0001"+
		"\u0000\u0000\u0000./\u0001\u0000\u0000\u0000/2\u0001\u0000\u0000\u0000"+
		"0.\u0001\u0000\u0000\u00001&\u0001\u0000\u0000\u000012\u0001\u0000\u0000"+
		"\u000023\u0001\u0000\u0000\u000036\u0005\t\u0000\u000045\u0005\u0004\u0000"+
		"\u000057\u0003\b\u0004\u000064\u0001\u0000\u0000\u000067\u0001\u0000\u0000"+
		"\u000078\u0001\u0000\u0000\u000089\u0003\n\u0005\u00009\u0003\u0001\u0000"+
		"\u0000\u0000:;\u0003\b\u0004\u0000;@\u0003\u0006\u0003\u0000<=\u0005\u0001"+
		"\u0000\u0000=?\u0003\u0006\u0003\u0000><\u0001\u0000\u0000\u0000?B\u0001"+
		"\u0000\u0000\u0000@>\u0001\u0000\u0000\u0000@A\u0001\u0000\u0000\u0000"+
		"AC\u0001\u0000\u0000\u0000B@\u0001\u0000\u0000\u0000CD\u0005\u0002\u0000"+
		"\u0000D\u0005\u0001\u0000\u0000\u0000EJ\u0005,\u0000\u0000FG\u0005,\u0000"+
		"\u0000GH\u0005\u0003\u0000\u0000HJ\u0003\u0014\n\u0000IE\u0001\u0000\u0000"+
		"\u0000IF\u0001\u0000\u0000\u0000J\u0007\u0001\u0000\u0000\u0000KN\u0005"+
		"\"\u0000\u0000LM\u0005\f\u0000\u0000MO\u0005\r\u0000\u0000NL\u0001\u0000"+
		"\u0000\u0000NO\u0001\u0000\u0000\u0000O`\u0001\u0000\u0000\u0000PS\u0005"+
		"#\u0000\u0000QR\u0005\f\u0000\u0000RT\u0005\r\u0000\u0000SQ\u0001\u0000"+
		"\u0000\u0000ST\u0001\u0000\u0000\u0000T`\u0001\u0000\u0000\u0000UX\u0005"+
		"$\u0000\u0000VW\u0005\f\u0000\u0000WY\u0005\r\u0000\u0000XV\u0001\u0000"+
		"\u0000\u0000XY\u0001\u0000\u0000\u0000Y`\u0001\u0000\u0000\u0000Z]\u0005"+
		"%\u0000\u0000[\\\u0005\f\u0000\u0000\\^\u0005\r\u0000\u0000][\u0001\u0000"+
		"\u0000\u0000]^\u0001\u0000\u0000\u0000^`\u0001\u0000\u0000\u0000_K\u0001"+
		"\u0000\u0000\u0000_P\u0001\u0000\u0000\u0000_U\u0001\u0000\u0000\u0000"+
		"_Z\u0001\u0000\u0000\u0000`\t\u0001\u0000\u0000\u0000ae\u0005\n\u0000"+
		"\u0000bd\u0003\u0004\u0002\u0000cb\u0001\u0000\u0000\u0000dg\u0001\u0000"+
		"\u0000\u0000ec\u0001\u0000\u0000\u0000ef\u0001\u0000\u0000\u0000fk\u0001"+
		"\u0000\u0000\u0000ge\u0001\u0000\u0000\u0000hj\u0003\f\u0006\u0000ih\u0001"+
		"\u0000\u0000\u0000jm\u0001\u0000\u0000\u0000ki\u0001\u0000\u0000\u0000"+
		"kl\u0001\u0000\u0000\u0000ln\u0001\u0000\u0000\u0000mk\u0001\u0000\u0000"+
		"\u0000no\u0005\u000b\u0000\u0000o\u000b\u0001\u0000\u0000\u0000pq\u0005"+
		"\u001b\u0000\u0000qr\u0003\u0014\n\u0000rs\u0005\u0002\u0000\u0000s\u0098"+
		"\u0001\u0000\u0000\u0000tu\u0003\u000e\u0007\u0000uv\u0005\u0003\u0000"+
		"\u0000vw\u0003\u0014\n\u0000wx\u0005\u0002\u0000\u0000x\u0098\u0001\u0000"+
		"\u0000\u0000yz\u0003\u0010\b\u0000z{\u0005\u0002\u0000\u0000{\u0098\u0001"+
		"\u0000\u0000\u0000|~\u0005\u001c\u0000\u0000}\u007f\u0003\u0014\n\u0000"+
		"~}\u0001\u0000\u0000\u0000~\u007f\u0001\u0000\u0000\u0000\u007f\u0080"+
		"\u0001\u0000\u0000\u0000\u0080\u0098\u0005\u0002\u0000\u0000\u0081\u0098"+
		"\u0003\n\u0005\u0000\u0082\u0083\u0005\u001d\u0000\u0000\u0083\u0084\u0005"+
		"\b\u0000\u0000\u0084\u0085\u0003\u0014\n\u0000\u0085\u0086\u0005\t\u0000"+
		"\u0000\u0086\u0087\u0003\f\u0006\u0000\u0087\u0098\u0001\u0000\u0000\u0000"+
		"\u0088\u0089\u0005\u001e\u0000\u0000\u0089\u008a\u0005\b\u0000\u0000\u008a"+
		"\u008b\u0003\u0014\n\u0000\u008b\u008c\u0005\t\u0000\u0000\u008c\u008d"+
		"\u0003\f\u0006\u0000\u008d\u008e\u0005\u001f\u0000\u0000\u008e\u008f\u0003"+
		"\f\u0006\u0000\u008f\u0098\u0001\u0000\u0000\u0000\u0090\u0091\u0005\u001e"+
		"\u0000\u0000\u0091\u0092\u0005\b\u0000\u0000\u0092\u0093\u0003\u0014\n"+
		"\u0000\u0093\u0094\u0005\t\u0000\u0000\u0094\u0095\u0003\f\u0006\u0000"+
		"\u0095\u0098\u0001\u0000\u0000\u0000\u0096\u0098\u0005\u0002\u0000\u0000"+
		"\u0097p\u0001\u0000\u0000\u0000\u0097t\u0001\u0000\u0000\u0000\u0097y"+
		"\u0001\u0000\u0000\u0000\u0097|\u0001\u0000\u0000\u0000\u0097\u0081\u0001"+
		"\u0000\u0000\u0000\u0097\u0082\u0001\u0000\u0000\u0000\u0097\u0088\u0001"+
		"\u0000\u0000\u0000\u0097\u0090\u0001\u0000\u0000\u0000\u0097\u0096\u0001"+
		"\u0000\u0000\u0000\u0098\r\u0001\u0000\u0000\u0000\u0099\u00a0\u0005,"+
		"\u0000\u0000\u009a\u009b\u0005,\u0000\u0000\u009b\u009c\u0005\f\u0000"+
		"\u0000\u009c\u009d\u0003\u0014\n\u0000\u009d\u009e\u0005\r\u0000\u0000"+
		"\u009e\u00a0\u0001\u0000\u0000\u0000\u009f\u0099\u0001\u0000\u0000\u0000"+
		"\u009f\u009a\u0001\u0000\u0000\u0000\u00a0\u000f\u0001\u0000\u0000\u0000"+
		"\u00a1\u00a2\u0005,\u0000\u0000\u00a2\u00a4\u0005\b\u0000\u0000\u00a3"+
		"\u00a5\u0003\u0012\t\u0000\u00a4\u00a3\u0001\u0000\u0000\u0000\u00a4\u00a5"+
		"\u0001\u0000\u0000\u0000\u00a5\u00a6\u0001\u0000\u0000\u0000\u00a6\u00a7"+
		"\u0005\t\u0000\u0000\u00a7\u0011\u0001\u0000\u0000\u0000\u00a8\u00ad\u0003"+
		"\u0014\n\u0000\u00a9\u00aa\u0005\u0001\u0000\u0000\u00aa\u00ac\u0003\u0014"+
		"\n\u0000\u00ab\u00a9\u0001\u0000\u0000\u0000\u00ac\u00af\u0001\u0000\u0000"+
		"\u0000\u00ad\u00ab\u0001\u0000\u0000\u0000\u00ad\u00ae\u0001\u0000\u0000"+
		"\u0000\u00ae\u0013\u0001\u0000\u0000\u0000\u00af\u00ad\u0001\u0000\u0000"+
		"\u0000\u00b0\u00b1\u0006\n\uffff\uffff\u0000\u00b1\u00b2\u0005\b\u0000"+
		"\u0000\u00b2\u00b3\u0003\u0014\n\u0000\u00b3\u00b4\u0005\t\u0000\u0000"+
		"\u00b4\u00ce\u0001\u0000\u0000\u0000\u00b5\u00b6\u0007\u0000\u0000\u0000"+
		"\u00b6\u00ce\u0003\u0014\n\u0011\u00b7\u00b8\u0005 \u0000\u0000\u00b8"+
		"\u00b9\u0007\u0001\u0000\u0000\u00b9\u00ba\u0005\f\u0000\u0000\u00ba\u00bb"+
		"\u0003\u0014\n\u0000\u00bb\u00bc\u0005\r\u0000\u0000\u00bc\u00ce\u0001"+
		"\u0000\u0000\u0000\u00bd\u00be\u0005!\u0000\u0000\u00be\u00bf\u0005\b"+
		"\u0000\u0000\u00bf\u00c0\u0003\u0014\n\u0000\u00c0\u00c1\u0005\t\u0000"+
		"\u0000\u00c1\u00ce\u0001\u0000\u0000\u0000\u00c2\u00c3\u0005,\u0000\u0000"+
		"\u00c3\u00c4\u0005\f\u0000\u0000\u00c4\u00c5\u0003\u0014\n\u0000\u00c5"+
		"\u00c6\u0005\r\u0000\u0000\u00c6\u00ce\u0001\u0000\u0000\u0000\u00c7\u00ce"+
		"\u0003\u0010\b\u0000\u00c8\u00ce\u0005\u0016\u0000\u0000\u00c9\u00ce\u0005"+
		"\u0017\u0000\u0000\u00ca\u00ce\u0005\u0019\u0000\u0000\u00cb\u00ce\u0005"+
		"\u0018\u0000\u0000\u00cc\u00ce\u0005,\u0000\u0000\u00cd\u00b0\u0001\u0000"+
		"\u0000\u0000\u00cd\u00b5\u0001\u0000\u0000\u0000\u00cd\u00b7\u0001\u0000"+
		"\u0000\u0000\u00cd\u00bd\u0001\u0000\u0000\u0000\u00cd\u00c2\u0001\u0000"+
		"\u0000\u0000\u00cd\u00c7\u0001\u0000\u0000\u0000\u00cd\u00c8\u0001\u0000"+
		"\u0000\u0000\u00cd\u00c9\u0001\u0000\u0000\u0000\u00cd\u00ca\u0001\u0000"+
		"\u0000\u0000\u00cd\u00cb\u0001\u0000\u0000\u0000\u00cd\u00cc\u0001\u0000"+
		"\u0000\u0000\u00ce\u00e6\u0001\u0000\u0000\u0000\u00cf\u00d0\n\r\u0000"+
		"\u0000\u00d0\u00d1\u0007\u0002\u0000\u0000\u00d1\u00e5\u0003\u0014\n\u000e"+
		"\u00d2\u00d3\n\f\u0000\u0000\u00d3\u00d4\u0007\u0003\u0000\u0000\u00d4"+
		"\u00e5\u0003\u0014\n\r\u00d5\u00d6\n\u000b\u0000\u0000\u00d6\u00d7\u0005"+
		"\u0007\u0000\u0000\u00d7\u00e5\u0003\u0014\n\f\u00d8\u00d9\n\n\u0000\u0000"+
		"\u00d9\u00da\u0007\u0004\u0000\u0000\u00da\u00e5\u0003\u0014\n\u000b\u00db"+
		"\u00dc\n\t\u0000\u0000\u00dc\u00dd\u0007\u0005\u0000\u0000\u00dd\u00e5"+
		"\u0003\u0014\n\n\u00de\u00df\n\b\u0000\u0000\u00df\u00e0\u0005(\u0000"+
		"\u0000\u00e0\u00e5\u0003\u0014\n\t\u00e1\u00e2\n\u0007\u0000\u0000\u00e2"+
		"\u00e3\u0005)\u0000\u0000\u00e3\u00e5\u0003\u0014\n\b\u00e4\u00cf\u0001"+
		"\u0000\u0000\u0000\u00e4\u00d2\u0001\u0000\u0000\u0000\u00e4\u00d5\u0001"+
		"\u0000\u0000\u0000\u00e4\u00d8\u0001\u0000\u0000\u0000\u00e4\u00db\u0001"+
		"\u0000\u0000\u0000\u00e4\u00de\u0001\u0000\u0000\u0000\u00e4\u00e1\u0001"+
		"\u0000\u0000\u0000\u00e5\u00e8\u0001\u0000\u0000\u0000\u00e6\u00e4\u0001"+
		"\u0000\u0000\u0000\u00e6\u00e7\u0001\u0000\u0000\u0000\u00e7\u0015\u0001"+
		"\u0000\u0000\u0000\u00e8\u00e6\u0001\u0000\u0000\u0000\u0016\u0019\u001f"+
		".16@INSX]_ek~\u0097\u009f\u00a4\u00ad\u00cd\u00e4\u00e6";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}