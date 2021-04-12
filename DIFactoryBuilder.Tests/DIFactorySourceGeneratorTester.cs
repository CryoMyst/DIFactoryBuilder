// ***********************************************************************
// Assembly         : DIFactoryBuilder.Tests
// Author           : CryoM
// Created          : 04-09-2021
//
// Last Modified By : CryoM
// Last Modified On : 04-12-2021
// ***********************************************************************
// <copyright file="DIFactorySourceGeneratorTester.cs" company="DIFactoryBuilder.Tests">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace DIFactoryBuilder.Tests
{
    using System.Reflection;

    using DIFactoryBuilder.SourceGenerator;

    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;

    /// <summary>
    /// Class DIFactorySourceGeneratorTester.
    /// Implements the <see cref="Microsoft.CodeAnalysis.CSharp.Testing.CSharpSourceGeneratorTest{DIFactoryBuilder.SourceGenerator.DIFactoryBuilderSourceGenerator, Microsoft.CodeAnalysis.Testing.Verifiers.XUnitVerifier}" />
    /// </summary>
    /// <seealso cref="Microsoft.CodeAnalysis.CSharp.Testing.CSharpSourceGeneratorTest{DIFactoryBuilder.SourceGenerator.DIFactoryBuilderSourceGenerator, Microsoft.CodeAnalysis.Testing.Verifiers.XUnitVerifier}" />
    public class DIFactorySourceGeneratorTester : CSharpSourceGeneratorTest<DIFactoryBuilderSourceGenerator, XUnitVerifier>
    {
        /// <summary>
        /// The extra required assemblies
        /// </summary>
        public readonly Assembly[] ExtraRequiredAssemblies = new[]
                                                        {
                                                            Assembly.GetAssembly(typeof(IDIFactory<>)),
                                                            Assembly.GetAssembly(typeof(Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions)),
                                                        };

        /// <summary>
        /// Initializes a new instance of the <see cref="DIFactorySourceGeneratorTester"/> class.
        /// </summary>
        public DIFactorySourceGeneratorTester()
        {
            foreach (var extraAssembly in ExtraRequiredAssemblies)
            {
                this.TestState.AdditionalReferences.Add(extraAssembly);
            }
        }
    }
}
