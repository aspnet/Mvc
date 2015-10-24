// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Extensions.DiagnosticAdapter;

namespace Microsoft.AspNet.Mvc
{
    public class TestDiagnosticListener
    {
        public class OnBeforeActionEventData
        {
            public IProxyActionDescriptor ActionDescriptor { get; set; }
            public IProxyHttpContext HttpContext { get; set; }
            public IProxyRouteData RouteData { get; set; }
        }

        public OnBeforeActionEventData BeforeAction { get; set; }

        [DiagnosticName("Microsoft.AspNet.Mvc.BeforeAction")]
        public virtual void OnBeforeAction(
            IProxyHttpContext httpContext,
            IProxyRouteData routeData,
            IProxyActionDescriptor actionDescriptor)
        {
            BeforeAction = new OnBeforeActionEventData()
            {
                ActionDescriptor = actionDescriptor,
                HttpContext = httpContext,
                RouteData = routeData,
            };
        }

        public class OnAfterActionEventData
        {
            public IProxyActionDescriptor ActionDescriptor { get; set; }
            public IProxyHttpContext HttpContext { get; set; }
        }

        public OnAfterActionEventData AfterAction { get; set; }

        [DiagnosticName("Microsoft.AspNet.Mvc.AfterAction")]
        public virtual void OnAfterAction(
            IProxyHttpContext httpContext,
            IProxyActionDescriptor actionDescriptor)
        {
            AfterAction = new OnAfterActionEventData()
            {
                ActionDescriptor = actionDescriptor,
                HttpContext = httpContext,
            };
        }

        public class OnBeforeActionMethodEventData
        {
            public IProxyActionContext ActionContext { get; set; }
            public IReadOnlyDictionary<string, object> Arguments { get; set; }
        }

        public OnBeforeActionMethodEventData BeforeActionMethod { get; set; }

        [DiagnosticName("Microsoft.AspNet.Mvc.BeforeActionMethod")]
        public virtual void OnBeforeActionMethod(
            IProxyActionContext actionContext,
            IReadOnlyDictionary<string, object> arguments)
        {
            BeforeActionMethod = new OnBeforeActionMethodEventData()
            {
                ActionContext = actionContext,
                Arguments = arguments,
            };
        }

        public class OnAfterActionMethodEventData
        {
            public IProxyActionContext ActionContext { get; set; }
            public IProxyActionResult Result { get; set; }
        }

        public OnAfterActionMethodEventData AfterActionMethod { get; set; }

        [DiagnosticName("Microsoft.AspNet.Mvc.AfterActionMethod")]
        public virtual void OnAfterActionMethod(
            IProxyActionContext actionContext,
            IProxyActionResult result)
        {
            AfterActionMethod = new OnAfterActionMethodEventData()
            {
                ActionContext = actionContext,
                Result = result,
            };
        }

        public class OnBeforeActionResultEventData
        {
            public IProxyActionContext ActionContext { get; set; }
            public IProxyActionResult Result { get; set; }
        }

        public OnBeforeActionResultEventData BeforeActionResult { get; set; }

        [DiagnosticName("Microsoft.AspNet.Mvc.BeforeActionResult")]
        public virtual void OnBeforeActionResult(IProxyActionContext actionContext, IProxyActionResult result)
        {
            BeforeActionResult = new OnBeforeActionResultEventData()
            {
                ActionContext = actionContext,
                Result = result,
            };
        }

        public class OnAfterActionResultEventData
        {
            public IProxyActionContext ActionContext { get; set; }
            public IProxyActionResult Result { get; set; }
        }

        public OnAfterActionResultEventData AfterActionResult { get; set; }

        [DiagnosticName("Microsoft.AspNet.Mvc.AfterActionResult")]
        public virtual void OnAfterActionResult(IProxyActionContext actionContext, IProxyActionResult result)
        {
            AfterActionResult = new OnAfterActionResultEventData()
            {
                ActionContext = actionContext,
                Result = result,
            };
        }

        public class OnViewFoundEventData
        {
            public IProxyActionContext ActionContext { get; set; }
            public bool IsPartial { get; set; }
            public IProxyActionResult Result { get; set; }
            public string ViewName { get; set; }
            public IProxyView View { get; set; }
        }

        public OnViewFoundEventData ViewFound { get; set; }

        [DiagnosticName("Microsoft.AspNet.Mvc.ViewFound")]
        public virtual void OnViewFound(
            IProxyActionContext actionContext,
            bool isPartial,
            IProxyActionResult result,
            string viewName,
            IProxyView view)
        {
           ViewFound = new OnViewFoundEventData()
            {
                ActionContext = actionContext,
                IsPartial = isPartial,
                Result = result,
                ViewName = viewName,
                View = view,
            };
        }

        public class OnViewNotFoundEventData
        {
            public IProxyActionContext ActionContext { get; set; }
            public bool IsPartial { get; set; }
            public IProxyActionResult Result { get; set; }
            public string ViewName { get; set; }
            public IEnumerable<string> SearchedLocations { get; set; }
        }

        public OnViewNotFoundEventData ViewNotFound { get; set; }

