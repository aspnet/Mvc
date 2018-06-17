// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Core;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.AspNetCore.Routing.Template;

namespace Microsoft.AspNetCore.Builder
{
    public class MvcEndpointInfo
    {
        public MvcEndpointInfo(
            string name,
            string template,
            RouteValueDictionary defaults,
            IDictionary<string, object> constraints,
            RouteValueDictionary dataTokens,
            IInlineConstraintResolver constraintResolver)
        {
            Name = name;
            Template = template ?? string.Empty;
            DataTokens = dataTokens;

            try
            {
                // Data we parse from the template will be used to fill in the rest of the constraints or
                // defaults. The parser will throw for invalid routes.
                ParsedTemplate = TemplateParser.Parse(template);

                Constraints = GetConstraints(constraintResolver, ParsedTemplate, constraints);
                Defaults = GetDefaults(ParsedTemplate, defaults);
            }
            catch (Exception exception)
            {
                throw new RouteCreationException(
                    string.Format(CultureInfo.CurrentCulture, "An error occurred while creating the route with name '{0}' and template '{1}'.", name, template), exception);
            }
        }

        public string Name { get; }
        public string Template { get; }
        public RouteValueDictionary Defaults { get; }
        public IDictionary<string, IRouteConstraint> Constraints { get; }
        public RouteValueDictionary DataTokens { get; }
        internal RouteTemplate ParsedTemplate { get; private set; }

        private static IDictionary<string, IRouteConstraint> GetConstraints(
            IInlineConstraintResolver inlineConstraintResolver,
            RouteTemplate parsedTemplate,
            IDictionary<string, object> constraints)
        {
            var constraintBuilder = new RouteConstraintBuilder(inlineConstraintResolver, parsedTemplate.TemplateText);

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

                foreach (var inlineConstraint in parameter.InlineConstraints)
                {
                    constraintBuilder.AddResolvedConstraint(parameter.Name, inlineConstraint.Constraint);
                }
            }

            return constraintBuilder.Build();
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

    public class MvcEndpointInfoBuilder
    {
        public MvcEndpointInfoBuilder(IInlineConstraintResolver constraintResolver)
        {
            ConstraintResolver = constraintResolver;
        }

        public List<MvcEndpointInfo> EndpointInfos { get; } = new List<MvcEndpointInfo>();
        public IInlineConstraintResolver ConstraintResolver { get; }
    }

    /// <summary>
    /// Provides extension methods for <see cref="MvcEndpointInfoBuilder" /> to add endpoints.
    /// </summary>
    public static class MvcEndpointInfoBuilderExtensions
    {
        #region MapEndpoint
        /// <summary>
        /// Adds a endpoint to the <see cref="MvcEndpointInfoBuilder" /> with the specified name and template.
        /// </summary>
        /// <param name="endpointBuilder">The <see cref="MvcEndpointInfoBuilder" /> to add the endpoint to.</param>
        /// <param name="name">The name of the endpoint.</param>
        /// <param name="template">The URL pattern of the endpoint.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static MvcEndpointInfoBuilder MapEndpoint(this MvcEndpointInfoBuilder endpointBuilder, string name, string template)
        {
            endpointBuilder.MapEndpoint(name, template, null);
            return endpointBuilder;
        }

        /// <summary>
        /// Adds a endpoint to the <see cref="MvcEndpointInfoBuilder" /> with the specified name, template, and default values.
        /// </summary>
        /// <param name="endpointBuilder">The <see cref="MvcEndpointInfoBuilder" /> to add the endpoint to.</param>
        /// <param name="name">The name of the endpoint.</param>
        /// <param name="template">The URL pattern of the endpoint.</param>
        /// <param name="defaults">
        /// An object that contains default values for endpoint parameters. The object's properties represent the names
        /// and values of the default values.
        /// </param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static MvcEndpointInfoBuilder MapEndpoint(this MvcEndpointInfoBuilder endpointBuilder, string name, string template, object defaults)
        {
            return endpointBuilder.MapEndpoint(name, template, defaults, null);
        }

