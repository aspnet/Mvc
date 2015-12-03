// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using Microsoft.Extensions.Localization;

namespace Microsoft.AspNet.Mvc.Localization
{
    /// <summary>
    /// This service does not HTML encode the resource string. It HTML encodes all arguments that are formatted in
    /// the resource string.
    /// </summary>
    public interface IHtmlLocalizer : IStringLocalizer
    {
        /// <summary>
        /// Creates a new <see cref="HtmlLocalizer"/> for a specific <see cref="CultureInfo"/>.
        /// </summary>
        /// <param name="culture">The <see cref="CultureInfo"/> to use.</param>
        /// <returns>A culture-specific <see cref="IHtmlLocalizer"/>.</returns>
        new IHtmlLocalizer WithCulture(CultureInfo culture);

        // Reviewers: I suggest the Html() methods should just return IHtmlContent because the specialized information
        // in LocalizedHtmlString is unused. Any objections?

        /// <summary>
        /// Gets the <see cref="LocalizedHtmlString"/> resource for a specific key.
        /// </summary>
        /// <param name="key">The key to use.</param>
        /// <returns>The <see cref="LocalizedHtmlString"/> resource.</returns>
        LocalizedHtmlString Html(string key);

        /// <summary>
        /// Gets the <see cref="LocalizedHtmlString"/> resource for a specific key.
        /// </summary>
        /// <param name="key">The key to use.</param>
        /// <param name="arguments">The values to format the string with.</param>
        /// <returns>The <see cref="LocalizedHtmlString"/> resource.</returns>
        LocalizedHtmlString Html(string key, params object[] arguments);
    }
}