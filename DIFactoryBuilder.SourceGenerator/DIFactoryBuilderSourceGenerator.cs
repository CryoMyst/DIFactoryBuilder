using DIFactoryBuilder.Attributes;
using DIFactoryBuilder.SourceGenerator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DIFactoryBuilder.SourceGenerator
{
    [Generator]
    public class DIFactoryBuilderSourceGenerator : ISourceGenerator
    {
        protected const string InjectAttributeName = @"DIFactoryBuilder.Attributes.InjectAttribute";
        protected const string RequiresFactoryAttributeName = @"DIFactoryBuilder.Attributes.RequiresFactory";
        protected const string IDIFactoryClassName = @"DIFactoryBuilder.IDIFactory";


        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new FilterSyntaxTypeReceiver<ClassDeclarationSyntax>());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is FilterSyntaxTypeReceiver<ClassDeclarationSyntax> syntaxReceiver)
            {
                var requiresFactoryAttributeSymbol = context.Compilation.GetTypeByMetadataName(typeof(RequiresFactoryAttribute).FullName);

                foreach (var @class in syntaxReceiver.CandidateSyntaxes)
                {
                    var model = context.Compilation.GetSemanticModel(@class.SyntaxTree, true);
                    var classTypeSymbol = model.GetDeclaredSymbol(@class) as INamedTypeSymbol;

                    if (classTypeSymbol.DeclaredAccessibility != Accessibility.Public)
                    {
                        continue;
                    }

                    if (classTypeSymbol?.HasAttribute(requiresFactoryAttributeSymbol) ?? false)
                    {
                        try
                        {
                            GenerateFactory(context, classTypeSymbol);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e);
                        }
                        
                    }
                }
            }
        }

        public void GenerateFactory(GeneratorExecutionContext context, INamedTypeSymbol classTypeSymbol)
        {
            var injectAttributeSymbol = context.Compilation.GetTypeByMetadataName(typeof(InjectAttribute).FullName);

            // Only get constructors that need injected parameters
            var validConstructors = classTypeSymbol.Constructors
                .Where(c => c.DeclaredAccessibility == Accessibility.Public)
                .Where(c => c.Parameters.Any(p => p.HasAttribute(injectAttributeSymbol)))
                .ToList();

            if (validConstructors.Any())
            {
                var source = GenerateFactorySource(context, classTypeSymbol, validConstructors);
                context.AddSource($"{classTypeSymbol.Name}_Factory", source);
            }
        }

        private string GenerateFactorySource(GeneratorExecutionContext context, INamedTypeSymbol classSymbol, IEnumerable<IMethodSymbol> constructors)
        {
            var useMicrosoftDiExtensions = context.Compilation.GetTypeByMetadataName("Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions") is not null;
            // No support for generics due to difficulty
            // var generics = GetGenericArguments(classSymbol);

            var sb = new IndentStringBuilder();
            if (useMicrosoftDiExtensions)
            {
                sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
                sb.AppendLine();
            }

            sb.AppendLine($@"namespace {classSymbol.ContainingNamespace}");
            sb.AppendLine($@"{{");
            using (var _namespaceIndent = sb.IndentScope)
            {
                sb.AppendLine($@"public partial class {classSymbol.Name}Factory : {(typeof(IDIFactory<>).Namespace)}.IDIFactory<{classSymbol.Name}>");
                sb.AppendLine($@"{{");
                using (var _classIndent = sb.IndentScope)
                {
                    sb.AppendLine("private readonly System.IServiceProvider _serviceProvider;");
                    sb.AppendLine();

                    sb.AppendLine($"public {classSymbol.Name}Factory(System.IServiceProvider serviceProvider)");
                    sb.AppendLine("{");
                    using (var _constructorIndent = sb.IndentScope)
                    {
                        sb.AppendLine("this._serviceProvider = serviceProvider;");
                    }
                    sb.AppendLine("}");
                    sb.AppendLine();

                    foreach (var constructor in constructors)
                    {
                        GenerateConstructorFactoryMethod(context, constructor, classSymbol, sb, useMicrosoftDiExtensions);
                    }
                    sb.AppendLine();
                }
                sb.AppendLine($@"}}");
            }
            sb.AppendLine($@"}}");

            return sb.ToString();
        }

        private void GenerateConstructorFactoryMethod(GeneratorExecutionContext context, IMethodSymbol constructor, INamedTypeSymbol typeSymbol, IndentStringBuilder sb, bool useMicrosoftDi)
        {
            var injectAttributeSymbol = context.Compilation.GetTypeByMetadataName(typeof(InjectAttribute).FullName);

            // Generate method signature
            sb.Append($@"public {typeSymbol.Name} Create(");
            var parameters = constructor.Parameters;

            var passthroughParameters = parameters
                .Where(p => !p.HasAttribute(injectAttributeSymbol))
                .ToList();

            foreach (var param in passthroughParameters)
            {
                var name = param.Name.Trim();

                sb.AppendNoIndent(param.Type?.ToString()).AppendNoIndent(" ").AppendNoIndent(name);

                if (param.HasExplicitDefaultValue)
                {
                    sb.AppendNoIndent(" = ").AppendNoIndent(param.GetExplicitDefaultValueRepresentation()).AppendNoIndent(" ");
                }

                if (!SymbolEqualityComparer.Default.Equals(param, passthroughParameters.Last()))
                {
                    sb.AppendNoIndent(", ");
                }
            }
            sb.AppendNoIndent($@")").AppendLine();
            sb.AppendLine($@"{{");
            using (var factoryMethodScope = sb.IndentScope)
            {
                sb.AppendLine($"return new {typeSymbol.Name}(");
                using (var constructorParamScope = sb.IndentScope)
                {
                    foreach (var param in parameters)
                    {
                        if (param.HasAttribute(injectAttributeSymbol))
                        {
                            if (useMicrosoftDi)
                            {
                                sb.Append($"this._serviceProvider.GetService<{param.Type}>()");
                            }
                            else
                            {
                                sb.Append($"({param.Type}) this._serviceProvider.GetService(typeof({param.Type}))");
                            }
                        }
                        else
                        {
                            sb.Append($"{param.Name}");
                        }

                        if (!SymbolEqualityComparer.Default.Equals(param, parameters.Last()))
                        {
                            sb.AppendNoIndent(", ")
                                .AppendLine();
                        }
                    }
                }

                sb.AppendLine();
                sb.AppendLine(");");
            }
            sb.AppendLine();
            sb.AppendLine($@"}}");
        }


    }
}
