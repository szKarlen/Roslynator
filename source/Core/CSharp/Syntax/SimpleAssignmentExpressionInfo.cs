// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Roslynator.CSharp.Syntax.SyntaxInfoHelpers;

namespace Roslynator.CSharp.Syntax
{
    public struct SimpleAssignmentExpressionInfo
    {
        private static SimpleAssignmentExpressionInfo Default { get; } = new SimpleAssignmentExpressionInfo();

        private SimpleAssignmentExpressionInfo(
            AssignmentExpressionSyntax assignmentExpression,
            ExpressionSyntax left,
            ExpressionSyntax right)
        {
            AssignmentExpression = assignmentExpression;
            Left = left;
            Right = right;
        }

        public AssignmentExpressionSyntax AssignmentExpression { get; }

        public ExpressionSyntax Left { get; }

        public ExpressionSyntax Right { get; }

        public bool Success
        {
            get { return AssignmentExpression != null; }
        }

        internal static SimpleAssignmentExpressionInfo Create(
            SyntaxNode node,
            bool allowMissing = false,
            bool walkDownParentheses = true)
        {
            return CreateCore(Walk(node, walkDownParentheses) as AssignmentExpressionSyntax, allowMissing, walkDownParentheses);
        }

        internal static SimpleAssignmentExpressionInfo Create(
            AssignmentExpressionSyntax assignmentExpression,
            bool allowMissing = false,
            bool walkDownParentheses = true)
        {
            return CreateCore(assignmentExpression, allowMissing, walkDownParentheses);
        }

        internal static SimpleAssignmentExpressionInfo CreateCore(
            AssignmentExpressionSyntax assignmentExpression,
            bool allowMissing = false,
            bool walkDownParentheses = true)
        {
            if (assignmentExpression?.Kind() != SyntaxKind.SimpleAssignmentExpression)
                return Default;

            ExpressionSyntax left = WalkAndCheck(assignmentExpression.Left, allowMissing, walkDownParentheses);

            if (left == null)
                return Default;

            ExpressionSyntax right = WalkAndCheck(assignmentExpression.Right, allowMissing, walkDownParentheses);

            if (right == null)
                return Default;

            return new SimpleAssignmentExpressionInfo(assignmentExpression, left, right);
        }

        public override string ToString()
        {
            return AssignmentExpression?.ToString() ?? base.ToString();
        }
    }
}
