# Arithmetic-expressions-evaluator

A parser for the arithmetic exprissions using C# language. It consists of (as the main components): 
- Lexer.
- Parser.
- Evaluator.

## Lexer
The lexer is implemented in the Lexer.cs file. It's all about divide the input stream into tokens, each is one type of the following tokens : 
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

These tokens are later implemented as an enum in the SyntaxKind file.

The following DFA represents the lexer, which accepts only these tokens :
![Image of the DFA](Images/DFA.png)


## Parser
The parser is all about receiving an array of tokens from the lexer and then construct these tokens as 
a tree of nodes. The parser is implemented in the Parser.cs file. 
We used the following CFG (Context Free Grammer) to derive our expressions:
- E → E + T | T
- T → T * F | F
- F → (E) | Number

After converting the CFG to be LL(1) the following is our new grammer:
- E → T E’
- E’→ + T E’ | ε
- T → F T’
- T’→ *F T’ | ε
- F → (E) | num

Computing the first and follow :
![Image of first and follow implementation](https://github.com/yossef-khaled/Arithmetic-expressions-evaluator/Images/FirstAndFollow.png)

Calculating parsing table :
![Image of parsing table](https://github.com/yossef-khaled/Arithmetic-expressions-evaluator/Images/ParsingTable.png)

This “SyntaxTree” class represents the tree that the parser will return. It has a root, a list of diagnostics for errors, and an end of file.
It will be something like this :
![Image of the tree](https://github.com/yossef-khaled/Arithmetic-expressions-evaluator/Images/HierarchyTree.png)

#### Note: the parser is using recursive descent to parse the stream input.

## Evaluator
The evaluator is where we calculate the result starting from the tree. It takes the root of the tree that the parser passes. It wlaks over all the nodes and when it arrive to the leaves of the tree, it calculates the result and go up tell it calculates htw wholw stream.
The evaluator is implemented in the Evaluator.cs file.

At the end, "Pretty Print" function (which is not that pretty) will output the result and the tree into the console as follow:
![Image of the output](https://github.com/yossef-khaled/Arithmetic-expressions-evaluator/Images/Output.png)


