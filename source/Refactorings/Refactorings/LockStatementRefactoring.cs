﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Refactorings
{
    internal static class LockStatementRefactoring
    {
        public static void ComputeRefactorings(RefactoringContext context, LockStatementSyntax lockStatement)
        {
            if (!context.IsRefactoringEnabled(RefactoringIdentifiers.IntroduceFieldToLockOn))
            {
                return;
            }

            ExpressionSyntax expression = lockStatement.Expression;

            if (expression == null)
            {
                return;
            }

            if (!expression.IsMissing && !expression.IsKind(SyntaxKind.ThisExpression))
            {
                return;
            }

            if (!context.Span.IsContainedInSpanOrBetweenSpans(expression))
            {
                return;
            }

            context.RegisterRefactoring(
                "Introduce field to lock on",
                cancellationToken => IntroduceFieldToLockOnRefactoring.RefactorAsync(context.Document, lockStatement, cancellationToken));
        }
    }
}