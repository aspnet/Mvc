// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc.ModelBinding
{
    /// <summary>
    /// A <see cref="IValueProviderFactory"/> that creates <see cref="IValueProvider"/> instances that
    /// read values from the request headers.
    /// </summary>
    public class HeaderValueProviderFactory : IValueProviderFactory
    {
        /// <inheritdoc />
        public Task CreateValueProviderAsync(ValueProviderFactoryContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var valueProvider = new HeaderValueProvider(
                BindingSource.Header,
                context.ActionContext.HttpContext.Request.Headers,
                CultureInfo.InvariantCulture);

            context.ValueProviders.Add(valueProvider);

            return Task.CompletedTask;
        }
    }
}
