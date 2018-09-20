// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc.Core;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    public class AttributeRouteModel
    {
        private static readonly AttributeRouteModel _default = new AttributeRouteModel();

        public AttributeRouteModel()
        {
        }

        public AttributeRouteModel(IRouteTemplateProvider templateProvider)
        {
            if (templateProvider == null)
            {
                throw new ArgumentNullException(nameof(templateProvider));
            }

            Attribute = templateProvider;
            Template = templateProvider.Template;
            Order = templateProvider.Order;
            Name = templateProvider.Name;
        }

        public AttributeRouteModel(AttributeRouteModel other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Attribute = other.Attribute;
            Name = other.Name;
            Order = other.Order;
            Template = other.Template;
            SuppressLinkGeneration = other.SuppressLinkGeneration;
            SuppressPathMatching = other.SuppressPathMatching;
        }

        public IRouteTemplateProvider Attribute { get;}

        public string Template { get; set; }

        public int? Order { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value that determines if this model participates in link generation.
        /// </summary>
        public bool SuppressLinkGeneration { get; set; }

        /// <summary>
        /// Gets or sets a value that determines if this model participates in path matching (inbound routing).
        /// </summary>
        public bool SuppressPathMatching { get; set; }

        public bool IsAbsoluteTemplate => Template != null && IsOverridePattern(Template);

        /// <summary>
        /// Combines two <see cref="AttributeRouteModel"/> instances and returns
        /// a new <see cref="AttributeRouteModel"/> instance with the result.
        /// </summary>
        /// <param name="left">The left <see cref="AttributeRouteModel"/>.</param>
        /// <param name="right">The right <see cref="AttributeRouteModel"/>.</param>
        /// <returns>A new instance of <see cref="AttributeRouteModel"/> that represents the
        /// combination of the two <see cref="AttributeRouteModel"/> instances or <c>null</c> if both
        /// parameters are <c>null</c>.</returns>
        public static AttributeRouteModel CombineAttributeRouteModel(
            AttributeRouteModel left,
            AttributeRouteModel right)
        {
            right = right ?? _default;

            // If the right template is an override template (starts with / or ~/)
            // we ignore the values from left.
            if (left == null || IsOverridePattern(right.Template))
            {
                left = _default;
            }

            var combinedTemplate = CombineTemplates(left.Template, right.Template);

            // The action is not attribute routed.
            if (combinedTemplate == null)
            {
                return null;
            }

            return new AttributeRouteModel()
            {
                Template = combinedTemplate,
                Order = right.Order ?? left.Order,
                Name = ChooseName(left, right),
                SuppressLinkGeneration = left.SuppressLinkGeneration || right.SuppressLinkGeneration,
                SuppressPathMatching = left.SuppressPathMatching || right.SuppressPathMatching,
            };
        }

        /// <summary>
        /// Combines the prefix and route template for an attribute route.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <param name="template">The route template.</param>
        /// <returns>The combined pattern.</returns>
        public static string CombineTemplates(string prefix, string template)
        {
            var result = CombineCore(prefix, template);
            return CleanTemplate(result);
        }

        /// <summary>
        /// Determines if a template pattern can be used to override a prefix.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <returns><c>true</c> if this is an overriding template, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// Route templates starting with "~/" or "/" can be used to override the prefix.
        /// </remarks>
        public static bool IsOverridePattern(string template)
        {
            return template != null &&
                (template.StartsWith("~/", StringComparison.Ordinal) ||
                template.StartsWith("/", StringComparison.Ordinal));
        }

        private static string ChooseName(
            AttributeRouteModel left,
            AttributeRouteModel right)
        {
            if (right.Name == null && string.IsNullOrEmpty(right.Template))
            {
                return left.Name;
            }
            else
            {
                return right.Name;
            }
        }

        private static string CombineCore(string left, string right)
        {
            if (left == null && right == null)
            {
                return null;
            }
            else if (right == null)
            {
                return left;
            }
            else if (IsEmptyLeftSegment(left) || IsOverridePattern(right))
            {
                return right;
            }

            if (left.EndsWith("/", StringComparison.Ordinal))
            {
                return left + right;
            }

            // Both templates contain some text.
            return left + "/" + right;
        }

        private static bool IsEmptyLeftSegment(string template)
        {
            return template == null ||
                template.Equals(string.Empty, StringComparison.Ordinal) ||
                template.Equals("~/", StringComparison.Ordinal) ||
                template.Equals("/", StringComparison.Ordinal);
        }

        private static string CleanTemplate(string result)
        {
            if (result == null)
            {
                return null;
            }

            // This is an invalid combined template, so we don't want to
            // accidentally clean it and produce a valid template. For that
            // reason we ignore the clean up process for it.
            if (result.Equals("//", StringComparison.Ordinal))
            {
                return result;
            }

            var startIndex = 0;
            if (result.StartsWith("/", StringComparison.Ordinal))
            {
                startIndex = 1;
            }
            else if (result.StartsWith("~/", StringComparison.Ordinal))
            {
                startIndex = 2;
            }

            // We are in the case where the string is "/" or "~/"
            if (startIndex == result.Length)
            {
                return string.Empty;
            }

            var subStringLength = result.Length - startIndex;
            if (result.EndsWith("/", StringComparison.Ordinal))
            {
                subStringLength--;
            }

            return result.Substring(startIndex, subStringLength);
        }

        public static string ReplaceTokens(string template, IDictionary<string, string> values)
        {
            return ReplaceTokens(template, values, routeTokenTransformer: null);
        }

        public static string ReplaceTokens(string template, IDictionary<string, string> values, IOutboundParameterTransformer routeTokenTransformer)
        {
            var builder = new StringBuilder();
            var state = TemplateParserState.Plaintext;

            int? tokenStart = null;

            // We'll run the loop one extra time with 'null' to detect the end of the string.
            for (var i = 0; i <= template.Length; i++)
            {
                var c = i < template.Length ? (char?)template[i] : null;
                switch (state)
                {
                    case TemplateParserState.Plaintext:
                        if (c == '[')
                        {
                            state = TemplateParserState.SeenLeft;
                            break;
                        }
                        else if (c == ']')
                        {
                            state = TemplateParserState.SeenRight;
                            break;
                        }
                        else if (c == null)
                        {
                            // We're at the end of the string, nothing left to do.
                            break;
                        }
                        else
                        {
                            builder.Append(c);
                            break;
                        }
                    case TemplateParserState.SeenLeft:
                        if (c == '[')
                        {
                            // This is an escaped left-bracket
                            builder.Append(c);
                            state = TemplateParserState.Plaintext;
                            break;
                        }
                        else if (c == ']')
                        {
                            // This is zero-width parameter - not allowed.
                            var message = Resources.FormatAttributeRoute_TokenReplacement_InvalidSyntax(
                                template,
                                Resources.AttributeRoute_TokenReplacement_EmptyTokenNotAllowed);
                            throw new InvalidOperationException(message);
                        }
                        else if (c == null)
                        {
                            // This is a left-bracket at the end of the string.
                            var message = Resources.FormatAttributeRoute_TokenReplacement_InvalidSyntax(
                                template,
                                Resources.AttributeRoute_TokenReplacement_UnclosedToken);
                            throw new InvalidOperationException(message);
                        }
                        else
                        {
                            tokenStart = i;
                            state = TemplateParserState.InsideToken;
                            break;
                        }
                    case TemplateParserState.SeenRight:
                        if (c == ']')
                        {
                            // This is an escaped right-bracket
                            builder.Append(c);
                            state = TemplateParserState.Plaintext;
                            break;
                        }
                        else if (c == null)
                        {
                            // This is an imbalanced right-bracket at the end of the string.
                            var message = Resources.FormatAttributeRoute_TokenReplacement_InvalidSyntax(
                                template,
                                Resources.AttributeRoute_TokenReplacement_ImbalancedSquareBrackets);
                            throw new InvalidOperationException(message);
                        }
                        else
                        {
                            // This is an imbalanced right-bracket.
                            var message = Resources.FormatAttributeRoute_TokenReplacement_InvalidSyntax(
                                template,
                                Resources.AttributeRoute_TokenReplacement_ImbalancedSquareBrackets);
                            throw new InvalidOperationException(message);
                        }
                    case TemplateParserState.InsideToken:
                        if (c == '[')
                        {
                            state = TemplateParserState.InsideToken | TemplateParserState.SeenLeft;
                            break;
                        }
                        else if (c == ']')
                        {
                            state = TemplateParserState.InsideToken | TemplateParserState.SeenRight;
                            break;
                        }
                        else if (c == null)
                        {
                            // This is an unclosed replacement token
                            var message = Resources.FormatAttributeRoute_TokenReplacement_InvalidSyntax(
                                template,
                                Resources.AttributeRoute_TokenReplacement_UnclosedToken);
                            throw new InvalidOperationException(message);
                        }
                        else
                        {
                            // This is a just part of the parameter
                            break;
                        }
                    case TemplateParserState.InsideToken | TemplateParserState.SeenLeft:
                        if (c == '[')
                        {
                            // This is an escaped left-bracket
                            state = TemplateParserState.InsideToken;
                            break;
                        }
                        else
                        {
                            // Unescaped left-bracket is not allowed inside a token.
                            var message = Resources.FormatAttributeRoute_TokenReplacement_InvalidSyntax(
                                template,
                                Resources.AttributeRoute_TokenReplacement_UnescapedBraceInToken);
                            throw new InvalidOperationException(message);
                        }
                    case TemplateParserState.InsideToken | TemplateParserState.SeenRight:
                        if (c == ']')
                        {
                            // This is an escaped right-bracket
                            state = TemplateParserState.InsideToken;
                            break;
                        }
                        else
                        {
                            // This is the end of a replacement token.
                            var token = template
                                .Substring(tokenStart.Value, i - tokenStart.Value - 1)
                                .Replace("[[", "[")
                                .Replace("]]", "]");

                            if (!values.TryGetValue(token, out var value))
                            {
                                // Value not found
                                var message = Resources.FormatAttributeRoute_TokenReplacement_ReplacementValueNotFound(
                                    template,
                                    token,
                                    string.Join(", ", values.Keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase)));
                                throw new InvalidOperationException(message);
                            }

                            if (routeTokenTransformer != null)
                            {
                                value = routeTokenTransformer.TransformOutbound(value);
                            }

                            builder.Append(value);

                            if (c == '[')
                            {
                                state = TemplateParserState.SeenLeft;
                            }
                            else if (c == ']')
                            {
                                state = TemplateParserState.SeenRight;
                            }
                            else if (c == null)
                            {
                                state = TemplateParserState.Plaintext;
                            }
                            else
                            {
                                builder.Append(c);
                                state = TemplateParserState.Plaintext;
                            }

                            tokenStart = null;
                            break;
                        }
                }
            }

            return builder.ToString();
        }

        [Flags]
        private enum TemplateParserState : uint
        {
            // default state - allow non-special characters to pass through to the
            // buffer.
            Plaintext = 0,

            // We're inside a replacement token, may be combined with other states to detect
            // a possible escaped bracket inside the token.
            InsideToken = 1,

            // We've seen a left brace, need to see the next character to find out if it's escaped
            // or not.
            SeenLeft = 2,

            // We've seen a right brace, need to see the next character to find out if it's escaped
            // or not.
            SeenRight = 4,
        }
    }
}