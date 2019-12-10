grammar Lab2Grammar;

/*
 * Parser Rules
 */

unit : expr EOF;

expr 
	: SUBTRACTION NUM #NegNum 
	| INC LBRACKET expr RBRACKET #Inc
	| DEC LBRACKET expr RBRACKET #Dec
	| LBRACKET expr RBRACKET #Brackets
	| expr POWER expr #Power
	| expr operatorToken=(DIV|MULTIPLICATION) expr #MultiplicationOrDivision
	| expr operatorToken=(ADDITION|SUBTRACTION) expr #AdditionOrSubtraction
	| expr MOD expr #Mod
	| NUM #Num
	| CELLREF #CellRef
	| INVALID #Invalid
	;

/*
 * Lexer Rules
 */

LBRACKET : '(';
RBRACKET : ')';
CELLREF : ('A'..'Z')+ NUM;
POWER : '^';
MULTIPLICATION : '*';
INC : 'inc';
DEC : 'dec';
DIV : '/';
ADDITION : '+';
SUBTRACTION : '-';
MOD : ' mod ';
NUM : ('0'..'9')+;
WHITESPACE : (' ' | '\t')+ -> skip;
INVALID : .;
