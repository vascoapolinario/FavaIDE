grammar Fava;

prog
    : decl* funcDecl+ EOF
    ;

funcDecl
    : FUNCTION ID LPAREN (type ID (',' type ID)*)? RPAREN (ARROW type)? block
    ;

decl
    : type varDecl (',' varDecl)* ';'
    ;

varDecl
    : ID
    | ID ASSIGN expr
    ;

type
    : baseType arraySuffix*
    ;

baseType
    : TYPEBOOL
    | TYPEINTEGER
    | TYPEREAL
    | TYPESTRING
    ;

arraySuffix
    : LBRACK RBRACK
    ;

block
    : LBRACE (decl | stmt)* RBRACE
    ;

stmt
    : PRINT expr ';'                           # PrintStmt
    | lvalue ASSIGN expr ';'                   # AssignStmt
    | call ';'                                 # CallStmt
    | RETURN expr? ';'                         # ReturnStmt
    | block                                    # BlockStmt
    | WHILE LPAREN expr RPAREN stmt            # WhileStmt
    | FOR LPAREN type ID ASSIGN expr ';' expr ';' ID INC RPAREN stmt # ForStmt
    | FOR ID IN expr stmt                      # ForEachStmt
    | IF LPAREN expr RPAREN stmt ELSE stmt     # IfElseStmt
    | IF LPAREN expr RPAREN stmt               # IfStmt
    | ';'                                      # EmptyStmt
    ;

lvalue
    : ID (LBRACK expr RBRACK)*
    ;

call
    : ID LPAREN argList? RPAREN
    ;

argList
    : expr (',' expr)*
    ;

expr
    : LPAREN expr RPAREN
    | (NOT | MINUS) expr
    | NEW baseType arraySuffix* LBRACK expr RBRACK
    | expr LBRACK expr RBRACK
    | expr (TIMES | DIV | MOD) expr
    | expr (PLUS | MINUS) expr
    | expr CONCAT expr
    | expr (SMALLER | LARGER | SMALLEROREQUAL | LARGEROREQUAL) expr
    | expr (EQUAL | NEQUAL) expr
    | expr AND expr
    | expr OR expr
    | call
    | INT
    | REAL
    | STRING
    | BOOL
    | ID
    ;

ASSIGN           : ':=';
ARROW            : '->';
EQUAL            : '=';
NEQUAL           : '<>';
CONCAT           : '||';
INC              : '++';
LPAREN           : '(';
RPAREN           : ')';
LBRACE           : '{';
RBRACE           : '}';
LBRACK           : '[';
RBRACK           : ']';
PLUS             : '+';
MINUS            : '-';
TIMES            : '*';
DIV              : '/';
SMALLER          : '<';
LARGER           : '>';
SMALLEROREQUAL   : '<=';
LARGEROREQUAL    : '>=';

INT              : DIGIT+;
REAL             : DIGIT+ '.' DIGIT* | '.' DIGIT+;
BOOL             : TRUE | FALSE;
STRING           : '"' .*? '"';

FUNCTION         : [fF] [uU] [nN] [cC] [tT] [iI] [oO] [nN];
PRINT            : [pP] [rR] [iI] [nN] [tT];
RETURN           : [rR] [eE] [tT] [uU] [rR] [nN];
WHILE            : [wW] [hH] [iI] [lL] [eE];
FOR              : [fF] [oO] [rR];
IN               : [iI] [nN];
IF               : [iI] [fF];
ELSE             : [eE] [lL] [sS] [eE];
NEW              : [nN] [eE] [wW];

TYPEBOOL         : [bB] [oO] [oO] [lL];
TYPEINTEGER      : [iI] [nN] [tT] [eE] [gG] [eE] [rR];
TYPEREAL         : [rR] [eE] [aA] [lL];
TYPESTRING       : [sS] [tT] [rR] [iI] [nN] [gG];

MOD              : [mM] [oO] [dD];
NOT              : [nN] [oO] [tT];
AND              : [aA] [nN] [dD];
OR               : [oO] [rR];
TRUE             : [tT] [rR] [uU] [eE];
FALSE            : [fF] [aA] [lL] [sS] [eE];

ID               : LETTER (LETTER | DIGIT | '_')*;

WS               : [ \t\r\n]+ -> skip;
SL_COMMENT       : '//' .*? (EOF | '\n') -> skip;
ML_COMMENT       : '/*' .*? '*/' -> skip;

fragment DIGIT   : [0-9];
fragment LETTER  : [a-zA-Z_];
