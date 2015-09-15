// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Microsoft.AspNet.Html.Abstractions;

namespace Microsoft.AspNet.Mvc.Razor
{
    public abstract class HtmlTextWriter : TextWriter
    {
        public abstract void Write(IHtmlContent content);

        /// <inheritdoc />
        public override void Write(object value)
        {
            var htmlContent = value as IHtmlContent;
            if (htmlContent == null)
            {
                base.Write(value);
            }
            else
            {
                Write(htmlContent);
            }
        }

        /// <inheritdoc />
        public override void WriteLine(object value)
        {
            var htmlContent = value as IHtmlContent;
            if (htmlContent == null)
            {
                base.Write(value);
            }
            else
            {
                Write(htmlContent);
            }

            base.WriteLine();
        }
    }
}
