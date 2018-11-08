﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc.Core;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Mvc.ModelBinding
{
    /// <summary>
    /// A factory for <see cref="IModelBinder"/> instances.
    /// </summary>
    public class ModelBinderFactory : IModelBinderFactory
    {
        private readonly IModelMetadataProvider _metadataProvider;
        private readonly IModelBinderProvider[] _providers;
        private readonly ConcurrentDictionary<Key, IModelBinder> _cache;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// <para>This constructor is obsolete and will be removed in a future version. The recommended alternative
        /// is the overload that also takes an <see cref="IServiceProvider"/>.</para>
        /// <para>Creates a new <see cref="ModelBinderFactory"/>.</para>
        /// </summary>
        /// <param name="metadataProvider">The <see cref="IModelMetadataProvider"/>.</param>
        /// <param name="options">The <see cref="IOptions{TOptions}"/> for <see cref="MvcOptions"/>.</param>
        [Obsolete("This constructor is obsolete and will be removed in a future version. The recommended alternative"
            + " is the overload that also takes an " + nameof(IServiceProvider) + ".")]
        public ModelBinderFactory(IModelMetadataProvider metadataProvider, IOptions<MvcOptions> options)
            : this(metadataProvider, options, GetDefaultServices())
        {
        }

        /// <summary>
        /// Creates a new <see cref="ModelBinderFactory"/>.
        /// </summary>
        /// <param name="metadataProvider">The <see cref="IModelMetadataProvider"/>.</param>
        /// <param name="options">The <see cref="IOptions{TOptions}"/> for <see cref="MvcOptions"/>.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        public ModelBinderFactory(
            IModelMetadataProvider metadataProvider,
            IOptions<MvcOptions> options,
            IServiceProvider serviceProvider)
        {
            _metadataProvider = metadataProvider;
            _providers = options.Value.ModelBinderProviders.ToArray();
            _serviceProvider = serviceProvider;
            _cache = new ConcurrentDictionary<Key, IModelBinder>();

            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<ModelBinderFactory>();
            logger.RegisteredModelBinderProviders(_providers);
        }

        /// <inheritdoc />
        public IModelBinder CreateBinder(ModelBinderFactoryContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (_providers.Length == 0)
            {
                throw new InvalidOperationException(Resources.FormatModelBinderProvidersAreRequired(
                    typeof(MvcOptions).FullName,
                    nameof(MvcOptions.ModelBinderProviders),
                    typeof(IModelBinderProvider).FullName));
            }

            if (TryGetCachedBinder(context.Metadata, context.CacheToken, out var binder))
            {
                return binder;
            }

            // Perf: We're calling the Uncached version of the API here so we can:
            // 1. avoid allocating a context when the value is already cached
            // 2. avoid checking the cache twice when the value is not cached
            var providerContext = new DefaultModelBinderProviderContext(this, context);
            binder = CreateBinderCoreUncached(providerContext, context.CacheToken);
            if (binder == null)
            {
                var message = Resources.FormatCouldNotCreateIModelBinder(providerContext.Metadata.ModelType);
                throw new InvalidOperationException(message);
            }

            Debug.Assert(!(binder is PlaceholderBinder));
            AddToCache(context.Metadata, context.CacheToken, binder);

            return binder;
        }

        // Called by the DefaultModelBinderProviderContext when we're recursively creating a binder
        // so that all intermediate results can be cached.
        private IModelBinder CreateBinderCoreCached(DefaultModelBinderProviderContext providerContext, object token)
        {
            if (TryGetCachedBinder(providerContext.Metadata, token, out var binder))
            {
                return binder;
            }

            // We're definitely creating a binder for an non-root node here, so it's OK for binder creation
            // to fail.
            binder = CreateBinderCoreUncached(providerContext, token) ?? NoOpBinder.Instance;

            if (!(binder is PlaceholderBinder))
            {
                AddToCache(providerContext.Metadata, token, binder);
            }

            return binder;
        }

        private IModelBinder CreateBinderCoreUncached(DefaultModelBinderProviderContext providerContext, object token)
        {
            if (!providerContext.Metadata.IsBindingAllowed)
            {
                return NoOpBinder.Instance;
            }

            // A non-null token will usually be passed in at the top level (ParameterDescriptor likely).
            // This prevents us from treating a parameter the same as a collection-element - which could
            // happen looking at just model metadata.
            var key = new Key(providerContext.Metadata, token);

            // The providerContext.Visited is used here to break cycles in recursion. We need a separate
            // per-operation cache for cycle breaking because the global cache (_cache) needs to always stay
            // in a valid state.
            //
            // We store null as a sentinel inside the providerContext.Visited to track the fact that we've visited
            // a given node but haven't yet created a binder for it. We don't want to eagerly create a
            // PlaceholderBinder because that would result in lots of unnecessary indirection and allocations.
            var visited = providerContext.Visited;

            if (visited.TryGetValue(key, out var binder))
            {
                if (binder != null)
                {
                    return binder;
                }

                // If we're currently recursively building a binder for this type, just return
                // a PlaceholderBinder. We'll fix it up later to point to the 'real' binder
                // when the stack unwinds.
                binder = new PlaceholderBinder();
                visited[key] = binder;
                return binder;
            }

            // OK this isn't a recursive case (yet) so add an entry and then ask the providers
            // to create the binder.
            visited.Add(key, null);

            IModelBinder result = null;

            for (var i = 0; i < _providers.Length; i++)
            {
                var provider = _providers[i];
                result = provider.GetBinder(providerContext);
                if (result != null)
                {
                    break;
                }
            }

            // If the PlaceholderBinder was created, then it means we recursed. Hook it up to the 'real' binder.
            if (visited[key] is PlaceholderBinder placeholderBinder)
            {
                // It's also possible that user code called into `CreateBinder` but then returned null, we don't
                // want to create something that will null-ref later so just hook this up to the no-op binder.
                placeholderBinder.Inner = result ?? NoOpBinder.Instance;
            }

            if (result != null)
            {
                visited[key] = result;
            }

            return result;
        }

        private void AddToCache(ModelMetadata metadata, object cacheToken, IModelBinder binder)
        {
            Debug.Assert(metadata != null);
            Debug.Assert(binder != null);

            if (cacheToken == null)
            {
                return;
            }

            _cache.TryAdd(new Key(metadata, cacheToken), binder);
        }

        private bool TryGetCachedBinder(ModelMetadata metadata, object cacheToken, out IModelBinder binder)
        {
            Debug.Assert(metadata != null);

            if (cacheToken == null)
            {
                binder = null;
                return false;
            }

            return _cache.TryGetValue(new Key(metadata, cacheToken), out binder);
        }

        private static IServiceProvider GetDefaultServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
            return services.BuildServiceProvider();
        }

        private class DefaultModelBinderProviderContext : ModelBinderProviderContext
        {
            private readonly ModelBinderFactory _factory;

            public DefaultModelBinderProviderContext(
                ModelBinderFactory factory,
                ModelBinderFactoryContext factoryContext)
            {
                _factory = factory;
                Metadata = factoryContext.Metadata;
                BindingInfo bindingInfo;
                if (factoryContext.BindingInfo != null)
                {
                    bindingInfo = new BindingInfo(factoryContext.BindingInfo);
                }
                else
                {
                    bindingInfo = new BindingInfo();
                }

                bindingInfo.TryApplyBindingInfo(Metadata);
                BindingInfo = bindingInfo;

                MetadataProvider = _factory._metadataProvider;
                Visited = new Dictionary<Key, IModelBinder>();
            }

            private DefaultModelBinderProviderContext(
                DefaultModelBinderProviderContext parent,
                ModelMetadata metadata,
                BindingInfo bindingInfo)
            {
                Metadata = metadata;

                _factory = parent._factory;
                MetadataProvider = parent.MetadataProvider;
                Visited = parent.Visited;
                BindingInfo = bindingInfo;
            }

            public override BindingInfo BindingInfo { get; }

            public override ModelMetadata Metadata { get; }

            public override IModelMetadataProvider MetadataProvider { get; }

            public Dictionary<Key, IModelBinder> Visited { get; }

            public override IServiceProvider Services => _factory._serviceProvider;

            public override IModelBinder CreateBinder(ModelMetadata metadata)
            {
                var bindingInfo = new BindingInfo();
                bindingInfo.TryApplyBindingInfo(metadata);

                return CreateBinder(metadata, bindingInfo);
            }

            public override IModelBinder CreateBinder(ModelMetadata metadata, BindingInfo bindingInfo)
            {
                if (metadata == null)
                {
                    throw new ArgumentNullException(nameof(metadata));
                }

                if (bindingInfo == null)
                {
                    throw new ArgumentNullException(nameof(bindingInfo));
                }

                // For non-root nodes we use the ModelMetadata as the cache token. This ensures that all non-root
                // nodes with the same metadata will have the same binder. This is OK because for an non-root
                // node there's no opportunity to customize binding info like there is for a parameter.
                var token = metadata;

                var nestedContext = new DefaultModelBinderProviderContext(this, metadata, bindingInfo);
                return _factory.CreateBinderCoreCached(nestedContext, token);
            }
        }

        // This key allows you to specify a ModelMetadata which represents the type/property being bound
        // and a 'token' which acts as an arbitrary discriminator.
        //
        // This is necessary because the same metadata might be bound as a top-level parameter (with BindingInfo on
        // the ParameterDescriptor) or in a call to TryUpdateModel (no BindingInfo) or as a collection element.
        //
        // We need to be able to tell the difference between these things to avoid over-caching.
        private readonly struct Key : IEquatable<Key>
        {
            private readonly ModelMetadata _metadata;
            private readonly object _token; // Explicitly using ReferenceEquality for tokens.

            public Key(ModelMetadata metadata, object token)
            {
                _metadata = metadata;
                _token = token;
            }

            public bool Equals(Key other)
            {
                return _metadata.Equals(other._metadata) && object.ReferenceEquals(_token, other._token);
            }

            public override bool Equals(object obj)
            {
                var other = obj as Key?;
                return other.HasValue && Equals(other.Value);
            }

            public override int GetHashCode()
            {
                var hash = new HashCodeCombiner();
                hash.Add(_metadata);
                hash.Add(RuntimeHelpers.GetHashCode(_token));
                return hash;
            }

            public override string ToString()
            {
                switch (_metadata.MetadataKind)
                {
                    case ModelMetadataKind.Parameter:
                        return $"{_token} (Parameter: '{_metadata.ParameterName}' Type: '{_metadata.ModelType.Name}')";
                    case ModelMetadataKind.Property:
                        return $"{_token} (Property: '{_metadata.ContainerType.Name}.{_metadata.PropertyName}' " +
                            $"Type: '{_metadata.ModelType.Name}')";
                    case ModelMetadataKind.Type:
                        return $"{_token} (Type: '{_metadata.ModelType.Name}')";
                    default:
                        return $"Unsupported MetadataKind '{_metadata.MetadataKind}'.";
                }
            }
        }
    }
}
