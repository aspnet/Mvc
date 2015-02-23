// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.AspNet.Mvc.Logging;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Core
{
    /// <summary>
    /// Default implementation for ActionDescriptors.
    /// This implementation caches the results at first call, and is not responsible for updates.
    /// </summary>
    public class DefaultActionDescriptorsCollectionProvider : IActionDescriptorsCollectionProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private ActionDescriptorsCollection _collection;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultActionDescriptorsCollectionProvider" /> class.
        /// </summary>
        /// <param name="serviceProvider">The application IServiceProvider.</param>
        public DefaultActionDescriptorsCollectionProvider(IServiceProvider serviceProvider, ILoggerFactory factory)
        {
            _serviceProvider = serviceProvider;
            _logger = factory.Create<DefaultActionDescriptorsCollectionProvider>();
        }

        /// <summary>
        /// Returns a cached collection of <see cref="ActionDescriptor" />.
        /// </summary>
        public ActionDescriptorsCollection ActionDescriptors
        {
            get
            {
                if (_collection == null)
                {
                    _collection = GetCollection();
                }

                return _collection;
            }
        }

        private ActionDescriptorsCollection GetCollection()
        {
            var providers =
                _serviceProvider.GetRequiredServices<IActionDescriptorProvider>()
                                .OrderBy(p => p.Order)
                                .ToArray();

            var context = new ActionDescriptorProviderContext();

            foreach (var provider in providers)
            {
                provider.OnProvidersExecuting(context);
            }

            for (var i = providers.Length - 1; i >= 0; i--)
            {
                providers[i].OnProvidersExecuted(context);
            }

            if (_logger.IsEnabled(LogLevel.Verbose))
            {
                foreach (var actionDescriptor in context.Results)
                {
                    _logger.WriteVerbose(new ActionDescriptorValues(actionDescriptor));
                }
            }

            return new ActionDescriptorsCollection(
                new ReadOnlyCollection<ActionDescriptor>(context.Results), 0);
        }
    }
}