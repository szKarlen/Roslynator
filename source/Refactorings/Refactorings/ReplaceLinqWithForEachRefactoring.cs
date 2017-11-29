// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Refactorings
{
    internal static class ReplaceLinqWithForEachRefactoring
    {
        private class ClassName
        {
            private void FooCondition()
            {
                var items = new List<string>();

                if (items.Any(f => f.StartsWith("")))
                {
                    FooTrue();
                }

                FooFalse();
            }

            private void FooCondition2()
            {
                var items = new List<string>();

                foreach (string item in items)
                {
                    if (item.StartsWith(""))
                    {
                        FooTrue();
                    }
                }

                FooFalse();
            }

            private bool FooReturn()
            {
                var items = new List<string>();

                return items.Any(f => f.StartsWith(""));
            }

            private bool FooReturn2()
            {
                var items = new List<string>();

                foreach (string item in items)
                {
                    if (item.StartsWith(""))
                    {
                        return true;
                    }
                }

                return false;
            }

            private void FooDeclaration()
            {
                var items = new List<string>();

                bool x = items.Any(f => f.StartsWith(""));
            }

            private void FooDeclaration2()
            {
                var items = new List<string>();

                bool x = false;

                foreach (string item in items)
                {
                    if (item.StartsWith(""))
                    {
                        x = true;
                        break;
                    }
                }
            }

            private void FooAssignment()
            {
                bool x = false;

                var items = new List<string>();

                x = items.Any(f => f.StartsWith(""));
            }

            private void FooAssignment2()
            {
                var items = new List<string>();

                bool x = false;

                foreach (string item in items)
                {
                    if (item.StartsWith(""))
                    {
                        x = true;
                        break;
                    }
                }
            }

            private void FooTrue()
            {
            }

            private void FooFalse()
            {
            }
        }

        public static void ComputeRefactoring(
            RefactoringContext context,
            InvocationExpressionSyntax invocationExpression,
            SemanticModel semanticModel)
        {
            if (!CheckExpression(invocationExpression.WalkUpParentheses()))
                return;

            if (!semanticModel.TryGetExtensionMethodInfo(invocationExpression, out MethodInfo methodInfo, ExtensionMethodKind.None, context.CancellationToken))
                return;

            if (!methodInfo.IsLinqExtensionOfIEnumerableOfTWithPredicate())
                return;

            switch (methodInfo.Name)
            {
                case "Any":
                    {
                        break;
                    }
                default:
                    {
                        return;
                    }
            }

            context.RegisterRefactoring(
                $"Replace '{methodInfo.Name}' with foreach",
                cancellationToken => RefactorAsync(context.Document, invocationExpression, cancellationToken));
        }

        private static bool CheckExpression(ExpressionSyntax expression)
        {
            SyntaxNode node = expression.Parent;

            switch (node.Kind())
            {
                case SyntaxKind.IfStatement:
                    {
                        return ((IfStatementSyntax)node).Condition == expression;
                    }
                case SyntaxKind.ReturnStatement:
                    {
                        return true;
                    }
                case SyntaxKind.SimpleAssignmentExpression:
                    {
                        return ((AssignmentExpressionSyntax)node).Right == expression;
                    }
                case SyntaxKind.EqualsValueClause:
                    {
                        SingleLocalDeclarationStatementInfo localInfo = SyntaxInfo.SingleLocalDeclarationStatementInfo(expression);

                        return localInfo.Success
                            && localInfo.Initializer.Value == expression;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private static Task<Document> RefactorAsync(
            Document document,
            InvocationExpressionSyntax invocationExpression,
            CancellationToken cancellationToken)
        {
            return document.ReplaceNodeAsync(invocationExpression, invocationExpression, cancellationToken);
        }
    }
}
