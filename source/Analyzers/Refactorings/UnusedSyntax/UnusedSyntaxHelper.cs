﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp;

namespace Roslynator.CSharp.Refactorings.UnusedSyntax
{
    internal static class UnusedSyntaxHelper
    {
        public static bool ContainsOnlyThrowNewException(IndexerDeclarationSyntax indexerDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            ArrowExpressionClauseSyntax expressionBody = indexerDeclaration.ExpressionBody;

            if (expressionBody != null)
            {
                return ContainsOnlyThrowNewException(expressionBody, semanticModel, cancellationToken);
            }
            else
            {
                return indexerDeclaration.AccessorList?
                    .Accessors
                    .All(f => ContainsOnlyThrowNewException(f.BodyOrExpressionBody(), semanticModel, cancellationToken)) == true;
            }
        }

        public static bool ContainsOnlyThrowNewException(
            CSharpSyntaxNode bodyOrExpressionBody,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            switch (bodyOrExpressionBody?.Kind())
            {
                case SyntaxKind.Block:
                    return ContainsOnlyThrowNewException((BlockSyntax)bodyOrExpressionBody, semanticModel, cancellationToken);
                case SyntaxKind.ArrowExpressionClause:
                    return ContainsOnlyThrowNewException((ArrowExpressionClauseSyntax)bodyOrExpressionBody, semanticModel, cancellationToken);
            }

            return false;
        }

        public static bool ContainsOnlyThrowNewException(
            BlockSyntax body,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            StatementSyntax statement = body?.Statements.SingleOrDefault(throwException: false);

            if (statement?.IsKind(SyntaxKind.ThrowStatement) != true)
            {
                return false;
            }

            var throwStatement = (ThrowStatementSyntax)statement;

            return IsThrowNewException(throwStatement.Expression, semanticModel, cancellationToken);
        }

        public static bool ContainsOnlyThrowNewException(
            ArrowExpressionClauseSyntax expressionBody,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            ExpressionSyntax expression = expressionBody?.Expression;

            if (expression?.IsKind(SyntaxKind.ThrowExpression) != true)
            {
                return false;
            }

            var throwExpression = (ThrowExpressionSyntax)expression;

            return IsThrowNewException(throwExpression.Expression, semanticModel, cancellationToken);
        }

        private static bool IsThrowNewException(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (expression?.IsKind(SyntaxKind.ObjectCreationExpression) != true)
            {
                return false;
            }

            var objectCreation = (ObjectCreationExpressionSyntax)expression;

            ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(objectCreation, cancellationToken);

            if (typeSymbol == null)
            {
                return false;
            }

            if (typeSymbol.Equals(semanticModel.GetTypeByMetadataName(MetadataNames.System_NotImplementedException))
                || typeSymbol.Equals(semanticModel.GetTypeByMetadataName(MetadataNames.System_NotSupportedException)))
            {
                return true;
            }

            return false;
        }
    }
}
