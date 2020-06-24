using System;
using System.Collections.Generic;
using System.Linq;

namespace Parser.CodeAnalysis 
{    
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
            // BinaryExpresion, NumberExpression, Parentheses

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