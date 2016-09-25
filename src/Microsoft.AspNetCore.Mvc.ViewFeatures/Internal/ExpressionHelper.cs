// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Internal
{
    public static class ExpressionHelper
    {
        public static string GetExpressionText(string expression)
        {
            // If it's exactly "model", then give them an empty string, to replicate the lambda behavior.
            return string.Equals(expression, "model", StringComparison.OrdinalIgnoreCase) ? string.Empty : expression;
        }

        public static string GetExpressionText(LambdaExpression expression)
        {
            return GetExpressionText(expression, expressionTextCache: null);
        }

        public static string GetExpressionText(LambdaExpression expression, ExpressionTextCache expressionTextCache)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            string expressionText;
            if (expressionTextCache != null &&
                expressionTextCache.Entries.TryGetValue(expression, out expressionText))
            {
                return expressionText;
            }

            // Determine size of string needed and number of segments.
            var lastIsModel = false;
            var length = 0;
            var part = expression.Body;
            var segmentCount = 0;
            var trailingMemberExpressions = 0;
            while (part != null)
            {
                switch (part.NodeType)
                {
                    case ExpressionType.Call:
                        var methodExpression = (MethodCallExpression)part;
                        if (IsSingleArgumentIndexer(methodExpression))
                        {
                            lastIsModel = false;
                            length += 4;    // allow room for [99]
                            part = methodExpression.Object;
                            segmentCount++;
                            trailingMemberExpressions = 0;
                        }
                        else
                        {
                            // Unsupported.
                            part = null;
                        }
                        break;

                    case ExpressionType.ArrayIndex:
                        var binaryExpression = (BinaryExpression)part;

                        lastIsModel = false;
                        length += 4;    // allow room for [99]
                        part = binaryExpression.Left;
                        segmentCount++;
                        trailingMemberExpressions = 0;
                        break;

                    case ExpressionType.MemberAccess:
                        var memberExpressionPart = (MemberExpression)part;
                        var name = memberExpressionPart.Member.Name;

                        // If identifier contains "__", it is "reserved for use by the implementation" and likely
                        // compiler- or Razor-generated e.g. the name of a field in a delegate's generated class.
                        if (name.Contains("__"))
                        {
                            // Exit loop.
                            part = null;
                        }
                        else
                        {
                            lastIsModel = string.Equals("model", name, StringComparison.OrdinalIgnoreCase);
                            length += name.Length + 1;
                            part = memberExpressionPart.Expression;
                            segmentCount += 2;
                            trailingMemberExpressions++;
                        }
                        break;

                    case ExpressionType.Parameter:
                        // Unsupported but indicates previous member access was not the view's Model.
                        lastIsModel = false;
                        part = null;
                        break;

                    default:
                        // Unsupported.
                        part = null;
                        break;
                }
            }

            // Expression must contain indexers if not all parts are member expressions.
            var containsIndexers = (trailingMemberExpressions * 2) != segmentCount;

            // If name would start with ".model", then strip that part away.
            if (lastIsModel)
            {
                length -= 6;    // ".model".Length
                segmentCount -= 2;
                trailingMemberExpressions--;
            }

            // Trim the leading "." if present.
            if (trailingMemberExpressions > 0)
            {
                length--;
                segmentCount--;
            }

            Debug.Assert(segmentCount >= 0);
            if (segmentCount == 0)
            {
                Debug.Assert(!containsIndexers);
                if (expressionTextCache != null)
                {
                    expressionTextCache.Entries.TryAdd(expression, string.Empty);
                }

                return string.Empty;
            }

            var builder = new StringBuilder(length);
            part = expression.Body;
            while (part != null && segmentCount > 0)
            {
                switch (part.NodeType)
                {
                    case ExpressionType.Call:
                        Debug.Assert(containsIndexers);
                        var methodExpression = (MethodCallExpression)part;

                        InsertIndexerInvocationText(builder, methodExpression.Arguments.Single(), expression);
                        segmentCount--;

                        part = methodExpression.Object;
                        break;

                    case ExpressionType.ArrayIndex:
                        Debug.Assert(containsIndexers);
                        var binaryExpression = (BinaryExpression)part;

                        InsertIndexerInvocationText(builder, binaryExpression.Right, expression);
                        segmentCount--;

                        part = binaryExpression.Left;
                        break;

                    case ExpressionType.MemberAccess:
                        var memberExpression = (MemberExpression)part;
                        var name = memberExpression.Member.Name;
                        Debug.Assert(!name.Contains("__"));

                        builder.Insert(0, name);
                        segmentCount--;
                        if (segmentCount > 0)
                        {
                            builder.Insert(0, '.');
                            segmentCount--;
                        }

                        part = memberExpression.Expression;
                        break;

                    default:
                        // Should be unreachable due to handling in above loop.
                        Debug.Assert(false);
                        break;
                }
            }

            Debug.Assert(segmentCount == 0);
            expressionText = builder.ToString();
            if (expressionTextCache != null && !containsIndexers)
            {
                expressionTextCache.Entries.TryAdd(expression, expressionText);
            }

            return expressionText;
        }

        private static void InsertIndexerInvocationText(
            StringBuilder builder,
            Expression indexExpression,
            LambdaExpression parentExpression)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

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

            builder.Insert(0, ']');
            builder.Insert(0, Convert.ToString(func(null), CultureInfo.InvariantCulture));
            builder.Insert(0, '[');
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
