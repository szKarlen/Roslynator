﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.Analysis;

namespace Roslynator.CSharp.DiagnosticAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UseVarInForEachDiagnosticAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.UseVarInsteadOfExplicitTypeInForEach); }
        }

        public override void Initialize(AnalysisContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            base.Initialize(context);

            context.RegisterSyntaxNodeAction(AnalyzeForEachStatement, SyntaxKind.ForEachStatement);
        }

        private static void AnalyzeForEachStatement(SyntaxNodeAnalysisContext context)
        {
            var forEachStatement = (ForEachStatementSyntax)context.Node;

            TypeAnalysisFlags flags = CSharpAnalysis.AnalyzeType(forEachStatement, context.SemanticModel);

            if (!flags.IsExplicit()
                || !flags.SupportsImplicit())
            {
                return;
            }

            context.ReportDiagnostic(DiagnosticDescriptors.UseVarInsteadOfExplicitTypeInForEach, forEachStatement.Type);
        }
    }
}
