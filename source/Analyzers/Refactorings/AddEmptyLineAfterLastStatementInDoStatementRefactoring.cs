// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
    internal static class AddEmptyLineAfterLastStatementInDoStatementRefactoring
    {
        public static void Analyze(SyntaxNodeAnalysisContext context, DoStatementSyntax doStatement)
        {
            StatementSyntax statement = doStatement.Statement;

            if (statement?.IsKind(SyntaxKind.Block) != true)
            {
                return;
            }

            var block = (BlockSyntax)statement;

            SyntaxList<StatementSyntax> statements = block.Statements;

            if (!statements.Any())
            {
                return;
            }

            SyntaxToken closeBrace = block.CloseBraceToken;

            if (closeBrace.IsMissing)
            {
                return;
            }

            SyntaxToken whileKeyword = doStatement.WhileKeyword;

            if (whileKeyword.IsMissing)
            {
                return;
            }

            int closeBraceLine = closeBrace.GetSpanEndLine();

            if (closeBraceLine != whileKeyword.GetSpanStartLine())
            {
                return;
            }

            StatementSyntax last = statements.Last();

            int line = last.GetSpanEndLine(context.CancellationToken);

            if (closeBraceLine - line != 1)
            {
                return;
            }

            SyntaxTrivia trivia = last
                .GetTrailingTrivia()
                .FirstOrDefault(f => f.IsEndOfLineTrivia());

            if (!trivia.IsEndOfLineTrivia())
            {
                return;
            }

            context.ReportDiagnostic(
                DiagnosticDescriptors.AddEmptyLineAfterLastStatementInDoStatement,
                Location.Create(doStatement.SyntaxTree, trivia.Span));
        }

        public static Task<Document> RefactorAsync(
            Document document,
            StatementSyntax statement,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            SyntaxTriviaList trailingTrivia = statement.GetTrailingTrivia();

            int index = trailingTrivia.IndexOf(SyntaxKind.EndOfLineTrivia);

            SyntaxTriviaList newTrailingTrivia = trailingTrivia.Insert(index, CSharpFactory.NewLine());

            StatementSyntax newStatement = statement.WithTrailingTrivia(newTrailingTrivia);

            return document.ReplaceNodeAsync(statement, newStatement, cancellationToken);
        }
    }
}
