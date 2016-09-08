// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures
{
    public interface IHtmlGeneratorTutu : IHtmlGenerator
    {
        /// <summary>
        /// Generate a &lt;input type="checkbox".../&gt; element.
        /// </summary>
        /// <param name="viewContext">The <see cref="ViewContext"/> instance for the current scope.</param>
        /// <param name="modelExplorer">The <see cref="ModelExplorer"/> for the <paramref name="expression"/>.</param>
        /// <param name="expression">Expression name, relative to the current model.</param>
        /// <param name="isChecked">The initial state of the checkbox element.</param>
        /// <param name="htmlAttributes">
        /// An <see cref="object"/> that contains the HTML attributes for the element. Alternatively, an
        /// <see cref="IDictionary{String, Object}"/> instance containing the HTML attributes.
        /// </param>
        /// <returns>
        /// A <see cref="TagBuilder"/> instance for the &lt;input type="checkbox".../&gt; element.
        /// </returns>
        TagBuilder GenerateCheckBox(
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            StringValuesTutu expression,
            bool? isChecked,
            object htmlAttributes);

        TagBuilder GenerateHidden(
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            StringValuesTutu expression,
            object value,
            bool useViewData,
            object htmlAttributes);

        /// <summary>
        /// Generate an additional &lt;input type="hidden".../&gt; for checkboxes. This addresses scenarios where
        /// unchecked checkboxes are not sent in the request. Sending a hidden input makes it possible to know that the
        /// checkbox was present on the page when the request was submitted.
        /// </summary>
        TagBuilder GenerateHiddenForCheckbox(
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            StringValuesTutu expression);

        TagBuilder GenerateLabel(
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            StringValuesTutu expression,
            string labelText,
            object htmlAttributes);

        TagBuilder GeneratePassword(
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            StringValuesTutu expression,
            object value,
            object htmlAttributes);

        TagBuilder GenerateRadioButton(
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            StringValuesTutu expression,
            object value,
            bool? isChecked,
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
        /// <see cref="IDictionary{String, Object}"/> instance containing the HTML attributes.
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
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            string optionLabel,
            StringValuesTutu expression,
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
        /// An <see cref="ICollection{String}"/> containing values for &lt;option&gt; elements to select. If
        /// <c>null</c>, selects &lt;option&gt; elements based on <see cref="SelectListItem.Selected"/> values in
        /// <paramref name="selectList"/>.
        /// </param>
        /// <param name="allowMultiple">
        /// If <c>true</c>, includes a <c>multiple</c> attribute in the generated HTML. Otherwise generates a
        /// single-selection &lt;select&gt; element.
        /// </param>
        /// <param name="htmlAttributes">
        /// An <see cref="object"/> that contains the HTML attributes for the &lt;select&gt; element. Alternatively, an
        /// <see cref="IDictionary{String, Object}"/> instance containing the HTML attributes.
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
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            string optionLabel,
            StringValuesTutu expression,
            IEnumerable<SelectListItem> selectList,
            ICollection<string> currentValues,
            bool allowMultiple,
            object htmlAttributes);

        TagBuilder GenerateTextArea(
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            StringValuesTutu expression,
            int rows,
            int columns,
            object htmlAttributes);

        TagBuilder GenerateTextBox(
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            StringValuesTutu expression,
            object value,
            string format,
            object htmlAttributes);

        /// <summary>
        /// Generate a <paramref name="tag"/> element if the <paramref name="viewContext"/>'s
        /// <see cref="ActionContext.ModelState"/> contains an error for the <paramref name="expression"/>.
        /// </summary>
        /// <param name="viewContext">A <see cref="ViewContext"/> instance for the current scope.</param>
        /// <param name="modelExplorer">The <see cref="ModelExplorer"/> for the <paramref name="expression"/>.</param>
        /// <param name="expression">Expression name, relative to the current model.</param>
        /// <param name="message">
        /// The message to be displayed. If <c>null</c> or empty, method extracts an error string from the
        /// <see cref="ModelBinding.ModelStateDictionary"/> object. Message will always be visible but client-side
        /// validation may update the associated CSS class.
        /// </param>
        /// <param name="tag">
        /// The tag to wrap the <paramref name="message"/> in the generated HTML. Its default value is
        /// <see cref="ViewContext.ValidationMessageElement"/>.
        /// </param>
        /// <param name="htmlAttributes">
        /// An <see cref="object"/> that contains the HTML attributes for the element. Alternatively, an
        /// <see cref="IDictionary{String, Object}"/> instance containing the HTML attributes.
        /// </param>
        /// <returns>
        /// A <see cref="TagBuilder"/> containing a <paramref name="tag"/> element if the
        /// <paramref name="viewContext"/>'s <see cref="ActionContext.ModelState"/> contains an error for the
        /// <paramref name="expression"/> or (as a placeholder) if client-side validation is enabled. <c>null</c> if
        /// the <paramref name="expression"/> is valid and client-side validation is disabled.
        /// </returns>
        /// <remarks><see cref="ViewContext.ValidationMessageElement"/> is <c>"span"</c> by default.</remarks>
        TagBuilder GenerateValidationMessage(
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            StringValuesTutu expression,
            string message,
            string tag,
            object htmlAttributes);

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
        /// If <c>true</c>, require a collection <paramref name="expression"/> result. Otherwise, treat result as a
        /// single value.
        /// </param>
        /// <returns>
        /// <para>
        /// <c>null</c> if no <paramref name="expression"/> result is found. Otherwise a
        /// <see cref="ICollection{String}"/> containing current values for the given
        /// <paramref name="expression"/>.
        /// </para>
        /// <para>
        /// Converts the <paramref name="expression"/> result to a <see cref="string"/>. If that result is an
        /// <see cref="System.Collections.IEnumerable"/> type, instead converts each item in the collection and returns
        /// them separately.
        /// </para>
        /// <para>
        /// If the <paramref name="expression"/> result or the element type is an <see cref="System.Enum"/>, returns a
        /// <see cref="string"/> containing the integer representation of the <see cref="System.Enum"/> value as well
        /// as all <see cref="System.Enum"/> names for that value. Otherwise returns the default <see cref="string"/>
        /// conversion of the value.
        /// </para>
        /// </returns>
        /// <remarks>
        /// See <see cref="M:GenerateSelect"/> for information about how the return value may be used.
        /// </remarks>
        ICollection<string> GetCurrentValues(
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            StringValuesTutu expression,
            bool allowMultiple);
    }
}