﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.ModelBinding.Validation;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.Rendering
{
    /// <summary>
    /// Contract for a service supporting <see cref="IHtmlHelper"/> and <c>ITagHelper</c> implementations.
    /// </summary>
    public interface IHtmlGenerator
    {
        string IdAttributeDotReplacement { get; set; }

        string Encode(string value);

        string Encode(object value);

        string FormatValue(object value, string format);

        TagBuilder GenerateActionLink(
            [NotNull] string linkText,
            string actionName,
            string controllerName,
            string protocol,
            string hostname,
            string fragment,
            object routeValues,
            object htmlAttributes);

        TagBuilder GenerateAntiForgery([NotNull] ViewContext viewContext);

        /// <summary>
        /// Generate a &lt;input type="checkbox".../&gt; element. 
        /// </summary>
        /// <param name="viewContext">The <see cref="ViewContext"/> instance for the current scope.</param>
        /// <param name="modelExplorer">The <see cref="ModelExplorer"/> for the model.</param>
        /// <param name="expression">The model expression.</param>
        /// <param name="isChecked">The initial state of the checkbox element.</param>
        /// <param name="htmlAttributes">
        /// An <see cref="object"/> that contains the HTML attributes for the element. Alternatively, an
        /// <see cref="IDictionary{string, object}"/> instance containing the HTML attributes.
        /// </param>
        /// <returns>
        /// A <see cref="TagBuilder"/> instance for the &lt;input type="checkbox".../&gt; element.
        /// </returns>
        TagBuilder GenerateCheckBox(
            [NotNull] ViewContext viewContext,
            ModelExplorer modelExplorer,
            string expression,
            bool? isChecked,
            object htmlAttributes);

        /// <summary>
        /// Generate an additional &lt;input type="hidden".../&gt; for checkboxes. This addresses scenarios where
        /// unchecked checkboxes are not sent in the request. Sending a hidden input makes it possible to know that the
        /// checkbox was present on the page when the request was submitted.
        /// </summary>
        TagBuilder GenerateHiddenForCheckbox(
            [NotNull] ViewContext viewContext,
            ModelExplorer modelExplorer,
            string expression);

        /// <summary>
        /// Generate a &lt;form&gt; element. When the user submits the form, the action with name
        /// <paramref name="actionName"/> will process the request.
        /// </summary>
        /// <param name="viewContext">A <see cref="ViewContext"/> instance for the current scope.</param>
        /// <param name="actionName">The name of the action method.</param>
        /// <param name="controllerName">The name of the controller.</param>
        /// <param name="routeValues">
        /// An <see cref="object"/> that contains the parameters for a route. The parameters are retrieved through
        /// reflection by examining the properties of the <see cref="object"/>. This <see cref="object"/> is typically
        /// created using <see cref="object"/> initializer syntax. Alternatively, an
        /// <see cref="IDictionary{string, object}"/> instance containing the route parameters.
        /// </param>
        /// <param name="method">The HTTP method for processing the form, either GET or POST.</param>
        /// <param name="htmlAttributes">
        /// An <see cref="object"/> that contains the HTML attributes for the element. Alternatively, an
        /// <see cref="IDictionary{string, object}"/> instance containing the HTML attributes.
        /// </param>
        /// <returns>
        /// A <see cref="TagBuilder"/> instance for the &lt;/form&gt; element.
        /// </returns>
        TagBuilder GenerateForm(
            [NotNull] ViewContext viewContext,
            string actionName,
            string controllerName,
            object routeValues,
            string method,
            object htmlAttributes);

        /// <summary>
        /// Generate a &lt;form&gt; element. The route with name <paramref name="routeName"/> generates the
        /// &lt;form&gt;'s <c>action</c> attribute value.
        /// </summary>
        /// <param name="viewContext">A <see cref="ViewContext"/> instance for the current scope.</param>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="routeValues">
        /// An <see cref="object"/> that contains the parameters for a route. The parameters are retrieved through
        /// reflection by examining the properties of the <see cref="object"/>. This <see cref="object"/> is typically
        /// created using <see cref="object"/> initializer syntax. Alternatively, an
        /// <see cref="IDictionary{string, object}"/> instance containing the route parameters.
        /// </param>
        /// <param name="method">The HTTP method for processing the form, either GET or POST.</param>
        /// <param name="htmlAttributes">
        /// An <see cref="object"/> that contains the HTML attributes for the element. Alternatively, an
        /// <see cref="IDictionary{string, object}"/> instance containing the HTML attributes.
        /// </param>
        /// <returns>
        /// A <see cref="TagBuilder"/> instance for the &lt;/form&gt; element.
        /// </returns>
        TagBuilder GenerateRouteForm(
            [NotNull] ViewContext viewContext,
            string routeName,
            object routeValues,
            string method,
            object htmlAttributes);

        TagBuilder GenerateHidden(
            [NotNull] ViewContext viewContext,
            ModelExplorer modelExplorer,
            string expression,
            object value,
            bool useViewData,
            object htmlAttributes);

        TagBuilder GenerateLabel(
            [NotNull] ViewContext viewContext,
            [NotNull] ModelExplorer modelExplorer,
            string expression,
            string labelText,
            object htmlAttributes);

        TagBuilder GeneratePassword(
            [NotNull] ViewContext viewContext,
            ModelExplorer modelExplorer,
            string expression,
            object value,
            object htmlAttributes);

        TagBuilder GenerateRadioButton(
            [NotNull] ViewContext viewContext,
            ModelExplorer modelExplorer,
            string expression,
            object value,
            bool? isChecked,
            object htmlAttributes);

        TagBuilder GenerateRouteLink(
            [NotNull] string linkText,
            string routeName,
            string protocol,
            string hostName,
            string fragment,
            object routeValues,
            object htmlAttributes);

        /// <summary>
        /// Generate a &lt;select&gt; element for the <paramref name="expression"/>.
        /// </summary>
        /// <param name="viewContext">A <see cref="ViewContext"/> instance for the current scope.</param>
        /// <param name="modelExplorer">
        /// <see cref="ModelExplorer"/> for the <paramref name="expression"/>. If <c>null</c>, determines validation
        /// attributes using <paramref name="viewContext"/> and the <paramref name="expression"/>.
        /// </param>
        /// <param name="optionLabel">Optional text for a default empty &lt;option&gt; element.</param>
        /// <param name="expression">Expression name, relative to the current model.</param>
        /// <param name="selectList">
        /// A collection of <see cref="SelectListItem"/> objects used to populate the &lt;select&gt; element with
        /// &lt;optgroup&gt; and &lt;option&gt; elements. If <c>null</c>, finds this collection at
        /// <c>ViewContext.ViewData[expression]</c>.
        /// </param>
        /// <param name="allowMultiple">
        /// If <c>true</c>, includes a <c>multiple</c> attribute in the generated HTML. Otherwise generates a
        /// single-selection &lt;select&gt; element.
        /// </param>
        /// <param name="htmlAttributes">
        /// An <see cref="object"/> that contains the HTML attributes for the &lt;select&gt; element. Alternatively, an
        /// <see cref="IDictionary{string, object}"/> instance containing the HTML attributes.
        /// </param>
        /// <returns>A new <see cref="TagBuilder"/> describing the &lt;select&gt; element.</returns>
        /// <remarks>
        /// <para>
        /// Combines <see cref="TemplateInfo.HtmlFieldPrefix"/> and <paramref name="expression"/> to set
        /// &lt;select&gt; element's "name" attribute. Sanitizes <paramref name="expression"/> to set element's "id"
        /// attribute.
        /// </para>
        /// <para>
        /// See <see cref="GetCurrentValues"/> for information about how current values are determined.
        /// </para>
        /// </remarks>
        TagBuilder GenerateSelect(
            [NotNull] ViewContext viewContext,
            ModelExplorer modelExplorer,
            string optionLabel,
            string expression,
            IEnumerable<SelectListItem> selectList,
            bool allowMultiple,
            object htmlAttributes);

        /// <summary>
        /// Generate a &lt;select&gt; element for the <paramref name="expression"/>.
        /// </summary>
        /// <param name="viewContext">A <see cref="ViewContext"/> instance for the current scope.</param>
        /// <param name="modelExplorer">
        /// <see cref="ModelExplorer"/> for the <paramref name="expression"/>. If <c>null</c>, determines validation
        /// attributes using <paramref name="viewContext"/> and the <paramref name="expression"/>.
        /// </param>
        /// <param name="optionLabel">Optional text for a default empty &lt;option&gt; element.</param>
        /// <param name="expression">Expression name, relative to the current model.</param>
        /// <param name="selectList">
        /// A collection of <see cref="SelectListItem"/> objects used to populate the &lt;select&gt; element with
        /// &lt;optgroup&gt; and &lt;option&gt; elements. If <c>null</c>, finds this collection at
        /// <c>ViewContext.ViewData[expression]</c>.
        /// </param>
        /// <param name="currentValues">
        /// An <see cref="IReadOnlyCollection{string}"/> containing values for &lt;option&gt; elements to select. If
        /// <c>null</c>, selects &lt;option&gt; elements based on <see cref="SelectListItem.Selected"/> values in
        /// <paramref name="selectList"/>.
        /// </param>
        /// <param name="allowMultiple">
        /// If <c>true</c>, includes a <c>multiple</c> attribute in the generated HTML. Otherwise generates a
        /// single-selection &lt;select&gt; element.
        /// </param>
        /// <param name="htmlAttributes">
        /// An <see cref="object"/> that contains the HTML attributes for the &lt;select&gt; element. Alternatively, an
        /// <see cref="IDictionary{string, object}"/> instance containing the HTML attributes.
        /// </param>
        /// <returns>A new <see cref="TagBuilder"/> describing the &lt;select&gt; element.</returns>
        /// <remarks>
        /// <para>
        /// Combines <see cref="TemplateInfo.HtmlFieldPrefix"/> and <paramref name="expression"/> to set
        /// &lt;select&gt; element's "name" attribute. Sanitizes <paramref name="expression"/> to set element's "id"
        /// attribute.
        /// </para>
        /// <para>
        /// See <see cref="GetCurrentValues"/> for information about how the <paramref name="currentValues"/>
        /// collection may be created.
        /// </para>
        /// </remarks>
        TagBuilder GenerateSelect(
            [NotNull] ViewContext viewContext,
            ModelExplorer modelExplorer,
            string optionLabel,
            string expression,
            IEnumerable<SelectListItem> selectList,
            IReadOnlyCollection<string> currentValues,
            bool allowMultiple,
            object htmlAttributes);

        TagBuilder GenerateTextArea(
            [NotNull] ViewContext viewContext,
            ModelExplorer modelExplorer,
            string expression,
            int rows,
            int columns,
            object htmlAttributes);

        TagBuilder GenerateTextBox(
            [NotNull] ViewContext viewContext,
            ModelExplorer modelExplorer,
            string expression,
            object value,
            string format,
            object htmlAttributes);

        TagBuilder GenerateValidationMessage(
            [NotNull] ViewContext viewContext,
            string expression,
            string message,
            string tag,
            object htmlAttributes);

        TagBuilder GenerateValidationSummary(
            [NotNull] ViewContext viewContext,
            bool excludePropertyErrors,
            string message,
            string headerTag,
            object htmlAttributes);

        /// <remarks>
        /// Not used directly in <see cref="HtmlHelper"/>. Exposed publicly for use in other <see cref="IHtmlHelper"/>
        /// implementations.
        /// </remarks>
        IEnumerable<ModelClientValidationRule> GetClientValidationRules(
            [NotNull] ViewContext viewContext,
            ModelExplorer modelExplorer,
            string expression);

        /// <summary>
        /// Gets the collection of current values for the given <paramref name="expression"/>.
        /// </summary>
        /// <param name="viewContext">A <see cref="ViewContext"/> instance for the current scope.</param>
        /// <param name="modelExplorer">
        /// <see cref="ModelExplorer"/> for the <paramref name="expression"/>. If <c>null</c>, calculates the
        /// <paramref name="expression"/> result using <see cref="ViewDataDictionary.Eval(string)"/>.
        /// </param>
        /// <param name="expression">Expression name, relative to the current model.</param>
        /// <param name="allowMultiple">
        /// If <c>true</c>, allow multiple items in the return value. Otherwise returns at most one entry.
        /// </param>
        /// <returns>
        /// <para>
        /// <c>null</c> if no <paramref name="expression"/> result is found. Otherwise an
        /// <see cref="IReadOnlyCollection{string}"/> containing current values for the given
        /// <paramref name="expression"/>.
        /// </para>
        /// <para>
        /// Converts the <paramref name="expression"/> result to a <see cref="string"/>. If that result is an
        /// <see cref="System.Collections.IEnumerable"/> type, instead converts each item in the collection and returns
        /// them separately.
        /// </para>
        /// <para>
        /// If the <paramref name="expression"/> result or the element type is an <see cref="System.Enum"/>, returns a
        /// <see cref="string"/> containing the integer representation of the <see cref="System.Enum"/> value.
        /// Otherwise returns the default <see cref="string"/> conversion of the value.
        /// </para>
        /// </returns>
        /// <remarks>
        /// See <see cref="GenerateSelect"/> for information about how the return value may be used.
        /// </remarks>
        IReadOnlyCollection<string> GetCurrentValues(
            [NotNull] ViewContext viewContext,
            ModelExplorer modelExplorer,
            string expression,
            bool allowMultiple);
    }
}
