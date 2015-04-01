﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding.Metadata
{
    /// <summary>
    /// A default implementation of <see cref="IModelMetadataProvider"/> based on reflection.
    /// </summary>
    public class DefaultModelMetadataProvider : IModelMetadataProvider
    {
        private readonly TypeCache _typeCache = new TypeCache();
        private readonly Func<ModelMetadataIdentity, ModelMetadataCacheEntry> _cacheEntryFactory;

        /// <summary>
        /// Creates a new <see cref="DefaultModelMetadataProvider"/>.
        /// </summary>
        /// <param name="detailsProvider">The <see cref="ICompositeMetadataDetailsProvider"/>.</param>
        public DefaultModelMetadataProvider(ICompositeMetadataDetailsProvider detailsProvider)
        {
            DetailsProvider = detailsProvider;

            _cacheEntryFactory = CreateCacheEntry;
        }

        /// <summary>
        /// Gets the <see cref="ICompositeMetadataDetailsProvider"/>.
        /// </summary>
        protected ICompositeMetadataDetailsProvider DetailsProvider { get; }

        /// <inheritdoc />
        public virtual IEnumerable<ModelMetadata> GetMetadataForProperties([NotNull]Type modelType)
        {
            var key = ModelMetadataIdentity.ForType(modelType);

            var cacheEntry = _typeCache.GetOrAdd(key, _cacheEntryFactory);

            // We're relying on a safe race-condition for Properties - take care only
            // to set the value onces the properties are fully-initialized.
            if (cacheEntry.Details.Properties == null)
            {
                var propertyDetails = CreatePropertyDetails(key);

                var properties = new ModelMetadata[propertyDetails.Length];
                for (var i = 0; i < properties.Length; i++)
                {
                    properties[i] = CreateModelMetadata(propertyDetails[i]);
                }

                cacheEntry.Details.Properties = properties;
            }

            return cacheEntry.Details.Properties;
        }

        /// <inheritdoc />
        public virtual ModelMetadata GetMetadataForType([NotNull] Type modelType)
        {
            var key = ModelMetadataIdentity.ForType(modelType);

            var cacheEntry = _typeCache.GetOrAdd(key, _cacheEntryFactory);
            return cacheEntry.Metadata;
        }

        private ModelMetadataCacheEntry CreateCacheEntry(ModelMetadataIdentity key)
        {
            var details = CreateTypeDetails(key);
            var metadata = CreateModelMetadata(details);
            return new ModelMetadataCacheEntry(metadata, details);
        }

        /// <summary>
        /// Creates a new <see cref="ModelMetadata"/> from a <see cref="DefaultMetadataDetails"/>.
        /// </summary>
        /// <param name="entry">The <see cref="DefaultMetadataDetails"/> entry with cached data.</param>
        /// <returns>A new <see cref="ModelMetadata"/> instance.</returns>
        /// <remarks>
        /// <see cref="DefaultModelMetadataProvider"/> will always create instances of
        /// <see cref="DefaultModelMetadata"/> .Override this method to create a <see cref="ModelMetadata"/>
        /// of a different concrete type.
        /// </remarks>
        protected virtual ModelMetadata CreateModelMetadata(DefaultMetadataDetails entry)
        {
            return new DefaultModelMetadata(this, DetailsProvider, entry);
        }

        /// <summary>
        /// Creates the <see cref="DefaultMetadataDetails"/> entries for the properties of a model
        /// <see cref="Type"/>.
        /// </summary>
        /// <param name="key">
        /// The <see cref="ModelMetadataIdentity"/> identifying the model <see cref="Type"/>.
        /// </param>
        /// <returns>A details object for each property of the model <see cref="Type"/>.</returns>
        /// <remarks>
        /// The results of this method will be cached and used to satisfy calls to
        /// <see cref="GetMetadataForProperties(Type)"/>. Override this method to provide a different
        /// set of property data.
        /// </remarks>
        protected virtual DefaultMetadataDetails[] CreatePropertyDetails([NotNull] ModelMetadataIdentity key)
        {
            var propertyHelpers = PropertyHelper.GetVisibleProperties(key.ModelType);

            var propertyEntries = new List<DefaultMetadataDetails>(propertyHelpers.Length);
            for (var i = 0; i < propertyHelpers.Length; i++)
            {
                var propertyHelper = propertyHelpers[i];
                var propertyKey = ModelMetadataIdentity.ForProperty(
                    propertyHelper.Property.PropertyType,
                    propertyHelper.Name,
                    key.ModelType);

                var attributes = new List<object>(ModelAttributes.GetAttributesForProperty(
                    key.ModelType,
                    propertyHelper.Property));

                var propertyEntry = new DefaultMetadataDetails(propertyKey, attributes);
                if (propertyHelper.Property.CanRead && propertyHelper.Property.GetMethod?.IsPrivate == true)
                {
                    propertyEntry.PropertyAccessor = PropertyHelper.MakeFastPropertyGetter(propertyHelper.Property);
                }

                if (propertyHelper.Property.CanWrite && propertyHelper.Property.SetMethod?.IsPrivate == true)
                {
                    propertyEntry.PropertySetter = PropertyHelper.MakeFastPropertySetter(propertyHelper.Property);
                }

                propertyEntries.Add(propertyEntry);
            }

            return propertyEntries.ToArray();
        }

        /// <summary>
        /// Creates the <see cref="DefaultMetadataDetails"/> entry for a model <see cref="Type"/>.
        /// </summary>
        /// <param name="key">
        /// The <see cref="ModelMetadataIdentity"/> identifying the model <see cref="Type"/>.
        /// </param>
        /// <returns>A details object for the model <see cref="Type"/>.</returns>
        /// <remarks>
        /// The results of this method will be cached and used to satisfy calls to
        /// <see cref="GetMetadataForType(Type)"/>. Override this method to provide a different
        /// set of attributes.
        /// </remarks>
        protected virtual DefaultMetadataDetails CreateTypeDetails([NotNull] ModelMetadataIdentity key)
        {
            var attributes = new List<object>(ModelAttributes.GetAttributesForType(key.ModelType));
            return new DefaultMetadataDetails(key, attributes);
        }

        private class TypeCache : ConcurrentDictionary<ModelMetadataIdentity, ModelMetadataCacheEntry>
        {
            public TypeCache()
                : base(ModelMetadataIdentityComparer.Instance)
            {
            }
        }

        private struct ModelMetadataCacheEntry
        {
            public ModelMetadataCacheEntry(ModelMetadata metadata, DefaultMetadataDetails details)
            {
                Metadata = metadata;
                Details = details;
            }

            public ModelMetadata Metadata { get; private set; }

            public DefaultMetadataDetails Details { get; private set; }
        }

        private class ModelMetadataIdentityComparer : IEqualityComparer<ModelMetadataIdentity>
        {
            public static readonly ModelMetadataIdentityComparer Instance = new ModelMetadataIdentityComparer();

            public bool Equals(ModelMetadataIdentity x, ModelMetadataIdentity y)
            {
                return
                    x.ContainerType == y.ContainerType &&
                    x.ModelType == y.ModelType &&
                    x.Name == y.Name;
            }

            public int GetHashCode(ModelMetadataIdentity obj)
            {
                var hash = 17;
                hash = hash * 23 + obj.ModelType.GetHashCode();

                if (obj.ContainerType != null)
                {
                    hash = hash * 23 + obj.ContainerType.GetHashCode();
                }

                if (obj.Name != null)
                {
                    hash = hash * 23 + obj.Name.GetHashCode();
                }

                return hash;
            }
        }
    }
}