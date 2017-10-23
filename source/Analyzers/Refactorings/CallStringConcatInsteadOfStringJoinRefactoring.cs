// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Roslynator.CSharp.Refactorings
{
    internal static class CallStringConcatInsteadOfStringJoinRefactoring
    {
        public static void AnalyzeInvocationExpression(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            ExpressionSyntax expression = invocation.Expression;

            if (expression?.IsKind(SyntaxKind.SimpleMemberAccessExpression) != true)
            {
                return;
            }

            var memberAccess = (MemberAccessExpressionSyntax)expression;

            ArgumentListSyntax argumentList = invocation.ArgumentList;

            if (argumentList == null)
            {
                return;
            }

            SeparatedSyntaxList<ArgumentSyntax> arguments = argumentList.Arguments;

            if (!arguments.Any())
            {
                return;
            }

            SimpleNameSyntax name = memberAccess.Name;

            if (name?.Identifier.ValueText != "Join")
            {
                return;
            }

            SemanticModel semanticModel = context.SemanticModel;
            CancellationToken cancellationToken = context.CancellationToken;

            MethodInfo info;
            if (!semanticModel.TryGetMethodInfo(invocation, out info, cancellationToken)
                || !info.IsName("Join")
                || !info.IsContainingType(SpecialType.System_String)
                || !info.IsPublic
                || !info.IsStatic
                || !info.IsReturnType(SpecialType.System_String)
                || info.IsGenericMethod
                || info.IsExtensionMethod)
            {
                return;
            }

            ImmutableArray<IParameterSymbol> parameters = info.Parameters;

            if (parameters.Length != 2
                || !parameters[0].Type.IsString())
            {
                return;
            }

            IParameterSymbol parameter = parameters[1];

            if (!parameter.IsParamsOf(SpecialType.System_String, SpecialType.System_Object)
                && !parameter.Type.IsConstructedFromIEnumerableOfT())
            {
                return;
            }

            ArgumentSyntax firstArgument = arguments.First();
            ExpressionSyntax argumentExpression = firstArgument.Expression;

            if (argumentExpression == null
                || !CSharpUtility.IsEmptyString(argumentExpression, semanticModel, cancellationToken)
                || invocation.ContainsDirectives(TextSpan.FromBounds(invocation.SpanStart, firstArgument.Span.End)))
            {
                return;
            }

            context.ReportDiagnostic(
                DiagnosticDescriptors.CallStringConcatInsteadOfStringJoin,
                name);
        }

        public static Task<Document> RefactorAsync(
            Document document,
            InvocationExpressionSyntax invocation,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;

            MemberAccessExpressionSyntax newMemberAccess = memberAccess.WithName(IdentifierName("Concat").WithTriviaFrom(memberAccess.Name));

            ArgumentListSyntax argumentList = invocation.ArgumentList;
            SeparatedSyntaxList<ArgumentSyntax> arguments = argumentList.Arguments;

            ArgumentListSyntax newArgumentList = argumentList
                .WithArguments(arguments.RemoveAt(0))
                .WithOpenParenToken(argumentList.OpenParenToken.AppendToTrailingTrivia(arguments[0].GetLeadingAndTrailingTrivia()));

            InvocationExpressionSyntax newInvocation = invocation
                .WithExpression(newMemberAccess)
                .WithArgumentList(newArgumentList);

            return document.ReplaceNodeAsync(invocation, newInvocation, cancellationToken);
        }
    }
}
