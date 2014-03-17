// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc.Rendering
{
    /// <summary>
    /// Provider of top-level settings for rendering HTML.
    /// </summary>
    // Will definitely have more settings in the future for HTML rendering; a subset may need to be settable without
    // affecting other scopes i.e. aren't static. May also make sense to expand this class to hold other settings.
    public interface IHtmlSettings
    {
        /// <summary>
        /// If <c>true</c> (the default), generate HTML elements that include validation-related attributes.
        /// </summary>
        bool ClientValidationEnabled { get; set; }

        /// <summary>
        /// Character to replace '.' in HTML id attributes.
        /// </summary>
        string IdAttributeDotReplacement { get; set; }

        /// <summary>
        /// If <c>true</c> (the default), generate validation attributes that are specifically intended for use with
        /// Microsoft.jQuery.Unobtrusive.Validation. Otherwise generate traditional client-side validation attributes.
        /// Ignored if <see cref="ClientValidationEnabled"/> is <c>false</c>.
        /// </summary>
        bool UnobtrusiveJavaScriptEnabled { get; set; }
    }
}
