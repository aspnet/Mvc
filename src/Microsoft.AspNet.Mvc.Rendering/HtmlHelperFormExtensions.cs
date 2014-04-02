// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Web.Routing;
using System.Web.WebPages;

namespace System.Web.Mvc.Html
{
    public static class FormExtensions
    {
        public static MvcForm BeginForm(this HtmlHelper htmlHelper)
        {
            // generates <form action="{current url}" method="post">...</form>
            string formAction = htmlHelper.ViewContext.HttpContext.Request.RawUrl;
            return FormHelper(htmlHelper, formAction, FormMethod.Post, new RouteValueDictionary());
        }

        public static MvcForm BeginForm(this HtmlHelper htmlHelper, object routeValues)
        {
            return BeginForm(htmlHelper, null, null, TypeHelper.ObjectToDictionary(routeValues), FormMethod.Post, new RouteValueDictionary());
        }

        public static MvcForm BeginForm(this HtmlHelper htmlHelper, RouteValueDictionary routeValues)
        {
            return BeginForm(htmlHelper, null, null, routeValues, FormMethod.Post, new RouteValueDictionary());
        }

        public static MvcForm BeginForm(this HtmlHelper htmlHelper, string actionName, string controllerName)
        {
            return BeginForm(htmlHelper, actionName, controllerName, new RouteValueDictionary(), FormMethod.Post, new RouteValueDictionary());
        }

        public static MvcForm BeginForm(this HtmlHelper htmlHelper, string actionName, string controllerName, object routeValues)
        {
            return BeginForm(htmlHelper, actionName, controllerName, TypeHelper.ObjectToDictionary(routeValues), FormMethod.Post, new RouteValueDictionary());
        }

        public static MvcForm BeginForm(this HtmlHelper htmlHelper, string actionName, string controllerName, RouteValueDictionary routeValues)
        {
            return BeginForm(htmlHelper, actionName, controllerName, routeValues, FormMethod.Post, new RouteValueDictionary());
        }

        public static MvcForm BeginForm(this HtmlHelper htmlHelper, string actionName, string controllerName, FormMethod method)
        {
            return BeginForm(htmlHelper, actionName, controllerName, new RouteValueDictionary(), method, new RouteValueDictionary());
        }

        public static MvcForm BeginForm(this HtmlHelper htmlHelper, string actionName, string controllerName, object routeValues, FormMethod method)
        {
            return BeginForm(htmlHelper, actionName, controllerName, TypeHelper.ObjectToDictionary(routeValues), method, new RouteValueDictionary());
        }

        public static MvcForm BeginForm(this HtmlHelper htmlHelper, string actionName, string controllerName, RouteValueDictionary routeValues, FormMethod method)
        {
            return BeginForm(htmlHelper, actionName, controllerName, routeValues, method, new RouteValueDictionary());
        }

