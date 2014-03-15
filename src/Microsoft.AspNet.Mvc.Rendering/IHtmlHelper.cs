// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
namespace Microsoft.AspNet.Mvc.Rendering
{
    public interface IHtmlHelper : IHtmlSettings
    {
        /// <summary>
        /// Gets the view bag.
        /// </summary>
        dynamic ViewBag { get; }

        /// <summary>
        /// Gets the context information about the view.
        /// </summary>
        ViewContext ViewContext { get; }

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
        /// Formats the value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="format">The format string.</param>
        /// <returns>The formatted value.</returns>
        string FormatValue(object value, string format);

        /// <summary>
        /// Creates an HTML element ID using the specified element name.
        /// </summary>
        /// <param name="name">The name of the HTML element.</param>
        /// <returns>The ID of the HTML element.</returns>
        string GenerateIdFromName(string name);

        /// <summary>
        /// Returns a hidden input element that identifies the override method for the specified verb that represents
        /// the HTTP data-transfer method used by the client.
        /// </summary>
        /// <param name="httpVerb">The verb that represents the HTTP data-transfer method used by the client.</param>
        /// <returns>
        /// The override method that uses the verb that represents the HTTP data-transfer method used by the client.
        /// </returns>
        HtmlString HttpMethodOverride(HttpVerbs httpVerb);

        /// <summary>
        /// Returns a hidden input element that identifies the override method for the specified HTTP data-transfer
        /// method that was used by the client.
        /// </summary>
        /// <param name="httpMethod">
        /// The HTTP data-transfer method that was used by the client (DELETE, HEAD, or PUT).
        /// </param>
        /// <returns>The override method that uses the HTTP data-transfer method that was used by the client.</returns>
        HtmlString HttpMethodOverride(string httpMethod);

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

        #region Input helpers
        /// <summary>
        /// Render an input element of type "checkbox" with value "true" and an input element of type "hidden" with
        /// value "false".
        /// </summary>
        /// <param name="name">
        /// Rendered element's name. Also use this name to find value in submitted data or view data. Use view data
        /// only if value is not in submitted data and <paramref name="value"/> is <c>null</c>.
        /// </param>
        /// <param name="isChecked">
        /// If <c>true</c>, checkbox is initially checked. Ignore if named value is found in submitted data. Finally
        /// fall back to an existing "checked" value in <paramref name="htmlAttributes"/>.
        /// </param>
        /// <param name="htmlAttributes">
        /// <see cref="IDictionary{string, object}"/> containing additional HTML attributes.
        /// </param>
        /// <returns>New <see cref="HtmlString"/> containing the rendered HTML.</returns>
        HtmlString CheckBox(string name, bool? isChecked, IDictionary<string, object> htmlAttributes);

        /// <summary>
        /// Render an input element of type "hidden".
        /// </summary>
        /// <param name="name">
        /// Rendered element's name. Also use this name to find value in submitted data or view data. Use view data
        /// only if value is not in submitted data and <paramref name="value"/> is <c>null</c>.
        /// </param>
        /// <param name="value">
        /// If non-<c>null</c>, value to include in the element. Ignore if named value is found in submitted data.
        /// </param>
        /// <param name="htmlAttributes">
        /// <see cref="IDictionary{string, object}"/> containing additional HTML attributes.
        /// </param>
        /// <returns>New <see cref="HtmlString"/> containing the rendered HTML.</returns>
        HtmlString Hidden(string name, object value, IDictionary<string, object> htmlAttributes);

        /// <summary>
        /// Render an input element of type "password".
        /// </summary>
        /// <param name="name">
        /// Rendered element's name. Also use this name to find value in view data. Use view data
        /// only if value is not in submitted data and <paramref name="value"/> is <c>null</c>.
        /// </param>
        /// <param name="value">
        /// If non-<c>null</c>, value to include in the element.
        /// </param>
        /// <param name="htmlAttributes">
        /// <see cref="IDictionary{string, object}"/> containing additional HTML attributes.
        /// </param>
        /// <returns>New <see cref="HtmlString"/> containing the rendered HTML.</returns>
        HtmlString Password(string name, object value, IDictionary<string, object> htmlAttributes);

        /// <summary>
        /// Render an input element of type "radio".
        /// </summary>
        /// <param name="name">
        /// Rendered element's name. Also use this name to find value in submitted data or view data. Use view data
        /// only if value is not in submitted data and <paramref name="value"/> is <c>null</c>.
        /// </param>
        /// <param name="value">
        /// If non-<c>null</c>, value to include in the element. May be <c>null</c> only if
        /// <paramref name="isChecked"/> is also <c>null</c>. Also compared to value in submitted data or view data to
        /// determine <paramref name="isChecked"/> if that parameter is <c>null</c>. Ignore if named value is found in
        /// submitted data.
        /// </param>
        /// <param name="isChecked">
        /// If <c>true</c>, radio button is initially selected. Ignore if named value is found in submitted data. Fall
        /// back to comparing <paramref name="value"/> with view data if this parameter is <c>null</c>. Finally
        /// fall back to an existing "checked" value in <paramref name="htmlAttributes"/>.
        /// </param>
        /// <param name="htmlAttributes">
        /// <see cref="IDictionary{string, object}"/> containing additional HTML attributes.
        /// </param>
        /// <returns>New <see cref="HtmlString"/> containing the rendered HTML.</returns>
        HtmlString RadioButton(string name, object value, bool? isChecked, IDictionary<string, object> htmlAttributes);

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
        #endregion
    }
}
