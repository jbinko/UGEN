// MIT License

// Copyright (c) 2020 Jiri Binko

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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
