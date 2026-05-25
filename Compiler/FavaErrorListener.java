import Fava.FavaLexer;
import org.antlr.v4.runtime.BaseErrorListener;
import org.antlr.v4.runtime.RecognitionException;
import org.antlr.v4.runtime.Recognizer;

public class FavaErrorListener extends BaseErrorListener {
    private int lexerErrors = 0;
    private int parserErrors = 0;
    private final boolean showLexerErrors;
    private final boolean showParserErrors;

    public FavaErrorListener(boolean showLexerErrors, boolean showParserErrors) {
        this.showLexerErrors = showLexerErrors;
        this.showParserErrors = showParserErrors;
    }

    public int getLexerErrors() {
        return lexerErrors;
    }

    public int getParserErrors() {
        return parserErrors;
    }

    @Override
    public void syntaxError(Recognizer<?, ?> recognizer,
                            Object offendingSymbol,
                            int line,
                            int charPositionInLine,
                            String msg,
                            RecognitionException e) {

        if (recognizer instanceof FavaLexer) {
            lexerErrors++;
            if (showLexerErrors) {
                System.err.printf("line %d:%d error: %s%n", line, charPositionInLine, msg);
            }
        } else {
            parserErrors++;
            if (showParserErrors) {
                System.err.printf("line %d:%d error: %s%n", line, charPositionInLine, msg);
            }
        }
    }
}