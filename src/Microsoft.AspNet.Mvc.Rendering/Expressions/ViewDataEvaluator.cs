using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Microsoft.AspNet.Mvc.Rendering.Expressions
{
    public static class ViewDataEvaluator
    {
        public static ViewDataInfo Eval(ViewDataDictionary vdd, string expression)
        {
            //Given an expression "foo.bar.baz" we look up the following (pseudocode):
            //  this["foo.bar.baz.quux"]
            //  this["foo.bar.baz"]["quux"]
            //  this["foo.bar"]["baz.quux]
            //  this["foo.bar"]["baz"]["quux"]
            //  this["foo"]["bar.baz.quux"]
            //  this["foo"]["bar.baz"]["quux"]
            //  this["foo"]["bar"]["baz.quux"]
            //  this["foo"]["bar"]["baz"]["quux"]

            ViewDataInfo evaluated = EvalComplexExpression(vdd, expression);
            return evaluated;
        }

        private static ViewDataInfo EvalComplexExpression(object indexableObject, string expression)
        {
            foreach (ExpressionPair expressionPair in GetRightToLeftExpressions(expression))
            {
                string subExpression = expressionPair.Left;
                string postExpression = expressionPair.Right;

                ViewDataInfo subTargetInfo = GetPropertyValue(indexableObject, subExpression);
                if (subTargetInfo != null)
                {
                    if (String.IsNullOrEmpty(postExpression))
                    {
                        return subTargetInfo;
                    }

                    if (subTargetInfo.Value != null)
                    {
                        ViewDataInfo potential = EvalComplexExpression(subTargetInfo.Value, postExpression);
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

            yield return new ExpressionPair(expression, String.Empty);

            int lastDot = expression.LastIndexOf('.');

            string subExpression = expression;
            string postExpression = String.Empty;

            while (lastDot > -1)
            {
                subExpression = expression.Substring(0, lastDot);
                postExpression = expression.Substring(lastDot + 1);
                yield return new ExpressionPair(subExpression, postExpression);

                lastDot = subExpression.LastIndexOf('.');
            }
        }

        private static ViewDataInfo GetIndexedPropertyValue(object indexableObject, string key)
        {
            IDictionary<string, object> dict = indexableObject as IDictionary<string, object>;
            object value = null;
            bool success = false;

            if (dict != null)
            {
                success = dict.TryGetValue(key, out value);
            }
            else
            {
                TryGetValueDelegate tgvDel = TypeHelpers.CreateTryGetValueDelegate(indexableObject.GetType());
                if (tgvDel != null)
                {
                    success = tgvDel(indexableObject, key, out value);
                }
            }

            if (success)
            {
                return new ViewDataInfo()
                {
                    Container = indexableObject,
                    Value = value
                };
            }

            return null;
        }

        private static ViewDataInfo GetPropertyValue(object container, string propertyName)
        {
            // This method handles one "segment" of a complex property expression

            // First, we try to evaluate the property based on its indexer
            ViewDataInfo value = GetIndexedPropertyValue(container, propertyName);
            if (value != null)
            {
                return value;
            }

            // If the indexer didn't return anything useful, continue...

            // If the container is a ViewDataDictionary then treat its Model property
            // as the container instead of the ViewDataDictionary itself.
            ViewDataDictionary vdd = container as ViewDataDictionary;
            if (vdd != null)
            {
                container = vdd.Model;
            }

            // If the container is null, we're out of options
            if (container == null)
            {
                return null;
            }

            // Second, we try to use PropertyDescriptors and treat the expression as a property name
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(container).Find(propertyName, true);
            if (descriptor == null)
            {
                return null;
            }

            return new ViewDataInfo(() => descriptor.GetValue(container))
            {
                Container = container,
                PropertyDescriptor = descriptor
            };
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