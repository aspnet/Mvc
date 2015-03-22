// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
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

            // PropertyBindingPredicateProvider
            var predicateProviders = context.Attributes.OfType<IPropertyBindingPredicateProvider>().ToArray();
            if (predicateProviders.Length > 0)
            {
                context.BindingMetadata.PropertyBindingPredicateProvider = new CompositePredicateProvider(
                    predicateProviders);
            }

            // BindingBehaviorAttribute (CanBeBound, IsRequired)
            if (context.Key.MetadataKind == ModelMetadataKind.Property)
            {
                BindingBehaviorAttribute bindingBehaviorAttribute = null;

                // Not using context.Attributes here because we need to fall back to an attribute
                // on the container type, that means that attributes on the property type need to be ignored.
                var containerType = context.Key.ContainerType;
                var property = containerType.GetProperty(context.Key.Name);
                if (property != null)
                {
                    bindingBehaviorAttribute = property.GetCustomAttribute<BindingBehaviorAttribute>();
                }
                
                if (bindingBehaviorAttribute == null)
                {
                    var containerTypeInfo = containerType.GetTypeInfo();
                    bindingBehaviorAttribute = containerTypeInfo.GetCustomAttribute<BindingBehaviorAttribute>();
                }

                if (bindingBehaviorAttribute != null)
                {
                    if (bindingBehaviorAttribute.Behavior == BindingBehavior.Never)
                    {
                        // We're explicitly not using IsReadOnly here. IsReadOnly = true can be model bound if the
                        // model type has mutable properties.
                        context.BindingMetadata.CanBeBound = false;
                    }
                    else if (bindingBehaviorAttribute.Behavior == BindingBehavior.Required)
                    {
                        context.BindingMetadata.IsRequired = true;
                    }
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