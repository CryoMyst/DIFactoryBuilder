// ***********************************************************************
// Assembly         : DIFactoryBuilder.SourceGenerator
// Author           : CryoM
// Created          : 04-12-2021
//
// Last Modified By : CryoM
// Last Modified On : 04-12-2021
// ***********************************************************************
// <copyright file="IParameterSymbolExtensions.cs" company="DIFactoryBuilder.SourceGenerator">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace DIFactoryBuilder.SourceGenerator.Extensions
{
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Class IParameterSymbolExtensions.
    /// </summary>
    public static class IParameterSymbolExtensions
    {
        /// <summary>
        /// Determines whether the specified attribute symbol has attribute.
        /// </summary>
        /// <param name="typeSymbol">The type symbol.</param>
        /// <param name="attributeSymbol">The attribute symbol.</param>
        /// <returns><c>true</c> if the specified attribute symbol has attribute; otherwise, <c>false</c>.</returns>
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
