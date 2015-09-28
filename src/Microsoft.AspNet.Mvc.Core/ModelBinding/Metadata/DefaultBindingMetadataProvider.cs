// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding.Metadata
{
    /// <summary>
    /// A default implementation of <see cref="IBindingMetadataProvider"/>.
    /// </summary>
    public class DefaultBindingMetadataProvider : IBindingMetadataProvider
    {
        private readonly ModelBindingMessages _modelBindingMessages;

        public DefaultBindingMetadataProvider(ModelBindingMessages modelBindingMessages)
        {
            if (modelBindingMessages == null)
            {
                throw new ArgumentNullException(nameof(modelBindingMessages));
            }

            _modelBindingMessages = modelBindingMessages;
        }

        /// <inheritdoc />
        public void GetBindingMetadata([NotNull] BindingMetadataProviderContext context)
        {
            // BinderModelName
            foreach (var binderModelNameAttribute in context.Attributes.OfType<IModelNameProvider>())
            {
                if (binderModelNameAttribute?.Name != null)
                {
                    context.BindingMetadata.BinderModelName = binderModelNameAttribute.Name;
                    break;
                }
            }

            // BinderType
            foreach (var binderTypeAttribute in context.Attributes.OfType<IBinderTypeProviderMetadata>())
            {
                if (binderTypeAttribute.BinderType != null)
                {
                    context.BindingMetadata.BinderType = binderTypeAttribute.BinderType;
                    break;
                }
            }

            // BindingSource
            foreach (var bindingSourceAttribute in context.Attributes.OfType<IBindingSourceMetadata>())
            {
                if (bindingSourceAttribute.BindingSource != null)
                {
                    context.BindingMetadata.BindingSource = bindingSourceAttribute.BindingSource;
                    break;
                }
            }

            // ModelBindingMessages
            // Provide a unique instance based on one passed to the constructor.
            context.BindingMetadata.ModelBindingMessages = new ModelBindingMessages(_modelBindingMessages);

            // PropertyBindingPredicateProvider
            var predicateProviders = context.Attributes.OfType<IPropertyBindingPredicateProvider>().ToArray();
            if (predicateProviders.Length > 0)
            {
                context.BindingMetadata.PropertyBindingPredicateProvider = new CompositePredicateProvider(
                    predicateProviders);
            }

            if (context.Key.MetadataKind == ModelMetadataKind.Property)
            {
                // BindingBehavior can fall back to attributes on the Container Type, but we should ignore
                // attributes on the Property Type.
                var bindingBehavior = context.PropertyAttributes.OfType<BindingBehaviorAttribute>().FirstOrDefault();
                if (bindingBehavior == null)
                {
                    bindingBehavior =
                        context.Key.ContainerType.GetTypeInfo()
                        .GetCustomAttributes(typeof(BindingBehaviorAttribute), inherit: true)
                        .OfType<BindingBehaviorAttribute>()
                        .FirstOrDefault();
                }

                if (bindingBehavior != null)
                {
                    context.BindingMetadata.IsBindingAllowed = bindingBehavior.Behavior != BindingBehavior.Never;
                    context.BindingMetadata.IsBindingRequired = bindingBehavior.Behavior == BindingBehavior.Required;
                }
            }
        }

        private class CompositePredicateProvider : IPropertyBindingPredicateProvider
        {
            private readonly IEnumerable<IPropertyBindingPredicateProvider> _providers;

            public CompositePredicateProvider(IEnumerable<IPropertyBindingPredicateProvider> providers)
            {
                _providers = providers;
            }

            public Func<ModelBindingContext, string, bool> PropertyFilter
            {
                get
                {
                    return CreatePredicate();
                }
            }

            private Func<ModelBindingContext, string, bool> CreatePredicate()
            {
                var predicates = _providers
                    .Select(p => p.PropertyFilter)
                    .Where(p => p != null);

                return (context, propertyName) =>
                {
                    foreach (var predicate in predicates)
                    {
                        if (!predicate(context, propertyName))
                        {
                            return false;
                        }
                    }

                    return true;
                };
            }
        }
    }
}