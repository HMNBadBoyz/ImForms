using CodeGeneration.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

public class CompileTimeIDGenGenerator : ICodeGenerator
{
    public CompileTimeIDGenGenerator(AttributeData attributeData)
    {
        

    }

    public Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
    {
        var results = SyntaxFactory.List<MemberDeclarationSyntax>();

        // Our generator is applied to any class that our attribute is applied to.
        if (context.ProcessingNode is MethodDeclarationSyntax applyToMethod)
        {
            var copy = applyToMethod;
            copy = copy.WithAttributeLists(new SyntaxList<AttributeListSyntax>()).WithParameterList(ParameterList(new SeparatedSyntaxList<ParameterSyntax>().AddRange( applyToMethod.ParameterList.Parameters.Take(applyToMethod.ParameterList.Parameters.Count - 1).ToArray())).AddParameters(Parameter(
                                Identifier("srcFilePath"))
                            .WithAttributeLists(
                                SingletonList(
                                    AttributeList(
                                        SingletonSeparatedList(
                                            Attribute(
                                                QualifiedName(
                                                    QualifiedName(
                                                        QualifiedName(
                                                            IdentifierName("System"),
                                                            IdentifierName("Runtime")),
                                                        IdentifierName("CompilerServices")),
                                                    IdentifierName("CallerFilePath")))))))
                            .WithType(
                                PredefinedType(
                                    Token(SyntaxKind.StringKeyword)))
                            .WithDefault(
                                EqualsValueClause(
                                    LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        Literal("")))), Parameter(
                                Identifier("srcLineNumber"))
                            .WithAttributeLists(
                                SingletonList(
                                    AttributeList(
                                        SingletonSeparatedList(
                                            Attribute(
                                                QualifiedName(
                                                    QualifiedName(
                                                        QualifiedName(
                                                            IdentifierName("System"),
                                                            IdentifierName("Runtime")),
                                                        IdentifierName("CompilerServices")),
                                                    IdentifierName("CallerLineNumber")))))))
                            .WithType(
                                PredefinedType(
                                    Token(SyntaxKind.IntKeyword)))
                            .WithDefault(
                                EqualsValueClause(
                                    LiteralExpression(
                                        SyntaxKind.NumericLiteralExpression,
                                        Literal(0))))));

            copy = copy.WithBody(Block(
                    SingletonList<StatementSyntax>(
                        LocalDeclarationStatement(
                            VariableDeclaration(
                                PredefinedType(
                                    Token(SyntaxKind.StringKeyword)))
                            .WithVariables(
                                SingletonSeparatedList<VariableDeclaratorSyntax>(
                                    VariableDeclarator(
                                        Identifier("id"))
                                    .WithInitializer(
                                        EqualsValueClause(
                                            BinaryExpression(
                                                SyntaxKind.AddExpression,
                                                IdentifierName("srcFilePath"),
                                                IdentifierName("srcLineNumber"))))))))).AddStatements(applyToMethod.Body));
            copy = copy.WithIdentifier(SyntaxFactory.Identifier(applyToMethod.Identifier.ValueText.Trim('_'))).NormalizeWhitespace();
            // Return our modified copy. It will be added to the user's project for compilation.
           if(copy != null) results = results.Add(copy);
        }
        return Task.FromResult(results);
    }
}