﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.Comparers;
using Roslynator.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.Analyzers.MarkLocalVariableAsConst
{
    internal static class MarkLocalVariableAsConstRefactoring
    {
        public static void AnalyzeLocalDeclarationStatement(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.ContainsDiagnostics)
                return;

            var localDeclaration = (LocalDeclarationStatementSyntax)context.Node;

            if (localDeclaration.IsConst)
                return;

            StatementsInfo statementsInfo = SyntaxInfo.StatementsInfo(localDeclaration);

            if (!statementsInfo.Success)
                return;

            SyntaxList<StatementSyntax> statements = statementsInfo.Statements;

            if (statements.Count <= 1)
                return;

            int index = statements.IndexOf(localDeclaration);

            if (index == statements.Count - 1)
                return;

            LocalDeclarationStatementInfo localInfo = SyntaxInfo.LocalDeclarationStatementInfo(localDeclaration);

            if (!localInfo.Success)
                return;

            ITypeSymbol typeSymbol = context.SemanticModel.GetTypeSymbol(localInfo.Type, context.CancellationToken);

            if (typeSymbol?.SupportsConstantValue() != true)
                return;

            foreach (VariableDeclaratorSyntax declarator in localInfo.Variables)
            {
                if (!HasConstantValue(declarator.Initializer?.Value, typeSymbol, context.SemanticModel, context.CancellationToken))
                    return;
            }

            if (IsAnyVariableAssigned(localInfo.Variables, statements, index + 1))
                return;

            context.ReportDiagnostic(DiagnosticDescriptors.MarkLocalVariableAsConst, localInfo.Type);
        }

        private static bool IsAnyVariableAssigned(SeparatedSyntaxList<VariableDeclaratorSyntax> variables, SyntaxList<StatementSyntax> statements, int startIndex)
        {
            for (int i = startIndex; i < statements.Count; i++)
            {
                MarkLocalVariableAsConstWalker walker = MarkLocalVariableAsConstWalkerCache.Acquire();

                walker.Visit(statements[i]);

                HashSet<string> assigned = MarkLocalVariableAsConstWalkerCache.GetAssignedAndRelease(walker);

                if (assigned != null)
                {
                    foreach (VariableDeclaratorSyntax variable in variables)
                    {
                        if (assigned.Contains(variable.Identifier.ValueText))
                            return true;
                    }

                    walker.Clear();
                }
            }

            return false;
        }

        private static bool HasConstantValue(
            ExpressionSyntax expression,
            ITypeSymbol typeSymbol,
            SemanticModel semanticModel,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (expression?.IsMissing != false)
                return false;

            switch (typeSymbol.SpecialType)
            {
                case SpecialType.System_Boolean:
                    {
                        if (expression.Kind().IsBooleanLiteralExpression())
                            return true;

                        break;
                    }
                case SpecialType.System_Char:
                    {
                        if (expression.Kind() == SyntaxKind.CharacterLiteralExpression)
                            return true;

                        break;
                    }
                case SpecialType.System_SByte:
                case SpecialType.System_Byte:
                case SpecialType.System_Int16:
                case SpecialType.System_UInt16:
                case SpecialType.System_Int32:
                case SpecialType.System_UInt32:
                case SpecialType.System_Int64:
                case SpecialType.System_UInt64:
                case SpecialType.System_Decimal:
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                    {
                        if (expression.Kind() == SyntaxKind.NumericLiteralExpression)
                            return true;

                        break;
                    }
                case SpecialType.System_String:
                    {
                        if (expression.Kind() == SyntaxKind.StringLiteralExpression)
                            return true;

                        break;
                    }
            }

            return semanticModel.GetConstantValue(expression, cancellationToken).HasValue;
        }

        public static async Task<Document> RefactorAsync(
            Document document,
            LocalDeclarationStatementSyntax localDeclaration,
            CancellationToken cancellationToken)
        {
            TypeSyntax type = localDeclaration.Declaration.Type;

            LocalDeclarationStatementSyntax newNode = localDeclaration;

            if (type.IsVar)
            {
                SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

                ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(type, cancellationToken);

                TypeSyntax newType = typeSymbol.ToMinimalTypeSyntax(semanticModel, localDeclaration.SpanStart);

                newNode = newNode.ReplaceNode(type, newType.WithTriviaFrom(type));
            }

            Debug.Assert(!newNode.Modifiers.Any(), newNode.Modifiers.ToString());

            if (newNode.Modifiers.Any())
            {
                newNode = newNode.InsertModifier(SyntaxKind.ConstKeyword, ModifierComparer.Instance);
            }
            else
            {
                newNode = newNode
                    .WithoutLeadingTrivia()
                    .WithModifiers(TokenList(ConstKeyword().WithLeadingTrivia(newNode.GetLeadingTrivia())));
            }

            return await document.ReplaceNodeAsync(localDeclaration, newNode).ConfigureAwait(false);
        }
    }
}
