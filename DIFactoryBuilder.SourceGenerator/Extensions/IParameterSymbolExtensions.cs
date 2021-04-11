using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

namespace DIFactoryBuilder.SourceGenerator.Extensions
{
    public static class IParameterSymbolExtensions
    {
        public static bool HasAttribute(this IParameterSymbol typeSymbol, INamedTypeSymbol attributeSymbol)
        {
            foreach (var attribute in typeSymbol.GetAttributes())
            {
                if (attribute.AttributeClass?.Equals(attributeSymbol, SymbolEqualityComparer.Default) == true)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
