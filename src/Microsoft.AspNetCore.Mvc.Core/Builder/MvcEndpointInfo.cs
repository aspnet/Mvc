// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;

namespace Microsoft.AspNetCore.Builder
{
    internal class MvcEndpointInfo
    {
        public MvcEndpointInfo(
            string name,
            string pattern,
            RouteValueDictionary defaults,
            IDictionary<string, object> constraints,
            RouteValueDictionary dataTokens,
            IInlineConstraintResolver constraintResolver)
        {
            Name = name;
            Pattern = pattern ?? string.Empty;
            DataTokens = dataTokens;

            try
            {
                // Data we parse from the pattern will be used to fill in the rest of the constraints or
                // defaults. The parser will throw for invalid routes.
                ParsedPattern = RoutePatternFactory.Parse(pattern);

                Constraints = GetConstraints(constraintResolver, ParsedPattern, constraints);
                Defaults = defaults;
                MergedDefaults = GetDefaults(ParsedPattern, defaults);
            }
            catch (Exception exception)
            {
                throw new RouteCreationException(
                    string.Format(CultureInfo.CurrentCulture, "An error occurred while creating the route with name '{0}' and pattern '{1}'.", name, pattern), exception);
            }
        }

        public string Name { get; }
        public string Pattern { get; }

        // Non-inline defaults
        public RouteValueDictionary Defaults { get; }

        // Inline and non-inline defaults merged into one
        public RouteValueDictionary MergedDefaults { get; }

        public IDictionary<string, IRouteConstraint> Constraints { get; }
        public RouteValueDictionary DataTokens { get; }
        internal RoutePattern ParsedPattern { get; private set; }

        private static IDictionary<string, IRouteConstraint> GetConstraints(
            IInlineConstraintResolver inlineConstraintResolver,
            RoutePattern parsedTemplate,
            IDictionary<string, object> constraints)
        {
            var constraintBuilder = new RouteConstraintBuilder(inlineConstraintResolver, parsedTemplate.RawText);

            if (constraints != null)
            {
                foreach (var kvp in constraints)
                {
                    constraintBuilder.AddConstraint(kvp.Key, kvp.Value);
                }
            }

            foreach (var parameter in parsedTemplate.Parameters)
            {
                if (parameter.IsOptional)
                {
                    constraintBuilder.SetOptional(parameter.Name);
                }

                foreach (var inlineConstraint in parameter.Constraints)
                {
                    constraintBuilder.AddResolvedConstraint(parameter.Name, inlineConstraint.Content);
                }
            }

            return constraintBuilder.Build();
        }

        private static RouteValueDictionary GetDefaults(
            RoutePattern parsedTemplate,
            RouteValueDictionary defaults)
        {
            var result = defaults == null ? new RouteValueDictionary() : new RouteValueDictionary(defaults);

            foreach (var parameter in parsedTemplate.Parameters)
            {
                if (parameter.Default != null)
                {
                    if (result.TryGetValue(parameter.Name, out var value))
                    {
                        if (!object.Equals(value, parameter.Default))
                        {
                            throw new InvalidOperationException(
                                string.Format(CultureInfo.CurrentCulture, "The route parameter '{0}' has both an inline default value and an explicit default value specified. A route parameter cannot contain an inline default value when a default value is specified explicitly. Consider removing one of them.", parameter.Name));
                        }
                    }
                    else
                    {
                        result.Add(parameter.Name, parameter.Default);
                    }
                }
            }

            return result;
        }
    }
}
