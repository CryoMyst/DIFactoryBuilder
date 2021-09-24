// ***********************************************************************
// Assembly         : DIFactoryBuilder.SourceGenerator
// Author           : CryoM
// Created          : 04-09-2021
//
// Last Modified By : CryoM
// Last Modified On : 05-06-2021
// ***********************************************************************
// <copyright file="DIFactoryBuilderSourceGenerator.cs" company="DIFactoryBuilder.SourceGenerator">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using DIFactoryBuilder.SourceGenerator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace DIFactoryBuilder.SourceGenerator
{
    /// <summary>
    /// Class DIFactoryBuilderSourceGenerator.
    /// Implements the <see cref="Microsoft.CodeAnalysis.ISourceGenerator" />
    /// </summary>
    /// <seealso cref="Microsoft.CodeAnalysis.ISourceGenerator" />
    [Generator]
    public class DIFactoryBuilderSourceGenerator : ISourceGenerator
    {
        /// <summary>
        /// The inject attribute name
        /// </summary>
        protected const string InjectAttributeName = @"DIFactoryBuilder.Attributes.InjectAttribute";
        /// <summary>
        /// The required inject attribute name
        /// </summary>
        protected const string RequiredInjectAttributeName = @"DIFactoryBuilder.Attributes.RequiredInjectAttribute";
        /// <summary>
        /// The requires factory attribute name
        /// </summary>
        protected const string RequiresFactoryAttributeName = @"DIFactoryBuilder.Attributes.RequiresFactoryAttribute";
        /// <summary>
        /// The factory method name attribute name
        /// </summary>
        protected const string FactoryMethodNameAttributeName = @"DIFactoryBuilder.Attributes.FactoryMethodNameAttribute";
        /// <summary>
        /// The idi factory class name
        /// </summary>
        protected const string IDIFactoryClassName = @"DIFactoryBuilder.IDIFactory`1";

        /// <summary>
        /// Called before generation occurs. A generator can use the <paramref name="context" />
        /// to register callbacks required to perform generation.
        /// </summary>
        /// <param name="context">The <see cref="T:Microsoft.CodeAnalysis.GeneratorInitializationContext" /> to register callbacks on</param>
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new AttributedClassSyntaxReciever(RequiresFactoryAttributeName));
        }

        /// <summary>
        /// Called to perform source generation. A generator can use the <paramref name="context" />
        /// to add source files via the <see cref="M:Microsoft.CodeAnalysis.GeneratorExecutionContext.AddSource(System.String,Microsoft.CodeAnalysis.Text.SourceText)" />
        /// method.
        /// </summary>
        /// <param name="context">The <see cref="T:Microsoft.CodeAnalysis.GeneratorExecutionContext" /> to add source to</param>
        /// <remarks>This call represents the main generation step. It is called after a <see cref="T:Microsoft.CodeAnalysis.Compilation" /> is
        /// created that contains the user written code.
        /// A generator can use the <see cref="P:Microsoft.CodeAnalysis.GeneratorExecutionContext.Compilation" /> property to
        /// discover information about the users compilation and make decisions on what source to
        /// provide.</remarks>
        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is not AttributedClassSyntaxReciever syntaxReceiver)
            {
                return;
            }

            var requiresFactoryAttributeSymbol = context.Compilation.GetTypeByMetadataName(RequiresFactoryAttributeName);
            var injectAttributeSymbol = context.Compilation.GetTypeByMetadataName(InjectAttributeName);
            var requiredInjectAttributeSymbol = context.Compilation.GetTypeByMetadataName(RequiredInjectAttributeName);
            var iDiFactorySymbol = context.Compilation.GetTypeByMetadataName(IDIFactoryClassName);
            var factoryMethodNameAttributeSymbol = context.Compilation.GetTypeByMetadataName(FactoryMethodNameAttributeName);

            if (requiresFactoryAttributeSymbol is null 
                || injectAttributeSymbol is null
                || requiredInjectAttributeSymbol is null
                || iDiFactorySymbol is null
                || factoryMethodNameAttributeSymbol is null)
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
                    var classSource = ProcessClass(
                        injectableClassSymbol, 
                        generatedClassName, 
                        injectAttributeSymbol, 
                        requiredInjectAttributeSymbol, 
                        iDiFactorySymbol,
                        factoryMethodNameAttributeSymbol);

                    if (classSource is not null)
                    {
                        context.AddSource($"{generatedClassName}.cs", SourceText.From(classSource, Encoding.UTF8));
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    throw;
                }
            }
        }

        /// <summary>
        /// Processes the class.
        /// </summary>
        /// <param name="classSymbol">The class symbol.</param>
        /// <param name="generatedClassName">Name of the generated class.</param>
        /// <param name="injectAttributeSymbol">The inject attribute symbol.</param>
        /// <param name="requiredInjectAttributeSymbol">The required inject attribute symbol.</param>
        /// <param name="iDiFactoryClassSymbol">The u du factory class symbol.</param>
        /// <param name="factoryMethodNameAttributeSymbol">The factory method name attribute symbol.</param>
        /// <returns>System.Nullable&lt;System.String&gt;.</returns>
        private static string? ProcessClass(
            INamedTypeSymbol classSymbol, 
            string generatedClassName, 
            INamedTypeSymbol injectAttributeSymbol, 
            INamedTypeSymbol requiredInjectAttributeSymbol, 
            INamedTypeSymbol iDiFactoryClassSymbol,
            INamedTypeSymbol factoryMethodNameAttributeSymbol)
        {
            // Valid properties with an Inject attribute
            var validProperties = classSymbol.GetMembers()
                .Where(m => m.DeclaredAccessibility == Accessibility.Public)
                .Where(m => m.Kind == SymbolKind.Property)
                .OfType<IPropertySymbol>()
                .Where(p => p.SetMethod?.DeclaredAccessibility == Accessibility.Public) // With a public setter
                .Where(m => m.HasAttribute(injectAttributeSymbol) || m.HasAttribute(requiredInjectAttributeSymbol))
                .ToList();

            List<IMethodSymbol> validConstructors = null!;
            if (validProperties.Any())
            {
                // If ANY properties are injected then all constructors need factory methods
                validConstructors = classSymbol.Constructors
                    .Where(c => c.DeclaredAccessibility == Accessibility.Public)
                    .ToList();
            }
            else
            {
                // Valid constructors that have attributes parameters
                validConstructors = classSymbol.Constructors
                    .Where(c => c.DeclaredAccessibility == Accessibility.Public)
                    .Where(c => 
                        c.Parameters.Any(p => p.HasAttribute(injectAttributeSymbol) || p.HasAttribute(requiredInjectAttributeSymbol)))
                    .ToList();
            }

            if (!validConstructors.Any() && !validProperties.Any())
            {
                // Class is valid but no public constructors or parameters contain an attributed parameter, no generated code is needed
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
                                    IdentifierName(iDiFactoryClassSymbol.ContainingNamespace.ToDisplayString()),
                                    GenericName(
                                        Identifier(iDiFactoryClassSymbol.Name))
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

            var factoryMethods = GenerateFactoryMethods(
                validConstructors, 
                validProperties,
                classSymbol, 
                injectAttributeSymbol, 
                requiredInjectAttributeSymbol,
                factoryMethodNameAttributeSymbol);

            comilationUnit = comilationUnit
                .AddMembers(
                    namespaceDeclaration
                        .AddMembers(
                            classDeclaration
                                .AddMembers(constructorDeclaration)
                                .AddMembers(factoryMethods)));

            var classString = comilationUnit.NormalizeWhitespace(eol: Environment.NewLine).ToFullString();
            return classString;
        }

        /// <summary>
        /// Generates the factory methods.
        /// </summary>
        /// <param name="validConstructors">The valid constructors.</param>
        /// <param name="classSymbol">The class symbol.</param>
        /// <param name="injectAttributeSymbol">The inject attribute symbol.</param>
        /// <param name="requiredInjectAttributeSymbol">The required inject attribute symbol.</param>
        /// <param name="factoryMethodNameAttributeSymbol">The factory method name attribute symbol.</param>
        /// <returns>MemberDeclarationSyntax[].</returns>
        private static MemberDeclarationSyntax[] GenerateFactoryMethods(
            List<IMethodSymbol> validConstructors,
            List<IPropertySymbol> validProperties,
            INamedTypeSymbol classSymbol, 
            INamedTypeSymbol injectAttributeSymbol, 
            INamedTypeSymbol requiredInjectAttributeSymbol,
            INamedTypeSymbol factoryMethodNameAttributeSymbol)
        {
            var methodCount = validConstructors.Count
                + (validProperties.Any() ? 0 : 1);
            var methods = new List<MethodDeclarationSyntax>(methodCount);

            // Constructor creation for each valid constructor
            // Default constructor is used even if not defined in the class
            foreach (var validConstructor in validConstructors)
            {
                var method = GenerateFactoryMethod(
                    validConstructor,
                    validProperties,
                    classSymbol, 
                    injectAttributeSymbol, 
                    requiredInjectAttributeSymbol,
                    factoryMethodNameAttributeSymbol);
                methods.Add(method);
            }

            return methods.OfType<MemberDeclarationSyntax>().ToArray();
        }

        /// <summary>
        /// Generates the factory method.
        /// </summary>
        /// <param name="validConstructor">The valid constructor.</param>
        /// <param name="validProperties"></param>
        /// <param name="classSymbol">The class symbol.</param>
        /// <param name="injectAttributeSymbol">The inject attribute symbol.</param>
        /// <param name="requiredInjectAttributeSymbol">The required inject attribute symbol.</param>
        /// <param name="factoryMethodNameAttributeSymbol">The factory method name attribute symbol.</param>
        /// <returns>MethodDeclarationSyntax.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static MethodDeclarationSyntax GenerateFactoryMethod(
            IMethodSymbol validConstructor,
            List<IPropertySymbol> validProperties,
            INamedTypeSymbol classSymbol, 
            INamedTypeSymbol injectAttributeSymbol, 
            INamedTypeSymbol requiredInjectAttributeSymbol,
            INamedTypeSymbol factoryMethodNameAttributeSymbol)
        {
            var factoryMethodParameters = new List<ParameterSyntax>();
            var constructorArguments = new List<ArgumentSyntax>();
            var propertySetters = new List<AssignmentExpressionSyntax>();

            foreach (var parmeter in validConstructor.Parameters)
            {
                var diMethodToCall = parmeter.HasAttribute(requiredInjectAttributeSymbol)
                    ? "GetRequiredService"
                    : parmeter.HasAttribute(injectAttributeSymbol)
                        ? "GetService"
                        : null;

                if (diMethodToCall is not null)
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
                                        Identifier(diMethodToCall))
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

            foreach (var property in validProperties)
            {
                var diMethodToCall = property.HasAttribute(requiredInjectAttributeSymbol)
                    ? "GetRequiredService"
                    : property.HasAttribute(injectAttributeSymbol)
                        ? "GetService"
                        : throw new InvalidOperationException("Valid property found without an injection attribute");
                
                var initializer = AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName(property.Name),
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                ThisExpression(),
                                IdentifierName("_serviceProvider")),
                            GenericName(
                                    Identifier(diMethodToCall))
                                .WithTypeArgumentList(
                                    TypeArgumentList(
                                        SingletonSeparatedList(
                                            ParseTypeName(property.Type.ToDisplayString())))))));
                propertySetters.Add(initializer);
            }

            InitializerExpressionSyntax? propertyInitializer = null;
            if (propertySetters.Any())
            {
                propertyInitializer = InitializerExpression(
                    SyntaxKind.ObjectInitializerExpression,
                    SeparatedList<ExpressionSyntax>(
                        propertySetters
                            .Select(x => (SyntaxNodeOrToken) x)
                            .Intersperse<SyntaxNodeOrToken>(Token(SyntaxKind.CommaToken))
                        )
                );
            }
            
            var body = ReturnStatement(
                ObjectCreationExpression(
                    ParseTypeName(classSymbol.Name))
                .AddArgumentListArguments(constructorArguments.ToArray())
                .WithInitializer(propertyInitializer));

            // Find a method name attached to this constructor using FactoryMethodNameAttribute else default it to Create
            var methodName = validConstructor.GetAttribute(factoryMethodNameAttributeSymbol)
                ?.ConstructorArguments.First().Value as string
                ?? "Create";

            var methodDeclaration = MethodDeclaration(
                    ParseTypeName(classSymbol.Name),
                    Identifier(methodName))
                .AddModifiers(
                    Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(factoryMethodParameters.ToArray())
                .WithBody(
                    Block(body));

            return methodDeclaration;
        }
    }
}
