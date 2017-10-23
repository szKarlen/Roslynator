// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.Refactorings
{
    internal static class OverridingMemberCannotChangeParamsModifierRefactoring
    {
        public static void AnalyzeMethodSymbol(SymbolAnalysisContext context)
        {
            var symbol = (IMethodSymbol)context.Symbol;

            IMethodSymbol baseSymbol = symbol.OverriddenMethod;

            if (baseSymbol == null)
            {
                return;
            }

            IParameterSymbol baseParameterSymbol = baseSymbol.Parameters.LastOrDefault();

            if (baseParameterSymbol == null)
            {
                return;
            }

            MethodDeclarationSyntax methodDeclaration;
            if (!symbol.TryGetSyntax(out methodDeclaration))
            {
                return;
            }

            ParameterSyntax parameter = methodDeclaration.ParameterList?.Parameters.LastOrDefault();

            if (parameter == null
                || parameter.IsParams() == baseParameterSymbol.IsParams)
            {
                return;
            }

            context.ReportDiagnostic(DiagnosticDescriptors.OverridingMemberCannotChangeParamsModifier, parameter);
        }

        public static void AnalyzePropertySymbol(SymbolAnalysisContext context)
        {
            var symbol = (IPropertySymbol)context.Symbol;

            if (!symbol.IsIndexer)
            {
                return;
            }

            IPropertySymbol baseSymbol = symbol.OverriddenProperty;

            if (baseSymbol == null)
            {
                return;
            }

            IParameterSymbol baseParameterSymbol = baseSymbol.Parameters.LastOrDefault();

            if (baseParameterSymbol == null)
            {
                return;
            }

            IndexerDeclarationSyntax indexerDeclaration;
            if (!symbol.TryGetSyntax(out indexerDeclaration))
            {
                return;
            }

            ParameterSyntax parameter = indexerDeclaration.ParameterList?.Parameters.LastOrDefault();

            if (parameter == null
                || parameter.IsParams() == baseParameterSymbol.IsParams)
            {
                return;
            }

            context.ReportDiagnostic(DiagnosticDescriptors.OverridingMemberCannotChangeParamsModifier, parameter);
        }
    }
}
