grammar UGEN;

start
: pattern* EOF
;

pattern
: ( TYPE )? IDENTIFIER ':' patternExprList
;

patternExprList
: patternExprOperatorPipe ( ',' patternExprOperatorPipe )*
;

patternExprOperatorPipe
: patternExprOperatorOptional ( '|' patternExprOperatorOptional )*
;

patternExprOperatorOptional
: patternExprTerm optionalFlag
;

patternExprTerm
: STRING_TUPLE
| IDENTIFIER
| '(' patternExprList ')'
;

optionalFlag
: '?'?
;

TYPE
: I N T E N T
| E N T I T Y
;

IDENTIFIER
: LETTER ( LETTER | '0'..'9' | '-' )*
;

STRING_TUPLE
: '[' STRING ( ',' STRING )* ']'
;

BLOCKCOMMENT
: '/*' .*? '*/' -> skip
;

LINECOMMENT
: ( '//' ~[\r\n]* | '#' ~[\r\n]* ) -> skip
;

WHITESPACE
: [ \t]+ -> skip
;

NEWLINE
: ( '\r''\n'? | '\n' ) -> skip
;

fragment STRING
: (~(','|']'))+
;

fragment LETTER
: 'A'..'Z' | 'a'..'z' | '_'
;

fragment A : [aA];
fragment B : [bB];
fragment C : [cC];
fragment D : [dD];
fragment E : [eE];
fragment F : [fF];
fragment G : [gG];
fragment H : [hH];
fragment I : [iI];
fragment J : [jJ];
fragment K : [kK];
fragment L : [lL];
fragment M : [mM];
fragment N : [nN];
fragment O : [oO];
fragment P : [pP];
fragment Q : [qQ];
fragment R : [rR];
fragment S : [sS];
fragment T : [tT];
fragment U : [uU];
fragment V : [vV];
fragment W : [wW];
fragment X : [xX];
fragment Y : [yY];
fragment Z : [zZ];
