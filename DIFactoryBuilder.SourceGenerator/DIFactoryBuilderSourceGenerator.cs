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

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace DIFactoryBuilder.SourceGenerator
{
    [Generator]
    public class DIFactoryBuilderSourceGenerator : ISourceGenerator
    {
        protected const string InjectAttributeName = @"DIFactoryBuilder.Attributes.InjectAttribute";
        protected const string RequiresFactoryAttributeName = @"DIFactoryBuilder.Attributes.RequiresFactoryAttribute";
        protected const string IDIFactoryClassName = @"DIFactoryBuilder.IDIFactory`1";

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new AttributedClassSyntaxReciever(RequiresFactoryAttributeName));
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is not AttributedClassSyntaxReciever syntaxReceiver)
            {
                return;
            }

            var requiresFactoryAttributeSymbol = context.Compilation.GetTypeByMetadataName(RequiresFactoryAttributeName);
            var injectAttributeSymbol = context.Compilation.GetTypeByMetadataName(InjectAttributeName);
            var iDiFactorySymbol = context.Compilation.GetTypeByMetadataName(IDIFactoryClassName);

            if (requiresFactoryAttributeSymbol is null || injectAttributeSymbol is null || iDiFactorySymbol is null)
            {
                return;
            }

            foreach (var injectableClassSymbol in syntaxReceiver.Classes)
            {
                if (injectableClassSymbol.DeclaredAccessibility != Accessibility.Public)
                {
                    continue;
                }

                try
                {
                    var generatedClassName = $"{injectableClassSymbol.Name}Factory";
                    var classSource = ProcessClass(injectableClassSymbol, generatedClassName, injectAttributeSymbol, iDiFactorySymbol);
                    if (classSource is not null)
                    {
                        context.AddSource($"{generatedClassName}.cs", SourceText.From(classSource, Encoding.UTF8));
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }

        private static string? ProcessClass(INamedTypeSymbol classSymbol, string generatedClassName, INamedTypeSymbol injectAttributeSymbol, INamedTypeSymbol uDuFactoryClassSymbol)
        {
            var validConstructors = classSymbol.Constructors
                .Where(c => c.DeclaredAccessibility == Accessibility.Public)
                .Where(c => c.Parameters.Any(p => p.HasAttribute(injectAttributeSymbol)))
                .ToList();

            if (!validConstructors.Any())
            {
                // Class is valid but no public constructors contain an attributed parameter, no generated code is needed
                return null;
            }

            // File compilation unit
            var comilationUnit = CompilationUnit()
                .AddUsings(
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.Extensions.DependencyInjection")));

            // The namespace
            var namespaceDeclaration = NamespaceDeclaration(
                        SyntaxFactory.ParseName(classSymbol.ContainingNamespace.ToDisplayString()));

            // Create the factory class that inherits from 
            var classDeclaration = SyntaxFactory.ClassDeclaration(generatedClassName)
                .AddModifiers(
                    Token(SyntaxKind.PublicKeyword))
                // Use a base of IDIFactory for finding by reflection later
                .WithBaseList(
                    BaseList(
                        SingletonSeparatedList<BaseTypeSyntax>(
                            SimpleBaseType(
                                QualifiedName(
                                    IdentifierName(uDuFactoryClassSymbol.ContainingNamespace.ToDisplayString()),
                                    GenericName(
                                        Identifier(uDuFactoryClassSymbol.Name))
                                    .WithTypeArgumentList(
                                        TypeArgumentList(
                                            SingletonSeparatedList<TypeSyntax>(
                                                IdentifierName(classSymbol.Name)))))))))
                // Contains a private readonly IServiceProvider for finding services
                .AddMembers(
                    FieldDeclaration(
                        VariableDeclaration(
                            ParseTypeName("System.IServiceProvider"))
                        .AddVariables(
                            VariableDeclarator("_serviceProvider")))
                    .AddModifiers(
                        Token(SyntaxKind.PrivateKeyword),
                        Token(SyntaxKind.ReadOnlyKeyword)));

            // Create constructor
            var constructorDeclaration = ConstructorDeclaration(generatedClassName)
                .AddModifiers(
                    Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(
                    Parameter(
                        Identifier("serviceProvider"))
                    .WithType(
                        ParseTypeName("System.IServiceProvider")))
                .AddBodyStatements(
                    ParseStatement(@"this._serviceProvider = serviceProvider;"));

            var factoryMethods = GenerateFactoryMethods(validConstructors, classSymbol, injectAttributeSymbol);

            comilationUnit = comilationUnit
                .AddMembers(
                    namespaceDeclaration
                        .AddMembers(
                            classDeclaration
                            .AddMembers(factoryMethods)
                            .AddMembers(constructorDeclaration)));
            
            var classString = comilationUnit.NormalizeWhitespace().ToFullString();
            return classString;
        }

        private static MemberDeclarationSyntax[] GenerateFactoryMethods(List<IMethodSymbol> validConstructors, INamedTypeSymbol classSymbol, INamedTypeSymbol injectAttributeSymbol)
        {
            var methods = new List<MethodDeclarationSyntax>();

            foreach (var validConstructor in validConstructors)
            {
                var method = GenerateFactoryMethod(validConstructor, classSymbol, injectAttributeSymbol);
                methods.Add(method);
            }

            return methods.OfType<MemberDeclarationSyntax>().ToArray();
        }

        private static MethodDeclarationSyntax GenerateFactoryMethod(IMethodSymbol validConstructor, INamedTypeSymbol classSymbol, INamedTypeSymbol injectAttributeSymbol)
        {
            var factoryMethodParameters = new List<ParameterSyntax>();
            var constructorArguments = new List<ArgumentSyntax>();

            foreach (var parmeter in validConstructor.Parameters)
            {
                if (parmeter.HasAttribute(injectAttributeSymbol))
                {
                    var argument = Argument(
                        InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    ThisExpression(),
                                    IdentifierName("_serviceProvider")),
                                GenericName(
                                    Identifier("GetService"))
                                .WithTypeArgumentList(
                                    TypeArgumentList(
                                        SingletonSeparatedList(
                                            ParseTypeName(parmeter.Type.ToDisplayString())))))));
                    constructorArguments.Add(argument);
                }
                else
                {
                    // If it does not have inject we need to pass it through the factory
                    // The following is the easiest way of copying the parameter syntax up to the factory
                    var constructorParamSyntax = (parmeter.DeclaringSyntaxReferences.First().GetSyntax() as ParameterSyntax) ?? throw new InvalidOperationException();

                    var paramSyntax = Parameter(
                        constructorParamSyntax.Identifier)
                        .WithDefault(constructorParamSyntax.Default)
                        .WithModifiers(constructorParamSyntax.Modifiers)
                        .WithAttributeLists(constructorParamSyntax.AttributeLists)
                        .WithType(
                            ParseTypeName(parmeter.Type.ToDisplayString()));

                    factoryMethodParameters.Add(paramSyntax);

                    // Add the argument to the constructor
                    var argument = Argument(
                        IdentifierName(parmeter.Name));
                    constructorArguments.Add(argument);
                }
            }

            var body = ReturnStatement(
                ObjectCreationExpression(
                    ParseTypeName(classSymbol.Name))
                .AddArgumentListArguments(constructorArguments.ToArray()));

            var methodDeclaration = MethodDeclaration(
                    ParseTypeName(classSymbol.Name),
                    Identifier("Create"))
                .AddModifiers(
                    Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(factoryMethodParameters.ToArray())
                .WithBody(
                    Block(body));

            return methodDeclaration;
        }
    }
}
