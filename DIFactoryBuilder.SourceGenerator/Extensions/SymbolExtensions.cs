using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIFactoryBuilder.SourceGenerator.Extensions
{
    public static class SymbolExtensions
    {
        public static bool HasAttribute(this ISymbol typeSymbol, INamedTypeSymbol attributeSymbol)
        {
            foreach (var attribute in typeSymbol.GetAttributes())
            {
                if (attribute.AttributeClass?.EqualsType(attributeSymbol) == true)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
