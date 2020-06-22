using System;
using System.Collections.Generic;
using System.Linq;

namespace Parser
{
    // the tree from the parse function will look like :
    // (1 + 2) * 3
    //            (
    //         /     \
    //        +       )
    //       / \     /
    //      1   2   *
    //             / 
    //            3
    //_____________________________________________________ or :
    //
    // 1 + 2 + 3
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

                var parser = new Parser(line);
                var syntaxTree = parser.Parse();
                var color = Console.ForegroundColor;
                PrettyPrint(syntaxTree.Root);

                if(!parser.Diagnostics.Any()){
                    var e = new Evaluator(syntaxTree.Root);
                    float result = e.Evaluate();
                    Console.WriteLine(result);
                }
                else{
                    Console.ForegroundColor = ConsoleColor.Red;
                    foreach(var diagnostic in parser.Diagnostics){
                        Console.Write(diagnostic);
                    }

                    Console.ForegroundColor = color;
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

        static void PrettyPrint(SyntaxNode node, string indent = ""){
            Console.Write(indent);
            Console.Write(node.Kind);

            if(node is SyntaxToken t && t.Value != null){
                Console.Write(" ");
                Console.Write(t.Value);
            }

            Console.WriteLine();

            indent += "    ";

            foreach(var child in node.GetChildren())
                PrettyPrint(child, indent);
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
        BinaryExpression,
        ParenthesizedExpression
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


    sealed class ParenthesizedExpressionSyntax : ExpressionSyntax {
        public ParenthesizedExpressionSyntax(SyntaxToken openParenthesisToken, ExpressionSyntax expression, SyntaxToken closeParenthesisToken){
            OpenParenthesisToken = openParenthesisToken;
            Expression = expression;
            CloseParenthesisToken = closeParenthesisToken;
        }

        public SyntaxToken OpenParenthesisToken {get;}
        public ExpressionSyntax Expression {get;}
        public SyntaxToken CloseParenthesisToken {get;}

        public override SyntaxKind Kind => SyntaxKind.ParenthesizedExpression;
        
        public override IEnumerable<SyntaxNode> GetChildren(){
            yield return OpenParenthesisToken;
            yield return Expression;
            yield return CloseParenthesisToken;
        } 
    }
    sealed class SyntaxTree{
        public SyntaxTree(IEnumerable<string> diagnostocs, ExpressionSyntax root, SyntaxToken endOfFileToken){
            Diagnostics = diagnostocs.ToArray();
            Root = root;
            EndOfFileToken = endOfFileToken;
        }
        public IReadOnlyList<string> Diagnostics {get;}
        public ExpressionSyntax Root {get;}
        public SyntaxToken EndOfFileToken {get;}
    }

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
            var left = ParseOperation();

            while (Current.Kind == SyntaxKind.PlusToken ||
                   Current.Kind == SyntaxKind.MinusToken)
            {
                var operatorToken = NextToken();
                var right = ParseOperation();
                left = new BinaryExpressionSyntax(left, operatorToken, right);
            }

            return left;    
        }
        public ExpressionSyntax ParseOperation(){       
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
    
    class Evaluator {

        private readonly ExpressionSyntax _root;
        public Evaluator(ExpressionSyntax root){
            this._root = root;
        }

        public float Evaluate(){
            return EvaluateExpression(_root); 
        }

        private float EvaluateExpression(ExpressionSyntax node){
            // We have (until now):
            // BinaryExpresion, NumberExpression

            if(node is NumberExpressionSyntax n){
                return(int) n.NumberToken.Value;
            }

            if(node is BinaryExpressionSyntax b){
                var left = EvaluateExpression(b.Left);
                var right = EvaluateExpression(b.Right);
                
                if(b.OperatorToken.Kind == SyntaxKind.PlusToken){
                    return left + right;
                }
                else if(b.OperatorToken.Kind == SyntaxKind.MinusToken){
                    return left - right;
                }
                else if(b.OperatorToken.Kind == SyntaxKind.MultiplyToken){
                    return left * right;
                }
                else if(b.OperatorToken.Kind == SyntaxKind.DivideToken){
                    return left / right;
                }
                else
                    throw new Exception($"Unexpected binary operator: {b.OperatorToken.Kind}");
            }

            if(node is ParenthesizedExpressionSyntax p){
                return EvaluateExpression(p.Expression);
            }
            throw new Exception($"Unexpected node: {node.Kind}");
        }
    }
}
