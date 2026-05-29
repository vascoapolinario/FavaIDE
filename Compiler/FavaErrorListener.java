import Fava.FavaLexer;
import org.antlr.v4.runtime.BaseErrorListener;
import org.antlr.v4.runtime.RecognitionException;
import org.antlr.v4.runtime.Recognizer;

import java.util.ArrayList;
import java.util.List;

public class FavaErrorListener extends BaseErrorListener {
    private int lexerErrors = 0;
    private int parserErrors = 0;
    private final boolean showLexerErrors;
    private final boolean showParserErrors;
    private final List<String> messages = new ArrayList<>();

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

    public List<String> getMessages() {
        return messages;
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
                messages.add(String.format("line %d:%d error: %s", line, charPositionInLine, msg));
            }
        } else {
            parserErrors++;
            if (showParserErrors) {
                messages.add(String.format("line %d:%d error: %s", line, charPositionInLine, msg));
            }
        }
    }
}
