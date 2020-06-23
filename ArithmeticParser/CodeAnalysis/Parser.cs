using System;
using System.Collections.Generic;
using System.Linq;

namespace Parser.CodeAnalysis{
class Parser {

    private readonly SyntaxToken[] _tokens;
    private int _position;
    private List<string> _diagnostics => new List<string>();
    public Parser(string text){        
        var tokens = new List<SyntaxToken>();
        var lexer = new Lexer(text);
        SyntaxToken token;
            
        do {
            token = lexer.NextToken();

            if(token.Kind != SyntaxKind.WhitespaceToken && 
                token.Kind != SyntaxKind.BadToken)
            {
                tokens.Add(token);
            }

        } while(token.Kind != SyntaxKind.EndOfFileToken);  

        _tokens = tokens.ToArray();
        _diagnostics.AddRange(lexer.Diagnostics);
    }

    public IEnumerable<string> Diagnostics => _diagnostics;

    private SyntaxToken Peek(int offset) {
        var index = _position + offset;
        if(index >= _tokens.Length)
            return _tokens[_position - 1];
            
        return _tokens[index];
    }

    private SyntaxToken Current => Peek(0);

    private SyntaxToken NextToken(){
        var current = Current;
        _position ++;
        return current;
    }

    private SyntaxToken Match(SyntaxKind kind){
        if(Current.Kind == kind)
            return NextToken();
            
        _diagnostics.Add($"Error: Unexpected token <{Current.Kind}>, expected <{kind}>");
        return new SyntaxToken(kind, Current.Position, null, null);
    }
     
    //Return the tokens as a tree
    public SyntaxTree Parse(){
        var expression = ParseTerm();
        var endOfFileToken = Match(SyntaxKind.EndOfFileToken);
        return new SyntaxTree(_diagnostics, expression, endOfFileToken);
    }
    public ExpressionSyntax ParseTerm(){
        //      +
        //     / \  
        //    +   3
        //   / \
        //  1   2            
        var left = ParseFactor();

        while (Current.Kind == SyntaxKind.PlusToken ||
               Current.Kind == SyntaxKind.MinusToken)
        {
            var operatorToken = NextToken();
            var right = ParseFactor();
            left = new BinaryExpressionSyntax(left, operatorToken, right);
        }

        return left;    
    }
    public ExpressionSyntax ParseFactor(){       
        var left = ParsePrimaryExpression();
        while (Current.Kind == SyntaxKind.MultiplyToken ||
               Current.Kind == SyntaxKind.DivideToken)
        {
            var operatorToken = NextToken();
            var right = ParsePrimaryExpression();
            left = new BinaryExpressionSyntax(left, operatorToken, right);
        }
        return left;    
    }

    private ExpressionSyntax ParsePrimaryExpression(){
        if(Current.Kind == SyntaxKind.OpenParenthesisToken){
            var left = NextToken();
            var expression = ParseTerm();
            var right = Match(SyntaxKind.CloseParenthesisToken);
            return new ParenthesizedExpressionSyntax(left, expression, right); 
        }

        var numberToken = Match(SyntaxKind.NumberToken);
        return new NumberExpressionSyntax(numberToken);                
    }
    }
}