        [DiagnosticName("Microsoft.AspNet.Mvc.ViewNotFound")]
        public virtual void OnViewNotFound(
            IProxyActionContext actionContext,
            bool isPartial,
            IProxyActionResult result,
            string viewName,
            IEnumerable<string> searchedLocations)
        {
            ViewNotFound = new OnViewNotFoundEventData()
            {
                ActionContext = actionContext,
                IsPartial = isPartial,
                Result = result,
                ViewName = viewName,
                SearchedLocations = searchedLocations,
            };
        }

        public class OnBeforeViewEventData
        {
            public IProxyView View { get; set; }
            public IProxyViewContext ViewContext { get; set; }
        }

        public OnBeforeViewEventData BeforeView { get; set; }

        [DiagnosticName("Microsoft.AspNet.Mvc.BeforeView")]
        public virtual void OnBeforeView(IProxyView view, IProxyViewContext viewContext)
        {
            BeforeView = new OnBeforeViewEventData()
            {
                View = view,
                ViewContext = viewContext,
            };
        }

        public class OnAfterViewEventData
        {
            public IProxyView View { get; set; }
            public IProxyViewContext ViewContext { get; set; }
        }

        public OnAfterViewEventData AfterView { get; set; }

        [DiagnosticName("Microsoft.AspNet.Mvc.AfterView")]
        public virtual void OnAfterView(IProxyView view, IProxyViewContext viewContext)
        {
            AfterView = new OnAfterViewEventData()
            {
                View = view,
                ViewContext = viewContext,
            };
        }

        public class OnBeforeViewComponentEventData
        {
            public IProxyActionDescriptor ActionDescriptor { get; set; }

            public IProxyViewComponentContext ViewComponentContext { get; set; }

            public object ViewComponent { get; set; }
        }

        public OnBeforeViewComponentEventData BeforeViewComponent { get; set; }

        [DiagnosticName("Microsoft.AspNet.Mvc.BeforeViewComponent")]
        public virtual void OnBeforeViewComponent(
            IProxyActionDescriptor actionDescriptor,
            IProxyViewComponentContext viewComponentContext,
            object viewComponent)
        {
            BeforeViewComponent = new OnBeforeViewComponentEventData()
            {
                ActionDescriptor = actionDescriptor,
                ViewComponentContext = viewComponentContext,
                ViewComponent = viewComponent
            };
        }

        public class OnAfterViewComponentEventData
        {
            public IProxyActionDescriptor ActionDescriptor { get; set; }

            public IProxyViewComponentContext ViewComponentContext { get; set; }

            public IProxyViewComponentResult ViewComponentResult { get; set; }

            public object ViewComponent { get; set; }
        }

        public OnAfterViewComponentEventData AfterViewComponent { get; set; }

        [DiagnosticName("Microsoft.AspNet.Mvc.AfterViewComponent")]
        public virtual void OnAfterViewComponent(
            IProxyActionDescriptor actionDescriptor,
            IProxyViewComponentContext viewComponentContext,
            IProxyViewComponentResult viewComponentResult,
            object viewComponent)
        {
            AfterViewComponent = new OnAfterViewComponentEventData()
            {
                ActionDescriptor = actionDescriptor,
                ViewComponentContext = viewComponentContext,
                ViewComponentResult = viewComponentResult,
                ViewComponent = viewComponent
            };
        }

        public class OnViewComponentBeforeViewExecuteEventData
        {
            public IProxyActionDescriptor ActionDescriptor { get; set; }

            public IProxyViewComponentContext ViewComponentContext { get; set; }

            public IProxyView View { get; set; }
        }

        public OnViewComponentBeforeViewExecuteEventData ViewComponentBeforeViewExecute { get; set; }

        [DiagnosticName("Microsoft.AspNet.Mvc.ViewComponentBeforeViewExecute")]
        public virtual void OnViewComponentBeforeViewExecute(
            IProxyActionDescriptor actionDescriptor,
            IProxyViewComponentContext viewComponentContext,
            IProxyView view)
        {
            ViewComponentBeforeViewExecute = new OnViewComponentBeforeViewExecuteEventData()
            {
                ActionDescriptor = actionDescriptor,
                ViewComponentContext = viewComponentContext,
                View = view
            };
        }

        public class OnViewComponentAfterViewExecuteEventData
        {
            public IProxyActionDescriptor ActionDescriptor { get; set; }

            public IProxyViewComponentContext ViewComponentContext { get; set; }

            public IProxyView View { get; set; }
        }

        public OnViewComponentAfterViewExecuteEventData ViewComponentAfterViewExecute { get; set; }

        [DiagnosticName("Microsoft.AspNet.Mvc.ViewComponentAfterViewExecute")]
        public virtual void OnViewComponentAfterViewExecute(
            IProxyActionDescriptor actionDescriptor,
            IProxyViewComponentContext viewComponentContext,
            IProxyView view)
        {
            ViewComponentAfterViewExecute = new OnViewComponentAfterViewExecuteEventData()
            {
                ActionDescriptor = actionDescriptor,
                ViewComponentContext = viewComponentContext,
                View = view
            };
        }
    }
}
