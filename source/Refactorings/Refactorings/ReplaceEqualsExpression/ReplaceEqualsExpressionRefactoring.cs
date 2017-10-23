// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.Refactorings.ReplaceEqualsExpression
{
    internal abstract class ReplaceEqualsExpressionRefactoring
    {
        public abstract string MethodName { get; }

        public static async Task ComputeRefactoringsAsync(RefactoringContext context, BinaryExpressionSyntax binaryExpression)
        {
            if (!binaryExpression.IsKind(SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression))
            {
                return;
            }

            ExpressionSyntax left = binaryExpression.Left;

            if (left?.IsKind(SyntaxKind.NullLiteralExpression) != false)
            {
                return;
            }

            ExpressionSyntax right = binaryExpression.Right;

            if (right?.IsKind(SyntaxKind.NullLiteralExpression) != true)
            {
                return;
            }

            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

            ITypeSymbol leftSymbol = semanticModel.GetTypeInfo(left, context.CancellationToken).ConvertedType;

            if (leftSymbol?.IsString() != true)
            {
                return;
            }

            if (context.IsRefactoringEnabled(RefactoringIdentifiers.ReplaceEqualsExpressionWithStringIsNullOrEmpty))
            {
                var refactoring2 = new ReplaceEqualsExpressionWithStringIsNullOrEmptyRefactoring();
                refactoring2.RegisterRefactoring(context, binaryExpression, left);
            }

            if (!context.IsRefactoringEnabled(RefactoringIdentifiers.ReplaceEqualsExpressionWithStringIsNullOrWhiteSpace))
            {
                return;
            }

            var refactoring = new ReplaceEqualsExpressionWithStringIsNullOrWhiteSpaceRefactoring();
            refactoring.RegisterRefactoring(context, binaryExpression, left);
        }

        private void RegisterRefactoring(RefactoringContext context, BinaryExpressionSyntax binaryExpression, ExpressionSyntax left)
        {
            string title = (binaryExpression.IsKind(SyntaxKind.EqualsExpression))
                ? $"Replace '{binaryExpression}' with 'string.{MethodName}({left})'"
                : $"Replace '{binaryExpression}' with '!string.{MethodName}({left})'";

            context.RegisterRefactoring(
                title,
                cancellationToken => RefactorAsync(context.Document, binaryExpression, cancellationToken));
        }

        private Task<Document> RefactorAsync(
            Document document,
            BinaryExpressionSyntax binaryExpression,
            CancellationToken cancellationToken)
        {
            ExpressionSyntax newNode = SimpleMemberInvocationExpression(
                StringType(),
                IdentifierName(MethodName),
                Argument(binaryExpression.Left));

            if (binaryExpression.OperatorToken.IsKind(SyntaxKind.ExclamationEqualsToken))
                newNode = LogicalNotExpression(newNode);

            newNode = newNode
                .WithTriviaFrom(binaryExpression)
                .WithFormatterAnnotation();

            return document.ReplaceNodeAsync(binaryExpression, newNode, cancellationToken);
        }
    }
}