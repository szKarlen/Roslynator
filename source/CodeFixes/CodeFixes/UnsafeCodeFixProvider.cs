﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp.Comparers;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UnsafeCodeFixProvider))]
    [Shared]
    public class UnsafeCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(CompilerDiagnosticIdentifiers.PointersAndFixedSizeBuffersMayOnlyBeUsedInUnsafeContext); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            if (!Settings.IsAnyCodeFixEnabled(
                CodeFixIdentifiers.WrapInUnsafeStatement,
                CodeFixIdentifiers.AddUnsafeModifier))
            {
                return;
            }

            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindNode(root, context.Span, out SyntaxNode node))
                return;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case CompilerDiagnosticIdentifiers.PointersAndFixedSizeBuffersMayOnlyBeUsedInUnsafeContext:
                        {
                            bool fStatement = false;
                            bool fMemberDeclaration = false;

                            foreach (SyntaxNode ancestor in node.AncestorsAndSelf())
                            {
                                if (fStatement
                                    && fMemberDeclaration)
                                {
                                    break;
                                }

                                if (!fStatement
                                    && ancestor is StatementSyntax)
                                {
                                    fStatement = true;

                                    if (!Settings.IsCodeFixEnabled(CodeFixIdentifiers.WrapInUnsafeStatement))
                                        continue;

                                    var statement = (StatementSyntax)ancestor;

                                    if (statement.IsKind(SyntaxKind.Block)
                                        && statement.Parent is StatementSyntax)
                                    {
                                        statement = (StatementSyntax)statement.Parent;
                                    }

                                    if (statement.IsKind(SyntaxKind.UnsafeStatement))
                                        break;

                                    CodeAction codeAction = CodeAction.Create(
                                        "Wrap in unsafe block",
                                        cancellationToken =>
                                        {
                                            BlockSyntax block = (statement.IsKind(SyntaxKind.Block))
                                                ? (BlockSyntax)statement
                                                : SyntaxFactory.Block(statement);

                                            UnsafeStatementSyntax unsafeStatement = SyntaxFactory.UnsafeStatement(block).WithFormatterAnnotation();

                                            return context.Document.ReplaceNodeAsync(statement, unsafeStatement, context.CancellationToken);
                                        },
                                        GetEquivalenceKey(diagnostic, CodeFixIdentifiers.WrapInUnsafeStatement));

                                    context.RegisterCodeFix(codeAction, diagnostic);
                                    continue;
                                }
                                else if (!fMemberDeclaration
                                    && ancestor is MemberDeclarationSyntax)
                                {
                                    fMemberDeclaration = true;

                                    if (!Settings.IsCodeFixEnabled(CodeFixIdentifiers.AddUnsafeModifier))
                                        continue;

                                    if (!ancestor.Kind().SupportsModifiers())
                                        continue;

                                    //TODO: test
                                    ModifiersCodeFixRegistrator.AddModifier(
                                        context,
                                        diagnostic,
                                        ancestor,
                                        SyntaxKind.UnsafeKeyword,
                                        title: "Add 'unsafe' modifier to containing declaration",
                                        additionalKey: CodeFixIdentifiers.AddUnsafeModifier);
                                }
                            }

                            break;
                        }
                }
            }
        }
    }
}
