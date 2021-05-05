// ***********************************************************************
// Assembly         : DIFactoryBuilder.SourceGenerator
// Author           : CryoM
// Created          : 04-12-2021
//
// Last Modified By : CryoM
// Last Modified On : 05-06-2021
// ***********************************************************************
// <copyright file="ISymbolExtensions.cs" company="DIFactoryBuilder.SourceGenerator">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace DIFactoryBuilder.SourceGenerator.Extensions
{
    using System.Collections.Generic;

    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Class IParameterSymbolExtensions.
    /// </summary>
    static class ISymbolExtensions
    {
        /// <summary>
        /// Determines whether the specified attribute symbol has attribute.
        /// </summary>
        /// <param name="typeSymbol">The type symbol.</param>
        /// <param name="attributeSymbol">The attribute symbol.</param>
        /// <returns><c>true</c> if the specified attribute symbol has attribute; otherwise, <c>false</c>.</returns>
        public static bool HasAttribute(this ISymbol typeSymbol, INamedTypeSymbol attributeSymbol)
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

        /// <summary>
        /// Gets the attribute.
        /// </summary>
        /// <param name="typeSymbol">The type symbol.</param>
        /// <param name="attributeSymbol">The attribute symbol.</param>
        /// <returns>System.Nullable&lt;AttributeData&gt;.</returns>
        public static AttributeData? GetAttribute(this ISymbol typeSymbol, INamedTypeSymbol attributeSymbol)
        {
            foreach (var attribute in typeSymbol.GetAttributes())
            {
                if (attribute.AttributeClass?.Equals(attributeSymbol, SymbolEqualityComparer.Default) == true)
                {
                    return attribute;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the attributes.
        /// </summary>
        /// <param name="typeSymbol">The type symbol.</param>
        /// <param name="attributeSymbol">The attribute symbol.</param>
        /// <returns>IEnumerable&lt;AttributeData&gt;.</returns>
        public static IEnumerable<AttributeData> GetAttributes(this ISymbol typeSymbol, INamedTypeSymbol attributeSymbol)
        {
            foreach (var attribute in typeSymbol.GetAttributes())
            {
                if (attribute.AttributeClass?.Equals(attributeSymbol, SymbolEqualityComparer.Default) == true)
                {
                    yield return attribute;
                }
            }
        }
    }
}
