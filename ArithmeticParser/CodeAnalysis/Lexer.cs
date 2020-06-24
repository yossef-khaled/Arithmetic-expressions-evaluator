using System;
using System.Collections.Generic;
using System.Linq;

namespace Parser.CodeAnalysis{
    class Lexer {
    private readonly string _text;
    private int _position;
    private List<string> _diagnostics = new List<string>();
    public Lexer(String text){
        _text = text;
    }

    public IEnumerable<string> Diagnostics => _diagnostics;

    private char Current{
        get {
            if(_position >= _text.Length){
                return '\0';
            }
            else{
                return _text[_position];
            }
        }
    }

    private void Next(){
        _position ++;
    }
        
    public SyntaxToken NextToken(){
        // numbers
        // + - * / ()
        // <whitespace>
        if(Current == '\0'){
            return new SyntaxToken(SyntaxKind.EndOfFileToken, _position, "\0", null);
        }

        if(char.IsDigit(Current)){

            var start = _position;
            while(char.IsDigit(Current)){
                Next();
            }

            var length = _position - start;
            var text = _text.Substring(start, length);
            if(!int.TryParse(text, out var value)){ 
                _diagnostics.Add($"The number {_text} isn't a valid Int32");
                return new SyntaxToken(SyntaxKind.BadToken, start, text, value);
            }
            else{
                return new SyntaxToken(SyntaxKind.NumberToken, start, text, value);
            }
        }

        if(char.IsWhiteSpace(Current)){

            var start = _position;
            while(char.IsWhiteSpace(Current)){
                Next();
            }

            var length = _position - start;
            var text = _text.Substring(start, length);
            return new SyntaxToken(SyntaxKind.WhitespaceToken, start, text, null);
        }
    
        if(Current == '+'){
            //We can replace the coming two lines with ++ as the following if statements
            var start = _position;
            Next();
            return new SyntaxToken(SyntaxKind.PlusToken, start, "+", null);
        }
            
        if(Current == '-')
            return new SyntaxToken(SyntaxKind.MinusToken, _position++, "-", null);
        else if(Current == '*')
            return new SyntaxToken(SyntaxKind.MultiplyToken, _position++, "*", null); 
        else if(Current == '/')
            return new SyntaxToken(SyntaxKind.DivideToken, _position++, "/", null);
        else if(Current == '(')
            return new SyntaxToken(SyntaxKind.OpenParenthesisToken, _position++, "(", null);
        else if(Current == ')')
            return new SyntaxToken(SyntaxKind.CloseParenthesisToken, _position++, ")", null);
        
        _diagnostics.Add($"Error: bad chracter input: '{Current}'.");
        return new SyntaxToken(SyntaxKind.BadToken, _position++, _text.Substring(_position - 1, 1), null);
            
    }
}
}