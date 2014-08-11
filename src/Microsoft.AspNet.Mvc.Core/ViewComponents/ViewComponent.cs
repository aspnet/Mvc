// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.Rendering;

namespace Microsoft.AspNet.Mvc
{
    [ViewComponent]
    public abstract class ViewComponent
    {
        private dynamic _viewBag;

        public HttpContext Context
        {
            get { return ViewContext == null ? null : ViewContext.HttpContext; }
        }

        public dynamic ViewBag
        {
            get
            {
                if (_viewBag == null)
                {
                    _viewBag = new DynamicViewData(() => ViewData);
                }

                return _viewBag;
            }
        }

        [Activate]
        public ViewContext ViewContext { get; set; }

        [Activate]
        public ViewDataDictionary ViewData { get; set; }

        [Activate]
        public ICompositeViewEngine ViewEngine { get; set; }

        public ContentViewComponentResult Content([NotNull] string content)
        {
            return new ContentViewComponentResult(content);
        }

        public JsonViewComponentResult Json([NotNull] object value)
        {
            return new JsonViewComponentResult(value);
        }

        public ViewViewComponentResult View()
        {
            return View<object>(null, null);
        }

        public ViewViewComponentResult View(string viewName)
        {
            return View<object>(viewName, null);
        }

        public ViewViewComponentResult View<TModel>(TModel model)
        {
            return View(null, model);
        }

        public ViewViewComponentResult View<TModel>(string viewName, TModel model)
        {
            var viewData = new ViewDataDictionary<TModel>(ViewData);
            if (model != null)
            {
                viewData.Model = model;
            }

            return new ViewViewComponentResult(ViewEngine, viewName, viewData);
        }
    }
}
