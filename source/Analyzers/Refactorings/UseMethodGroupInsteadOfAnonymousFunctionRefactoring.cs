﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp;

namespace Roslynator.CSharp.Refactorings
{
    internal static class UseMethodGroupInsteadOfAnonymousFunctionRefactoring
    {
        public static void AnalyzeSimpleLambdaExpression(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.SpanContainsDirectives())
                return;

            var lambda = (SimpleLambdaExpressionSyntax)context.Node;

            InvocationExpressionSyntax invocationExpression = GetInvocationExpression(lambda.Body);

            if (invocationExpression == null)
                return;

            ExpressionSyntax expression = invocationExpression.Expression;

            if (!IsSimpleInvocation(expression))
                return;

            SemanticModel semanticModel = context.SemanticModel;
            CancellationToken cancellationToken = context.CancellationToken;

            var methodSymbol = semanticModel.GetSymbol(invocationExpression, cancellationToken) as IMethodSymbol;

            if (methodSymbol == null)
                return;

            bool isReduced = methodSymbol.MethodKind == MethodKind.ReducedExtension;

            ImmutableArray<IParameterSymbol> parameterSymbols = (isReduced) ? methodSymbol.ReducedFrom.Parameters : methodSymbol.Parameters;

            if (parameterSymbols.Length != 1)
                return;

            ArgumentListSyntax argumentList = invocationExpression.ArgumentList;

            SeparatedSyntaxList<ArgumentSyntax> arguments = argumentList.Arguments;

            if (arguments.Count != ((isReduced) ? 0 : 1))
                return;

            ParameterSyntax parameter = lambda.Parameter;

            MemberAccessExpressionSyntax memberAccessExpression = (isReduced) ? (MemberAccessExpressionSyntax)expression : null;

            if (!CheckParameter(
                parameter,
                (isReduced) ? memberAccessExpression.Expression : arguments[0].Expression,
                parameterSymbols[0]))
            {
                return;
            }

            methodSymbol = (isReduced) ? methodSymbol.GetConstructedReducedFrom() : methodSymbol;

            if (!CheckInvokeMethod(lambda, methodSymbol, semanticModel, context.CancellationToken))
                return;

            if (!CheckSpeculativeSymbol(
                lambda,
                (isReduced) ? memberAccessExpression.Name.WithoutTrivia() : expression,
                methodSymbol,
                semanticModel))
            {
                return;
            }

            context.ReportDiagnostic(DiagnosticDescriptors.UseMethodGroupInsteadOfAnonymousFunction, lambda);

            FadeOut(context, parameter, null, lambda.Body as BlockSyntax, argumentList, lambda.ArrowToken, memberAccessExpression);
        }

        public static void AnalyzeParenthesizedLambdaExpression(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.SpanContainsDirectives())
                return;

            var lambda = (ParenthesizedLambdaExpressionSyntax)context.Node;

            InvocationExpressionSyntax invocationExpression = GetInvocationExpression(lambda.Body);

            if (invocationExpression == null)
                return;

            ExpressionSyntax expression = invocationExpression.Expression;

            if (!IsSimpleInvocation(expression))
                return;

            SemanticModel semanticModel = context.SemanticModel;
            CancellationToken cancellationToken = context.CancellationToken;

            var methodSymbol = semanticModel.GetSymbol(invocationExpression, cancellationToken) as IMethodSymbol;

            if (methodSymbol == null)
                return;

            ImmutableArray<IParameterSymbol> parameterSymbols = methodSymbol.Parameters;

            ArgumentListSyntax argumentList = invocationExpression.ArgumentList;

            SeparatedSyntaxList<ArgumentSyntax> arguments = argumentList.Arguments;

            if (arguments.Count != parameterSymbols.Length)
                return;

            ParameterListSyntax parameterList = lambda.ParameterList;

            SeparatedSyntaxList<ParameterSyntax> parameters = parameterList.Parameters;

            bool isReduced = methodSymbol.MethodKind == MethodKind.ReducedExtension;

            if (parameters.Count != ((isReduced) ? arguments.Count + 1 : arguments.Count))
                return;

            MemberAccessExpressionSyntax memberAccessExpression = (isReduced) ? (MemberAccessExpressionSyntax)expression : null;

            if (isReduced)
            {
                if (!CheckParameter(
                    parameters[0],
                    memberAccessExpression.Expression,
                    methodSymbol.ReducedFrom.Parameters[0]))
                {
                    return;
                }

                parameters = parameters.RemoveAt(0);
            }

            if (!CheckParameters(parameters, arguments, parameterSymbols))
                return;

            methodSymbol = (isReduced) ? methodSymbol.GetConstructedReducedFrom() : methodSymbol;

            if (!CheckInvokeMethod(lambda, methodSymbol, semanticModel, context.CancellationToken))
                return;

            if (!CheckSpeculativeSymbol(
                lambda,
                (isReduced) ? memberAccessExpression.Name.WithoutTrivia() : expression,
                methodSymbol,
                semanticModel))
            {
                return;
            }

            context.ReportDiagnostic(DiagnosticDescriptors.UseMethodGroupInsteadOfAnonymousFunction, lambda);

