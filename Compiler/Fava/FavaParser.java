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
		LARGEROREQUAL=22, INT=23, REAL=24, BOOL=25, STRING=26, FUNCTION=27, MODULE=28, 
		IMPORT=29, PRINT=30, RETURN=31, WHILE=32, FOR=33, IN=34, IF=35, ELSE=36, 
		TRY=37, CATCH=38, EXCEPTION=39, AS=40, NEW=41, TYPEBOOL=42, TYPEINTEGER=43, 
		TYPEREAL=44, TYPESTRING=45, MOD=46, NOT=47, AND=48, OR=49, TRUE=50, FALSE=51, 
		ID=52, WS=53, SL_COMMENT=54, ML_COMMENT=55;
	public static final int
		RULE_prog = 0, RULE_moduleDecl = 1, RULE_importDecl = 2, RULE_funcDecl = 3, 
		RULE_decl = 4, RULE_varDecl = 5, RULE_type = 6, RULE_baseType = 7, RULE_arraySuffix = 8, 
		RULE_block = 9, RULE_stmt = 10, RULE_lvalue = 11, RULE_call = 12, RULE_argList = 13, 
		RULE_expr = 14;
	private static String[] makeRuleNames() {
		return new String[] {
			"prog", "moduleDecl", "importDecl", "funcDecl", "decl", "varDecl", "type", 
			"baseType", "arraySuffix", "block", "stmt", "lvalue", "call", "argList", 
			"expr"
		};
	}
	public static final String[] ruleNames = makeRuleNames();

	private static String[] makeLiteralNames() {
		return new String[] {
			null, "';'", "','", "':='", "'->'", "'='", "'<>'", "'||'", "'++'", "'('", 
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
			"INT", "REAL", "BOOL", "STRING", "FUNCTION", "MODULE", "IMPORT", "PRINT", 
			"RETURN", "WHILE", "FOR", "IN", "IF", "ELSE", "TRY", "CATCH", "EXCEPTION", 
			"AS", "NEW", "TYPEBOOL", "TYPEINTEGER", "TYPEREAL", "TYPESTRING", "MOD", 
			"NOT", "AND", "OR", "TRUE", "FALSE", "ID", "WS", "SL_COMMENT", "ML_COMMENT"
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
		public ModuleDeclContext moduleDecl() {
			return getRuleContext(ModuleDeclContext.class,0);
		}
		public List<ImportDeclContext> importDecl() {
			return getRuleContexts(ImportDeclContext.class);
		}
		public ImportDeclContext importDecl(int i) {
			return getRuleContext(ImportDeclContext.class,i);
		}
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
			setState(31);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==MODULE) {
				{
				setState(30);
				moduleDecl();
				}
			}

			setState(36);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==IMPORT) {
				{
				{
				setState(33);
				importDecl();
				}
				}
				setState(38);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(42);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 65970697666560L) != 0)) {
				{
				{
				setState(39);
				decl();
				}
				}
				setState(44);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(48);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==FUNCTION) {
				{
				{
				setState(45);
				funcDecl();
				}
				}
				setState(50);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(51);
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
	public static class ModuleDeclContext extends ParserRuleContext {
		public TerminalNode MODULE() { return getToken(FavaParser.MODULE, 0); }
		public TerminalNode ID() { return getToken(FavaParser.ID, 0); }
		public ModuleDeclContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_moduleDecl; }
		@Override
		public void enterRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).enterModuleDecl(this);
		}
		@Override
		public void exitRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).exitModuleDecl(this);
		}
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof FavaVisitor ) return ((FavaVisitor<? extends T>)visitor).visitModuleDecl(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ModuleDeclContext moduleDecl() throws RecognitionException {
		ModuleDeclContext _localctx = new ModuleDeclContext(_ctx, getState());
		enterRule(_localctx, 2, RULE_moduleDecl);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(53);
			match(MODULE);
			setState(54);
			match(ID);
			setState(55);
			match(T__0);
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
	public static class ImportDeclContext extends ParserRuleContext {
		public TerminalNode IMPORT() { return getToken(FavaParser.IMPORT, 0); }
		public TerminalNode ID() { return getToken(FavaParser.ID, 0); }
		public ImportDeclContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_importDecl; }
		@Override
		public void enterRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).enterImportDecl(this);
		}
		@Override
		public void exitRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).exitImportDecl(this);
		}
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof FavaVisitor ) return ((FavaVisitor<? extends T>)visitor).visitImportDecl(this);
			else return visitor.visitChildren(this);
		}
	}

	public final ImportDeclContext importDecl() throws RecognitionException {
		ImportDeclContext _localctx = new ImportDeclContext(_ctx, getState());
		enterRule(_localctx, 4, RULE_importDecl);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(57);
			match(IMPORT);
			setState(58);
			match(ID);
			setState(60);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==T__0) {
				{
				setState(59);
				match(T__0);
				}
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
		enterRule(_localctx, 6, RULE_funcDecl);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(62);
			match(FUNCTION);
			setState(63);
			match(ID);
			setState(64);
			match(LPAREN);
			setState(76);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if ((((_la) & ~0x3f) == 0 && ((1L << _la) & 65970697666560L) != 0)) {
				{
				setState(65);
				type();
				setState(66);
				match(ID);
				setState(73);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while (_la==T__1) {
					{
					{
					setState(67);
					match(T__1);
					setState(68);
					type();
					setState(69);
					match(ID);
					}
					}
					setState(75);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				}
			}

			setState(78);
			match(RPAREN);
			setState(81);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==ARROW) {
				{
				setState(79);
				match(ARROW);
				setState(80);
				type();
				}
			}

			setState(83);
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
		enterRule(_localctx, 8, RULE_decl);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(85);
			type();
			setState(86);
			varDecl();
			setState(91);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==T__1) {
				{
				{
				setState(87);
				match(T__1);
				setState(88);
				varDecl();
				}
				}
				setState(93);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(94);
			match(T__0);
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
		enterRule(_localctx, 10, RULE_varDecl);
		try {
			setState(100);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,9,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(96);
				match(ID);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(97);
				match(ID);
				setState(98);
				match(ASSIGN);
				setState(99);
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
		enterRule(_localctx, 12, RULE_type);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(102);
			baseType();
			setState(106);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==LBRACK) {
				{
				{
				setState(103);
				arraySuffix();
				}
				}
				setState(108);
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
		enterRule(_localctx, 14, RULE_baseType);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(109);
			_la = _input.LA(1);
			if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & 65970697666560L) != 0)) ) {
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
		enterRule(_localctx, 16, RULE_arraySuffix);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(111);
			match(LBRACK);
			setState(112);
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
		enterRule(_localctx, 18, RULE_block);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(114);
			match(LBRACE);
			setState(119);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 4569758229858306L) != 0)) {
				{
				setState(117);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case TYPEBOOL:
				case TYPEINTEGER:
				case TYPEREAL:
				case TYPESTRING:
					{
					setState(115);
					decl();
					}
					break;
				case T__0:
				case LBRACE:
				case PRINT:
				case RETURN:
				case WHILE:
				case FOR:
				case IF:
				case TRY:
				case ID:
					{
					setState(116);
					stmt();
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(121);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(122);
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
	@SuppressWarnings("CheckReturnValue")
	public static class TryCatchStmtContext extends StmtContext {
		public TerminalNode TRY() { return getToken(FavaParser.TRY, 0); }
		public List<StmtContext> stmt() {
			return getRuleContexts(StmtContext.class);
		}
		public StmtContext stmt(int i) {
			return getRuleContext(StmtContext.class,i);
		}
		public TerminalNode CATCH() { return getToken(FavaParser.CATCH, 0); }
		public TerminalNode LPAREN() { return getToken(FavaParser.LPAREN, 0); }
		public TerminalNode EXCEPTION() { return getToken(FavaParser.EXCEPTION, 0); }
		public TerminalNode AS() { return getToken(FavaParser.AS, 0); }
		public TerminalNode ID() { return getToken(FavaParser.ID, 0); }
		public TerminalNode RPAREN() { return getToken(FavaParser.RPAREN, 0); }
		public TryCatchStmtContext(StmtContext ctx) { copyFrom(ctx); }
		@Override
		public void enterRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).enterTryCatchStmt(this);
		}
		@Override
		public void exitRule(ParseTreeListener listener) {
			if ( listener instanceof FavaListener ) ((FavaListener)listener).exitTryCatchStmt(this);
		}
		@Override
		public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
			if ( visitor instanceof FavaVisitor ) return ((FavaVisitor<? extends T>)visitor).visitTryCatchStmt(this);
			else return visitor.visitChildren(this);
		}
	}

	public final StmtContext stmt() throws RecognitionException {
		StmtContext _localctx = new StmtContext(_ctx, getState());
		enterRule(_localctx, 20, RULE_stmt);
		int _la;
		try {
			setState(195);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,15,_ctx) ) {
			case 1:
				_localctx = new PrintStmtContext(_localctx);
				enterOuterAlt(_localctx, 1);
				{
				setState(124);
				match(PRINT);
				setState(125);
				expr(0);
				setState(126);
				match(T__0);
				}
				break;
			case 2:
				_localctx = new AssignStmtContext(_localctx);
				enterOuterAlt(_localctx, 2);
				{
				setState(128);
				lvalue();
				setState(129);
				match(ASSIGN);
				setState(130);
				expr(0);
				setState(131);
				match(T__0);
				}
				break;
			case 3:
				_localctx = new CallStmtContext(_localctx);
				enterOuterAlt(_localctx, 3);
				{
				setState(133);
				call();
				setState(134);
				match(T__0);
				}
				break;
			case 4:
				_localctx = new ReturnStmtContext(_localctx);
				enterOuterAlt(_localctx, 4);
				{
				setState(136);
				match(RETURN);
				setState(138);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if ((((_la) & ~0x3f) == 0 && ((1L << _la) & 4646536264876544L) != 0)) {
					{
					setState(137);
					expr(0);
					}
				}

				setState(140);
				match(T__0);
				}
				break;
			case 5:
				_localctx = new BlockStmtContext(_localctx);
				enterOuterAlt(_localctx, 5);
				{
				setState(141);
				block();
				}
				break;
			case 6:
				_localctx = new WhileStmtContext(_localctx);
				enterOuterAlt(_localctx, 6);
				{
				setState(142);
				match(WHILE);
				setState(143);
				match(LPAREN);
				setState(144);
				expr(0);
				setState(145);
				match(RPAREN);
				setState(146);
				stmt();
				}
				break;
			case 7:
				_localctx = new ForStmtContext(_localctx);
				enterOuterAlt(_localctx, 7);
				{
				setState(148);
				match(FOR);
				setState(149);
				match(LPAREN);
				setState(150);
				type();
				setState(151);
				match(ID);
				setState(152);
				match(ASSIGN);
				setState(153);
				expr(0);
				setState(154);
				match(T__0);
				setState(155);
				expr(0);
				setState(156);
				match(T__0);
				setState(157);
				match(ID);
				setState(158);
				match(INC);
				setState(159);
				match(RPAREN);
				setState(160);
				stmt();
				}
				break;
			case 8:
				_localctx = new ForEachStmtContext(_localctx);
				enterOuterAlt(_localctx, 8);
				{
				setState(162);
				match(FOR);
				setState(163);
				match(ID);
				setState(164);
				match(IN);
				setState(165);
				expr(0);
				setState(166);
				stmt();
				}
				break;
			case 9:
				_localctx = new TryCatchStmtContext(_localctx);
				enterOuterAlt(_localctx, 9);
				{
				setState(168);
				match(TRY);
				setState(169);
				stmt();
				setState(170);
				match(CATCH);
				setState(176);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==LPAREN) {
					{
					setState(171);
					match(LPAREN);
					setState(172);
					match(EXCEPTION);
					setState(173);
					match(AS);
					setState(174);
					match(ID);
					setState(175);
					match(RPAREN);
					}
				}

				setState(178);
				stmt();
				}
				break;
			case 10:
				_localctx = new IfElseStmtContext(_localctx);
				enterOuterAlt(_localctx, 10);
				{
				setState(180);
				match(IF);
				setState(181);
				match(LPAREN);
				setState(182);
				expr(0);
				setState(183);
				match(RPAREN);
				setState(184);
				stmt();
				setState(185);
				match(ELSE);
				setState(186);
				stmt();
				}
				break;
			case 11:
				_localctx = new IfStmtContext(_localctx);
				enterOuterAlt(_localctx, 11);
				{
				setState(188);
				match(IF);
				setState(189);
				match(LPAREN);
				setState(190);
				expr(0);
				setState(191);
				match(RPAREN);
				setState(192);
				stmt();
				}
				break;
			case 12:
				_localctx = new EmptyStmtContext(_localctx);
				enterOuterAlt(_localctx, 12);
				{
				setState(194);
				match(T__0);
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
		enterRule(_localctx, 22, RULE_lvalue);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(197);
			match(ID);
			setState(204);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==LBRACK) {
				{
				{
				setState(198);
				match(LBRACK);
				setState(199);
				expr(0);
				setState(200);
				match(RBRACK);
				}
				}
				setState(206);
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
		enterRule(_localctx, 24, RULE_call);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(207);
			match(ID);
			setState(208);
			match(LPAREN);
			setState(210);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if ((((_la) & ~0x3f) == 0 && ((1L << _la) & 4646536264876544L) != 0)) {
				{
				setState(209);
				argList();
				}
			}

			setState(212);
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
		enterRule(_localctx, 26, RULE_argList);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(214);
			expr(0);
			setState(219);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==T__1) {
				{
				{
				setState(215);
				match(T__1);
				setState(216);
				expr(0);
				}
				}
				setState(221);
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
		int _startState = 28;
		enterRecursionRule(_localctx, 28, RULE_expr, _p);
		int _la;
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(247);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,20,_ctx) ) {
			case 1:
				{
				setState(223);
				match(LPAREN);
				setState(224);
				expr(0);
				setState(225);
				match(RPAREN);
				}
				break;
			case 2:
				{
				setState(227);
				_la = _input.LA(1);
				if ( !(_la==MINUS || _la==NOT) ) {
				_errHandler.recoverInline(this);
				}
				else {
					if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
					_errHandler.reportMatch(this);
					consume();
				}
				setState(228);
				expr(16);
				}
				break;
			case 3:
				{
				setState(229);
				match(NEW);
				setState(230);
				baseType();
				setState(234);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,19,_ctx);
				while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						setState(231);
						arraySuffix();
						}
						} 
					}
					setState(236);
					_errHandler.sync(this);
					_alt = getInterpreter().adaptivePredict(_input,19,_ctx);
				}
				setState(237);
				match(LBRACK);
				setState(238);
				expr(0);
				setState(239);
				match(RBRACK);
				}
				break;
			case 4:
				{
				setState(241);
				call();
				}
				break;
			case 5:
				{
				setState(242);
				match(INT);
				}
				break;
			case 6:
				{
				setState(243);
				match(REAL);
				}
				break;
			case 7:
				{
				setState(244);
				match(STRING);
				}
				break;
			case 8:
				{
				setState(245);
				match(BOOL);
				}
				break;
			case 9:
				{
				setState(246);
				match(ID);
				}
				break;
			}
			_ctx.stop = _input.LT(-1);
			setState(277);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,22,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					if ( _parseListeners!=null ) triggerExitRuleEvent();
					_prevctx = _localctx;
					{
					setState(275);
					_errHandler.sync(this);
					switch ( getInterpreter().adaptivePredict(_input,21,_ctx) ) {
					case 1:
						{
						_localctx = new ExprContext(_parentctx, _parentState);
						pushNewRecursionContext(_localctx, _startState, RULE_expr);
						setState(249);
						if (!(precpred(_ctx, 13))) throw new FailedPredicateException(this, "precpred(_ctx, 13)");
						setState(250);
						_la = _input.LA(1);
						if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & 70368744570880L) != 0)) ) {
						_errHandler.recoverInline(this);
						}
						else {
							if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
							_errHandler.reportMatch(this);
							consume();
						}
						setState(251);
						expr(14);
						}
						break;
					case 2:
						{
						_localctx = new ExprContext(_parentctx, _parentState);
						pushNewRecursionContext(_localctx, _startState, RULE_expr);
						setState(252);
						if (!(precpred(_ctx, 12))) throw new FailedPredicateException(this, "precpred(_ctx, 12)");
						setState(253);
						_la = _input.LA(1);
						if ( !(_la==PLUS || _la==MINUS) ) {
						_errHandler.recoverInline(this);
						}
						else {
							if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
							_errHandler.reportMatch(this);
							consume();
						}
						setState(254);
						expr(13);
						}
						break;
					case 3:
						{
						_localctx = new ExprContext(_parentctx, _parentState);
						pushNewRecursionContext(_localctx, _startState, RULE_expr);
						setState(255);
						if (!(precpred(_ctx, 11))) throw new FailedPredicateException(this, "precpred(_ctx, 11)");
						setState(256);
						match(CONCAT);
						setState(257);
						expr(12);
						}
						break;
					case 4:
						{
						_localctx = new ExprContext(_parentctx, _parentState);
						pushNewRecursionContext(_localctx, _startState, RULE_expr);
						setState(258);
						if (!(precpred(_ctx, 10))) throw new FailedPredicateException(this, "precpred(_ctx, 10)");
						setState(259);
						_la = _input.LA(1);
						if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & 7864320L) != 0)) ) {
						_errHandler.recoverInline(this);
						}
						else {
							if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
							_errHandler.reportMatch(this);
							consume();
						}
						setState(260);
						expr(11);
						}
						break;
					case 5:
						{
						_localctx = new ExprContext(_parentctx, _parentState);
						pushNewRecursionContext(_localctx, _startState, RULE_expr);
						setState(261);
						if (!(precpred(_ctx, 9))) throw new FailedPredicateException(this, "precpred(_ctx, 9)");
						setState(262);
						_la = _input.LA(1);
						if ( !(_la==EQUAL || _la==NEQUAL) ) {
						_errHandler.recoverInline(this);
						}
						else {
							if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
							_errHandler.reportMatch(this);
							consume();
						}
						setState(263);
						expr(10);
						}
						break;
					case 6:
						{
						_localctx = new ExprContext(_parentctx, _parentState);
						pushNewRecursionContext(_localctx, _startState, RULE_expr);
						setState(264);
						if (!(precpred(_ctx, 8))) throw new FailedPredicateException(this, "precpred(_ctx, 8)");
						setState(265);
						match(AND);
						setState(266);
						expr(9);
						}
						break;
					case 7:
						{
						_localctx = new ExprContext(_parentctx, _parentState);
						pushNewRecursionContext(_localctx, _startState, RULE_expr);
						setState(267);
						if (!(precpred(_ctx, 7))) throw new FailedPredicateException(this, "precpred(_ctx, 7)");
						setState(268);
						match(OR);
						setState(269);
						expr(8);
						}
						break;
					case 8:
						{
						_localctx = new ExprContext(_parentctx, _parentState);
						pushNewRecursionContext(_localctx, _startState, RULE_expr);
						setState(270);
						if (!(precpred(_ctx, 14))) throw new FailedPredicateException(this, "precpred(_ctx, 14)");
						setState(271);
						match(LBRACK);
						setState(272);
						expr(0);
						setState(273);
						match(RBRACK);
						}
						break;
					}
					} 
				}
				setState(279);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,22,_ctx);
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
		case 14:
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
		"\u0004\u00017\u0119\u0002\u0000\u0007\u0000\u0002\u0001\u0007\u0001\u0002"+
		"\u0002\u0007\u0002\u0002\u0003\u0007\u0003\u0002\u0004\u0007\u0004\u0002"+
		"\u0005\u0007\u0005\u0002\u0006\u0007\u0006\u0002\u0007\u0007\u0007\u0002"+
		"\b\u0007\b\u0002\t\u0007\t\u0002\n\u0007\n\u0002\u000b\u0007\u000b\u0002"+
		"\f\u0007\f\u0002\r\u0007\r\u0002\u000e\u0007\u000e\u0001\u0000\u0003\u0000"+
		" \b\u0000\u0001\u0000\u0005\u0000#\b\u0000\n\u0000\f\u0000&\t\u0000\u0001"+
		"\u0000\u0005\u0000)\b\u0000\n\u0000\f\u0000,\t\u0000\u0001\u0000\u0005"+
		"\u0000/\b\u0000\n\u0000\f\u00002\t\u0000\u0001\u0000\u0001\u0000\u0001"+
		"\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0002\u0001\u0002\u0001"+
		"\u0002\u0003\u0002=\b\u0002\u0001\u0003\u0001\u0003\u0001\u0003\u0001"+
		"\u0003\u0001\u0003\u0001\u0003\u0001\u0003\u0001\u0003\u0001\u0003\u0005"+
		"\u0003H\b\u0003\n\u0003\f\u0003K\t\u0003\u0003\u0003M\b\u0003\u0001\u0003"+
		"\u0001\u0003\u0001\u0003\u0003\u0003R\b\u0003\u0001\u0003\u0001\u0003"+
		"\u0001\u0004\u0001\u0004\u0001\u0004\u0001\u0004\u0005\u0004Z\b\u0004"+
		"\n\u0004\f\u0004]\t\u0004\u0001\u0004\u0001\u0004\u0001\u0005\u0001\u0005"+
		"\u0001\u0005\u0001\u0005\u0003\u0005e\b\u0005\u0001\u0006\u0001\u0006"+
		"\u0005\u0006i\b\u0006\n\u0006\f\u0006l\t\u0006\u0001\u0007\u0001\u0007"+
		"\u0001\b\u0001\b\u0001\b\u0001\t\u0001\t\u0001\t\u0005\tv\b\t\n\t\f\t"+
		"y\t\t\u0001\t\u0001\t\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n"+
		"\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0003"+
		"\n\u008b\b\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001"+
		"\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001"+
		"\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001"+
		"\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001"+
		"\n\u0001\n\u0003\n\u00b1\b\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001"+
		"\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001\n\u0001"+
		"\n\u0001\n\u0001\n\u0003\n\u00c4\b\n\u0001\u000b\u0001\u000b\u0001\u000b"+
		"\u0001\u000b\u0001\u000b\u0005\u000b\u00cb\b\u000b\n\u000b\f\u000b\u00ce"+
		"\t\u000b\u0001\f\u0001\f\u0001\f\u0003\f\u00d3\b\f\u0001\f\u0001\f\u0001"+
		"\r\u0001\r\u0001\r\u0005\r\u00da\b\r\n\r\f\r\u00dd\t\r\u0001\u000e\u0001"+
		"\u000e\u0001\u000e\u0001\u000e\u0001\u000e\u0001\u000e\u0001\u000e\u0001"+
		"\u000e\u0001\u000e\u0001\u000e\u0005\u000e\u00e9\b\u000e\n\u000e\f\u000e"+
		"\u00ec\t\u000e\u0001\u000e\u0001\u000e\u0001\u000e\u0001\u000e\u0001\u000e"+
		"\u0001\u000e\u0001\u000e\u0001\u000e\u0001\u000e\u0001\u000e\u0003\u000e"+
		"\u00f8\b\u000e\u0001\u000e\u0001\u000e\u0001\u000e\u0001\u000e\u0001\u000e"+
		"\u0001\u000e\u0001\u000e\u0001\u000e\u0001\u000e\u0001\u000e\u0001\u000e"+
		"\u0001\u000e\u0001\u000e\u0001\u000e\u0001\u000e\u0001\u000e\u0001\u000e"+
		"\u0001\u000e\u0001\u000e\u0001\u000e\u0001\u000e\u0001\u000e\u0001\u000e"+
		"\u0001\u000e\u0001\u000e\u0001\u000e\u0005\u000e\u0114\b\u000e\n\u000e"+
		"\f\u000e\u0117\t\u000e\u0001\u000e\u0000\u0001\u001c\u000f\u0000\u0002"+
		"\u0004\u0006\b\n\f\u000e\u0010\u0012\u0014\u0016\u0018\u001a\u001c\u0000"+
		"\u0006\u0001\u0000*-\u0002\u0000\u0010\u0010//\u0002\u0000\u0011\u0012"+
		"..\u0001\u0000\u000f\u0010\u0001\u0000\u0013\u0016\u0001\u0000\u0005\u0006"+
		"\u0137\u0000\u001f\u0001\u0000\u0000\u0000\u00025\u0001\u0000\u0000\u0000"+
		"\u00049\u0001\u0000\u0000\u0000\u0006>\u0001\u0000\u0000\u0000\bU\u0001"+
		"\u0000\u0000\u0000\nd\u0001\u0000\u0000\u0000\ff\u0001\u0000\u0000\u0000"+
		"\u000em\u0001\u0000\u0000\u0000\u0010o\u0001\u0000\u0000\u0000\u0012r"+
		"\u0001\u0000\u0000\u0000\u0014\u00c3\u0001\u0000\u0000\u0000\u0016\u00c5"+
		"\u0001\u0000\u0000\u0000\u0018\u00cf\u0001\u0000\u0000\u0000\u001a\u00d6"+
		"\u0001\u0000\u0000\u0000\u001c\u00f7\u0001\u0000\u0000\u0000\u001e \u0003"+
		"\u0002\u0001\u0000\u001f\u001e\u0001\u0000\u0000\u0000\u001f \u0001\u0000"+
		"\u0000\u0000 $\u0001\u0000\u0000\u0000!#\u0003\u0004\u0002\u0000\"!\u0001"+
		"\u0000\u0000\u0000#&\u0001\u0000\u0000\u0000$\"\u0001\u0000\u0000\u0000"+
		"$%\u0001\u0000\u0000\u0000%*\u0001\u0000\u0000\u0000&$\u0001\u0000\u0000"+
		"\u0000\')\u0003\b\u0004\u0000(\'\u0001\u0000\u0000\u0000),\u0001\u0000"+
		"\u0000\u0000*(\u0001\u0000\u0000\u0000*+\u0001\u0000\u0000\u0000+0\u0001"+
		"\u0000\u0000\u0000,*\u0001\u0000\u0000\u0000-/\u0003\u0006\u0003\u0000"+
		".-\u0001\u0000\u0000\u0000/2\u0001\u0000\u0000\u00000.\u0001\u0000\u0000"+
		"\u000001\u0001\u0000\u0000\u000013\u0001\u0000\u0000\u000020\u0001\u0000"+
		"\u0000\u000034\u0005\u0000\u0000\u00014\u0001\u0001\u0000\u0000\u0000"+
		"56\u0005\u001c\u0000\u000067\u00054\u0000\u000078\u0005\u0001\u0000\u0000"+
		"8\u0003\u0001\u0000\u0000\u00009:\u0005\u001d\u0000\u0000:<\u00054\u0000"+
		"\u0000;=\u0005\u0001\u0000\u0000<;\u0001\u0000\u0000\u0000<=\u0001\u0000"+
		"\u0000\u0000=\u0005\u0001\u0000\u0000\u0000>?\u0005\u001b\u0000\u0000"+
		"?@\u00054\u0000\u0000@L\u0005\t\u0000\u0000AB\u0003\f\u0006\u0000BI\u0005"+
		"4\u0000\u0000CD\u0005\u0002\u0000\u0000DE\u0003\f\u0006\u0000EF\u0005"+
		"4\u0000\u0000FH\u0001\u0000\u0000\u0000GC\u0001\u0000\u0000\u0000HK\u0001"+
		"\u0000\u0000\u0000IG\u0001\u0000\u0000\u0000IJ\u0001\u0000\u0000\u0000"+
		"JM\u0001\u0000\u0000\u0000KI\u0001\u0000\u0000\u0000LA\u0001\u0000\u0000"+
		"\u0000LM\u0001\u0000\u0000\u0000MN\u0001\u0000\u0000\u0000NQ\u0005\n\u0000"+
		"\u0000OP\u0005\u0004\u0000\u0000PR\u0003\f\u0006\u0000QO\u0001\u0000\u0000"+
		"\u0000QR\u0001\u0000\u0000\u0000RS\u0001\u0000\u0000\u0000ST\u0003\u0012"+
		"\t\u0000T\u0007\u0001\u0000\u0000\u0000UV\u0003\f\u0006\u0000V[\u0003"+
		"\n\u0005\u0000WX\u0005\u0002\u0000\u0000XZ\u0003\n\u0005\u0000YW\u0001"+
		"\u0000\u0000\u0000Z]\u0001\u0000\u0000\u0000[Y\u0001\u0000\u0000\u0000"+
		"[\\\u0001\u0000\u0000\u0000\\^\u0001\u0000\u0000\u0000][\u0001\u0000\u0000"+
		"\u0000^_\u0005\u0001\u0000\u0000_\t\u0001\u0000\u0000\u0000`e\u00054\u0000"+
		"\u0000ab\u00054\u0000\u0000bc\u0005\u0003\u0000\u0000ce\u0003\u001c\u000e"+
		"\u0000d`\u0001\u0000\u0000\u0000da\u0001\u0000\u0000\u0000e\u000b\u0001"+
		"\u0000\u0000\u0000fj\u0003\u000e\u0007\u0000gi\u0003\u0010\b\u0000hg\u0001"+
		"\u0000\u0000\u0000il\u0001\u0000\u0000\u0000jh\u0001\u0000\u0000\u0000"+
		"jk\u0001\u0000\u0000\u0000k\r\u0001\u0000\u0000\u0000lj\u0001\u0000\u0000"+
		"\u0000mn\u0007\u0000\u0000\u0000n\u000f\u0001\u0000\u0000\u0000op\u0005"+
		"\r\u0000\u0000pq\u0005\u000e\u0000\u0000q\u0011\u0001\u0000\u0000\u0000"+
		"rw\u0005\u000b\u0000\u0000sv\u0003\b\u0004\u0000tv\u0003\u0014\n\u0000"+
		"us\u0001\u0000\u0000\u0000ut\u0001\u0000\u0000\u0000vy\u0001\u0000\u0000"+
		"\u0000wu\u0001\u0000\u0000\u0000wx\u0001\u0000\u0000\u0000xz\u0001\u0000"+
		"\u0000\u0000yw\u0001\u0000\u0000\u0000z{\u0005\f\u0000\u0000{\u0013\u0001"+
		"\u0000\u0000\u0000|}\u0005\u001e\u0000\u0000}~\u0003\u001c\u000e\u0000"+
		"~\u007f\u0005\u0001\u0000\u0000\u007f\u00c4\u0001\u0000\u0000\u0000\u0080"+
		"\u0081\u0003\u0016\u000b\u0000\u0081\u0082\u0005\u0003\u0000\u0000\u0082"+
		"\u0083\u0003\u001c\u000e\u0000\u0083\u0084\u0005\u0001\u0000\u0000\u0084"+
		"\u00c4\u0001\u0000\u0000\u0000\u0085\u0086\u0003\u0018\f\u0000\u0086\u0087"+
		"\u0005\u0001\u0000\u0000\u0087\u00c4\u0001\u0000\u0000\u0000\u0088\u008a"+
		"\u0005\u001f\u0000\u0000\u0089\u008b\u0003\u001c\u000e\u0000\u008a\u0089"+
		"\u0001\u0000\u0000\u0000\u008a\u008b\u0001\u0000\u0000\u0000\u008b\u008c"+
		"\u0001\u0000\u0000\u0000\u008c\u00c4\u0005\u0001\u0000\u0000\u008d\u00c4"+
		"\u0003\u0012\t\u0000\u008e\u008f\u0005 \u0000\u0000\u008f\u0090\u0005"+
		"\t\u0000\u0000\u0090\u0091\u0003\u001c\u000e\u0000\u0091\u0092\u0005\n"+
		"\u0000\u0000\u0092\u0093\u0003\u0014\n\u0000\u0093\u00c4\u0001\u0000\u0000"+
		"\u0000\u0094\u0095\u0005!\u0000\u0000\u0095\u0096\u0005\t\u0000\u0000"+
		"\u0096\u0097\u0003\f\u0006\u0000\u0097\u0098\u00054\u0000\u0000\u0098"+
		"\u0099\u0005\u0003\u0000\u0000\u0099\u009a\u0003\u001c\u000e\u0000\u009a"+
		"\u009b\u0005\u0001\u0000\u0000\u009b\u009c\u0003\u001c\u000e\u0000\u009c"+
		"\u009d\u0005\u0001\u0000\u0000\u009d\u009e\u00054\u0000\u0000\u009e\u009f"+
		"\u0005\b\u0000\u0000\u009f\u00a0\u0005\n\u0000\u0000\u00a0\u00a1\u0003"+
		"\u0014\n\u0000\u00a1\u00c4\u0001\u0000\u0000\u0000\u00a2\u00a3\u0005!"+
		"\u0000\u0000\u00a3\u00a4\u00054\u0000\u0000\u00a4\u00a5\u0005\"\u0000"+
		"\u0000\u00a5\u00a6\u0003\u001c\u000e\u0000\u00a6\u00a7\u0003\u0014\n\u0000"+
		"\u00a7\u00c4\u0001\u0000\u0000\u0000\u00a8\u00a9\u0005%\u0000\u0000\u00a9"+
		"\u00aa\u0003\u0014\n\u0000\u00aa\u00b0\u0005&\u0000\u0000\u00ab\u00ac"+
		"\u0005\t\u0000\u0000\u00ac\u00ad\u0005\'\u0000\u0000\u00ad\u00ae\u0005"+
		"(\u0000\u0000\u00ae\u00af\u00054\u0000\u0000\u00af\u00b1\u0005\n\u0000"+
		"\u0000\u00b0\u00ab\u0001\u0000\u0000\u0000\u00b0\u00b1\u0001\u0000\u0000"+
		"\u0000\u00b1\u00b2\u0001\u0000\u0000\u0000\u00b2\u00b3\u0003\u0014\n\u0000"+
		"\u00b3\u00c4\u0001\u0000\u0000\u0000\u00b4\u00b5\u0005#\u0000\u0000\u00b5"+
		"\u00b6\u0005\t\u0000\u0000\u00b6\u00b7\u0003\u001c\u000e\u0000\u00b7\u00b8"+
		"\u0005\n\u0000\u0000\u00b8\u00b9\u0003\u0014\n\u0000\u00b9\u00ba\u0005"+
		"$\u0000\u0000\u00ba\u00bb\u0003\u0014\n\u0000\u00bb\u00c4\u0001\u0000"+
		"\u0000\u0000\u00bc\u00bd\u0005#\u0000\u0000\u00bd\u00be\u0005\t\u0000"+
		"\u0000\u00be\u00bf\u0003\u001c\u000e\u0000\u00bf\u00c0\u0005\n\u0000\u0000"+
		"\u00c0\u00c1\u0003\u0014\n\u0000\u00c1\u00c4\u0001\u0000\u0000\u0000\u00c2"+
		"\u00c4\u0005\u0001\u0000\u0000\u00c3|\u0001\u0000\u0000\u0000\u00c3\u0080"+
		"\u0001\u0000\u0000\u0000\u00c3\u0085\u0001\u0000\u0000\u0000\u00c3\u0088"+
		"\u0001\u0000\u0000\u0000\u00c3\u008d\u0001\u0000\u0000\u0000\u00c3\u008e"+
		"\u0001\u0000\u0000\u0000\u00c3\u0094\u0001\u0000\u0000\u0000\u00c3\u00a2"+
		"\u0001\u0000\u0000\u0000\u00c3\u00a8\u0001\u0000\u0000\u0000\u00c3\u00b4"+
		"\u0001\u0000\u0000\u0000\u00c3\u00bc\u0001\u0000\u0000\u0000\u00c3\u00c2"+
		"\u0001\u0000\u0000\u0000\u00c4\u0015\u0001\u0000\u0000\u0000\u00c5\u00cc"+
		"\u00054\u0000\u0000\u00c6\u00c7\u0005\r\u0000\u0000\u00c7\u00c8\u0003"+
		"\u001c\u000e\u0000\u00c8\u00c9\u0005\u000e\u0000\u0000\u00c9\u00cb\u0001"+
		"\u0000\u0000\u0000\u00ca\u00c6\u0001\u0000\u0000\u0000\u00cb\u00ce\u0001"+
		"\u0000\u0000\u0000\u00cc\u00ca\u0001\u0000\u0000\u0000\u00cc\u00cd\u0001"+
		"\u0000\u0000\u0000\u00cd\u0017\u0001\u0000\u0000\u0000\u00ce\u00cc\u0001"+
		"\u0000\u0000\u0000\u00cf\u00d0\u00054\u0000\u0000\u00d0\u00d2\u0005\t"+
		"\u0000\u0000\u00d1\u00d3\u0003\u001a\r\u0000\u00d2\u00d1\u0001\u0000\u0000"+
		"\u0000\u00d2\u00d3\u0001\u0000\u0000\u0000\u00d3\u00d4\u0001\u0000\u0000"+
		"\u0000\u00d4\u00d5\u0005\n\u0000\u0000\u00d5\u0019\u0001\u0000\u0000\u0000"+
		"\u00d6\u00db\u0003\u001c\u000e\u0000\u00d7\u00d8\u0005\u0002\u0000\u0000"+
		"\u00d8\u00da\u0003\u001c\u000e\u0000\u00d9\u00d7\u0001\u0000\u0000\u0000"+
		"\u00da\u00dd\u0001\u0000\u0000\u0000\u00db\u00d9\u0001\u0000\u0000\u0000"+
		"\u00db\u00dc\u0001\u0000\u0000\u0000\u00dc\u001b\u0001\u0000\u0000\u0000"+
		"\u00dd\u00db\u0001\u0000\u0000\u0000\u00de\u00df\u0006\u000e\uffff\uffff"+
		"\u0000\u00df\u00e0\u0005\t\u0000\u0000\u00e0\u00e1\u0003\u001c\u000e\u0000"+
		"\u00e1\u00e2\u0005\n\u0000\u0000\u00e2\u00f8\u0001\u0000\u0000\u0000\u00e3"+
		"\u00e4\u0007\u0001\u0000\u0000\u00e4\u00f8\u0003\u001c\u000e\u0010\u00e5"+
		"\u00e6\u0005)\u0000\u0000\u00e6\u00ea\u0003\u000e\u0007\u0000\u00e7\u00e9"+
		"\u0003\u0010\b\u0000\u00e8\u00e7\u0001\u0000\u0000\u0000\u00e9\u00ec\u0001"+
		"\u0000\u0000\u0000\u00ea\u00e8\u0001\u0000\u0000\u0000\u00ea\u00eb\u0001"+
		"\u0000\u0000\u0000\u00eb\u00ed\u0001\u0000\u0000\u0000\u00ec\u00ea\u0001"+
		"\u0000\u0000\u0000\u00ed\u00ee\u0005\r\u0000\u0000\u00ee\u00ef\u0003\u001c"+
		"\u000e\u0000\u00ef\u00f0\u0005\u000e\u0000\u0000\u00f0\u00f8\u0001\u0000"+
		"\u0000\u0000\u00f1\u00f8\u0003\u0018\f\u0000\u00f2\u00f8\u0005\u0017\u0000"+
		"\u0000\u00f3\u00f8\u0005\u0018\u0000\u0000\u00f4\u00f8\u0005\u001a\u0000"+
		"\u0000\u00f5\u00f8\u0005\u0019\u0000\u0000\u00f6\u00f8\u00054\u0000\u0000"+
		"\u00f7\u00de\u0001\u0000\u0000\u0000\u00f7\u00e3\u0001\u0000\u0000\u0000"+
		"\u00f7\u00e5\u0001\u0000\u0000\u0000\u00f7\u00f1\u0001\u0000\u0000\u0000"+
		"\u00f7\u00f2\u0001\u0000\u0000\u0000\u00f7\u00f3\u0001\u0000\u0000\u0000"+
		"\u00f7\u00f4\u0001\u0000\u0000\u0000\u00f7\u00f5\u0001\u0000\u0000\u0000"+
		"\u00f7\u00f6\u0001\u0000\u0000\u0000\u00f8\u0115\u0001\u0000\u0000\u0000"+
		"\u00f9\u00fa\n\r\u0000\u0000\u00fa\u00fb\u0007\u0002\u0000\u0000\u00fb"+
		"\u0114\u0003\u001c\u000e\u000e\u00fc\u00fd\n\f\u0000\u0000\u00fd\u00fe"+
		"\u0007\u0003\u0000\u0000\u00fe\u0114\u0003\u001c\u000e\r\u00ff\u0100\n"+
		"\u000b\u0000\u0000\u0100\u0101\u0005\u0007\u0000\u0000\u0101\u0114\u0003"+
		"\u001c\u000e\f\u0102\u0103\n\n\u0000\u0000\u0103\u0104\u0007\u0004\u0000"+
		"\u0000\u0104\u0114\u0003\u001c\u000e\u000b\u0105\u0106\n\t\u0000\u0000"+
		"\u0106\u0107\u0007\u0005\u0000\u0000\u0107\u0114\u0003\u001c\u000e\n\u0108"+
		"\u0109\n\b\u0000\u0000\u0109\u010a\u00050\u0000\u0000\u010a\u0114\u0003"+
		"\u001c\u000e\t\u010b\u010c\n\u0007\u0000\u0000\u010c\u010d\u00051\u0000"+
		"\u0000\u010d\u0114\u0003\u001c\u000e\b\u010e\u010f\n\u000e\u0000\u0000"+
		"\u010f\u0110\u0005\r\u0000\u0000\u0110\u0111\u0003\u001c\u000e\u0000\u0111"+
		"\u0112\u0005\u000e\u0000\u0000\u0112\u0114\u0001\u0000\u0000\u0000\u0113"+
		"\u00f9\u0001\u0000\u0000\u0000\u0113\u00fc\u0001\u0000\u0000\u0000\u0113"+
		"\u00ff\u0001\u0000\u0000\u0000\u0113\u0102\u0001\u0000\u0000\u0000\u0113"+
		"\u0105\u0001\u0000\u0000\u0000\u0113\u0108\u0001\u0000\u0000\u0000\u0113"+
		"\u010b\u0001\u0000\u0000\u0000\u0113\u010e\u0001\u0000\u0000\u0000\u0114"+
		"\u0117\u0001\u0000\u0000\u0000\u0115\u0113\u0001\u0000\u0000\u0000\u0115"+
		"\u0116\u0001\u0000\u0000\u0000\u0116\u001d\u0001\u0000\u0000\u0000\u0117"+
		"\u0115\u0001\u0000\u0000\u0000\u0017\u001f$*0<ILQ[djuw\u008a\u00b0\u00c3"+
		"\u00cc\u00d2\u00db\u00ea\u00f7\u0113\u0115";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}