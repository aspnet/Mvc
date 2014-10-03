// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.AspNet.Razor.TagHelpers;

namespace TagHelpersWebSite.TagHelpers
{
    [ContentBehavior(ContentBehavior.Prepend)]
    public class ATagHelper : TagHelper
    {
        private static readonly string[] IgnoredAttributes = new[] { "class", "style" };

        private readonly IUrlHelper _urlHelper;

        public ATagHelper(IUrlHelper urlHelper)
        {
            _urlHelper = urlHelper;
        }

        public string Controller { get; set; }

        public string Action { get; set; }

        public override void Process(TagHelperOutput output, TagHelperContext context)
        {
            if (Controller != null && Action != null)
            {
                // We pull the href parameters from the HTML element if they aren't contained in the list of ignored
                // attributes.
                var validAttributes = output.Attributes.Where(attribute =>
                    !IgnoredAttributes.Contains(attribute.Key, StringComparer.OrdinalIgnoreCase));
                var methodParameters = validAttributes.ToDictionary(attribute => attribute.Key,
                                                                    attribute => (object)attribute.Value);

                // We remove all method attributes from the resulting HTML element because they're supposed to
                // be parameters to our final href value.
                foreach (var parameter in methodParameters.Keys)
                {
                    output.Attributes.Remove(parameter);
                }

                output.Attributes["href"] = _urlHelper.Action(Action, Controller, methodParameters);

                output.Content = "My ";
            }
        }
    }
}