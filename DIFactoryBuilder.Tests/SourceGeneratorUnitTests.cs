using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using DIFactoryBuilder.SourceGenerator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

using Xunit;

namespace DIFactoryBuilder.Tests
{
    public class SourceGeneratorUnitTests
    {
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
        public TestViewModel(int regularParam, ICollection<object> genericTypeParam, ICollection<IList<Int16>> genericTypeParam2, [Inject] IEnumerable<double> injectableParam, int paramWithDefault = 3)
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
        public TestViewModel Create(int regularParam, System.Collections.Generic.ICollection<object> genericTypeParam, System.Collections.Generic.ICollection<System.Collections.Generic.IList<short>> genericTypeParam2, int paramWithDefault = 3)
        {
            return new TestViewModel(regularParam, genericTypeParam, genericTypeParam2, this._serviceProvider.GetService<System.Collections.Generic.IEnumerable<double>>(), paramWithDefault);
        }

        public TestViewModelFactory(System.IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
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
