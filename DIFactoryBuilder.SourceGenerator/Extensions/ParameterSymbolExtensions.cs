using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIFactoryBuilder.SourceGenerator.Extensions
{
    public static class ParameterSymbolExtensions
    {
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
