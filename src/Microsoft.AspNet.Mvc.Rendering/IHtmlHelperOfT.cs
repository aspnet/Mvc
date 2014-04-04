﻿using System;
using System.Linq.Expressions;
using System.Collections.Generic;
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
        /// Returns HTML markup for each property in the object that is represented by the expression, using the specified template, HTML field ID, and additional view data.
        /// </summary>
        /// <param name="expression">An expression that identifies the object that contains the properties to display.</param>
        /// <param name="templateName">The name of the template that is used to render the object.</param>
        /// <param name="htmlFieldName">A string that is used to disambiguate the names of HTML input elements that are rendered for properties that have the same name.</param>
        /// <param name="additionalViewData">An anonymous object that can contain additional view data that will be merged into the <see cref="ViewDataDictionary{TModel}"/> instance that is created for the template.</param>
        /// <returns>The HTML markup for each property in the object that is represented by the expression.</returns>
        HtmlString Display(string expression,
                           string templateName,
                           string htmlFieldName,
                           object additionalViewData);

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
        /// Returns an unordered list (ul element) of validation messages that are in the <see cref="ModelStateDictionary"/> object.
        /// </summary>
        /// <param name="excludePropertyErrors">true to have the summary display model-level errors only, or false to have the summary display all errors.</param>
        /// <param name="message">The message to display with the validation summary.</param>
        /// <param name="htmlAttributes">A dictionary that contains the HTML attributes for the element.</param>
        /// <returns>An <see cref="HtmlString"/> that contains an unordered list (ul element) of validation messages.</returns>
        HtmlString ValidationSummary(bool excludePropertyErrors, string message, IDictionary<string, object> htmlAttributes);
    }
}
