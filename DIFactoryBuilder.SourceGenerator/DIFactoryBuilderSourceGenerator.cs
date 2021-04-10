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
        protected const string IDIFactoryClassName = @"DIFactoryBuilder.IDIFactory";


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

            var syntaxFactory = SyntaxFactory.CompilationUnit()
                .AddUsings(
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.Extensions.DependencyInjection")))
                .AddMembers(
                    SyntaxFactory.NamespaceDeclaration(
                        SyntaxFactory.ParseName(classSymbol.ContainingNamespace.ToDisplayString()))
                        .AddMembers(
                            SyntaxFactory.ClassDeclaration(generatedClassName)
                                .AddModifiers(
                                    SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                                .WithBaseList(
                                    BaseList(
                                        SingletonSeparatedList<BaseTypeSyntax>(
                                            SimpleBaseType(
                                                GenericName(
                                                        Identifier("IDIFactory"))
                                                    .WithTypeArgumentList(
                                                        TypeArgumentList(
                                                            SingletonSeparatedList<TypeSyntax>(
                                                                IdentifierName(classSymbol.Name))))))))
                                .AddMembers(
                                    SyntaxFactory.FieldDeclaration(
                                        SyntaxFactory.VariableDeclaration(
                                            SyntaxFactory.ParseTypeName("System.IServiceProvider"))
                                            .AddVariables(
                                                SyntaxFactory.VariableDeclarator("_serviceProvider")))
                                        .AddModifiers(
                                            SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                                            SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)
                                            )
                                    )
                                .AddMembers(
                                    SyntaxFactory.ConstructorDeclaration(generatedClassName)
                                        .AddModifiers(
                                            SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                                        .AddParameterListParameters(
                                            SyntaxFactory.Parameter(
                                                SyntaxFactory.Identifier("serviceProvider"))
                                                .WithType(
                                                    SyntaxFactory.ParseTypeName("System.IServiceProvider"))
                                            )
                                        .AddBodyStatements(
                                            SyntaxFactory.ParseStatement(@"this._serviceProvider = serviceProvider;")
                                            )
                                        )
                                        .AddMembers(
                                            GenerateFactoryMethods(validConstructors, classSymbol, injectAttributeSymbol))
                                    )
                                );

            var classString = syntaxFactory.NormalizeWhitespace().ToFullString();
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
            var methodDeclaration = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.ParseTypeName(classSymbol.Name),
                "Create")
                .AddModifiers(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithParameterList(
                                ParameterList(
                                    SeparatedList<ParameterSyntax>(
                                        new SyntaxNodeOrToken[]{
                                            Parameter(
                                                Identifier("a"))
                                            .WithType(
                                                PredefinedType(
                                                    Token(SyntaxKind.IntKeyword))),
                                            Token(SyntaxKind.CommaToken),
                                            Parameter(
                                                Identifier("b"))
                                            .WithType(
                                                PredefinedType(
                                                    Token(SyntaxKind.IntKeyword)))
                                            .WithDefault(
                                                EqualsValueClause(
                                                    LiteralExpression(
                                                        SyntaxKind.NumericLiteralExpression,
                                                        Literal(3)))),
                                            Token(SyntaxKind.CommaToken),
                                            Parameter(
                                                Identifier("c"))
                                            .WithType(
                                                GenericName(
                                                    Identifier("IEnumerable"))
                                                .WithTypeArgumentList(
                                                    TypeArgumentList(
                                                        SingletonSeparatedList<TypeSyntax>(
                                                            PredefinedType(
                                                                Token(SyntaxKind.DoubleKeyword))))))})))
                            .WithBody(
                                Block(
                                    SingletonList<StatementSyntax>(
                                        ReturnStatement(
                                            ObjectCreationExpression(
                                                IdentifierName("TestViewModelFactory"))
                                            .WithArgumentList(
                                                ArgumentList(
                                                    SeparatedList<ArgumentSyntax>(
                                                        new SyntaxNodeOrToken[]{
                                                            Argument(
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
                                                                                SingletonSeparatedList<TypeSyntax>(
                                                                                    IdentifierName("T1"))))))),
                                                            Token(SyntaxKind.CommaToken),
                                                            Argument(
                                                                IdentifierName("a")),
                                                            Token(SyntaxKind.CommaToken)})))))));


            foreach (var parmeter in validConstructor.Parameters)
            {
                if (parmeter.HasAttribute(injectAttributeSymbol))
                {
                    
                }
                else
                {

                }
            }

            return methodDeclaration;
        }

        private void GenerateConstructorFactoryMethod(GeneratorExecutionContext context, IMethodSymbol constructor, INamedTypeSymbol typeSymbol, IndentStringBuilder sb, bool useMicrosoftDi)
        {
            var injectAttributeSymbol = context.Compilation.GetTypeByMetadataName(InjectAttributeName);

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
