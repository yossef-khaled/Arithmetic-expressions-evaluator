using System;
using System.Collections.Generic;
using System.Linq;

namespace Parser
{
    // 1 + 2 + 3
    // the tree will look like :
    //      +
    //     / \
    //    +   3
    //   / \
    //  1   2
    //


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
                    
                    Console.Write($"{token.Kind}: '{token.Text}' ");
                    if(token.Value != null)
                        Console.WriteLine($" {token.Value}");

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
        EndOfFileToken,
        NumberExpression,
        BinaryExpression
    }

    class SyntaxToken : SyntaxNode{

        public SyntaxToken(SyntaxKind kind, int position, string text, object value){
            Kind = kind;
            Position = position;
            Text = text;
            Value = value;
        }

        public override SyntaxKind Kind {get ;}
        public int Position {get ;}
        public string Text{get ;}
        public object Value{get ;}

        public override IEnumerable<SyntaxNode> GetChildren(){
            return Enumerable.Empty<SyntaxNode>();
        }
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
                var text = _text.Substring(start, length);
                int.TryParse(text, out var value);
                return new SyntaxToken(SyntaxKind.NumberToken, start, text, value);
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
            
            return new SyntaxToken(SyntaxKind.BadToken, _position++, _text.Substring(_position - 1, 1), null);
        }
    }
    
    abstract class SyntaxNode {
        public abstract SyntaxKind Kind {get;}
        public abstract IEnumerable<SyntaxNode> GetChildren();
    }

    abstract class ExpressionSyntax : SyntaxNode {

    }

    sealed class NumberExpressionSyntax : ExpressionSyntax {
        public NumberExpressionSyntax(SyntaxToken numberToken){
            NumberToken = numberToken;
        }

        public override SyntaxKind Kind => SyntaxKind.NumberExpression;
        public SyntaxToken NumberToken {get;} 

        public override IEnumerable<SyntaxNode> GetChildren(){
            yield return NumberToken;
        }
    }

    //For operators
    sealed class BinaryExpressionSyntax : ExpressionSyntax {
        public BinaryExpressionSyntax(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right){
            Left = left;
            OperatorToken = operatorToken;
            Right = right;
        }

        public override SyntaxKind Kind => SyntaxKind.BinaryExpression;
        public ExpressionSyntax Left {get;}
        public SyntaxToken OperatorToken {get;}
        public ExpressionSyntax Right {get;}

        public override IEnumerable<SyntaxNode> GetChildren(){
            yield return Left;
            yield return OperatorToken;
            yield return Right;

        }
        
    }

    class Parser {

        private readonly SyntaxToken[] _tokens;
        private int _position;
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
        }

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

        private SyntaxToken Match(SyntaxKind Kind){
            if(Current.Kind == Kind)
                return NextToken();
            
            return new SyntaxToken(Kind, Current.Position, null, null);
        }

        public ExpressionSyntax Parse(){

        //      +
        //     / \
        //    +   3
        //   / \
        //  1   2
            
            var left = ParsePrimaryExpression();

            while (Current.Kind == SyntaxKind.PlusToken ||
                   Current.Kind == SyntaxKind.MinusToken)
            {
                var operatorToken = NextToken();
                var right = ParsePrimaryExpression();
                left = new BinaryExpressionSyntax(left, operatorToken, right);
            }

            return left;    
        }

        private ExpressionSyntax ParsePrimaryExpression(){
            var numberToken = Match(SyntaxKind.NumberToken);
            return new NumberExpressionSyntax(numberToken);                
        }
    }


}
