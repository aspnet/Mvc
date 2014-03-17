// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc.Rendering.Html
{
    /// <summary>
    /// Singleton implementation of <see cref="IHtmlSettings"/>.
    /// </summary>
    // Should at least get defaults from configuration.
    public class HtmlSettings : IHtmlSettings
    {
        /// <summary>
        /// Contruct new instance of <see cref="HtmlSettings"/>.
        /// </summary>
        public HtmlSettings()
        {
            ClientValidationEnabled = true;
            UnobtrusiveJavaScriptEnabled = true;

            // Underscores are fine characters in id's.
            IdAttributeDotReplacement = "_";
        }

        /// <inheritdoc />
        public bool ClientValidationEnabled { get; set; }

        /// <inheritdoc />
        public string IdAttributeDotReplacement { get; set; }

        /// <inheritdoc />
        public bool UnobtrusiveJavaScriptEnabled { get; set; }
    }
}
