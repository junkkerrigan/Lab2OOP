grammar Grammar;



rule : expression EOF;

expression 
	: MINUS_SIGN NUMBER_EXPRESSION #NegativeNumber
	| LEFT_BRACKET expression RIGHT_BRACKET #InBrackets
	| expression IN_POWER expression #InPower
	| expression operation = (MULTIPLICATION_SIGN | DIVISION_SIGN) expression #MultiplicationDivision
	| expression operation = (MINUS_SIGN | PLUS_SIGN) expression #AdditionSubtraction
	| expression ON_MODULO expression #OnModulo
	| NUMBER_EXPRESSION #Number
	| CELL_EXPRESSION #Cell
	| WRONG_EXPRESSION #Wrong
	;



LEFT_BRACKET : '(';
RIGHT_BRACKET : ')';
CELL_EXPRESSION : COLUMN_EXPRESSION NUMBER_EXPRESSION;
COLUMN_EXPRESSION : ('A'..'Z')+;
IN_POWER : '^';
MULTIPLICATION_SIGN : '*';
DIVISION_SIGN : '/';
PLUS_SIGN : '+';
MINUS_SIGN : '-';
ON_MODULO : ' ON_MODULO ';
NUMBER_EXPRESSION : ('0'..'9')+;
WHITESPACE_EXPRESSION : (' ' | '\t')+ -> skip;
WRONG_EXPRESSION : .;
