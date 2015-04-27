﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;

namespace Microsoft.AspNet.Mvc.TagHelpers
{
    /// <summary>
    /// <see cref="ITagHelper"/> implementation targeting &lt;form&gt; elements.
    /// </summary>
    public class FormTagHelper : TagHelper
    {
        private const string ActionAttributeName = "asp-action";
        private const string AntiForgeryAttributeName = "asp-anti-forgery";
        private const string ControllerAttributeName = "asp-controller";
        private const string RouteAttributeName = "asp-route";
        private const string RouteAttributePrefix = "asp-route-";
        private const string HtmlActionAttributeName = "action";

        [Activate, HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        [Activate, HtmlAttributeNotBound]
        public IHtmlGenerator Generator { get; set; }

        /// <summary>
        /// The name of the action method.
        /// </summary>
        [HtmlAttributeName(ActionAttributeName)]
        public string Action { get; set; }

        /// <summary>
        /// The name of the controller.
        /// </summary>
        [HtmlAttributeName(ControllerAttributeName)]
        public string Controller { get; set; }

        /// <summary>
        /// Whether the anti-forgery token should be generated.
        /// </summary>
        /// <value>Defaults to <c>false</c> if user provides an <c>action</c> attribute; <c>true</c> otherwise.</value>
        [HtmlAttributeName(AntiForgeryAttributeName)]
        public bool? AntiForgery { get; set; }

        /// <summary>
        /// Name of the route.
        /// </summary>
        /// <remarks>
        /// Must be <c>null</c> if <see cref="Action"/> or <see cref="Controller"/> is non-<c>null</c>.
        /// </remarks>
        [HtmlAttributeName(RouteAttributeName)]
        public string Route { get; set; }

        /// <inheritdoc />
        /// <remarks>
        /// Does nothing if user provides an <c>action</c> attribute and <see cref="AntiForgery"/> is <c>null</c> or
        /// <c>false</c>.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown if <c>action</c> attribute is provided and <see cref="Action"/> or <see cref="Controller"/> are
        /// non-<c>null</c> or if the user provided <c>asp-route-*</c> attributes.
        /// </exception>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var antiForgeryDefault = true;
            var routePrefixedAttributes = output.FindPrefixedAttributes(RouteAttributePrefix);

            // If "action" is already set, it means the user is attempting to use a normal <form>.
            if (output.Attributes.ContainsKey(HtmlActionAttributeName))
            {
                if (Action != null || Controller != null || Route != null || routePrefixedAttributes.Any())
                {
                    // User also specified bound attributes we cannot use.
                    throw new InvalidOperationException(
                        Resources.FormatFormTagHelper_CannotOverrideAction(
                            "<form>",
                            HtmlActionAttributeName,
                            ActionAttributeName,
                            ControllerAttributeName,
                            RouteAttributeName,
                            RouteAttributePrefix));
                }

                // User is using the FormTagHelper like a normal <form> tag. Anti-forgery default should be false to
                // not force the anti-forgery token on the user.
                antiForgeryDefault = false;
            }
            else
            {
                TagBuilder tagBuilder;
                var routeValues = GetRouteValues(output, routePrefixedAttributes);
                if (Route == null)
                {
                    tagBuilder = Generator.GenerateForm(
                        ViewContext,
                        Action,
                        Controller,
                        routeValues,
                        method: null,
                        htmlAttributes: null);
                }
                else if (Action != null || Controller != null)
                {
                    // Route and Action or Controller were specified. Can't determine the action attribute.
                    throw new InvalidOperationException(
                        Resources.FormatFormTagHelper_CannotDetermineActionWithRouteAndActionOrControllerSpecified(
                            "<form>",
                            RouteAttributeName,
                            ActionAttributeName,
                            ControllerAttributeName,
                            HtmlActionAttributeName));
                }
                else
                {
                    tagBuilder = Generator.GenerateRouteForm(
                        ViewContext,
                        Route,
                        routeValues,
                        method: null,
                        htmlAttributes: null);
                }

                if (tagBuilder != null)
                {
                    output.MergeAttributes(tagBuilder);
                    output.PostContent.Append(tagBuilder.InnerHtml);
                }
            }

            if (AntiForgery ?? antiForgeryDefault)
            {
                var antiForgeryTagBuilder = Generator.GenerateAntiForgery(ViewContext);
                if (antiForgeryTagBuilder != null)
                {
                    output.PostContent.Append(antiForgeryTagBuilder.ToString(TagRenderMode.SelfClosing));
                }
            }
        }

        // TODO: https://github.com/aspnet/Razor/issues/89 - We will not need this method once #89 is completed.
        private static Dictionary<string, object> GetRouteValues(
            TagHelperOutput output,
            IEnumerable<KeyValuePair<string, object>> routePrefixedAttributes)
        {
            Dictionary<string, object> routeValues = null;
            if (routePrefixedAttributes.Any())
            {
                // Prefixed values should be treated as bound attributes, remove them from the output.
                output.RemoveRange(routePrefixedAttributes);

                // Remove prefix from keys and convert all values to strings. HtmlString and similar classes are not
                // meaningful to routing.
                routeValues = routePrefixedAttributes.ToDictionary(
                    attribute => attribute.Key.Substring(RouteAttributePrefix.Length),
                    attribute => (object)attribute.Value.ToString());
            }

            return routeValues;
        }
    }
}