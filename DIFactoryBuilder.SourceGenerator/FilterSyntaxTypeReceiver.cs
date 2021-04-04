using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIFactoryBuilder.SourceGenerator
{
    public class FilterSyntaxTypeReceiver<TSyntax> : ISyntaxReceiver
        where TSyntax : SyntaxNode
    {
        public IList<TSyntax> CandidateSyntaxes { get; } = new List<TSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is TSyntax filteredSyntax)
            {
                this.CandidateSyntaxes.Add(filteredSyntax);
            }
        }
    }
}
