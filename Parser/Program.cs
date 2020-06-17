using System;

namespace Parser
{
    class Program
    {
        static void Main(string[] args)
        {
            while(true){
                Console.Write("> ");
                var line = Console.ReadLine();
                if(string.IsNullOrWhiteSpace(line)){
                    return;
                }

                var lexer = new Lexer(line);
                while(true){
                    var token = lexer.NextToken();
                    if(token.Kind == SyntaxKind.EndOfFileToken)
                        break;
                    
                    Console.Write($"{token.Kind}: {token.Text}");
                    if(token.Value != null)
                        Console.WriteLine($"{token.Value}");

                    Console.WriteLine();
                } 
            }
        }
    }

    enum SyntaxKind{
        NumberToken,
        WhitespaceToken,
        PlusToken,
        MinusToken,
        MultiplyToken,
        DivideToken,
        OpenParenthesisToken,
        CloseParenthesisToken,
        BadToken,
        EndOfFileToken
    }

    class SyntaxToken{

        public SyntaxToken(SyntaxKind kind, int position, string text, object value){
            Kind = kind;
            Position = position;
            Text = text;
            Value = value;
        }

        public SyntaxKind Kind {get ;}
        public int Position {get ;}
        public string Text{get ;}
        public object Value{get ;}
    }

    class Lexer {

        private readonly string _text;
        private int _position;
        public Lexer(String text){
            _text = text;
        }

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
                var text = _text.Substring(start, _position);
                int.TryParse(text, out var value);
                return new SyntaxToken(SyntaxKind.NumberToken, start, text, value);
            }

            if(char.IsWhiteSpace(Current)){

                var start = _position;
                while(char.IsWhiteSpace(Current)){
                    Next();
                }

                var length = _position - start;
                var text = _text.Substring(start, _position);
                int.TryParse(text, out var value);
                return new SyntaxToken(SyntaxKind.WhitespaceToken, start, text, value);
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
                return new SyntaxToken(SyntaxKind.MultiplyToken, _position++, "-", null); 
            else if(Current == '/')
                return new SyntaxToken(SyntaxKind.DivideToken, _position++, "-", null);
            else if(Current == '(')
                return new SyntaxToken(SyntaxKind.OpenParenthesisToken, _position++, "-", null);
            else if(Current == ')')
                return new SyntaxToken(SyntaxKind.CloseParenthesisToken, _position++, "-", null);
            
            return new SyntaxToken(SyntaxKind.BadToken, _position++, _text.Substring(_position - 1, 1), null);
        }
    }
}
