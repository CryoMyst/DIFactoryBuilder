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
        public IList<INamedTypeSymbol> Classes { get; } = new List<INamedTypeSymbol>();

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
                if (context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax) is not INamedTypeSymbol classSymbol)
                {
                    return;
                }

                if (classSymbol.GetAttributes().Any(att => att.AttributeClass?.Equals(attribute, SymbolEqualityComparer.Default) ?? false))
                {
                    this.Classes.Add(classSymbol);
                }

                return;
            }
        }
    }
}
