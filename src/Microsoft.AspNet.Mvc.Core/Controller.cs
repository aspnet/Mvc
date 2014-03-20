﻿using Microsoft.AspNet.Abstractions;
using Microsoft.AspNet.Mvc.Rendering;

namespace Microsoft.AspNet.Mvc
{
    public class Controller
    {
        public void Initialize(IActionResultHelper actionResultHelper)
        {
            Result = actionResultHelper;
            ViewData = new ViewData<object>();
        }

        public IActionResultHelper Result { get; private set; }

        public HttpContext Context { get; set; }

        public IUrlHelper Url { get; set; }

        public ViewData<object> ViewData { get; set; }

        public dynamic ViewBag
        {
            get { return ViewData; }
        }

        public IActionResult View()
        {
            return View(view: null);
        }

        public IActionResult View(string view)
        {
            return View(view, model: (object)null);
        }

        public IActionResult View<TModel>(TModel model)
        {
            return View(view: null, model: model);
        }

        public IActionResult View<TModel>(string view, TModel model)
        {
            var viewDataDictionary = new ViewData<TModel>
            {
                Model = model
            };
            return Result.View(view, viewDataDictionary);
        }
    }
}
