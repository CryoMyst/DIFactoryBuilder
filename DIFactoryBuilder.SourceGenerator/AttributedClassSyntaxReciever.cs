using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIFactoryBuilder.SourceGenerator
{
    public class AttributedClassSyntaxReciever : ISyntaxContextReceiver
    {
        private readonly string _attributeName;
        public IList<ClassDeclarationSyntax> Classes { get; } = new List<ClassDeclarationSyntax>();

        public AttributedClassSyntaxReciever(string attributeName)
        {
            this._attributeName = attributeName;
        }

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            var model = context.SemanticModel;
            var attribute = model.Compilation.GetTypeByMetadataName(_attributeName);
            if (attribute is null)
            {
                return;
            }

            if (context.Node is ClassDeclarationSyntax classDeclarationSyntax
                && classDeclarationSyntax.AttributeLists.Count > 0)
            {
                foreach (var attributeList in classDeclarationSyntax.AttributeLists)
                {

                }

                classDeclarationSyntax.AttributeLists.Any(als => als.Attributes.Any(ad => ad.Class?.Equals(attribute, SymbolEqualityComparer.Default) == true));
            }
        }
    }
}
