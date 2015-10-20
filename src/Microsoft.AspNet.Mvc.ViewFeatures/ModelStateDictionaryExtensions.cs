﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNet.Mvc.ViewFeatures;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// Extensions methods for <see cref="ModelStateDictionary"/>.
    /// </summary>
    public static class ModelStateDictionaryExtensions
    {
        /// <summary>
        /// Adds the specified <paramref name="errorMessage"/> to the <see cref="ModelStateEntry.Errors"/> instance
        /// that is associated with the specified <paramref name="expression"/>.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="modelState">The <see cref="ModelStateDictionary"/> instance this method extends.</param>
        /// <param name="expression">An expression to be evaluated against an item in the current model.</param>
        /// <param name="errorMessage">The error message to add.</param>
        public static void AddModelError<TModel>(
            this ModelStateDictionary modelState,
            Expression<Func<TModel, object>> expression,
            string errorMessage)
        {
            modelState.AddModelError(GetExpressionText(expression), errorMessage);
        }

        /// <summary>
        /// Adds the specified <paramref name="exception"/> to the <see cref="ModelStateEntry.Errors"/> instance
        /// that is associated with the specified <paramref name="expression"/>.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="modelState">The <see cref="ModelStateDictionary"/> instance this method extends.</param>
        /// <param name="expression">An expression to be evaluated against an item in the current model.</param>
        /// <param name="exception">The <see cref="Exception"/> to add.</param>
        public static void AddModelError<TModel>(
            this ModelStateDictionary modelState,
            Expression<Func<TModel, object>> expression,
            Exception exception)
        {
            modelState.AddModelError(GetExpressionText(expression), exception);
        }

        /// <summary>
        /// Removes the specified <paramref name="expression"/> from the <see cref="ModelStateDictionary"/>.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="modelState">The <see cref="ModelStateDictionary"/> instance this method extends.</param>
        /// <param name="expression">An expression to be evaluated against an item in the current model.</param>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false.
        /// This method also returns false if <paramref name="expression"/> was not found in the model-state dictionary.
        /// </returns>
        public static bool Remove<TModel>(
            this ModelStateDictionary modelState,
            Expression<Func<TModel, object>> expression)
        {
            return modelState.Remove(GetExpressionText(expression));
        }

        /// <summary>
        /// Removes all the entries for the specified <paramref name="expression"/> from the
        /// <see cref="ModelStateDictionary"/>.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="modelState">The <see cref="ModelStateDictionary"/> instance this method extends.</param>
        /// <param name="expression">An expression to be evaluated against an item in the current model.</param>
        public static void RemoveAll<TModel>(
            this ModelStateDictionary modelState,
            Expression<Func<TModel, object>> expression)
        {
            string modelKey = GetExpressionText(expression);
            if (string.IsNullOrEmpty(modelKey))
            {
                var modelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(TModel));

                foreach (var property in modelMetadata.Properties)
                {
                    var childKey = property.BinderModelName ?? property.PropertyName;
                    var entries = modelState.FindKeysWithPrefix(childKey).ToArray();
                    foreach (var entry in entries)
                    {
                        modelState.Remove(entry.Key);
                    }
                }
            }
            else
            {
                var entries = modelState.FindKeysWithPrefix(modelKey).ToArray();
                foreach (var entry in entries)
                {
                    modelState.Remove(entry.Key);
                }
            }
        }

        private static string GetExpressionText(LambdaExpression expression)
        {
            // We check if expression is wrapped with conversion to object expression
            // and unwrap it if necessary, because Expression<Func<TModel, object>>
            // automatically creates a convert to object expression for expresions
            // returning value types
            var unaryExpression = expression.Body as UnaryExpression;

            if (IsConversionToObject(unaryExpression))
            {
                return ExpressionHelper.GetExpressionText(Expression.Lambda(
                    unaryExpression.Operand,
                    expression.Parameters[0]));
            }

            return ExpressionHelper.GetExpressionText(expression);
        }

        private static bool IsConversionToObject(UnaryExpression expression)
        {
            return expression?.NodeType == ExpressionType.Convert &&
                expression.Operand?.NodeType == ExpressionType.MemberAccess &&
                expression.Type == typeof(object);
        }
    }
}
