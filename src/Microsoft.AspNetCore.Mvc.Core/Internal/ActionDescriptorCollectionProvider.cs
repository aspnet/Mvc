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
    public class ActionDescriptorCollectionProvider : IActionDescriptorCollectionProvider, IDisposable
    {
        private readonly IActionDescriptorProvider[] _actionDescriptorProviders;
        private readonly List<IDisposable> _disposables;
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

            _disposables = new List<IDisposable>();
            foreach (var changeTokenProvider in actionDescriptorChangeProviders)
            {
                _disposables.Add(ChangeToken.OnChange(changeTokenProvider.GetChangeToken, OnChange));
            }
        }

        /// <summary>
        /// Returns a cached collection of <see cref="ActionDescriptor" />.
        /// </summary>
        public ActionDescriptorCollection ActionDescriptors
        {
            get
            {
                var result = _collection;

                if (result == null)
                {
                    result = GetCollection();
                    _collection = result;
                }

                return result;
            }
        }

        private ActionDescriptorCollection GetCollection()
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

            return new ActionDescriptorCollection(
                new ReadOnlyCollection<ActionDescriptor>(context.Results),
                Interlocked.Increment(ref _version));
        }

        private void OnChange()
        {
            _collection = null;
        }

        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
        }
    }
}