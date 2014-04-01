﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc.Rendering
{
    /// <summary>
    /// An <see cref="IHtmlHelper"/> for Linq expressions.
    /// </summary>
    /// <typeparam name="TModel">The <see cref="Type"/> of the model.</typeparam>
    public interface IHtmlHelper<TModel>
    {
        /// <summary>
        /// Gets or sets the character that replaces periods in the ID attribute of an element.
        /// </summary>
        string IdAttributeDotReplacement { get; set; }

        /// <summary>
        /// Gets the view bag.
        /// </summary>
        dynamic ViewBag { get; }

        /// <summary>
        /// Gets the context information about the view.
        /// </summary>
        ViewContext ViewContext { get; }

        /// <summary>
        /// Gets the current view data.
        /// </summary>
        ViewDataDictionary<TModel> ViewData { get; }

        /// <summary>
        /// Converts the value of the specified object to an HTML-encoded string.
        /// </summary>
        /// <param name="value">The object to encode.</param>
        /// <returns>The HTML-encoded string.</returns>
        string Encode(object value);

        /// <summary>
        /// Converts the specified string to an HTML-encoded string.
        /// </summary>
        /// <param name="value">The string to encode.</param>
        /// <returns>The HTML-encoded string.</returns>
        string Encode(string value);

        /// <summary>
        /// Creates an HTML element ID using the specified element name.
        /// </summary>
        /// <param name="name">The name of the HTML element.</param>
        /// <returns>The ID of the HTML element.</returns>
        string GenerateIdFromName(string name);

        /// <summary>
        /// Gets the full HTML field name for the given expression <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Name of an expression, relative to the current model.</param>
        /// <returns>An <see cref="HtmlString"/> that represents HTML markup.</returns>
        HtmlString Name(string name);

        /// <summary>
        /// Gets the full HTML field name for the given <paramref name="expression"/>.
        /// </summary>
        /// <typeparam name="TProperty">The <see cref="Type"/> the <paramref name="expression"/> returns.</typeparam>
        /// <param name="expression">An expression, relative to the current model.</param>
        /// <returns>An <see cref="HtmlString"/> that represents HTML markup.</returns>
        HtmlString NameFor<TProperty>([NotNull] Expression<Func<TModel, TProperty>> expression);

        /// <summary>
        /// Wraps HTML markup in an <see cref="HtmlString"/>, which will enable HTML markup to be
        /// rendered to the output without getting HTML encoded.
        /// </summary>
        /// <param name="value">HTML markup string.</param>
        /// <returns>An <see cref="HtmlString"/> that represents HTML markup.</returns>
        HtmlString Raw(string value);

        /// <summary>
        /// Wraps HTML markup from the string representation of an object in an <see cref="HtmlString"/>,
        /// which will enable HTML markup to be rendered to the output without getting HTML encoded.
        /// </summary>
        /// <param name="value">object with string representation as HTML markup.</param>
        /// <returns>An <see cref="HtmlString"/> that represents HTML markup.</returns>
        HtmlString Raw(object value);

        /// <summary>
        /// Returns a partial view in string format.
        /// </summary>
        /// <param name="partialViewName">The name of the partial view to render and return.</param>
        /// <param name="model">A model to pass into the partial view.</param>
        /// <param name="viewData">A <see cref="ViewDataDictionary"/> to pass into the partial view.</param>
        /// <returns>A task that represents when rendering of the partial view into a string has completed.</returns>
        Task<HtmlString> PartialAsync([NotNull] string partialViewName, object model, ViewDataDictionary viewData);

        /// <summary>
        /// Renders a partial view.
        /// </summary>
        /// <param name="partialViewName">The name of the partial view to render.</param>
        /// <param name="model">A model to pass into the partial view.</param>
        /// <param name="viewData">A <see cref="ViewDataDictionary"/> to pass into the partial view.</param>
        /// <returns>A task that represents when rendering has completed.</returns>
        Task RenderPartialAsync([NotNull] string partialViewName, object model, ViewDataDictionary viewData);

        /// <summary>
        /// Render an input element of type "text".
        /// </summary>
        /// <param name="name">
        /// Rendered element's name. Also use this name to find value in submitted data or view data. Use view data
        /// only if value is not in submitted data and <paramref name="value"/> is <c>null</c>.
        /// </param>
        /// <param name="value">
        /// If non-<c>null</c>, value to include in the element. Ignore if named value is found in submitted data.
        /// </param>
        /// <param name="format"></param>
        /// <param name="htmlAttributes">
        /// <see cref="IDictionary{string, object}"/> containing additional HTML attributes.
        /// </param>
        /// <returns>New <see cref="HtmlString"/> containing the rendered HTML.</returns>
        HtmlString TextBox(string name, object value, string format, IDictionary<string, object> htmlAttributes);

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

        HtmlString Value([NotNull] string name, string format);

        HtmlString ValueFor<TProperty>([NotNull] Expression<Func<TModel, TProperty>> expression, string format);
    }
}
