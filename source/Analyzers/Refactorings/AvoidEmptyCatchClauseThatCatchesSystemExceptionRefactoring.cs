// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.Refactorings
{
    internal static class AvoidEmptyCatchClauseThatCatchesSystemExceptionRefactoring
    {
        public static void Analyze(SyntaxNodeAnalysisContext context, CatchClauseSyntax catchClause)
        {
            CatchDeclarationSyntax declaration = catchClause.Declaration;

            if (declaration == null)
            {
                return;
            }

            BlockSyntax block = catchClause.Block;

            if (block == null
                || declaration.Type == null
                || block.Statements.Any())
            {
                return;
            }

            ITypeSymbol typeSymbol = context
                .SemanticModel
                .GetTypeSymbol(declaration.Type, context.CancellationToken);

            if (typeSymbol?.IsErrorType() != false)
            {
                return;
            }

            INamedTypeSymbol exceptionTypeSymbol = context.GetTypeByMetadataName(MetadataNames.System_Exception);

            if (!typeSymbol.Equals(exceptionTypeSymbol))
            {
                return;
            }

            context.ReportDiagnostic(
                DiagnosticDescriptors.AvoidEmptyCatchClauseThatCatchesSystemException,
                catchClause.CatchKeyword);
        }
    }
}
