// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.Refactorings
{
    internal static class ReplaceIfStatementWithAssignmentRefactoring
    {
        public static void Analyze(SyntaxNodeAnalysisContext context, IfStatementSyntax ifStatement)
        {
            if (!CanRefactor(ifStatement, context.SemanticModel, context.CancellationToken)
                || ifStatement.SpanContainsDirectives())
            {
                return;
            }

            context.ReportDiagnostic(
                DiagnosticDescriptors.ReplaceIfStatementWithAssignment,
                ifStatement);
        }

        public static bool CanRefactor(
            IfStatementSyntax ifStatement,
            SemanticModel semanticModel,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!ifStatement.IsTopmostIf())
            {
                return false;
            }

            ElseClauseSyntax elseClause = ifStatement.Else;

            if (elseClause == null)
            {
                return false;
            }

            ExpressionSyntax condition = ifStatement.Condition;

            if (condition == null)
            {
                return false;
            }

            ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(condition, cancellationToken);

            if (typeSymbol?.IsBoolean() != true)
            {
                return false;
            }

            AssignmentExpressionSyntax trueExpression = GetSimpleAssignmentExpression(ifStatement.GetSingleStatementOrDefault());

            ExpressionSyntax trueRight = trueExpression?.Right;

            if (trueRight?.Kind().IsBooleanLiteralExpression() != true)
            {
                return false;
            }

            AssignmentExpressionSyntax falseExpression = GetSimpleAssignmentExpression(elseClause.GetSingleStatementOrDefault());

            ExpressionSyntax falseRight = falseExpression?.Right;

            if (falseRight?.Kind().IsBooleanLiteralExpression() != true)
            {
                return false;
            }

            var trueBooleanLiteral = (LiteralExpressionSyntax)trueRight;
            var falseBooleanLiteral = (LiteralExpressionSyntax)falseRight;

            return trueBooleanLiteral.IsKind(SyntaxKind.TrueLiteralExpression) != falseBooleanLiteral.IsKind(SyntaxKind.TrueLiteralExpression)
                && SyntaxComparer.AreEquivalent(trueExpression.Left, falseExpression.Left, requireNotNull: true);
        }

        private static AssignmentExpressionSyntax GetSimpleAssignmentExpression(StatementSyntax statement)
        {
            if (statement?.IsKind(SyntaxKind.ExpressionStatement) != true)
            {
                return null;
            }

            var expressionStatement = (ExpressionStatementSyntax)statement;

            ExpressionSyntax expression = expressionStatement.Expression;

            if (expression?.IsKind(SyntaxKind.SimpleAssignmentExpression) == true)
                return (AssignmentExpressionSyntax)expression;

            return null;
        }

        public static async Task<Document> RefactorAsync(
            Document document,
            IfStatementSyntax ifStatement,
            CancellationToken cancellationToken)
        {
            ExpressionSyntax condition = ifStatement.Condition;

            AssignmentExpressionSyntax assignment = GetSimpleAssignmentExpression(ifStatement.GetSingleStatementOrDefault());

            if (assignment.Right.IsKind(SyntaxKind.FalseLiteralExpression))
            {
                SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

                condition = CSharpUtility.LogicallyNegate(condition, semanticModel, cancellationToken);
            }

            ExpressionStatementSyntax newNode = SimpleAssignmentStatement(assignment.Left, condition)
                .WithTriviaFrom(ifStatement)
                .WithFormatterAnnotation();

            return await document.ReplaceNodeAsync(ifStatement, newNode, cancellationToken).ConfigureAwait(false);
        }
    }
}
