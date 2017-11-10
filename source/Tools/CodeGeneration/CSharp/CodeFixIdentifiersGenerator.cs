﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp;
using Roslynator.CSharp.CodeFixes;
using Roslynator.Metadata;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CodeGeneration.CSharp
{
    public static class CodeFixIdentifiersGenerator
    {
        public static CompilationUnitSyntax Generate(IEnumerable<CodeFixDescriptor> codeFixes, IComparer<string> comparer)
        {
            return CompilationUnit(
                UsingDirectives(),
                NamespaceDeclaration(
                    "Roslynator.CSharp.CodeFixes",
                    ClassDeclaration(
                        Modifiers.PublicStaticPartial(),
                        "CodeFixIdentifiers",
                        codeFixes
                            .OrderBy(f => f.Id, comparer)
                            .Select(f =>
                            {
                                return FieldDeclaration(
                                   Modifiers.PublicConst(),
                                   StringPredefinedType(),
                                   f.Identifier,
                                   AddExpression(IdentifierName("Prefix"), StringLiteralExpression(f.Id.Substring(CodeFixIdentifiers.Prefix.Length))));
                            })
                            .ToSyntaxList<MemberDeclarationSyntax>())));
        }
    }
}
