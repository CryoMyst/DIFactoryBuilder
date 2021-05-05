// ***********************************************************************
// Assembly         : DIFactoryBuilder.Tests
// Author           : CryoM
// Created          : 04-09-2021
//
// Last Modified By : CryoM
// Last Modified On : 04-12-2021
// ***********************************************************************
// <copyright file="SourceGeneratorUnitTests.cs" company="DIFactoryBuilder.Tests">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace DIFactoryBuilder.Tests
{
    using System.Threading.Tasks;

    using DIFactoryBuilder.SourceGenerator;

    using Microsoft.CodeAnalysis.Testing;

    using Xunit;

    /// <summary>
    /// Class SourceGeneratorUnitTests.
    /// </summary>
    public class SourceGeneratorUnitTests
    {
        /// <summary>
        /// Defines the test method TestSourceGeneratorOutput.
        /// </summary>
        [Fact]
        public async Task TestSourceGeneratorOutput()
        {
            string codeFile = @"
using System;
using System.Runtime;
using System.Collections.Generic;
using System.Collections;
using DIFactoryBuilder.Attributes;

namespace MyCode.TestNamespace
{
    [RequiresFactory]
    public class TestViewModel
    {
        public TestViewModel(int regularParam, ICollection<object> genericTypeParam, ICollection<IList<Int16>> genericTypeParam2, [Inject] IEnumerable<double> injectableParam, [RequiredInject] IEnumerable<long> requiredInjectableParam, int paramWithDefault = 3)
        {
            
        }
    }
}
".Trim();

            string generatedCode = @"
using System;
using Microsoft.Extensions.DependencyInjection;

namespace MyCode.TestNamespace
{
    public class TestViewModelFactory : DIFactoryBuilder.IDIFactory<TestViewModel>
    {
        private readonly System.IServiceProvider _serviceProvider;
        public TestViewModelFactory(System.IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        public TestViewModel Create(int regularParam, System.Collections.Generic.ICollection<object> genericTypeParam, System.Collections.Generic.ICollection<System.Collections.Generic.IList<short>> genericTypeParam2, int paramWithDefault = 3)
        {
            return new TestViewModel(regularParam, genericTypeParam, genericTypeParam2, this._serviceProvider.GetService<System.Collections.Generic.IEnumerable<double>>(), this._serviceProvider.GetRequiredService<System.Collections.Generic.IEnumerable<long>>(), paramWithDefault);
        }
    }
}
".Trim();

            await new DIFactorySourceGeneratorTester()
            {
                ReferenceAssemblies = ReferenceAssemblies.Net.Net50,
                TestState =
                {
                    Sources = { codeFile },
                    GeneratedSources =
                    {
                        (typeof(DIFactoryBuilderSourceGenerator), "TestViewModelFactory.cs", generatedCode),
                    },
                },
            }.RunAsync();
        }
    }
}
