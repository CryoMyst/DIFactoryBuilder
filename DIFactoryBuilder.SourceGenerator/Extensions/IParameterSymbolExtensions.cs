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

        public static string GetExplicitDefaultValueRepresentation(this IParameterSymbol symbol)
        {
            // From: https://github.com/frhagn/Typewriter/blob/master/src/Roslyn/RoslynParameterMetadata.cs
            if (symbol.ExplicitDefaultValue == null)
                return "null";

            var stringValue = symbol.ExplicitDefaultValue as string;
            if (stringValue != null)
                return $"\"{stringValue.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";

            if (symbol.ExplicitDefaultValue is bool)
                return (bool)symbol.ExplicitDefaultValue ? "true" : "false";

            return symbol.ExplicitDefaultValue.ToString();
        }
    }
}
