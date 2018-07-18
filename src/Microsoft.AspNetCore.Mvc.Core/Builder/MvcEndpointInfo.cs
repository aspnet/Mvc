// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.AspNetCore.Routing.Matchers;
using Microsoft.AspNetCore.Routing.Template;

namespace Microsoft.AspNetCore.Builder
{
    public class MvcEndpointInfo
    {
        public MvcEndpointInfo(
            string name,
            string template,
            RouteValueDictionary defaults,
            IDictionary<string, object> nonInlineConstraints,
            RouteValueDictionary dataTokens)
        {
            Name = name;
            Template = template ?? string.Empty;
            DataTokens = dataTokens;

            try
            {
                // Data we parse from the template will be used to fill in the rest of the constraints or
                // defaults. The parser will throw for invalid routes.
                ParsedTemplate = TemplateParser.Parse(template);

                MatchProcessorReferences = GetMatchProcessorReferences(ParsedTemplate, nonInlineConstraints);

                Defaults = defaults;
                MergedDefaults = GetDefaults(ParsedTemplate, defaults);
            }
            catch (Exception exception)
            {
                throw new RouteCreationException(
                    string.Format(CultureInfo.CurrentCulture, "An error occurred while creating the route with name '{0}' and template '{1}'.", name, template), exception);
            }
        }

        public string Name { get; }
        public string Template { get; }

        // Non-inline defaults
        public RouteValueDictionary Defaults { get; }

        // Inline and non-inline defaults merged into one
        public RouteValueDictionary MergedDefaults { get; }

        public List<MatchProcessorReference> MatchProcessorReferences { get; }
        public RouteValueDictionary DataTokens { get; }
        internal RouteTemplate ParsedTemplate { get; private set; }

        private static List<MatchProcessorReference> GetMatchProcessorReferences(
            RouteTemplate parsedTemplate,
            IDictionary<string, object> nonInlineConstraints)
        {
            var matchProcessorReferences = new List<MatchProcessorReference>();
            if (nonInlineConstraints != null)
            {
                foreach (var kvp in nonInlineConstraints)
                {
                    var constraint = kvp.Value as IRouteConstraint;
                    if (constraint == null)
                    {
                        var regexPattern = kvp.Value as string;
                        if (regexPattern == null)
                        {
                            throw new InvalidOperationException("Non inline constraint is not a valid IRouteConstraint");
                        }

                        var constraintsRegEx = "^(" + regexPattern + ")$";
                        constraint = new RegexRouteConstraint(constraintsRegEx);
                    }

                    matchProcessorReferences.Add(new MatchProcessorReference(kvp.Key, constraint));
                }
            }

            foreach (var parameter in parsedTemplate.Parameters)
            {
                if (parameter.InlineConstraints != null)
                {
                    foreach (var constraint in parameter.InlineConstraints)
                    {
                        matchProcessorReferences.Add(
                            new MatchProcessorReference(
                                parameter.Name,
                                optional: parameter.IsOptional,
                                constraintText: constraint.Constraint));
                    }
                }
            }

            return matchProcessorReferences;
        }

        private static RouteValueDictionary GetDefaults(
            RouteTemplate parsedTemplate,
            RouteValueDictionary defaults)
        {
            var result = defaults == null ? new RouteValueDictionary() : new RouteValueDictionary(defaults);

            foreach (var parameter in parsedTemplate.Parameters)
            {
                if (parameter.DefaultValue != null)
                {
                    if (result.ContainsKey(parameter.Name))
                    {
                        throw new InvalidOperationException(
                            string.Format(CultureInfo.CurrentCulture, "The route parameter '{0}' has both an inline default value and an explicit default value specified. A route parameter cannot contain an inline default value when a default value is specified explicitly. Consider removing one of them.", parameter.Name));
                    }
                    else
                    {
                        result.Add(parameter.Name, parameter.DefaultValue);
                    }
                }
            }

            return result;
        }
    }
}
