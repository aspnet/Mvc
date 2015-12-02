// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Html.Abstractions;
using Microsoft.AspNet.Mvc.Rendering;

namespace Microsoft.AspNet.Mvc.ViewFeatures
{
    public interface IDefaultEditorTemplates
    {
        IHtmlContent BooleanTemplate(IHtmlHelper htmlHelper);
        IHtmlContent CollectionTemplate(IHtmlHelper htmlHelper);
        IHtmlContent DateInputTemplate(IHtmlHelper htmlHelper);
        IHtmlContent DateTimeInputTemplate(IHtmlHelper htmlHelper);
        IHtmlContent DateTimeLocalInputTemplate(IHtmlHelper htmlHelper);
        IHtmlContent DecimalTemplate(IHtmlHelper htmlHelper);
        IHtmlContent EmailAddressInputTemplate(IHtmlHelper htmlHelper);
        IHtmlContent FileCollectionInputTemplate(IHtmlHelper htmlHelper);
        IHtmlContent FileInputTemplate(IHtmlHelper htmlHelper);
        IHtmlContent HiddenInputTemplate(IHtmlHelper htmlHelper);
        IHtmlContent MultilineTemplate(IHtmlHelper htmlHelper);
        IHtmlContent NumberInputTemplate(IHtmlHelper htmlHelper);
        IHtmlContent ObjectTemplate(IHtmlHelper htmlHelper);
        IHtmlContent PasswordTemplate(IHtmlHelper htmlHelper);
        IHtmlContent PhoneNumberInputTemplate(IHtmlHelper htmlHelper);
        IHtmlContent StringTemplate(IHtmlHelper htmlHelper);
        IHtmlContent TimeInputTemplate(IHtmlHelper htmlHelper);
        IHtmlContent UrlInputTemplate(IHtmlHelper htmlHelper);
    }
}