        /// <summary>
        /// Adds a endpoint to the <see cref="MvcEndpointInfoBuilder" /> with the specified name, template, default values, and
        /// constraints.
        /// </summary>
        /// <param name="endpointBuilder">The <see cref="MvcEndpointInfoBuilder" /> to add the endpoint to.</param>
        /// <param name="name">The name of the endpoint.</param>
        /// <param name="template">The URL pattern of the endpoint.</param>
        /// <param name="defaults">
        /// An object that contains default values for endpoint parameters. The object's properties represent the names
        /// and values of the default values.
        /// </param>
        /// <param name="constraints">
        /// An object that contains constraints for the endpoint. The object's properties represent the names and values
        /// of the constraints.
        /// </param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static MvcEndpointInfoBuilder MapEndpoint(this MvcEndpointInfoBuilder endpointBuilder, string name, string template, object defaults, object constraints)
        {
            return endpointBuilder.MapEndpoint(name, template, defaults, constraints, null);
        }

        /// <summary>
        /// Adds a endpoint to the <see cref="MvcEndpointInfoBuilder" /> with the specified name, template, default values, and
        /// data tokens.
        /// </summary>
        /// <param name="endpointBuilder">The <see cref="MvcEndpointInfoBuilder" /> to add the endpoint to.</param>
        /// <param name="name">The name of the endpoint.</param>
        /// <param name="template">The URL pattern of the endpoint.</param>
        /// <param name="defaults">
        /// An object that contains default values for endpoint parameters. The object's properties represent the names
        /// and values of the default values.
        /// </param>
        /// <param name="constraints">
        /// An object that contains constraints for the endpoint. The object's properties represent the names and values
        /// of the constraints.
        /// </param>
        /// <param name="dataTokens">
        /// An object that contains data tokens for the endpoint. The object's properties represent the names and values
        /// of the data tokens.
        /// </param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static MvcEndpointInfoBuilder MapEndpoint(this MvcEndpointInfoBuilder endpointBuilder, string name, string template, object defaults, object constraints, object dataTokens)
        {
            endpointBuilder.EndpointInfos.Add(new MvcEndpointInfo(
                name,
                template,
                new RouteValueDictionary(defaults),
                new RouteValueDictionary(constraints),
                new RouteValueDictionary(dataTokens),
                endpointBuilder.ConstraintResolver));

            return endpointBuilder;
        }
        #endregion

        #region MapAreaEndpoint
        /// <summary>
        /// Adds a endpoint to the <see cref="MvcEndpointInfoBuilder"/> with the given MVC area with the specified
        /// <paramref name="name"/>, <paramref name="areaName"/> and <paramref name="template"/>.
        /// </summary>
        /// <param name="endpointBuilder">The <see cref="MvcEndpointInfoBuilder"/> to add the endpoint to.</param>
        /// <param name="name">The name of the endpoint.</param>
        /// <param name="areaName">The MVC area name.</param>
        /// <param name="template">The URL pattern of the endpoint.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static MvcEndpointInfoBuilder MapAreaEndpoint(
            this MvcEndpointInfoBuilder endpointBuilder,
            string name,
            string areaName,
            string template)
        {
            MapAreaEndpoint(endpointBuilder, name, areaName, template, defaults: null, constraints: null, dataTokens: null);
            return endpointBuilder;
        }

