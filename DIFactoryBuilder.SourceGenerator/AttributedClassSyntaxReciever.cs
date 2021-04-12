// ***********************************************************************
// Assembly         : DIFactoryBuilder.SourceGenerator
// Author           : CryoM
// Created          : 04-09-2021
//
// Last Modified By : CryoM
// Last Modified On : 04-12-2021
// ***********************************************************************
// <copyright file="AttributedClassSyntaxReciever.cs" company="DIFactoryBuilder.SourceGenerator">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIFactoryBuilder.SourceGenerator
{
    /// <summary>
    /// Class AttributedClassSyntaxReciever.
    /// Implements the <see cref="Microsoft.CodeAnalysis.ISyntaxContextReceiver" />
    /// </summary>
    /// <seealso cref="Microsoft.CodeAnalysis.ISyntaxContextReceiver" />
    public class AttributedClassSyntaxReciever : ISyntaxContextReceiver
    {
        /// <summary>
        /// The attribute name
        /// </summary>
        private readonly string _attributeName;
        /// <summary>
        /// Gets the classes.
        /// </summary>
        /// <value>The classes.</value>
        public IList<INamedTypeSymbol> Classes { get; } = new List<INamedTypeSymbol>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributedClassSyntaxReciever" /> class.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        public AttributedClassSyntaxReciever(string attributeName)
        {
            this._attributeName = attributeName;
        }

        /// <summary>
        /// Called when [visit syntax node].
        /// </summary>
        /// <param name="context">The context.</param>
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
