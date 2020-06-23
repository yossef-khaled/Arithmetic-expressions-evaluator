using System;
using System.Collections.Generic;
using System.Linq;
using Parser.CodeAnalysis;

namespace ArithmeticParser
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


}
