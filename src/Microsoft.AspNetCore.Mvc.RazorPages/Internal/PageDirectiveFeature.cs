// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.AspNetCore.Razor.Language;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Internal
{
    public struct PageDirectiveFeature
    {
        private const string DirectiveToken = "@page";
        private static readonly char[] Separators = new[] { ' ' };

        private PageDirectiveFeature(string routeTemplate, string name)
        {
            RouteTemplate = routeTemplate;
            Name = name;
        }

        public string RouteTemplate { get; }

        public string Name { get; }

        public static bool TryGetPageDirective(
            RazorProjectItem projectItem,
            out PageDirectiveFeature directive)
        {
            if (projectItem == null)
            {
                throw new ArgumentNullException(nameof(projectItem));
            }

            return TryGetPageDirective(projectItem.Read, out directive);
        }

        public static bool TryGetPageDirective(
            Func<Stream> streamFactory,
            out PageDirectiveFeature directive)
        {
            if (streamFactory == null)
            {
                throw new ArgumentNullException(nameof(streamFactory));
            }

            var stream = streamFactory();
            string content;
            using (var streamReader = new StreamReader(stream))
            {
                do
                {
                    content = streamReader.ReadLine();
                } while (content != null && string.IsNullOrWhiteSpace(content));
            }

            directive = default(PageDirectiveFeature);
            if (content == null)
            {
                return false;
            }

            var tokens = content.Split(Separators, 4, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0 || tokens.Length > 3)
            {
                return false;
            }

            if (!string.Equals(tokens[0], DirectiveToken, StringComparison.Ordinal))
            {
                return false;
            }

            string template = null;
            string pageName = null;
            if (tokens.Length > 1)
            {
                template = tokens[1];
                if (!TryGetUnquotedValue(ref template))
                {
                    return false;
                }
            }

            if (tokens.Length > 2)
            {
                pageName = tokens[2];
                if (!TryGetUnquotedValue(ref pageName))
                {
                    return false;
                }
            }

            directive = new PageDirectiveFeature(template, pageName);
            return true;
        }

        private static bool TryGetUnquotedValue(ref string value)
        {
            if (!value.StartsWith("\"", StringComparison.Ordinal) ||
                !value.EndsWith("\"", StringComparison.Ordinal))
            {
                return false;
            }

            value = value.Substring(1, value.Length - 2);
            return true;
        }
    }
}
