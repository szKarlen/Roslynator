﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp
{
    internal static class CSharpSyntaxParts
    {
        public static MethodDeclarationSyntax ObjectEqualsMethodDeclaration(
            TypeSyntax type,
            string parameterName = "obj",
            string localName = "other")
        {
            return MethodDeclaration(
                Modifiers.PublicOverride(),
                BoolType(),
                Identifier("Equals"),
                ParameterList(Parameter(ObjectType(), parameterName)),
                Block(
                    IfNotReturnFalse(
                        IsPatternExpression(
                            IdentifierName(parameterName),
                            DeclarationPattern(
                                type,
                                SingleVariableDesignation(Identifier(localName))))),
                    ThrowNewNotImplementedExceptionStatement()));
        }

        public static MethodDeclarationSyntax ObjectGetHashCodeMethodDeclaration()
        {
            return MethodDeclaration(
                Modifiers.PublicOverride(),
                IntType(),
                "GetHashCode",
                ParameterList(),
                Block(ThrowNewNotImplementedExceptionStatement()));
        }

        #region If
        public static IfStatementSyntax IfReturn(ExpressionSyntax expression)
        {
            return IfStatement(expression, Block(ReturnStatement()));
        }

        public static IfStatementSyntax IfReturnNull(ExpressionSyntax expression)
        {
            return IfStatement(expression, Block(ReturnNull()));
        }

        public static IfStatementSyntax IfReturnTrue(ExpressionSyntax expression)
        {
            return IfStatement(expression, Block(ReturnTrue()));
        }

        public static IfStatementSyntax IfReturnFalse(ExpressionSyntax expression)
        {
            return IfStatement(expression, Block(ReturnFalse()));
        }
        #endregion If

        #region IfNull
        public static IfStatementSyntax IfNull(ExpressionSyntax expression, StatementSyntax statement)
        {
            return IfStatement(EqualsToNull(expression), statement);
        }

        public static IfStatementSyntax IfNullReturn(ExpressionSyntax expression)
        {
            return IfNull(expression, Block(ReturnStatement()));
        }

        public static IfStatementSyntax IfNullReturnNull(ExpressionSyntax expression)
        {
            return IfNull(expression, Block(ReturnNull()));
        }

        public static IfStatementSyntax IfNullReturnTrue(ExpressionSyntax expression)
        {
            return IfNull(expression, Block(ReturnTrue()));
        }

        public static IfStatementSyntax IfNullReturnFalse(ExpressionSyntax expression)
        {
            return IfNull(expression, Block(ReturnFalse()));
        }
        #endregion IfNull

        #region IfNot
        public static IfStatementSyntax IfNot(ExpressionSyntax expression, StatementSyntax statement)
        {
            return IfStatement(LogicalNotExpression(expression.Parenthesize()), statement);
        }

        public static IfStatementSyntax IfNotReturn(ExpressionSyntax expression)
        {
            return IfNot(expression, Block(ReturnStatement()));
        }

        public static IfStatementSyntax IfNotReturnNull(ExpressionSyntax expression)
        {
            return IfNot(expression, Block(ReturnNull()));
        }

        public static IfStatementSyntax IfNotReturnTrue(ExpressionSyntax expression)
        {
            return IfNot(expression, Block(ReturnTrue()));
        }

        public static IfStatementSyntax IfNotReturnFalse(ExpressionSyntax expression)
        {
            return IfNot(expression, Block(ReturnFalse()));
        }
        #endregion IfNot

        #region IfNotNull
        public static IfStatementSyntax IfNotNull(ExpressionSyntax expression, StatementSyntax statement)
        {
            return IfStatement(NotEqualsToNull(expression), statement);
        }

        public static IfStatementSyntax IfNotNullReturn(ExpressionSyntax expression)
        {
            return IfNotNull(expression, Block(ReturnStatement()));
        }

        public static IfStatementSyntax IfNotNullReturnNull(ExpressionSyntax expression)
        {
            return IfNotNull(expression, Block(ReturnNull()));
        }

        public static IfStatementSyntax IfNotNullReturnTrue(ExpressionSyntax expression)
        {
            return IfNotNull(expression, Block(ReturnTrue()));
        }

        public static IfStatementSyntax IfNotNullReturnFalse(ExpressionSyntax expression)
        {
            return IfNotNull(expression, Block(ReturnFalse()));
        }
        #endregion IfNotNull

        public static BinaryExpressionSyntax EqualsToNull(ExpressionSyntax expression)
        {
            return EqualsExpression(expression, NullLiteralExpression());
        }

        public static BinaryExpressionSyntax NotEqualsToNull(ExpressionSyntax expression)
        {
            return NotEqualsExpression(expression, NullLiteralExpression());
        }

        public static ReturnStatementSyntax ReturnNull()
        {
            return ReturnStatement(NullLiteralExpression());
        }

        public static ReturnStatementSyntax ReturnTrue()
        {
            return ReturnStatement(TrueLiteralExpression());
        }

        public static ReturnStatementSyntax ReturnFalse()
        {
            return ReturnStatement(FalseLiteralExpression());
        }

        public static ThrowExpressionSyntax ThrowNewNotImplementedExceptionExpression()
        {
            return ThrowExpression(ObjectCreationExpression(ParseTypeName(MetadataNames.System_NotImplementedException).WithSimplifierAnnotation(), ArgumentList()));
        }

        public static ThrowStatementSyntax ThrowNewNotImplementedExceptionStatement()
        {
            return ThrowStatement(ObjectCreationExpression(ParseTypeName(MetadataNames.System_NotImplementedException).WithSimplifierAnnotation(), ArgumentList()));
        }
    }
}