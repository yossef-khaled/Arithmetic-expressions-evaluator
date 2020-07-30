# Arithmetic-expressions-evaluator

A parser for the arithmetic exprissions using C# language. It consists of (as the main components): 
- Lexer.
- Parser.
- Evaluator.

## Lexer
The lexer is all about divide the input stream into tokens, each is one type of the following tokens : 
- Numbers                        {0-9}*
- Plus                           { + }
- Minus                          { - }
- Multiply                       { * }
- Divide                         { / }
- Open Parenthesis               { ( }
- Close Parenthesis              { ) }
- End Of File                    { \0 }
- White Space                    {   }*
- Bad Token                      { $ / @ / ! / % / ^ …. etc}*  

Which is later is implemented as an enum in the SyntaxKind file.

The following DFA represents the lexer, which accepts only these tokens :
![Image of the DFA](https://github.com/yossef-khaled/Arithmetic-expressions-evaluator/Images/DFA.png)


