// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Abstractions;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public class ViewContext
    {
        // Some values have to be stored in HttpContext.Items in order to be propagated between calls to
        // RenderPartial(), RenderAction(), etc.
        private static readonly object _formContextKey = new object();

        // We need a default FormContext if the user uses html <form> instead of an MvcForm
        private readonly FormContext _defaultFormContext = new FormContext();

        public ViewContext([NotNull] HttpContext context, [NotNull] ViewData viewData, IServiceProvider serviceProvider)
        {
            HttpContext = context;
            ViewData = viewData;
            ServiceProvider = serviceProvider;
        }

        public virtual FormContext FormContext
        {
            get
            {
                // HttpContext.Items[_formContextKey] will throw if key not found.
                object formContextObject;
                HttpContext.Items.TryGetValue(_formContextKey, out formContextObject);

                // Never return a null form context, this is important for validation purposes.
                return formContextObject as FormContext ?? _defaultFormContext;
            }
            set { HttpContext.Items[_formContextKey] = value; }
        }

        public HttpContext HttpContext { get; private set; }

        public IServiceProvider ServiceProvider { get; private set; }

        public IUrlHelper Url { get; set; }

        public ViewData ViewData { get; private set; }
    }
}
