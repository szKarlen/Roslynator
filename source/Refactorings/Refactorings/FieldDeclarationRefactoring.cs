﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Refactorings
{
    internal static class FieldDeclarationRefactoring
    {
        public static async Task ComputeRefactoringsAsync(RefactoringContext context, FieldDeclarationSyntax fieldDeclaration)
        {
            if (fieldDeclaration.IsConst())
            {
                if (context.IsRefactoringEnabled(RefactoringIdentifiers.ReplaceConstantWithField)
                    && fieldDeclaration.Span.Contains(context.Span))
                {
                    context.RegisterRefactoring(
                        "Replace constant with field",
                        cancellationToken => ReplaceConstantWithFieldRefactoring.RefactorAsync(context.Document, fieldDeclaration, cancellationToken));
                }

                if (context.IsRefactoringEnabled(RefactoringIdentifiers.InlineConstant)
                    && !fieldDeclaration.ContainsDiagnostics)
                {
                    VariableDeclaratorSyntax variableDeclarator = fieldDeclaration
                        .Declaration?
                        .Variables
                        .FirstOrDefault(f => context.Span.IsEmptyAndContainedInSpanOrBetweenSpans(f.Identifier));

                    if (variableDeclarator != null)
                    {
                        context.RegisterRefactoring(
                            "Inline constant",
                            cancellationToken => InlineConstantRefactoring.RefactorAsync(context.Document, fieldDeclaration, variableDeclarator, cancellationToken));
                    }
                }
            }
            else if (context.IsRefactoringEnabled(RefactoringIdentifiers.ReplaceFieldWithConstant)
                && fieldDeclaration.Modifiers.Contains(SyntaxKind.ReadOnlyKeyword)
                && fieldDeclaration.IsStatic()
                && fieldDeclaration.Span.Contains(context.Span))
            {
                if (await ReplaceFieldWithConstantRefactoring.CanRefactorAsync(context, fieldDeclaration).ConfigureAwait(false))
                {
                    context.RegisterRefactoring(
                        "Replace field with constant",
                        cancellationToken => ReplaceFieldWithConstantRefactoring.RefactorAsync(context.Document, fieldDeclaration, cancellationToken));
                }
            }
        }
    }
}
