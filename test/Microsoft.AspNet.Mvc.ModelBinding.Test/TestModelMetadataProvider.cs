// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNet.Mvc.ModelBinding.Metadata;
using Microsoft.Framework.Internal;
using Xunit;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class TestModelMetadataProvider : DefaultModelMetadataProvider
    {
        // Creates a provider with all the defaults - includes data annotations
        public static IModelMetadataProvider CreateDefaultProvider()
        {
            var detailsProviders = new IModelMetadataDetailsProvider[]
            {
                new DefaultModelMetadataBindingDetailsProvider(),
                new DataAnnotationsModelMetadataDetailsProvider(),
            };

            var compositeDetailsProvider = new DefaultCompositeModelMetadataDetailsProvider(detailsProviders);
            return new DefaultModelMetadataProvider(compositeDetailsProvider);
        }

        private readonly TestModelMetadataDetailsProvider _detailsProvider;

        public TestModelMetadataProvider()
            : this(new TestModelMetadataDetailsProvider())
        {

        }

        private TestModelMetadataProvider(TestModelMetadataDetailsProvider detailsProvider)
            : base(new DefaultCompositeModelMetadataDetailsProvider(new IModelMetadataDetailsProvider[]
                {
                    new DefaultModelMetadataBindingDetailsProvider(),
                    new DataAnnotationsModelMetadataDetailsProvider(),
                    detailsProvider
                }))
        {
            _detailsProvider = detailsProvider;
        }

        public IMetadataBuilder ForType(Type type)
        {
            var key = ModelMetadataIdentity.ForType(type);

            var builder = new MetadataBuilder(key);
            _detailsProvider.Builders.Add(builder);
            return builder;
        }

        public IMetadataBuilder ForType<TModel>()
        {
            return ForType(typeof(TModel));
        }

        public IMetadataBuilder ForProperty(Type containerType, string propertyName)
        {
            var property = containerType.GetRuntimeProperty(propertyName);
            Assert.NotNull(property);

            var key = ModelMetadataIdentity.ForProperty(property.PropertyType, propertyName, containerType);

            var builder = new MetadataBuilder(key);
            _detailsProvider.Builders.Add(builder);
            return builder;
        }

        public IMetadataBuilder ForProperty<TContainer>(string propertyName)
        {
            return ForProperty(typeof(TContainer), propertyName);
        }

        private class TestModelMetadataDetailsProvider :
            IModelMetadataBindingDetailsProvider,
            IModelMetadataDisplayDetailsProvider,
            IModelMetadataValidationDetailsProvider
        {
            public List<MetadataBuilder> Builders { get; } = new List<MetadataBuilder>();

            public void GetBindingDetails([NotNull] ModelMetadataBindingDetailsContext context)
            {
                foreach (var builder in Builders)
                {
                    builder.Apply(context);
                }
            }

            public void GetDisplayDetails([NotNull] ModelMetadataDisplayDetailsContext context)
            {
                foreach (var builder in Builders)
                {
                    builder.Apply(context);
                }
            }

            public void GetValidationDetails([NotNull] ModelMetadataValidationDetailsContext context)
            {
                foreach (var builder in Builders)
                {
                    builder.Apply(context);
                }
            }
        }

        public interface IMetadataBuilder
        {
            IMetadataBuilder BindingDetails(Action<ModelMetadataBindingDetails> action);

            IMetadataBuilder DisplayDetails(Action<ModelMetadataDisplayDetails> action);

            IMetadataBuilder ValidationDetails(Action<ModelMetadataValidationDetails> action);
        }

        private class MetadataBuilder : IMetadataBuilder
        {
            private List<Action<ModelMetadataBindingDetails>> _bindingActions = new List<Action<ModelMetadataBindingDetails>>();
            private List<Action<ModelMetadataDisplayDetails>> _displayActions = new List<Action<ModelMetadataDisplayDetails>>();
            private List<Action<ModelMetadataValidationDetails>> _valiationActions = new List<Action<ModelMetadataValidationDetails>>();

            private readonly ModelMetadataIdentity _key;

            public MetadataBuilder(ModelMetadataIdentity key)
            {
                _key = key;
            }

            public void Apply(ModelMetadataBindingDetailsContext context)
            {
                if (_key.Equals(context.Key))
                {
                    foreach (var action in _bindingActions)
                    {
                        action(context.BindingDetails);
                    }
                }
            }

            public void Apply(ModelMetadataDisplayDetailsContext context)
            {
                if (_key.Equals(context.Key))
                {
                    foreach (var action in _displayActions)
                    {
                        action(context.DisplayDetails);
                    }
                }
            }

            public void Apply(ModelMetadataValidationDetailsContext context)
            {
                if (_key.Equals(context.Key))
                {
                    foreach (var action in _valiationActions)
                    {
                        action(context.ValidationDetails);
                    }
                }
            }

            public IMetadataBuilder BindingDetails(Action<ModelMetadataBindingDetails> action)
            {
                _bindingActions.Add(action);
                return this;
            }

            public IMetadataBuilder DisplayDetails(Action<ModelMetadataDisplayDetails> action)
            {
                _displayActions.Add(action);
                return this;
            }

            public IMetadataBuilder ValidationDetails(Action<ModelMetadataValidationDetails> action)
            {
                _valiationActions.Add(action);
                return this;
            }
        }
    }
}