// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;

namespace Microsoft.AspNet.Mvc.Rendering
{
    /// <summary>
    /// Value-related extensions for <see cref="IHtmlHelper"/> and <see cref="IHtmlHelper{TModel}"/>.
    /// </summary>
    public static class HtmlHelperValueExtensions
    {
        /// <summary>
        /// Returns the formatted value for the specified <paramref name="expression"/>.
        /// </summary>
        /// <param name="htmlHelper">The <see cref="IHtmlHelper"/> instance this method extends.</param>
        /// <param name="expression">Expression name, relative to the current model.</param>
        /// <returns>A <see cref="string"/> containing the formatted value.</returns>
        /// <remarks>
        /// Converts the expression result to a <see cref="string"/> directly.
        /// </remarks>
        public static string Value(this IHtmlHelper htmlHelper, string expression)
        {
            if (htmlHelper == null)
            {
                throw new ArgumentNullException(nameof(htmlHelper));
            }

            return htmlHelper.Value(expression, format: null);
        }

        /// <summary>
        /// Returns the formatted value for the specified <paramref name="expression"/>.
        /// </summary>
        /// <param name="htmlHelper">The <see cref="IHtmlHelper{TModel}"/> instance this method extends.</param>
        /// <param name="expression">An expression to be evaluated against the current model.</param>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TResult">The type of the <paramref name="expression"/> result.</typeparam>
        /// <returns>A <see cref="string"/> containing the formatted value.</returns>
        /// <remarks>
        /// Converts the <paramref name="expression"/> result to a <see cref="string"/> directly.
        /// </remarks>
        public static string ValueFor<TModel, TResult>(
            this IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TResult>> expression)
        {
            if (htmlHelper == null)
            {
                throw new ArgumentNullException(nameof(htmlHelper));
            }

            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            return htmlHelper.ValueFor(expression, format: null);
        }

        /// <summary>
        /// Returns the formatted value for the current model.
        /// </summary>
        /// <param name="htmlHelper">The <see cref="IHtmlHelper"/> instance this method extends.</param>
        /// <returns>A <see cref="string"/> containing the formatted value.</returns>
        /// <remarks>
        /// Converts the model value to a <see cref="string"/> directly.
        /// </remarks>
        public static string ValueForModel(this IHtmlHelper htmlHelper)
        {
            if (htmlHelper == null)
            {
                throw new ArgumentNullException(nameof(htmlHelper));
            }

            return htmlHelper.Value(expression: null, format: null);
        }

        /// <summary>
        /// Returns the formatted value for the current model.
        /// </summary>
        /// <param name="htmlHelper">The <see cref="IHtmlHelper"/> instance this method extends.</param>
        /// <param name="format">
        /// The composite format <see cref="string"/> (see http://msdn.microsoft.com/en-us/library/txafckwd.aspx).
        /// </param>
        /// <returns>A <see cref="string"/> containing the formatted value.</returns>
        /// <remarks>
        /// Converts the model value to a <see cref="string"/> directly if
        /// <paramref name="format"/> is <c>null</c> or empty.
        /// </remarks>
        public static string ValueForModel(this IHtmlHelper htmlHelper, string format)
        {
            if (htmlHelper == null)
            {
                throw new ArgumentNullException(nameof(htmlHelper));
            }

            return htmlHelper.Value(expression: null, format: format);
        }
    }
}