            FadeOut(context, null, parameterList, lambda.Body as BlockSyntax, argumentList, lambda.ArrowToken, memberAccessExpression);
        }

        public static void AnalyzeAnonyousMethodExpression(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.SpanContainsDirectives())
                return;

            var anonymousMethod = (AnonymousMethodExpressionSyntax)context.Node;

            InvocationExpressionSyntax invocationExpression = GetInvocationExpression(anonymousMethod.Body);

            if (invocationExpression == null)
                return;

            ExpressionSyntax expression = invocationExpression.Expression;

            if (!IsSimpleInvocation(expression))
                return;

            SemanticModel semanticModel = context.SemanticModel;
            CancellationToken cancellationToken = context.CancellationToken;

            var methodSymbol = semanticModel.GetSymbol(invocationExpression, cancellationToken) as IMethodSymbol;

            if (methodSymbol == null)
                return;

            ImmutableArray<IParameterSymbol> parameterSymbols = methodSymbol.Parameters;

            ArgumentListSyntax argumentList = invocationExpression.ArgumentList;

            SeparatedSyntaxList<ArgumentSyntax> arguments = argumentList.Arguments;

            if (arguments.Count != parameterSymbols.Length)
                return;

            ParameterListSyntax parameterList = anonymousMethod.ParameterList;

            if (parameterList == null)
                return;

            SeparatedSyntaxList<ParameterSyntax> parameters = parameterList.Parameters;

            bool isReduced = methodSymbol.MethodKind == MethodKind.ReducedExtension;

            if (parameters.Count != ((isReduced) ? arguments.Count + 1 : arguments.Count))
                return;

            MemberAccessExpressionSyntax memberAccessExpression = (isReduced) ? (MemberAccessExpressionSyntax)expression : null;

            if (isReduced)
            {
                if (!CheckParameter(
                    parameters[0],
                    ((MemberAccessExpressionSyntax)expression).Expression,
                    methodSymbol.ReducedFrom.Parameters[0]))
                {
                    return;
                }

                parameters = parameters.RemoveAt(0);
            }

            if (!CheckParameters(parameters, arguments, parameterSymbols))
                return;

            methodSymbol = (isReduced) ? methodSymbol.GetConstructedReducedFrom() : methodSymbol;

            if (!CheckInvokeMethod(anonymousMethod, methodSymbol, semanticModel, context.CancellationToken))
                return;

            if (!CheckSpeculativeSymbol(
                anonymousMethod,
                (isReduced) ? memberAccessExpression.Name.WithoutTrivia() : expression,
                methodSymbol,
                semanticModel))
            {
                return;
            }

            context.ReportDiagnostic(DiagnosticDescriptors.UseMethodGroupInsteadOfAnonymousFunction, anonymousMethod);

            FadeOut(context, null, parameterList, anonymousMethod.Block, argumentList, anonymousMethod.DelegateKeyword, memberAccessExpression);
        }

        private static bool CheckInvokeMethod(
            AnonymousFunctionExpressionSyntax anonymousFunction,
            IMethodSymbol methodSymbol,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            var typeSymbol = semanticModel.GetTypeInfo(anonymousFunction, cancellationToken).ConvertedType as INamedTypeSymbol;

            IMethodSymbol invokeMethod = typeSymbol?.DelegateInvokeMethod;

            if (invokeMethod == null)
                return false;

            if (invokeMethod.ReturnType.IsVoid()
                && !methodSymbol.ReturnType.IsVoid())
            {
                return false;
            }

            ImmutableArray<IParameterSymbol> parameters = methodSymbol.Parameters;

            ImmutableArray<IParameterSymbol> parameters2 = invokeMethod.Parameters;

            if (parameters.Length != parameters2.Length)
                return false;

            for (int i = 0; i < parameters.Length; i++)
            {
                if (!parameters[i].Type.Equals(parameters2[i].Type))
                    return false;
            }

            return true;
        }

        private static bool CheckParameters(
            SeparatedSyntaxList<ParameterSyntax> parameters,
            SeparatedSyntaxList<ArgumentSyntax> arguments,
            ImmutableArray<IParameterSymbol> parameterSymbols)
        {
            for (int i = 0; i < parameters.Count; i++)
            {
                if (!CheckParameter(parameters[i], arguments[i].Expression, parameterSymbols[i]))
                    return false;
            }

            return true;
        }

        private static bool CheckParameter(
            ParameterSyntax parameter,
            ExpressionSyntax expression,
            IParameterSymbol parameterSymbol)
        {
            return !parameterSymbol.IsRefOrOut()
                && !parameterSymbol.IsParams
                && string.Equals(
                    parameter.Identifier.ValueText,
                    (expression as IdentifierNameSyntax)?.Identifier.ValueText,
                    StringComparison.Ordinal);
        }

        private static bool CheckSpeculativeSymbol(
            AnonymousFunctionExpressionSyntax anonymousFunction,
            ExpressionSyntax expression,
            IMethodSymbol methodSymbol,
            SemanticModel semanticModel)
        {
            SymbolInfo symbolInfo = semanticModel.GetSpeculativeSymbolInfo(anonymousFunction.SpanStart, expression, SpeculativeBindingOption.BindAsExpression);

            ISymbol symbol = symbolInfo.Symbol;

            if (symbol?.Equals(methodSymbol) == true)
                return true;

            ImmutableArray<ISymbol> candidateSymbols = symbolInfo.CandidateSymbols;

            if (candidateSymbols.Any())
            {
                if (candidateSymbols.Length == 1)
                {
                    if (candidateSymbols[0].Equals(methodSymbol))
                        return true;
                }
                else if (!anonymousFunction.WalkUpParentheses().IsParentKind(SyntaxKind.Argument, SyntaxKind.AttributeArgument))
                {
                    foreach (ISymbol candidateSymbol in candidateSymbols)
                    {
                        if (candidateSymbol.Equals(methodSymbol))
                            return true;
                    }
                }
            }

            return false;
        }

        private static bool IsSimpleInvocation(ExpressionSyntax expression)
        {
            while (true)
            {
                switch (expression?.Kind())
                {
                    case SyntaxKind.IdentifierName:
                        {
                            return true;
                        }
                    case SyntaxKind.SimpleMemberAccessExpression:
                        {
                            expression = ((MemberAccessExpressionSyntax)expression).Expression;
                            break;
                        }
                    default:
                        {
                            return false;
                        }
                }
            }
        }

        private static InvocationExpressionSyntax GetInvocationExpression(SyntaxNode node)
        {
            ExpressionSyntax expression = GetExpression(node)?.WalkDownParentheses();

            if (expression?.IsKind(SyntaxKind.InvocationExpression) == true)
                return (InvocationExpressionSyntax)expression;

            return null;
        }

        private static ExpressionSyntax GetExpression(SyntaxNode node)
        {
            if (node?.IsKind(SyntaxKind.Block) == true)
            {
                var block = (BlockSyntax)node;

                StatementSyntax statement = block.Statements.SingleOrDefault(shouldThrow: false);

                switch (statement?.Kind())
                {
                    case SyntaxKind.ExpressionStatement:
                        return ((ExpressionStatementSyntax)statement).Expression;
                    case SyntaxKind.ReturnStatement:
                        return ((ReturnStatementSyntax)statement).Expression;
                    default:
                        return null;
                }
            }

            return node as ExpressionSyntax;
        }

        private static void FadeOut(
            SyntaxNodeAnalysisContext context,
            ParameterSyntax parameter,
            ParameterListSyntax parameterList,
            BlockSyntax block,
            ArgumentListSyntax argumentList,
            SyntaxToken arrowTokenOrDelegateKeyword,
            MemberAccessExpressionSyntax memberAccessExpression)
        {
            if (parameter != null)
                context.ReportNode(DiagnosticDescriptors.UseMethodGroupInsteadOfAnonymousFunctionFadeOut, parameter);

            if (parameterList != null)
                context.ReportNode(DiagnosticDescriptors.UseMethodGroupInsteadOfAnonymousFunctionFadeOut, parameterList);

            if (!arrowTokenOrDelegateKeyword.IsKind(SyntaxKind.None))
                context.ReportToken(DiagnosticDescriptors.UseMethodGroupInsteadOfAnonymousFunctionFadeOut, arrowTokenOrDelegateKeyword);

            if (block != null)
            {
                context.ReportBraces(DiagnosticDescriptors.UseMethodGroupInsteadOfAnonymousFunctionFadeOut, block);

                if (block.Statements.SingleOrDefault(shouldThrow: false) is ReturnStatementSyntax returnStatement)
                    context.ReportToken(DiagnosticDescriptors.UseMethodGroupInsteadOfAnonymousFunctionFadeOut, returnStatement.ReturnKeyword);
            }

            if (memberAccessExpression != null)
            {
                context.ReportNode(DiagnosticDescriptors.UseMethodGroupInsteadOfAnonymousFunctionFadeOut, memberAccessExpression.Expression);
                context.ReportToken(DiagnosticDescriptors.UseMethodGroupInsteadOfAnonymousFunctionFadeOut, memberAccessExpression.OperatorToken);
            }

            context.ReportNode(DiagnosticDescriptors.UseMethodGroupInsteadOfAnonymousFunctionFadeOut, argumentList);
        }

        public static async Task<Document> RefactorAsync(
            Document document,
            AnonymousFunctionExpressionSyntax anonymousFunction,
            CancellationToken cancellationToken)
        {
            InvocationExpressionSyntax invocationExpression = GetInvocationExpression(anonymousFunction.Body);

            ExpressionSyntax newNode = invocationExpression.Expression;

            SemanticModel semanticModel = await document.GetSemanticModelAsync().ConfigureAwait(false);

            var methodSymbol = (IMethodSymbol)semanticModel.GetSymbol(invocationExpression, cancellationToken);

            if (methodSymbol.IsReducedExtensionMethod())
                newNode = ((MemberAccessExpressionSyntax)newNode).Name;

            newNode = newNode.WithTriviaFrom(anonymousFunction);

            return await document.ReplaceNodeAsync(anonymousFunction, newNode, cancellationToken).ConfigureAwait(false);
        }
    }
}
