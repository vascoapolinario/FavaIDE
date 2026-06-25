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
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.HashSet;
import java.util.Set;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

public class FavaCompileAndRun {

    static boolean showLexerErrors = true;
    static boolean showParserErrors = true;
    static boolean showAssembly = true;
    static boolean trace = false;
    static boolean checkOnly = false;

    private static final Pattern MODULE_DECL = Pattern.compile("(?im)^\\s*module\\s+([A-Za-z_][A-Za-z0-9_]*)\\s*;");
    private static final Pattern IMPORT_DECL = Pattern.compile("(?im)^\\s*import\\s+([A-Za-z_][A-Za-z0-9_]*)\\s*;?");
    private static final Pattern HEADER_DECL = Pattern.compile("(?im)^\\s*(module|import)\\s+[A-Za-z_][A-Za-z0-9_]*\\s*;?\\s*(?:\\R|$)");

    public static void main(String[] args) {
        InputStream inputStream = null;
        Path projectRoot = Paths.get("").toAbsolutePath().normalize();

        try {
            if (args.length > 0) {
                inputStream = new FileInputStream(args[0]);
                Path sourcePath = Paths.get(args[0]).toAbsolutePath().normalize();
                Path parent = sourcePath.getParent();
                if (parent != null) {
                    projectRoot = parent;
                }
                for (int i = 1; i < args.length; i++) {
                    if (args[i].equals("-trace")) {
                        trace = true;
                    } else if (args[i].equals("-check") || args[i].equals("--check")) {
                        checkOnly = true;
                    } else if ((args[i].equals("-root") || args[i].equals("--root")) && i + 1 < args.length) {
                        projectRoot = Paths.get(args[++i]).toAbsolutePath().normalize();
                    }
                }
            } else {
                inputStream = System.in;
            }

            String sourceText = stripBom(new String(inputStream.readAllBytes()));
            boolean primaryIsModule = hasModuleDeclaration(sourceText);
            String combinedSource = buildCombinedSource(sourceText, projectRoot);

            CharStream input = CharStreams.fromString(combinedSource);

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
                for (String message : errorListener.getMessages()) {
                    System.out.println(message);
                }
                System.out.println("Input has lexical errors");
                System.exit(1);
                return;
            }

            if (errorListener.getParserErrors() > 0) {
                for (String message : errorListener.getMessages()) {
                    System.out.println(message);
                }
                System.out.println("Input has parsing errors");
                System.exit(1);
                return;
            }

            TypeChecker checker = new TypeChecker();
            checker.visit(tree);

            if (checker.foundErrors()) {
                for (String err : checker.getSemanticErrors()) {
                    System.out.println(err);
                }
                System.exit(1);
                return;
            }

            if (primaryIsModule) {
                if (!checkOnly) {
                    System.out.println("Cannot run module files directly. Run a program that imports this module instead.");
                    System.exit(1);
                }
                return;
            }

            CodeGen codeGen = new CodeGen(checker);
            codeGen.visit(tree);

            if (showAssembly) {
                codeGen.dumpConstantPool();
                codeGen.dumpInstructions();
                codeGen.dumpTypeInfo();
                codeGen.dumpSourceMap();
            }

            if (checkOnly) {
                return;
            }

            codeGen.saveBytecodes("bytecodes.bc");

            byte[] bytecodes = Files.readAllBytes(Paths.get("bytecodes.bc"));
            vm machine = new vm(bytecodes, trace, projectRoot.toString());
            machine.run();

        } catch (IOException e) {
            System.err.println("I/O error: " + e.getMessage());
            System.exit(1);
        } catch (RuntimeException e) {
            System.err.println("Runtime error: " + e.getMessage());
            System.exit(1);
        } finally {
            if (inputStream != null && inputStream != System.in) {
                try {
                    inputStream.close();
                } catch (IOException ignored) {
                }
            }
        }
    }

    private static boolean hasModuleDeclaration(String source) {
        return MODULE_DECL.matcher(source).find();
    }

    private static String buildCombinedSource(String primarySource, Path projectRoot) throws IOException {
        StringBuilder combined = new StringBuilder();
        appendHeaders(combined, primarySource);

        Set<String> imported = new HashSet<>();
        appendImports(combined, primarySource, projectRoot, imported);

        combined.append(stripHeaders(primarySource));
        return combined.toString();
    }

    private static void appendHeaders(StringBuilder output, String source) {
        Matcher moduleMatcher = MODULE_DECL.matcher(source);
        if (moduleMatcher.find()) {
            output.append(moduleMatcher.group()).append(System.lineSeparator());
        }

        Matcher importMatcher = IMPORT_DECL.matcher(source);
        while (importMatcher.find()) {
            String text = importMatcher.group().trim();
            output.append(text.endsWith(";") ? text : text + ";").append(System.lineSeparator());
        }
    }

    private static void appendImports(StringBuilder output, String source, Path projectRoot, Set<String> imported) throws IOException {
        Matcher matcher = IMPORT_DECL.matcher(source);
        while (matcher.find()) {
            String moduleName = matcher.group(1);
            String key = moduleName.toLowerCase();
            if (!imported.add(key)) {
                continue;
            }

            Path modulePath = findModuleFile(projectRoot, moduleName);
            if (modulePath == null) {
                throw new IOException("Module '" + moduleName + "' was not found under " + projectRoot);
            }

            String moduleSource = stripBom(Files.readString(modulePath));
            appendImports(output, moduleSource, projectRoot, imported);
            output.append(stripHeaders(moduleSource)).append(System.lineSeparator());
        }
    }

    private static Path findModuleFile(Path projectRoot, String moduleName) throws IOException {
        if (!Files.isDirectory(projectRoot)) {
            return null;
        }

        try (var files = Files.walk(projectRoot)) {
            return files
                    .filter(path -> Files.isRegularFile(path) && path.getFileName().toString().toLowerCase().endsWith(".fava"))
                    .filter(path -> declaresModule(path, moduleName))
                    .findFirst()
                    .orElse(null);
        }
    }

    private static boolean declaresModule(Path path, String moduleName) {
        try {
            Matcher matcher = MODULE_DECL.matcher(stripBom(Files.readString(path)));
            return matcher.find() && matcher.group(1).equalsIgnoreCase(moduleName);
        } catch (IOException ignored) {
            return false;
        }
    }

    private static String stripHeaders(String source) {
        return HEADER_DECL.matcher(source).replaceAll("");
    }

    private static String stripBom(String source) {
        if (!source.isEmpty() && source.charAt(0) == '\uFEFF') {
            return source.substring(1);
        }
        return source;
    }
}
