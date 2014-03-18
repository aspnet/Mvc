// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.AspNet.Abstractions;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public class HtmlHelper<TModel> : HtmlHelper
    {
        public HtmlHelper([NotNull]HttpContext httpContext, ViewData<TModel> viewData)
            : base(httpContext, viewData)
        {
            ViewData = viewData;
        }

        public new ViewData<TModel> ViewData
        {
            get; private set;
        }
    }
}
