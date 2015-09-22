// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.ViewFeatures;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Extensions methods for <see cref="ModelStateDictionary"/>.
    /// </summary>
    public static class ModelStateDictionaryExtensions
    {
        /// <summary>
        /// Adds the specified <paramref name="errorMessage"/> to the <see cref="ModelState.Errors"/> instance
        /// that is associated with the specified <paramref name="expression"/> text key repressentation.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="modelState">The <see cref="ModelStateDictionary"/> instance this method extends.</param>
        /// <param name="expression">The expression of <typeparamref name="TModel"/> from
        /// which text representation will be used as a key of the <see cref="ModelState"/> to add errors to.</param>
        /// <param name="errorMessage"></param>
        public static void AddModelError<TModel>(this ModelStateDictionary modelState, Expression<Func<TModel, object>> expression, string errorMessage)
        {
            modelState.AddModelError(GetExpressionText(expression), errorMessage);
        }

        /// <summary>
        /// Adds the specified <paramref name="exception"/> to the <see cref="ModelState.Errors"/> instance
        /// that is associated with the specified <paramref name="expression"/> text key repressentation.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="modelState">The <see cref="ModelStateDictionary"/> instance this method extends.</param>
        /// <param name="expression">The expression of <typeparamref name="TModel"/> from
        /// which text representation will be used as a key of the <see cref="ModelState"/> to add errors to.</param>
        /// <param name="exception">The error message to add.</param>
        public static void AddModelError<TModel>(this ModelStateDictionary modelState, Expression<Func<TModel, object>> expression, Exception exception)
        {
            modelState.AddModelError(GetExpressionText(expression), exception);
        }

        /// <summary>
        /// Removes the specified <paramref name="expression"/> text key repressentation from the <see cref="ModelStateDictionary"/>.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="modelState">The <see cref="ModelStateDictionary"/> instance this method extends.</param>
        /// <param name="expression">The expression of <typeparamref name="TModel"/> from
        /// which text representation will be used as a key of the <see cref="ModelState"/> to add errors to.</param>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false.
        /// This method also returns false if expression's text key was not found in the model-state dictionary.
        /// </returns>
        public static bool Remove<TModel>(this ModelStateDictionary modelState, Expression<Func<TModel, object>> expression)
        {
            return modelState.Remove(GetExpressionText(expression));
        }

        private static string GetExpressionText(LambdaExpression expression)
        {
            var unaryExpression = expression.Body as UnaryExpression;

            if (IsConvertToObject(unaryExpression))
                return ExpressionHelper.GetExpressionText(Expression.Lambda(unaryExpression.Operand, expression.Parameters[0]));

            return ExpressionHelper.GetExpressionText(expression);
        }
        private static bool IsConvertToObject(UnaryExpression expression)
        {
            return expression?.NodeType == ExpressionType.Convert &&
                expression.Operand is MemberExpression &&
                expression.Type == typeof(object);
        }
    }
}
