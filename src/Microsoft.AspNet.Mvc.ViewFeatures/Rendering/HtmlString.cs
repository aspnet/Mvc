// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc.Rendering
{
    /// <summary>
    /// String content which knows how to write itself.
    /// </summary>
    public class HtmlString : StringHtmlContent
    {
        /// <summary>
        /// Returns an <see cref="HtmlString"/> with empty content.
        /// </summary>
        public static readonly HtmlString Empty = new HtmlString(string.Empty);

        /// <summary>
        /// Creates a new instance of <see cref="HtmlString"/>.
        /// </summary>
        /// <param name="input"><c>string</c>to initialize <see cref="HtmlString"/>.</param>
        public HtmlString(string input)
            : base(input, encodeOnWrite: false)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return base.ToString();
        }
    }
}
