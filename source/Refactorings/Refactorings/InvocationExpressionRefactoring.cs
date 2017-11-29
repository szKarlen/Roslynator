// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp.Refactorings.InlineMethod;

namespace Roslynator.CSharp.Refactorings
{
    internal static class InvocationExpressionRefactoring
    {
        public static async Task ComputeRefactoringsAsync(RefactoringContext context, InvocationExpressionSyntax invocationExpression)
        {
            if (context.IsAnyRefactoringEnabled(
                RefactoringIdentifiers.UseElementAccessInsteadOfEnumerableMethod,
                RefactoringIdentifiers.ReplaceAnyWithAllOrAllWithAny,
                RefactoringIdentifiers.CallExtensionMethodAsInstanceMethod,
                RefactoringIdentifiers.ReplaceStringContainsWithStringIndexOf))
            {
                ExpressionSyntax expression = invocationExpression.Expression;

                if (expression != null
                    && invocationExpression.ArgumentList != null)
                {
                    if (expression.IsKind(SyntaxKind.SimpleMemberAccessExpression)
                        && ((MemberAccessExpressionSyntax)expression).Name?.Span.Contains(context.Span) == true)
                    {
                        if (context.IsRefactoringEnabled(RefactoringIdentifiers.UseElementAccessInsteadOfEnumerableMethod))
                            await UseElementAccessInsteadOfEnumerableMethodRefactoring.ComputeRefactoringsAsync(context, invocationExpression).ConfigureAwait(false);

                        if (context.IsRefactoringEnabled(RefactoringIdentifiers.ReplaceAnyWithAllOrAllWithAny))
                        {
                            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                            ReplaceAnyWithAllOrAllWithAnyRefactoring.ComputeRefactoring(context, invocationExpression, semanticModel);
                        }

                        if (context.IsRefactoringEnabled(RefactoringIdentifiers.ReplaceLinqWithForEach))
                        {
                            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                            ReplaceLinqWithForEachRefactoring.ComputeRefactoring(context, invocationExpression, semanticModel);
                        }

                        if (context.IsRefactoringEnabled(RefactoringIdentifiers.ReplaceStringContainsWithStringIndexOf))
                        {
                            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                            ReplaceStringContainsWithStringIndexOfRefactoring.ComputeRefactoring(context, invocationExpression, semanticModel);
                        }
                    }

                    if (context.IsRefactoringEnabled(RefactoringIdentifiers.CallExtensionMethodAsInstanceMethod))
                    {
                        SyntaxNodeOrToken nodeOrToken = CallExtensionMethodAsInstanceMethodRefactoring.GetNodeOrToken(expression);

                        if (nodeOrToken.Span.Contains(context.Span))
                        {
                            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                            CallExtensionMethodAsInstanceMethodAnalysis analysis = CallExtensionMethodAsInstanceMethodRefactoring.Analyze(invocationExpression, semanticModel, allowAnyExpression: true, cancellationToken: context.CancellationToken);

                            if (analysis.Success)
                            {
                                context.RegisterRefactoring(
                                    CallExtensionMethodAsInstanceMethodRefactoring.Title,
                                    cancellationToken =>
                                    {
                                        return context.Document.ReplaceNodeAsync(
                                            analysis.InvocationExpression,
                                            analysis.NewInvocationExpression,
                                            context.CancellationToken);
                                    });
                            }
                        }
                    }
                }
            }

            if (context.IsRefactoringEnabled(RefactoringIdentifiers.ReplaceStringFormatWithInterpolatedString)
                && context.SupportsCSharp6)
            {
                await ReplaceStringFormatWithInterpolatedStringRefactoring.ComputeRefactoringsAsync(context, invocationExpression).ConfigureAwait(false);
            }

            if (context.IsRefactoringEnabled(RefactoringIdentifiers.UseBitwiseOperationInsteadOfCallingHasFlag))
            {
                SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                if (UseBitwiseOperationInsteadOfCallingHasFlagRefactoring.CanRefactor(invocationExpression, semanticModel, context.CancellationToken))
                {
                    context.RegisterRefactoring(
                        UseBitwiseOperationInsteadOfCallingHasFlagRefactoring.Title,
                        cancellationToken =>
                        {
                            return UseBitwiseOperationInsteadOfCallingHasFlagRefactoring.RefactorAsync(
                                context.Document,
                                invocationExpression,
                                cancellationToken);
                        });
                }
            }

            if (context.IsRefactoringEnabled(RefactoringIdentifiers.InlineMethod))
                await InlineMethodRefactoring.ComputeRefactoringsAsync(context, invocationExpression).ConfigureAwait(false);
        }
    }
}
