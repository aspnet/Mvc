// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Microsoft.AspNetCore.Mvc.RazorPages
{
    public class Page : IRazorPage
    {
        public IHtmlContent BodyContent { get; set; }

        public bool IsLayoutBeingRendered { get; set; }

        public string Layout { get; set; }

        public string Path { get; set; }

        public IDictionary<string, RenderAsyncDelegate> PreviousSectionWriters { get; set; }

        public IDictionary<string, RenderAsyncDelegate> SectionWriters { get; }

        public PageContext PageContext { get; set; }

        public ViewContext ViewContext { get; set; }

        public void EnsureRenderedBodyOrSections()
        {
        }

        public Task ExecuteAsync()
        {
            return TaskCache.CompletedTask;
        }
    }
}
