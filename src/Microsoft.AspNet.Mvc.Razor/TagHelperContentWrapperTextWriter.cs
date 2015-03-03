// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Text;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Implements a <see cref="TextWriter"/> that wraps around <see cref="TagHelperContent"/>.
    /// </summary>
    public class TagHelperContentWrapperTextWriter : TextWriter
    {
        /// <summary>
        /// Instantiates a new instance of <see cref="TagHelperContentWrapperTextWriter"/>.
        /// </summary>
        /// <param name="encoding">The <see cref="TextWriter"/>'s encoding.</param>
        public TagHelperContentWrapperTextWriter(Encoding encoding)
        {
            Content = new DefaultTagHelperContent();
            Encoding = encoding;
        }

        /// <summary>
        /// The <see cref="TagHelperContent"/> which is wrapped.
        /// </summary>
        public TagHelperContent Content { get; set; }

        /// <inheritdoc />
        public override Encoding Encoding { get; }

        /// <inheritdoc />
        public override void Write(string value)
        {
            Content.Append(value);
        }

        /// <inheritdoc />
        public override void Write(char value)
        {
            Content.Append(value.ToString());
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Content.ToString();
        }
    }
}