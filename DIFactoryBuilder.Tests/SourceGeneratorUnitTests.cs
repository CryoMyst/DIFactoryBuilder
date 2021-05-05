// ***********************************************************************
// Assembly         : DIFactoryBuilder.Tests
// Author           : CryoM
// Created          : 04-09-2021
//
// Last Modified By : CryoM
// Last Modified On : 05-06-2021
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
        /// Defines the test method TestInjectParameter.
        /// </summary>
        [Fact]
        public async Task TestInjectParameter()
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
    public class TestClass
    {
        public TestClass(int regularParam, [Inject] object injectableParam)
        {
            
        }
    }
}
".Trim();

            string expectedOutput = @"
using System;
using Microsoft.Extensions.DependencyInjection;

namespace MyCode.TestNamespace
{
    public class TestClassFactory : DIFactoryBuilder.IDIFactory<TestClass>
    {
        private readonly System.IServiceProvider _serviceProvider;
        public TestClassFactory(System.IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        public TestClass Create(int regularParam)
        {
            return new TestClass(regularParam, this._serviceProvider.GetService<object>());
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
                        (typeof(DIFactoryBuilderSourceGenerator), "TestClassFactory.cs", expectedOutput),
                    },
                },
            }.RunAsync();
        }

        /// <summary>
        /// Defines the test method TestGenericParameter.
        /// </summary>
        [Fact]
        public async Task TestGenericParameter()
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
    public class TestClass
    {
        public TestClass(int regularParam, [Inject] IEnumerable<object> injectableParam)
        {
            
        }
    }
}
".Trim();

            string expectedOutput = @"
using System;
using Microsoft.Extensions.DependencyInjection;

namespace MyCode.TestNamespace
{
    public class TestClassFactory : DIFactoryBuilder.IDIFactory<TestClass>
    {
        private readonly System.IServiceProvider _serviceProvider;
        public TestClassFactory(System.IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        public TestClass Create(int regularParam)
        {
            return new TestClass(regularParam, this._serviceProvider.GetService<System.Collections.Generic.IEnumerable<object>>());
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
                        (typeof(DIFactoryBuilderSourceGenerator), "TestClassFactory.cs", expectedOutput),
                    },
                },
            }.RunAsync();
        }

        /// <summary>
        /// Defines the test method TestRequiredInjectParameter.
        /// </summary>
        [Fact]
        public async Task TestRequiredInjectParameter()
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
    public class TestClass
    {
        public TestClass(int regularParam, [RequiredInject] object injectableParam)
        {
            
        }
    }
}
".Trim();

            string expectedOutput = @"
using System;
using Microsoft.Extensions.DependencyInjection;

namespace MyCode.TestNamespace
{
    public class TestClassFactory : DIFactoryBuilder.IDIFactory<TestClass>
    {
        private readonly System.IServiceProvider _serviceProvider;
        public TestClassFactory(System.IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        public TestClass Create(int regularParam)
        {
            return new TestClass(regularParam, this._serviceProvider.GetRequiredService<object>());
        }
    }
}"
.Trim();

            await new DIFactorySourceGeneratorTester()
            {
                ReferenceAssemblies = ReferenceAssemblies.Net.Net50,
                TestState =
                {
                    Sources = { codeFile },
                    GeneratedSources =
                    {
                        (typeof(DIFactoryBuilderSourceGenerator), "TestClassFactory.cs", expectedOutput),
                    },
                },
            }.RunAsync();
        }

        /// <summary>
        /// Defines the test method TestFactoryMethodNameAttribute.
        /// </summary>
        [Fact]
        public async Task TestFactoryMethodNameAttribute()
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
    public class TestClass
    {
        [FactoryMethodName(""TestClassCreator"")]
        public TestClass(int regularParam, [Inject] object injectableParam)
        {
            
        }
    }
}
".Trim();

            string expectedOutput = @"
using System;
using Microsoft.Extensions.DependencyInjection;

namespace MyCode.TestNamespace
{
    public class TestClassFactory : DIFactoryBuilder.IDIFactory<TestClass>
    {
        private readonly System.IServiceProvider _serviceProvider;
        public TestClassFactory(System.IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        public TestClass TestClassCreator(int regularParam)
        {
            return new TestClass(regularParam, this._serviceProvider.GetService<object>());
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
                        (typeof(DIFactoryBuilderSourceGenerator), "TestClassFactory.cs", expectedOutput),
                    },
                },
            }.RunAsync();
        }
    }
}
