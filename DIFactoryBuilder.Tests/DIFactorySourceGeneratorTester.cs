using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using DIFactoryBuilder.SourceGenerator;

using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace DIFactoryBuilder.Tests
{
    public class DIFactorySourceGeneratorTester : CSharpSourceGeneratorTest<DIFactoryBuilderSourceGenerator, XUnitVerifier>
    {
        public readonly Assembly[] ExtraRequiredAssemblies = new[]
                                                        {
                                                            Assembly.GetAssembly(typeof(IDIFactory<>)),
                                                            Assembly.GetAssembly(typeof(Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions)),
                                                        };

        public DIFactorySourceGeneratorTester()
        {
            foreach (var extraAssembly in ExtraRequiredAssemblies)
            {
                this.TestState.AdditionalReferences.Add(extraAssembly);
            }
        }
    }
}
