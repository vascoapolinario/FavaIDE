import CodeGenerator.CodeGen;
import Fava.FavaLexer;
import Fava.FavaParser;
import TypeChecker.TypeChecker;
import VM.vm;
import org.antlr.v4.runtime.*;
import org.antlr.v4.runtime.tree.ParseTree;

import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.nio.file.Files;
import java.nio.file.Paths;

public class FavaCompileAndRun {

    static boolean showLexerErrors = true;
    static boolean showParserErrors = true;
    static boolean showAssembly = true;
    static boolean trace = false;

    public static void main(String[] args) {
        InputStream inputStream = null;

        try {
            if (args.length > 0) {
                inputStream = new FileInputStream(args[0]);
                if (args.length > 1 && args[1].equals("-trace"))
                {
                    trace = true;
                }
            } else {
                inputStream = System.in;
            }

            CharStream input = CharStreams.fromStream(inputStream);

            FavaErrorListener errorListener = new FavaErrorListener(showLexerErrors, showParserErrors);

            FavaLexer lexer = new FavaLexer(input);
            lexer.removeErrorListeners();
            lexer.addErrorListener(errorListener);

            CommonTokenStream tokens = new CommonTokenStream(lexer);

            FavaParser parser = new FavaParser(tokens);
            parser.removeErrorListeners();
            parser.addErrorListener(errorListener);

            ParseTree tree = parser.prog();

            if (errorListener.getLexerErrors() > 0) {
                System.out.println("Input has lexical errors");
                return;
            }

            if (errorListener.getParserErrors() > 0) {
                System.out.println("Input has parsing errors");
                return;
            }

            TypeChecker checker = new TypeChecker();
            checker.visit(tree);

            if (checker.foundErrors()) {
                for (String err : checker.getSemanticErrors()) {
                    System.out.println(err);
                }
                return;
            }

            CodeGen codeGen = new CodeGen(checker);
            codeGen.visit(tree);

            if (showAssembly) {
                codeGen.dumpConstantPool();
                codeGen.dumpInstructions();
            }

            codeGen.saveBytecodes("bytecodes.bc");

            byte[] bytecodes = Files.readAllBytes(Paths.get("bytecodes.bc"));
            vm machine = new vm(bytecodes, trace);
            machine.run();

        } catch (IOException e) {
            System.err.println("I/O error: " + e.getMessage());
        } catch (RuntimeException e) {
            System.err.println("Runtime error: " + e.getMessage());
        } finally {
            if (inputStream != null && inputStream != System.in) {
                try {
                    inputStream.close();
                } catch (IOException ignored) {
                }
            }
        }
    }
}