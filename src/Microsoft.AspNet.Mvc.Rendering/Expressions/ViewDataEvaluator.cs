// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Microsoft.AspNet.Mvc.Rendering.Expressions
{
    public static class ViewDataEvaluator
    {
        public static string Eval([NotNull] ViewData viewData, [NotNull] string expression, string format)
        {
            var value = Eval(viewData, expression);
            return FormatValue(value, format);
        }

        public static object Eval([NotNull] ViewData viewData, [NotNull] string expression)
        {
            var info = GetViewDataInfo(viewData, expression);
            return (info != null) ? info.Value : null;
        }

        public static string FormatValue(object value, string format)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(format))
            {
                return Convert.ToString(value, CultureInfo.CurrentCulture);
            }
            else
            {
                return string.Format(CultureInfo.CurrentCulture, format, value);
            }
        }

        public static ViewDataInfo GetViewDataInfo([NotNull] ViewData viewData, [NotNull] string expression)
        {
            if (string.IsNullOrEmpty(expression))
            {
                throw new ArgumentException(Resources.FormatArgumentNullOrEmpty("expression"));
            }

            // Given an expression "one.two.three.four" we look up the following (pseudocode):
            //  this["one.two.three.four"]
            //  this["one.two.three"]["four"]
            //  this["one.two"]["three.four]
            //  this["one.two"]["three"]["four"]
            //  this["one"]["two.three.four"]
            //  this["one"]["two.three"]["four"]
            //  this["one"]["two"]["three.four"]
            //  this["one"]["two"]["three"]["four"]

            return EvalComplexExpression(viewData, expression);
        }

        private static ViewDataInfo EvalComplexExpression(object indexableObject, string expression)
        {
            foreach (var expressionPair in GetRightToLeftExpressions(expression))
            {
                var subExpression = expressionPair.Left;
                var postExpression = expressionPair.Right;

                var subTargetInfo = GetPropertyValue(indexableObject, subExpression);
                if (subTargetInfo != null)
                {
                    if (string.IsNullOrEmpty(postExpression))
                    {
                        return subTargetInfo;
                    }

                    if (subTargetInfo.Value != null)
                    {
                        var potential = EvalComplexExpression(subTargetInfo.Value, postExpression);
                        if (potential != null)
                        {
                            return potential;
                        }
                    }
                }
            }

            return null;
        }

        private static IEnumerable<ExpressionPair> GetRightToLeftExpressions(string expression)
        {
            // Produces an enumeration of all the combinations of complex property names
            // given a complex expression. See the list above for an example of the result
            // of the enumeration.

            yield return new ExpressionPair(expression, string.Empty);

            var lastDot = expression.LastIndexOf('.');

            var subExpression = expression;
            var postExpression = string.Empty;

            while (lastDot > -1)
            {
                subExpression = expression.Substring(0, lastDot);
                postExpression = expression.Substring(lastDot + 1);
                yield return new ExpressionPair(subExpression, postExpression);

                lastDot = subExpression.LastIndexOf('.');
            }
        }

        private static ViewDataInfo GetPropertyValue(object container, string propertyName)
        {
            // This method handles one "segment" of a complex property expression

            // First, we try to evaluate the property based on its indexer
            var value = GetIndexedPropertyValue(container, propertyName);
            if (value != null)
            {
                return value;
            }

            // If the indexer didn't return anything useful, continue...

            // If the container is a ViewData then treat its Model property
            // as the container instead of the ViewData itself.
            var viewData = container as ViewData;
            if (viewData != null)
            {
                container = viewData.Model;
            }

            // If the container is null, we're out of options
            if (container == null)
            {
                return null;
            }

            // Finally try to use PropertyInfo and treat the expression as a property name
            var propertyInfo = container.GetType().GetRuntimeProperty(propertyName);
            if (propertyInfo == null)
            {
                return null;
            }

            return new ViewDataInfo(container, propertyInfo, () => propertyInfo.GetValue(container));
        }

        private static ViewDataInfo GetIndexedPropertyValue(object indexableObject, string key)
        {
            var viewData = indexableObject as ViewData;
            var dict = indexableObject as IDictionary<string, object>;
            object value = null;
            var success = false;

            if (viewData != null)
            {
                success = viewData.TryGetValue(key, out value);
            }
            else if (dict != null)
            {
                success = dict.TryGetValue(key, out value);
            }

            if (!success)
            {
                // Fall back to TryGetValue() calls for other Dictionary types.
                var tryDelegate = TryGetValueProvider.CreateInstance(indexableObject.GetType());
                if (tryDelegate != null)
                {
                    success = tryDelegate(indexableObject, key, out value);
                }
            }

            if (success)
            {
                return new ViewDataInfo(indexableObject, value);
            }

            return null;
        }

        private struct ExpressionPair
        {
            public readonly string Left;
            public readonly string Right;

            public ExpressionPair(string left, string right)
            {
                Left = left;
                Right = right;
            }
        }
    }
}
