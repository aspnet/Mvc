// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Mvc.TagHelpers
{
    /// <summary>
    /// <see cref="ITagHelper"/> implementation targeting &lt;a&gt; elements.
    /// </summary>
    [HtmlTargetElement("a", Attributes = ActionAttributeName)]
    [HtmlTargetElement("a", Attributes = ControllerAttributeName)]
    [HtmlTargetElement("a", Attributes = AreaAttributeName)]
    [HtmlTargetElement("a", Attributes = PageAttributeName)]
    [HtmlTargetElement("a", Attributes = FragmentAttributeName)]
    [HtmlTargetElement("a", Attributes = HostAttributeName)]
    [HtmlTargetElement("a", Attributes = ProtocolAttributeName)]
    [HtmlTargetElement("a", Attributes = RouteAttributeName)]
    [HtmlTargetElement("a", Attributes = RouteValuesDictionaryName)]
    [HtmlTargetElement("a", Attributes = RouteValuesPrefix + "*")]
    public class AnchorTagHelper : TagHelper
    {
        private const string ActionAttributeName = "asp-action";
        private const string ControllerAttributeName = "asp-controller";
        private const string AreaAttributeName = "asp-area";
        private const string PageAttributeName = "asp-page";
        private const string FragmentAttributeName = "asp-fragment";
        private const string HostAttributeName = "asp-host";
        private const string ProtocolAttributeName = "asp-protocol";
        private const string RouteAttributeName = "asp-route";
        private const string RouteValuesDictionaryName = "asp-all-route-data";
        private const string RouteValuesPrefix = "asp-route-";
        private const string Href = "href";
        private IDictionary<string, string> _routeValues;

        /// <summary>
        /// Creates a new <see cref="AnchorTagHelper"/>.
        /// </summary>
        /// <param name="generator">The <see cref="IHtmlGenerator"/>.</param>
        public AnchorTagHelper(IHtmlGenerator generator)
        {
            Generator = generator;
        }

        /// <inheritdoc />
        public override int Order
        {
            get
            {
                return -1000;
            }
        }

        protected IHtmlGenerator Generator { get; }

        /// <summary>
        /// The name of the action method.
        /// </summary>
        /// <remarks>
        /// Must be <c>null</c> if <see cref="Route"/> or <see cref="Page"/> is non-<c>null</c>.
        /// </remarks>
        [HtmlAttributeName(ActionAttributeName)]
        public string Action { get; set; }

        /// <summary>
        /// The name of the controller.
        /// </summary>
        /// <remarks>
        /// Must be <c>null</c> if <see cref="Route"/> or <see cref="Page"/> is non-<c>null</c>.
        /// </remarks>
        [HtmlAttributeName(ControllerAttributeName)]
        public string Controller { get; set; }

        /// <summary>
        /// The name of the area.
        /// </summary>
        /// <remarks>
        /// Must be <c>null</c> if <see cref="Route"/> or <see cref="Page"/> is non-<c>null</c>.
        /// </remarks>
        [HtmlAttributeName(AreaAttributeName)]
        public string Area { get; set; }

        /// <summary>
        /// The name of the page.
        /// </summary>
        /// <remarks>
        /// Must be <c>null</c> if <see cref="Route"/> or <see cref="Action"/>, <see cref="Controller"/>
        /// or <see cref="Area"/> is non-<c>null</c>.
        /// </remarks>
        [HtmlAttributeName(PageAttributeName)]
        public string Page { get; set; }

        /// <summary>
        /// The protocol for the URL, such as &quot;http&quot; or &quot;https&quot;.
        /// </summary>
        [HtmlAttributeName(ProtocolAttributeName)]
        public string Protocol { get; set; }

        /// <summary>
        /// The host name.
        /// </summary>
        [HtmlAttributeName(HostAttributeName)]
        public string Host { get; set; }

        /// <summary>
        /// The URL fragment name.
        /// </summary>
        [HtmlAttributeName(FragmentAttributeName)]
        public string Fragment { get; set; }

        /// <summary>
        /// Name of the route.
        /// </summary>
        /// <remarks>
        /// Must be <c>null</c> if one of <see cref="Action"/>, <see cref="Controller"/>, <see cref="Area"/> 
        /// or <see cref="Page"/> is non-<c>null</c>.
        /// </remarks>
        [HtmlAttributeName(RouteAttributeName)]
        public string Route { get; set; }

        /// <summary>
        /// Additional parameters for the route.
        /// </summary>
        [HtmlAttributeName(RouteValuesDictionaryName, DictionaryAttributePrefix = RouteValuesPrefix)]
        public IDictionary<string, string> RouteValues
        {
            get
            {
                if (_routeValues == null)
                {
                    _routeValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }

                return _routeValues;
            }
            set
            {
                _routeValues = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Rendering.ViewContext"/> for the current request.
        /// </summary>
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        /// <inheritdoc />
        /// <remarks>Does nothing if user provides an <c>href</c> attribute.</remarks>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            // If "href" is already set, it means the user is attempting to use a normal anchor.
            if (output.Attributes.ContainsName(Href))
            {
                if (Action != null ||
                    Controller != null ||
                    Area != null ||
                    Page != null ||
                    Route != null ||
                    Protocol != null ||
                    Host != null ||
                    Fragment != null ||
                    (_routeValues != null && _routeValues.Count > 0))
                {
                    // User specified an href and one of the bound attributes; can't determine the href attribute.
                    throw new InvalidOperationException(
                        Resources.FormatAnchorTagHelper_CannotOverrideHref(
                            Href,
                            "<a>",
                            RouteValuesPrefix,
                            ActionAttributeName,
                            ControllerAttributeName,
                            AreaAttributeName,
                            RouteAttributeName,
                            ProtocolAttributeName,
                            HostAttributeName,
                            FragmentAttributeName,
                            PageAttributeName));
                }

                return;
            }

            var routeLink = Route != null;
            var actionLink = Controller != null || Action != null;
            var pageLink = Page != null;

            if ((routeLink && actionLink) || (routeLink && pageLink) || (actionLink && pageLink))
            {
                var message = string.Join(
                    Environment.NewLine,
                    Resources.FormatCannotDetermineAttributeFor(Href, "<a>"),
                    RouteAttributeName,
                    ControllerAttributeName + ", " + ActionAttributeName,
                    PageAttributeName);

                throw new InvalidOperationException(message);
            }

            RouteValueDictionary routeValues = null;
            if (_routeValues != null && _routeValues.Count > 0)
            {
                routeValues = new RouteValueDictionary(_routeValues);
            }

            if (Area != null)
            {
                // Unconditionally replace any value from asp-route-area.
                SetRouteValue(ref routeValues, "area", Area);
            }

            TagBuilder tagBuilder;
            if (pageLink)
            {
                tagBuilder = Generator.GeneratePageLink(
                    ViewContext,
                    linkText: string.Empty,
                    pageName: Page,
                    protocol: Protocol,
                    hostname: Host,
                    fragment: Fragment,
                    routeValues: routeValues,
                    htmlAttributes: null);
            }
            else if (routeLink)
            {
                tagBuilder = Generator.GenerateRouteLink(
                    ViewContext,
                    linkText: string.Empty,
                    routeName: Route,
                    protocol: Protocol,
                    hostName: Host,
                    fragment: Fragment,
                    routeValues: routeValues,
                    htmlAttributes: null);
            }
            else
            {
                tagBuilder = Generator.GenerateActionLink(
                   ViewContext,
                   linkText: string.Empty,
                   actionName: Action,
                   controllerName: Controller,
                   protocol: Protocol,
                   hostname: Host,
                   fragment: Fragment,
                   routeValues: routeValues,
                   htmlAttributes: null);
            }

            output.MergeAttributes(tagBuilder);
        }

        private static void SetRouteValue(ref RouteValueDictionary routeValues, string name, string value)
        {
            if (routeValues == null)
            {
                routeValues = new RouteValueDictionary();
            }

            routeValues[name] = value;
        }
    }
}