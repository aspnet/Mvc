// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Html.Abstractions;
using Microsoft.AspNet.Mvc.Rendering;

namespace Microsoft.AspNet.Mvc.ViewFeatures
{
    public interface IDefaultDisplayTemplates
    {
        IHtmlContent BooleanTemplate(IHtmlHelper htmlHelper);
        IHtmlContent CollectionTemplate(IHtmlHelper htmlHelper);
        IHtmlContent DecimalTemplate(IHtmlHelper htmlHelper);
        IHtmlContent EmailAddressTemplate(IHtmlHelper htmlHelper);
        IHtmlContent HiddenInputTemplate(IHtmlHelper htmlHelper);
        IHtmlContent HtmlTemplate(IHtmlHelper htmlHelper);
        IHtmlContent ObjectTemplate(IHtmlHelper htmlHelper);
        IHtmlContent StringTemplate(IHtmlHelper htmlHelper);
        IHtmlContent UrlTemplate(IHtmlHelper htmlHelper);
    }
}