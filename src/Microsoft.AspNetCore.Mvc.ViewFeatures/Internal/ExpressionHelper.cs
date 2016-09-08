// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Internal
{
    public static class ExpressionHelper
    {
        public static string GetExpressionText(string expression)
        {
            // If it's exactly "model", then give them an empty string, to replicate the lambda behavior.
            return string.Equals(expression, "model", StringComparison.OrdinalIgnoreCase) ? string.Empty : expression;
        }

        public static StringValuesTutu GetExpressionText(LambdaExpression expression)
        {
            return GetExpressionText(expression, expressionTextCache: null);
        }

        public static StringValuesTutu GetExpressionText(
            LambdaExpression expression,
            ExpressionTextCache expressionTextCache)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            StringValuesTutu stringValues;
            if (expressionTextCache != null &&
                expressionTextCache.Entries.TryGetValue(expression, out stringValues))
            {
                return stringValues;
            }

            stringValues = StringValuesTutu.Empty;
            var containsIndexers = false;
            var part = expression.Body;
            while (part != null)
            {
                if (part.NodeType == ExpressionType.Call)
                {
                    containsIndexers = true;
                    var methodExpression = (MethodCallExpression)part;
                    if (!IsSingleArgumentIndexer(methodExpression))
                    {
                        // Unsupported.
                        break;
                    }

                    stringValues = InsertIndexerInvocationText(
                        stringValues,
                        methodExpression.Arguments.Single(),
                        expression);

                    part = methodExpression.Object;
                }
                else if (part.NodeType == ExpressionType.ArrayIndex)
                {
                    containsIndexers = true;
                    var binaryExpression = (BinaryExpression)part;

                    stringValues = InsertIndexerInvocationText(
                        stringValues,
                        binaryExpression.Right,
                        expression);

                    part = binaryExpression.Left;
                }
                else if (part.NodeType == ExpressionType.MemberAccess)
                {
                    var memberExpressionPart = (MemberExpression)part;
                    var name = memberExpressionPart.Member.Name;

                    // If identifier contains "__", it is "reserved for use by the implementation" and likely compiler-
                    // or Razor-generated e.g. the name of a field in a delegate's generated class.
                    if (name.Contains("__"))
                    {
                        // Exit loop. Should have the entire name because previous MemberAccess has same name as the
                        // leftmost expression node (a variable).
                        break;
                    }

                    stringValues = StringValuesTutu.Concat(".", name, stringValues);
                    part = memberExpressionPart.Expression;
                }
                else
                {
                    break;
                }
            }

            // If parts start with "model", then strip that part away.
            var removeSegments = 0;
            if ((part == null || part.NodeType != ExpressionType.Parameter) &&
                stringValues.Count >= 2 &&
                string.Equals(".", stringValues[0], StringComparison.Ordinal) &&
                string.Equals("model", stringValues[1], StringComparison.OrdinalIgnoreCase))
            {
                removeSegments = 2;
            }

            // Trim the leading "." if present.
            if (removeSegments < stringValues.Count &&
                string.Equals(".", stringValues[removeSegments], StringComparison.Ordinal))
            {
                removeSegments++;
            }

            // Remove the leading segments found just above.
            if (removeSegments != 0)
            {
                if (removeSegments == stringValues.Count)
                {
                    stringValues = StringValuesTutu.Empty;
                }
                else
                {
                    var oldValues = stringValues.ToArray();
                    var newValues = new string[stringValues.Count - removeSegments];
                    Array.Copy(oldValues, removeSegments, newValues, destinationIndex: 0, length: newValues.Length);

                    stringValues = new StringValuesTutu(newValues);
                }
            }

            if (expressionTextCache != null && !containsIndexers)
            {
                expressionTextCache.Entries.TryAdd(expression, stringValues);
            }

            return stringValues;
        }

        private static StringValuesTutu InsertIndexerInvocationText(
            StringValuesTutu stringValues,
            Expression indexExpression,
            LambdaExpression parentExpression)
        {
            if (indexExpression == null)
            {
                throw new ArgumentNullException(nameof(indexExpression));
            }

            if (parentExpression == null)
            {
                throw new ArgumentNullException(nameof(parentExpression));
            }

            if (parentExpression.Parameters == null)
            {
                throw new ArgumentException(Resources.FormatPropertyOfTypeCannotBeNull(
                    nameof(parentExpression.Parameters),
                    nameof(parentExpression)));
            }

            var converted = Expression.Convert(indexExpression, typeof(object));
            var fakeParameter = Expression.Parameter(typeof(object), null);
            var lambda = Expression.Lambda<Func<object, object>>(converted, fakeParameter);
            Func<object, object> func;

            try
            {
                func = CachedExpressionCompiler.Process(lambda);
            }
            catch (InvalidOperationException ex)
            {
                var parameters = parentExpression.Parameters.ToArray();
                throw new InvalidOperationException(
                    Resources.FormatExpressionHelper_InvalidIndexerExpression(indexExpression, parameters[0].Name),
                    ex);
            }

            return StringValuesTutu.Concat(
                "[",
                Convert.ToString(func(null), CultureInfo.InvariantCulture),
                "]",
                stringValues);
        }

        public static bool IsSingleArgumentIndexer(Expression expression)
        {
            var methodExpression = expression as MethodCallExpression;
            if (methodExpression == null || methodExpression.Arguments.Count != 1)
            {
                return false;
            }

            // Check whether GetDefaultMembers() (if present in CoreCLR) would return a member of this type. Compiler
            // names the indexer property, if any, in a generated [DefaultMember] attribute for the containing type.
            var declaringType = methodExpression.Method.DeclaringType;
            var defaultMember = declaringType.GetTypeInfo().GetCustomAttribute<DefaultMemberAttribute>(inherit: true);
            if (defaultMember == null)
            {
                return false;
            }

            // Find default property (the indexer) and confirm its getter is the method in this expression.
            var runtimeProperties = declaringType.GetRuntimeProperties();
            foreach (var property in runtimeProperties)
            {
                if ((string.Equals(defaultMember.MemberName, property.Name, StringComparison.Ordinal) &&
                    property.GetMethod == methodExpression.Method))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
