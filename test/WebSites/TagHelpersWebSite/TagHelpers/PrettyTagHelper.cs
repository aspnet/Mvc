// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;

namespace TagHelpersWebSite.TagHelpers
{
    [TagName("*")]
    public class PrettyTagHelper : TagHelper, ICanHasViewContext
    {
        private static readonly Dictionary<string, string> PrettyTagStyles =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "a", @"background-color: gray;
                         color: white;
                         border-radius: 3px;
                         border: 1px solid black;
                         padding: 3px;
                         font-family: cursive;" },
                { "strong", @"font-size: 1.25em;
                              text-decoration: underline;" },
                { "h1", @"font-family: cursive;" },
                { "h3", @"font-family: cursive;" }
            };

        private bool _notPretty;

        public bool? MakePretty { get; set; }

        public void Contextualize([NotNull]ViewContext viewContext)
        {
            var requestQuery = viewContext.HttpContext.Request.Query;

            if (requestQuery.ContainsKey("notPretty"))
            {
                _notPretty = Convert.ToBoolean(requestQuery["notPretty"]);
            }
        }

        public override void Process(TagHelperOutput output, TagHelperContext context)
        {
            if (_notPretty || (MakePretty.HasValue && !MakePretty.Value))
            {
                return;
            }

            string prettyStyle;

            if (PrettyTagStyles.TryGetValue(output.TagName, out prettyStyle))
            {
                var style = string.Empty;

                if (output.Attributes.TryGetValue("style", out style))
                {
                    style += ";";
                }

                output.Attributes["style"] = style + prettyStyle;
            }
        }
    }
}