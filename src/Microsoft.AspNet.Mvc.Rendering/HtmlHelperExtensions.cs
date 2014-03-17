// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc.Rendering
{
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Converts the specified attribute value to an HTML-encoded string.
        /// </summary>
        /// <param name="htmlHelper"><see cref="IHtmlHelper"/> used to perform this operation.</param>
        /// <param name="value">The object to encode.</param>
        /// <returns>
        /// The HTML-encoded string. If the value parameter is null, this method returns an empty string.
        /// </returns>
        public static string AttributeEncode([NotNull] this IHtmlHelper htmlHelper, object value)
        {
            return htmlHelper.Encode(value);
        }

        /// <summary>
        /// Converts the specified attribute value to an HTML-encoded string.
        /// </summary>
        /// <param name="htmlHelper"><see cref="IHtmlHelper"/> used to perform this operation.</param>
        /// <param name="value">The string to encode.</param>
        /// <returns>
        /// The HTML-encoded string. If the value parameter is null or empty, this method returns an empty string.
        /// </returns>
        public static string AttributeEncode([NotNull] this IHtmlHelper htmlHelper, string value)
        {
            return htmlHelper.Encode(value);
        }

        /// <summary>
        /// Enable input validation that is performed using client script in the browser.
        /// </summary>
        public static void EnableClientValidation([NotNull] this IHtmlHelper htmlHelper)
        {
            EnableClientValidation(htmlHelper, enabled: true);
        }

        /// <summary>
        /// Enable client validation.
        /// </summary>
        /// <param name="enabled"><c>true</c> to enable client validation; otherwise, <c>false</c>.</param>
        public static void EnableClientValidation([NotNull] this IHtmlHelper htmlHelper, bool enabled)
        {
            htmlHelper.ClientValidationEnabled = enabled;
        }

        /// <summary>
        /// Enable unobtrusive JavaScript.
        /// </summary>
        public static void EnableUnobtrusiveJavaScript([NotNull] this IHtmlHelper htmlHelper)
        {
            EnableUnobtrusiveJavaScript(htmlHelper, enabled: true);
        }

        /// <summary>
        /// Enable or disable unobtrusive JavaScript.
        /// </summary>
        /// <param name="enabled"><c>true </c>to enable unobtrusive JavaScript; otherwise, <c>false</c>.</param>
        public static void EnableUnobtrusiveJavaScript([NotNull] this IHtmlHelper htmlHelper, bool enabled)
        {
            htmlHelper.UnobtrusiveJavaScriptEnabled = enabled;
        }
    }
}
