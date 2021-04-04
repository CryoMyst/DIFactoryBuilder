using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIFactoryBuilder.SourceGenerator.Extensions
{
    public static class TypeSymbolExtensions
    {
        public static bool EqualsType(this ITypeSymbol current, ITypeSymbol other)
        {
            return current.Name == other.Name
                && SymbolEqualityComparer.Default.Equals(current.ContainingNamespace, other.ContainingNamespace)
                && SymbolEqualityComparer.Default.Equals(current.ContainingAssembly, other.ContainingAssembly);
        }
    }
}
