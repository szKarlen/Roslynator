// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Syntax
{
    //TODO: 
    public struct EmbeddedStatementInfo
    {
        private static EmbeddedStatementInfo Default { get; } = new EmbeddedStatementInfo();

        private EmbeddedStatementInfo(
            StatementSyntax embeddedStatement,
            SyntaxNode containingNode)
        {
            EmbeddedStatement = embeddedStatement;

            ContainingNode = containingNode;
        }

        public StatementSyntax EmbeddedStatement { get; }

        public SyntaxNode ContainingNode { get; }

        public bool Success
        {
            get { return EmbeddedStatement != null; }
        }

        internal static EmbeddedStatementInfo Create(StatementSyntax statement)
        {
            if (statement == null)
                return Default;

            SyntaxNode parent = statement.Parent;

            switch (parent?.Kind())
            {
                case SyntaxKind.IfStatement:
                case SyntaxKind.ElseClause:
                case SyntaxKind.ForEachStatement:
                case SyntaxKind.ForEachVariableStatement:
                case SyntaxKind.ForStatement:
                case SyntaxKind.UsingStatement:
                case SyntaxKind.WhileStatement:
                case SyntaxKind.DoStatement:
                case SyntaxKind.LockStatement:
                case SyntaxKind.FixedStatement:
                    return new EmbeddedStatementInfo(statement, parent);
            }

            return Default;
        }

        public override string ToString()
        {
            return EmbeddedStatement?.ToString() ?? base.ToString();
        }
    }
}