        /// <summary>
        /// Adds a endpoint to the <see cref="MvcEndpointInfoBuilder"/> with the given MVC area with the specified
        /// <paramref name="name"/>, <paramref name="areaName"/>, <paramref name="template"/>, and
        /// <paramref name="defaults"/>.
        /// </summary>
        /// <param name="endpointBuilder">The <see cref="MvcEndpointInfoBuilder"/> to add the endpoint to.</param>
        /// <param name="name">The name of the endpoint.</param>
        /// <param name="areaName">The MVC area name.</param>
        /// <param name="template">The URL pattern of the endpoint.</param>
        /// <param name="defaults">
        /// An object that contains default values for endpoint parameters. The object's properties represent the
        /// names and values of the default values.
        /// </param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static MvcEndpointInfoBuilder MapAreaEndpoint(
            this MvcEndpointInfoBuilder endpointBuilder,
            string name,
            string areaName,
            string template,
            object defaults)
        {
            MapAreaEndpoint(endpointBuilder, name, areaName, template, defaults, constraints: null, dataTokens: null);
            return endpointBuilder;
        }

        /// <summary>
        /// Adds a endpoint to the <see cref="MvcEndpointInfoBuilder"/> with the given MVC area with the specified
        /// <paramref name="name"/>, <paramref name="areaName"/>, <paramref name="template"/>, 
        /// <paramref name="defaults"/>, and <paramref name="constraints"/>.
        /// </summary>
        /// <param name="endpointBuilder">The <see cref="MvcEndpointInfoBuilder"/> to add the endpoint to.</param>
        /// <param name="name">The name of the endpoint.</param>
        /// <param name="areaName">The MVC area name.</param>
        /// <param name="template">The URL pattern of the endpoint.</param>
        /// <param name="defaults">
        /// An object that contains default values for endpoint parameters. The object's properties represent the
        /// names and values of the default values.
        /// </param>
        /// <param name="constraints">
        /// An object that contains constraints for the endpoint. The object's properties represent the names and
        /// values of the constraints.
        /// </param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static MvcEndpointInfoBuilder MapAreaEndpoint(
            this MvcEndpointInfoBuilder endpointBuilder,
            string name,
            string areaName,
            string template,
            object defaults,
            object constraints)
        {
            MapAreaEndpoint(endpointBuilder, name, areaName, template, defaults, constraints, dataTokens: null);
            return endpointBuilder;
        }

        /// <summary>
        /// Adds a endpoint to the <see cref="MvcEndpointInfoBuilder"/> with the given MVC area with the specified
        /// <paramref name="name"/>, <paramref name="areaName"/>, <paramref name="template"/>, 
        /// <paramref name="defaults"/>, <paramref name="constraints"/>, and <paramref name="dataTokens"/>.
        /// </summary>
        /// <param name="endpointBuilder">The <see cref="MvcEndpointInfoBuilder"/> to add the endpoint to.</param>
        /// <param name="name">The name of the endpoint.</param>
        /// <param name="areaName">The MVC area name.</param>
        /// <param name="template">The URL pattern of the endpoint.</param>
        /// <param name="defaults">
        /// An object that contains default values for endpoint parameters. The object's properties represent the
        /// names and values of the default values.
        /// </param>
        /// <param name="constraints">
        /// An object that contains constraints for the endpoint. The object's properties represent the names and
        /// values of the constraints.
        /// </param>
        /// <param name="dataTokens">
        /// An object that contains data tokens for the endpoint. The object's properties represent the names and
        /// values of the data tokens.
        /// </param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static MvcEndpointInfoBuilder MapAreaEndpoint(
            this MvcEndpointInfoBuilder endpointBuilder,
            string name,
            string areaName,
            string template,
            object defaults,
            object constraints,
            object dataTokens)
        {
            if (endpointBuilder == null)
            {
                throw new ArgumentNullException(nameof(endpointBuilder));
            }

            if (string.IsNullOrEmpty(areaName))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, nameof(areaName));
            }

            var defaultsDictionary = new RouteValueDictionary(defaults);
            defaultsDictionary["area"] = defaultsDictionary["area"] ?? areaName;

            var constraintsDictionary = new RouteValueDictionary(constraints);
            constraintsDictionary["area"] = constraintsDictionary["area"] ?? new StringRouteConstraint(areaName);

            endpointBuilder.MapEndpoint(name, template, defaultsDictionary, constraintsDictionary, dataTokens);
            return endpointBuilder;
        }
        #endregion
    }
}
