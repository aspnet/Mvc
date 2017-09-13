// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Adds an <see cref="IFilterMetadata"/> that enhances some basic 4xx client error responses.
    /// <para>
    /// The <see cref="ProblemDetailsAttribute"/> adds an <see cref="IActionFilter"/> that adds details to the HTTP
    /// response, when the action signature matches certain patterns. By default, a <see cref="ProblemDetails"/>
    /// is returned in the response body. This can be further configured by registering instances of 
    /// <see cref="Infrastructure.IErrorDescriptorProvider"/> in the service container.
    /// </para>
    /// Patterns matched by the filter include:
    /// <list>
    /// <item>400 <see cref="StatusCodeResult"/> on any action which participates in model binding.</item>
    /// <item>404 <see cref="StatusCodeResult"/> on any action with an <c>id</c> parameter.</item>
    /// </list>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ProblemDetailsAttribute : Attribute, IFilterFactory
    {
        /// <inheritdoc />
        public bool IsReusable => true;

        /// <inheritdoc />
        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            return serviceProvider.GetRequiredService<ProblemDetailsFilter>();
        }
    }
}
