// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public interface IHtmlHelper<TModel> : IHtmlHelper
    {
        /// <summary>
        /// Gets the current view data.
        /// </summary>
        ViewData<TModel> ViewData { get; }

        #region Input helpers
        /// <summary>
        /// Render an input element of type "checkbox" with value "true" and an input element of type "hidden" with
        /// value "false".
        /// </summary>
        /// <param name="expression">
        /// An expression that identifies the object that contains the properties to render.
        /// </param>
        /// <param name="htmlAttributes">
        /// <see cref="IDictionary{string, object}"/> containing additional HTML attributes.
        /// </param>
        /// <returns>New <see cref="HtmlString"/> containing the rendered HTML.</returns>
        HtmlString CheckBoxFor([NotNull] Expression<Func<TModel, bool>> expression,
            IDictionary<string, object> htmlAttributes);

        /// <summary>
        /// Render an input element of type "hidden".
        /// </summary>
        /// <param name="expression">
        /// An expression that identifies the object that contains the properties to render.
        /// </param>
        /// <param name="htmlAttributes">
        /// <see cref="IDictionary{string, object}"/> containing additional HTML attributes.
        /// </param>
        /// <returns>New <see cref="HtmlString"/> containing the rendered HTML.</returns>
        HtmlString HiddenFor<TProperty>([NotNull] Expression<Func<TModel, TProperty>> expression,
            IDictionary<string, object> htmlAttributes);

        /// <summary>
        /// Render an input element of type "password".
        /// </summary>
        /// <param name="expression">
        /// An expression that identifies the object that contains the properties to render.
        /// </param>
        /// <param name="htmlAttributes">
        /// <see cref="IDictionary{string, object}"/> containing additional HTML attributes.
        /// </param>
        /// <returns>New <see cref="HtmlString"/> containing the rendered HTML.</returns>
        HtmlString PasswordFor<TProperty>([NotNull] Expression<Func<TModel, TProperty>> expression,
            IDictionary<string, object> htmlAttributes);

        /// <summary>
        /// Render an input element of type "radio".
        /// </summary>
        /// <param name="expression">
        /// An expression that identifies the object that contains the properties to render.
        /// </param>
        /// <param name="value">
        /// If non-<c>null</c>, value to compare with current expression value to determine whether radio button is
        /// checked.
        /// </param>
        /// <param name="htmlAttributes">
        /// <see cref="IDictionary{string, object}"/> containing additional HTML attributes.
        /// </param>
        /// <returns>New <see cref="HtmlString"/> containing the rendered HTML.</returns>
        HtmlString RadioButtonFor<TProperty>([NotNull] Expression<Func<TModel, TProperty>> expression, object value,
            IDictionary<string, object> htmlAttributes);

        /// <summary>
        /// Render an input element of type "text".
        /// </summary>
        /// <param name="expression">
        /// An expression that identifies the object that contains the properties to render.
        /// </param>
        /// <param name="format"></param>
        /// <param name="htmlAttributes">
        /// <see cref="IDictionary{string, object}"/> containing additional HTML attributes.
        /// </param>
        /// <returns>New <see cref="HtmlString"/> containing the rendered HTML.</returns>
        HtmlString TextBoxFor<TProperty>([NotNull] Expression<Func<TModel, TProperty>> expression, string format,
            IDictionary<string, object> htmlAttributes);
        #endregion
    }
}
