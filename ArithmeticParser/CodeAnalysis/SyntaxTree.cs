using System.Collections.Generic;
using System.Linq;

namespace Parser.CodeAnalysis{
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
}