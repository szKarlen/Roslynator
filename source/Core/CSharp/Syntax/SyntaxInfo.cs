// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Syntax
{
    public static class SyntaxInfo
    {
        public static AccessibilityInfo AccessibilityInfo(SyntaxNode node)
        {
            return Syntax.AccessibilityInfo.Create(node);
        }

        public static AsExpressionInfo AsExpressionInfo(
            SyntaxNode node,
            bool allowMissing = false,
            bool walkDownParentheses = true)
        {
            return Syntax.AsExpressionInfo.Create(
                node,
                allowMissing,
                walkDownParentheses);
        }

        public static AsExpressionInfo AsExpressionInfo(
            BinaryExpressionSyntax binaryExpression,
            bool allowMissing = false,
            bool walkDownParentheses = true)
        {
            return Syntax.AsExpressionInfo.Create(
                binaryExpression,
                allowMissing,
                walkDownParentheses);
        }

        public static BinaryExpressionChainInfo BinaryExpressionChainInfo(
            SyntaxNode node,
            SyntaxKind kind,
            bool walkDownParentheses = true)
        {
            return Syntax.BinaryExpressionChainInfo.Create(
                node,
                kind,
                walkDownParentheses);
        }

        public static BinaryExpressionChainInfo BinaryExpressionChainInfo(BinaryExpressionSyntax binaryExpression)
        {
            return Syntax.BinaryExpressionChainInfo.Create(binaryExpression);
        }

        public static BinaryExpressionInfo BinaryExpressionInfo(
            SyntaxNode node,
            bool allowMissing = false,
            bool walkDownParentheses = true)
        {
            return Syntax.BinaryExpressionInfo.Create(
                node,
                allowMissing,
                walkDownParentheses);
        }

        public static BinaryExpressionInfo BinaryExpressionInfo(
            BinaryExpressionSyntax binaryExpression,
            bool allowMissing = false,
            bool walkDownParentheses = true)
        {
            return Syntax.BinaryExpressionInfo.Create(
                binaryExpression,
                allowMissing,
                walkDownParentheses);
        }

        public static ConditionalExpressionInfo ConditionalExpressionInfo(
            SyntaxNode node,
            bool allowMissing = false,
            bool walkDownParentheses = true)
        {
            return Syntax.ConditionalExpressionInfo.Create(
                node,
                allowMissing,
                walkDownParentheses);
        }

        public static ConditionalExpressionInfo ConditionalExpressionInfo(
            ConditionalExpressionSyntax conditionalExpression,
            bool allowMissing = false,
            bool walkDownParentheses = true)
        {
            return Syntax.ConditionalExpressionInfo.Create(
                conditionalExpression,
                allowMissing,
                walkDownParentheses);
        }

        public static GenericInfo GenericInfo(TypeParameterConstraintSyntax typeParameterConstraint)
        {
            return Syntax.GenericInfo.Create(typeParameterConstraint);
        }

        public static GenericInfo GenericInfo(TypeParameterConstraintClauseSyntax constraintClause)
        {
            return Syntax.GenericInfo.Create(constraintClause);
        }

        public static GenericInfo GenericInfo(SyntaxNode declaration)
        {
            return Syntax.GenericInfo.Create(declaration);
        }

        public static GenericInfo GenericInfo(ClassDeclarationSyntax classDeclaration)
        {
            return Syntax.GenericInfo.Create(classDeclaration);
        }

        public static GenericInfo GenericInfo(DelegateDeclarationSyntax delegateDeclaration)
        {
            return Syntax.GenericInfo.Create(delegateDeclaration);
        }

        public static GenericInfo GenericInfo(InterfaceDeclarationSyntax interfaceDeclaration)
        {
            return Syntax.GenericInfo.Create(interfaceDeclaration);
        }

        public static GenericInfo GenericInfo(LocalFunctionStatementSyntax localFunctionStatement)
        {
            return Syntax.GenericInfo.Create(localFunctionStatement);
        }

        public static GenericInfo GenericInfo(MethodDeclarationSyntax methodDeclaration)
        {
            return Syntax.GenericInfo.Create(methodDeclaration);
        }

        public static GenericInfo GenericInfo(StructDeclarationSyntax structDeclaration)
        {
            return Syntax.GenericInfo.Create(structDeclaration);
        }

        public static HexadecimalLiteralInfo HexadecimalLiteralInfo(
            SyntaxNode node,
            bool walkDownParentheses = true)
        {
            return Syntax.HexadecimalLiteralInfo.Create(node, walkDownParentheses);
        }

        public static HexadecimalLiteralInfo HexadecimalLiteralInfo(LiteralExpressionSyntax literalExpression)
        {
            return Syntax.HexadecimalLiteralInfo.Create(literalExpression);
        }

        public static IfStatementInfo IfStatementInfo(IfStatementSyntax ifStatement)
        {
            return Syntax.IfStatementInfo.Create(ifStatement);
        }

        public static LocalDeclarationStatementInfo LocalDeclarationStatementInfo(
            LocalDeclarationStatementSyntax localDeclarationStatement,
            bool allowMissing = false)
        {
            return Syntax.LocalDeclarationStatementInfo.Create(localDeclarationStatement, allowMissing);
        }

        public static LocalDeclarationStatementInfo LocalDeclarationStatementInfo(
            ExpressionSyntax expression,
            bool allowMissing = false)
        {
            return Syntax.LocalDeclarationStatementInfo.Create(expression, allowMissing);
        }

        public static MemberInvocationExpressionInfo MemberInvocationExpressionInfo(
            SyntaxNode node,
            bool allowMissing = false,
            bool walkDownParentheses = true)
        {
            return Syntax.MemberInvocationExpressionInfo.Create(
                node,
                allowMissing,
                walkDownParentheses);
        }

        public static MemberInvocationExpressionInfo MemberInvocationExpressionInfo(
            InvocationExpressionSyntax invocationExpression,
            bool allowMissing = false)
        {
            return Syntax.MemberInvocationExpressionInfo.Create(
                invocationExpression,
                allowMissing);
        }

        public static MemberInvocationStatementInfo MemberInvocationStatementInfo(
            SyntaxNode node,
            bool allowMissing = false)
        {
            return Syntax.MemberInvocationStatementInfo.Create(
                node,
                allowMissing);
        }

        public static MemberInvocationStatementInfo MemberInvocationStatementInfo(
            ExpressionStatementSyntax expressionStatement,
            bool allowMissing = false)
        {
            return Syntax.MemberInvocationStatementInfo.Create(
                expressionStatement,
                allowMissing);
        }

        public static NullCheckExpressionInfo NullCheckExpressionInfo(
            SyntaxNode node,
            bool allowMissing = false,
            bool walkDownParentheses = true,
            NullCheckKind allowedKinds = NullCheckKind.All,
            SemanticModel semanticModel = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return Syntax.NullCheckExpressionInfo.Create(
                node,
                allowMissing,
                walkDownParentheses,
                allowedKinds,
                semanticModel,
                cancellationToken);
        }

        public static SimpleAssignmentExpressionInfo SimpleAssignmentExpressionInfo(
            SyntaxNode node,
            bool allowMissing = false,
            bool walkDownParentheses = true)
        {
            return Syntax.SimpleAssignmentExpressionInfo.Create(node, allowMissing, walkDownParentheses);
        }

        public static SimpleAssignmentExpressionInfo SimpleAssignmentExpressionInfo(
            AssignmentExpressionSyntax assignmentExpression,
            bool allowMissing = false,
            bool walkDownParentheses = true)
        {
            return Syntax.SimpleAssignmentExpressionInfo.Create(assignmentExpression, allowMissing, walkDownParentheses);
        }

        public static SimpleAssignmentStatementInfo SimpleAssignmentStatementInfo(
            SyntaxNode node,
            bool allowMissing = false,
            bool walkDownParentheses = true)
        {
            return Syntax.SimpleAssignmentStatementInfo.Create(node, allowMissing, walkDownParentheses);
        }

        public static SimpleAssignmentStatementInfo SimpleAssignmentStatementInfo(
            ExpressionStatementSyntax expressionStatement,
            bool allowMissing = false,
            bool walkDownParentheses = true)
        {
            return Syntax.SimpleAssignmentStatementInfo.Create(expressionStatement, allowMissing, walkDownParentheses);
        }

        public static SimpleIfElseInfo SimpleIfElseInfo(
            IfStatementSyntax ifStatement,
            bool allowMissing = false,
            bool walkDownParentheses = true)
        {
            return Syntax.SimpleIfElseInfo.Create(ifStatement, allowMissing, walkDownParentheses);
        }

        public static SimpleIfStatementInfo SimpleIfStatementInfo(
            SyntaxNode node,
            bool allowMissing = false,
            bool walkDownParentheses = true)
        {
            return Syntax.SimpleIfStatementInfo.Create(node, allowMissing, walkDownParentheses);
        }

        public static SimpleIfStatementInfo SimpleIfStatementInfo(
            IfStatementSyntax ifStatement,
            bool allowMissing = false,
            bool walkDownParentheses = true)
        {
            return Syntax.SimpleIfStatementInfo.Create(ifStatement, allowMissing, walkDownParentheses);
        }

        public static SingleLocalDeclarationStatementInfo SingleLocalDeclarationStatementInfo(
            LocalDeclarationStatementSyntax localDeclarationStatement,
            bool allowMissing = false)
        {
            return Syntax.SingleLocalDeclarationStatementInfo.Create(localDeclarationStatement, allowMissing);
        }

        public static SingleLocalDeclarationStatementInfo SingleLocalDeclarationStatementInfo(ExpressionSyntax expression)
        {
            return Syntax.SingleLocalDeclarationStatementInfo.Create(expression);
        }

        public static SingleParameterLambdaExpressionInfo SingleParameterLambdaExpressionInfo(
            SyntaxNode node,
            bool allowMissing = false,
            bool walkDownParentheses = true)
        {
            return Syntax.SingleParameterLambdaExpressionInfo.Create(node, allowMissing, walkDownParentheses);
        }

        public static SingleParameterLambdaExpressionInfo SingleParameterLambdaExpressionInfo(
            LambdaExpressionSyntax lambdaExpression,
            bool allowMissing = false)
        {
            return Syntax.SingleParameterLambdaExpressionInfo.Create(lambdaExpression, allowMissing);
        }

        public static StatementsInfo StatementsInfo(StatementSyntax statement)
        {
            return Syntax.StatementsInfo.Create(statement);
        }

        internal static StatementsInfo StatementsInfo(BlockSyntax block)
        {
            return Syntax.StatementsInfo.Create(block);
        }

        internal static StatementsInfo StatementsInfo(SwitchSectionSyntax switchSection)
        {
            return Syntax.StatementsInfo.Create(switchSection);
        }

        public static StringConcatenationExpressionInfo StringConcatenationExpressionInfo(
            BinaryExpressionSyntax binaryExpression,
            SemanticModel semanticModel,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return Syntax.StringConcatenationExpressionInfo.Create(binaryExpression, semanticModel, cancellationToken);
        }

        internal static StringConcatenationExpressionInfo StringConcatenationExpressionInfo(
            BinaryExpressionSelection binaryExpressionSelection,
            SemanticModel semanticModel,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return Syntax.StringConcatenationExpressionInfo.Create(binaryExpressionSelection, semanticModel, cancellationToken);
        }

        public static TypeParameterConstraintInfo TypeParameterConstraintInfo(TypeParameterConstraintSyntax constraint)
        {
            return Syntax.TypeParameterConstraintInfo.Create(constraint);
        }

        public static TypeParameterInfo TypeParameterInfo(TypeParameterSyntax typeParameter)
        {
            return Syntax.TypeParameterInfo.Create(typeParameter);
        }

        public static XmlElementInfo XmlElementInfo(XmlNodeSyntax xmlNode)
        {
            return Syntax.XmlElementInfo.Create(xmlNode);
        }
    }
}