        public static MvcForm BeginForm(this HtmlHelper htmlHelper, string actionName, string controllerName, FormMethod method, object htmlAttributes)
        {
            return BeginForm(htmlHelper, actionName, controllerName, new RouteValueDictionary(), method, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static MvcForm BeginForm(this HtmlHelper htmlHelper, string actionName, string controllerName, FormMethod method, IDictionary<string, object> htmlAttributes)
        {
            return BeginForm(htmlHelper, actionName, controllerName, new RouteValueDictionary(), method, htmlAttributes);
        }

        public static MvcForm BeginForm(this HtmlHelper htmlHelper, string actionName, string controllerName, object routeValues, FormMethod method, object htmlAttributes)
        {
            return BeginForm(htmlHelper, actionName, controllerName, TypeHelper.ObjectToDictionary(routeValues), method, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static MvcForm BeginForm(this HtmlHelper htmlHelper, string actionName, string controllerName, RouteValueDictionary routeValues, FormMethod method, IDictionary<string, object> htmlAttributes)
        {
            string formAction = UrlHelper.GenerateUrl(null /* routeName */, actionName, controllerName, routeValues, htmlHelper.RouteCollection, htmlHelper.ViewContext.RequestContext, true /* includeImplicitMvcValues */);
            return FormHelper(htmlHelper, formAction, method, htmlAttributes);
        }

        public static MvcForm BeginRouteForm(this HtmlHelper htmlHelper, object routeValues)
        {
            return BeginRouteForm(htmlHelper, null /* routeName */, TypeHelper.ObjectToDictionary(routeValues), FormMethod.Post, new RouteValueDictionary());
        }

        public static MvcForm BeginRouteForm(this HtmlHelper htmlHelper, RouteValueDictionary routeValues)
        {
            return BeginRouteForm(htmlHelper, null /* routeName */, routeValues, FormMethod.Post, new RouteValueDictionary());
        }

        public static MvcForm BeginRouteForm(this HtmlHelper htmlHelper, string routeName)
        {
            return BeginRouteForm(htmlHelper, routeName, new RouteValueDictionary(), FormMethod.Post, new RouteValueDictionary());
        }

        public static MvcForm BeginRouteForm(this HtmlHelper htmlHelper, string routeName, object routeValues)
        {
            return BeginRouteForm(htmlHelper, routeName, TypeHelper.ObjectToDictionary(routeValues), FormMethod.Post, new RouteValueDictionary());
        }

        public static MvcForm BeginRouteForm(this HtmlHelper htmlHelper, string routeName, RouteValueDictionary routeValues)
        {
            return BeginRouteForm(htmlHelper, routeName, routeValues, FormMethod.Post, new RouteValueDictionary());
        }

        public static MvcForm BeginRouteForm(this HtmlHelper htmlHelper, string routeName, FormMethod method)
        {
            return BeginRouteForm(htmlHelper, routeName, new RouteValueDictionary(), method, new RouteValueDictionary());
        }

        public static MvcForm BeginRouteForm(this HtmlHelper htmlHelper, string routeName, object routeValues, FormMethod method)
        {
            return BeginRouteForm(htmlHelper, routeName, TypeHelper.ObjectToDictionary(routeValues), method, new RouteValueDictionary());
        }

        public static MvcForm BeginRouteForm(this HtmlHelper htmlHelper, string routeName, RouteValueDictionary routeValues, FormMethod method)
        {
            return BeginRouteForm(htmlHelper, routeName, routeValues, method, new RouteValueDictionary());
        }

        public static MvcForm BeginRouteForm(this HtmlHelper htmlHelper, string routeName, FormMethod method, object htmlAttributes)
        {
            return BeginRouteForm(htmlHelper, routeName, new RouteValueDictionary(), method, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static MvcForm BeginRouteForm(this HtmlHelper htmlHelper, string routeName, FormMethod method, IDictionary<string, object> htmlAttributes)
        {
            return BeginRouteForm(htmlHelper, routeName, new RouteValueDictionary(), method, htmlAttributes);
        }

        public static MvcForm BeginRouteForm(this HtmlHelper htmlHelper, string routeName, object routeValues, FormMethod method, object htmlAttributes)
        {
            return BeginRouteForm(htmlHelper, routeName, TypeHelper.ObjectToDictionary(routeValues), method, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static MvcForm BeginRouteForm(this HtmlHelper htmlHelper, string routeName, RouteValueDictionary routeValues, FormMethod method, IDictionary<string, object> htmlAttributes)
        {
            string formAction = UrlHelper.GenerateUrl(routeName, null, null, routeValues, htmlHelper.RouteCollection, htmlHelper.ViewContext.RequestContext, false /* includeImplicitMvcValues */);
            return FormHelper(htmlHelper, formAction, method, htmlAttributes);
        }

        public static void EndForm(this HtmlHelper htmlHelper)
        {
            EndForm(htmlHelper.ViewContext);
        }

        internal static void EndForm(ViewContext viewContext)
        {
            viewContext.Writer.Write("</form>");
            viewContext.OutputClientValidation();
            viewContext.FormContext = null;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Because disposing the object would write to the response stream, you don't want to prematurely dispose of this object.")]
        private static MvcForm FormHelper(this HtmlHelper htmlHelper, string formAction, FormMethod method, IDictionary<string, object> htmlAttributes)
        {
            TagBuilder tagBuilder = new TagBuilder("form");
            tagBuilder.MergeAttributes(htmlAttributes);
            // action is implicitly generated, so htmlAttributes take precedence.
            tagBuilder.MergeAttribute("action", formAction);
            // method is an explicit parameter, so it takes precedence over the htmlAttributes.
            tagBuilder.MergeAttribute("method", HtmlHelper.GetFormMethodString(method), true);

            bool traditionalJavascriptEnabled = htmlHelper.ViewContext.ClientValidationEnabled
                                                && !htmlHelper.ViewContext.UnobtrusiveJavaScriptEnabled;

            if (traditionalJavascriptEnabled)
            {
                // forms must have an ID for client validation
                tagBuilder.GenerateId(htmlHelper.ViewContext.FormIdGenerator());
            }

            htmlHelper.ViewContext.Writer.Write(tagBuilder.ToString(TagRenderMode.StartTag));
            MvcForm theForm = new MvcForm(htmlHelper.ViewContext);

            if (traditionalJavascriptEnabled)
            {
                htmlHelper.ViewContext.FormContext.FormId = tagBuilder.Attributes["id"];
            }

            return theForm;
        }
    }
}
