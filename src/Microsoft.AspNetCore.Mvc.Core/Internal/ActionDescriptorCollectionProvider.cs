// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    /// <summary>
    /// Default implementation of <see cref="IActionDescriptorCollectionProvider"/>.
    /// </summary>
    public class ActionDescriptorCollectionProvider : IActionDescriptorCollectionProvider
    {
        private readonly IActionDescriptorProvider[] _actionDescriptorProviders;
        private readonly IActionDescriptorChangeProvider[] _actionDescriptorChangeProviders;
        private readonly ChangeTokenContext _changeTokenContext;
        private ActionDescriptorCollection _collection;
        private int _version = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionDescriptorCollectionProvider" /> class.
        /// </summary>
        /// <param name="actionDescriptorProviders">The sequence of <see cref="IActionDescriptorProvider"/>.</param>
        /// <param name="actionDescriptorChangeProviders">The sequence of <see cref="IActionDescriptorChangeProvider"/>.</param>
        public ActionDescriptorCollectionProvider(
            IEnumerable<IActionDescriptorProvider> actionDescriptorProviders,
            IEnumerable<IActionDescriptorChangeProvider> actionDescriptorChangeProviders)
        {
            _actionDescriptorProviders = actionDescriptorProviders
                .OrderBy(p => p.Order)
                .ToArray();

            _actionDescriptorChangeProviders = actionDescriptorChangeProviders.ToArray();

            _changeTokenContext = new ChangeTokenContext(GetCompositeChangeToken, UpdateCollection);
            OnChange(_changeTokenContext);
        }

        private IChangeToken GetCompositeChangeToken()
        {
            if (_actionDescriptorChangeProviders.Length == 1)
            {
                return _actionDescriptorChangeProviders[0].GetChangeToken();
            }

            var changeTokens = new IChangeToken[_actionDescriptorChangeProviders.Length];
            for (var i = 0; i < _actionDescriptorChangeProviders.Length; i++)
            {
                changeTokens[i] = _actionDescriptorChangeProviders[i].GetChangeToken();
            }

            return new CompositeChangeToken(changeTokens);
        }

        /// <summary>
        /// Returns a cached collection of <see cref="ActionDescriptor" />.
        /// </summary>
        public ActionDescriptorCollection ActionDescriptors
        {
            get
            {
                var changeToken = _changeTokenContext.ChangeToken;
                if (_collection == null || (changeToken != null && changeToken.HasChanged))
                {
                    UpdateCollection();
                }

                return _collection;
            }
        }

        private void UpdateCollection()
        {
            var context = new ActionDescriptorProviderContext();

            for (var i = 0; i < _actionDescriptorProviders.Length; i++)
            {
                _actionDescriptorProviders[i].OnProvidersExecuting(context);
            }

            for (var i = _actionDescriptorProviders.Length - 1; i >= 0; i--)
            {
                _actionDescriptorProviders[i].OnProvidersExecuted(context);
            }

            _collection = new ActionDescriptorCollection(
                new ReadOnlyCollection<ActionDescriptor>(context.Results),
                Interlocked.Increment(ref _version));
        }

        public class ChangeTokenContext
        {

            public ChangeTokenContext(Func<IChangeToken> producer, Action consumer)
            {
                Producer = producer;
                Consumer = consumer;
                ChangeToken = null;
            }

            public Func<IChangeToken> Producer { get; }

            public Action Consumer { get; }

            public IChangeToken ChangeToken;
        }

        private static IDisposable OnChange(ChangeTokenContext context)
        {
            Action<object> callback = null;
            callback = s =>
            {
                // The order here is important. We need to take the token and then apply our changes BEFORE
                // registering. This prevents us from possible having two change updates to process concurrently.
                //
                // If the token changes after we take the token, then we'll process the update immediately upon
                // registering the callback.
                var changeToken = context.Producer();
                context.ChangeToken = changeToken;
                try
                {
                    context.Consumer();
                }
                finally // We always want to ensure the callback is registered
                {
                    changeToken.RegisterChangeCallback(callback, null);
                }
            };

            return context.ChangeToken.RegisterChangeCallback(callback, null);
        }
    }
}