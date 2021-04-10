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
            const string codeFile = @"
using System.Runtime;
using System.Collections.Generic;
using DIFactoryBuilder.Attributes;
namespace MyCode.TestNamespace
{
    [RequiresFactory]
    public class TestViewModel
    {
        public TestViewModel(int regularParam, ICollection<object> genericTypeParam, [Inject] IEnumerable<double> injectableParam)
        {
            
        }
    }
}
";

            const string generatedCode = @"";

            await new DIFactorySourceGeneratorTester()
            {
                ReferenceAssemblies = ReferenceAssemblies.Net.Net50,
                TestState =
                {
                    Sources = { codeFile },
                    GeneratedSources =
                    {
                        (typeof(DIFactoryBuilderSourceGenerator), "TestViewModel_Factory.cs", generatedCode),
                    },
                },
            }.RunAsync();
        }
    }
}
