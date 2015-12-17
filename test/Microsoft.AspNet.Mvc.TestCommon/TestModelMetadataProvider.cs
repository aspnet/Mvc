// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNet.Mvc.ModelBinding.Metadata;
using Xunit;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    internal class TestModelMetadataProvider : DefaultModelMetadataProvider
    {
        // Creates a provider with all the defaults - includes data annotations
        public static IModelMetadataProvider CreateDefaultProvider()
        {
            var detailsProviders = new IMetadataDetailsProvider[]
            {
                new DefaultBindingMetadataProvider(CreateMessageProvider()),
                new DefaultValidationMetadataProvider(),
                new DataAnnotationsMetadataProvider(),
                new DataMemberRequiredBindingMetadataProvider(),
            };

            var compositeDetailsProvider = new DefaultCompositeMetadataDetailsProvider(detailsProviders);
            return new DefaultModelMetadataProvider(compositeDetailsProvider);
        }

        public static IModelMetadataProvider CreateDefaultProvider(IList<IMetadataDetailsProvider> providers)
        {
            var detailsProviders = new List<IMetadataDetailsProvider>()
            {
                new DefaultBindingMetadataProvider(CreateMessageProvider()),
                new DefaultValidationMetadataProvider(),
                new DataAnnotationsMetadataProvider(),
                new DataMemberRequiredBindingMetadataProvider(),
            };

            detailsProviders.AddRange(providers);

            var compositeDetailsProvider = new DefaultCompositeMetadataDetailsProvider(detailsProviders);
            return new DefaultModelMetadataProvider(compositeDetailsProvider);
        }

        public static IModelMetadataProvider CreateProvider(IList<IMetadataDetailsProvider> providers)
        {
            var detailsProviders = new List<IMetadataDetailsProvider>();
            if (providers != null)
            {
                detailsProviders.AddRange(providers);
            }

            var compositeDetailsProvider = new DefaultCompositeMetadataDetailsProvider(detailsProviders);
            return new DefaultModelMetadataProvider(compositeDetailsProvider);
        }

        private readonly TestModelMetadataDetailsProvider _detailsProvider;

        public TestModelMetadataProvider()
            : this(new TestModelMetadataDetailsProvider())
        {
        }

        private TestModelMetadataProvider(TestModelMetadataDetailsProvider detailsProvider)
            : base(new DefaultCompositeMetadataDetailsProvider(new IMetadataDetailsProvider[]
                {
                    new DefaultBindingMetadataProvider(CreateMessageProvider()),
                    new DefaultValidationMetadataProvider(),
                    new DataAnnotationsMetadataProvider(),
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

        public static ModelBindingMessageProvider CreateMessageProvider()
        {
            return new ModelBindingMessageProvider
            {
                MissingBindRequiredValueAccessor = name => $"A value for the '{ name }' property was not provided.",
                MissingKeyOrValueAccessor = () => $"A value is required.",
                ValueMustNotBeNullAccessor = value => $"The value '{ value }' is invalid.",
                ValueInvalid_UnknownErrorResource = value => $"The value '{ value }' is invalid.",
                ValueInvalid_WithValueResource =
                (value1, value2) => $"The value '{value1}' is not valid for {value2}.",
                ValueInvalid_WithoutValueResource = value => $"The supplied value is invalid for {value}.",
                NoEncodingFoundOnInputFormatter = (name) => $"No encoding found for input formatter '{name}'. There " +
                "must be at least one supported encoding registered in order for the formatter to read content.",
                UnsupportedContentType = (contentType) => $"Unsupported content type '{contentType}'."
            };
        }

        private class TestModelMetadataDetailsProvider :
            IBindingMetadataProvider,
            IDisplayMetadataProvider,
            IValidationMetadataProvider
        {
            public List<MetadataBuilder> Builders { get; } = new List<MetadataBuilder>();

            public void GetBindingMetadata(BindingMetadataProviderContext context)
            {
                foreach (var builder in Builders)
                {
                    builder.Apply(context);
                }
            }

            public void GetDisplayMetadata(DisplayMetadataProviderContext context)
            {
                foreach (var builder in Builders)
                {
                    builder.Apply(context);
                }
            }

            public void GetValidationMetadata(ValidationMetadataProviderContext context)
            {
                foreach (var builder in Builders)
                {
                    builder.Apply(context);
                }
            }
        }

        public interface IMetadataBuilder
        {
            IMetadataBuilder BindingDetails(Action<BindingMetadata> action);

            IMetadataBuilder DisplayDetails(Action<DisplayMetadata> action);

            IMetadataBuilder ValidationDetails(Action<ValidationMetadata> action);
        }

        private class MetadataBuilder : IMetadataBuilder
        {
            private List<Action<BindingMetadata>> _bindingActions = new List<Action<BindingMetadata>>();
            private List<Action<DisplayMetadata>> _displayActions = new List<Action<DisplayMetadata>>();
            private List<Action<ValidationMetadata>> _valiationActions = new List<Action<ValidationMetadata>>();

            private readonly ModelMetadataIdentity _key;

            public MetadataBuilder(ModelMetadataIdentity key)
            {
                _key = key;
            }

            public void Apply(BindingMetadataProviderContext context)
            {
                if (_key.Equals(context.Key))
                {
                    foreach (var action in _bindingActions)
                    {
                        action(context.BindingMetadata);
                    }
                }
            }

            public void Apply(DisplayMetadataProviderContext context)
            {
                if (_key.Equals(context.Key))
                {
                    foreach (var action in _displayActions)
                    {
                        action(context.DisplayMetadata);
                    }
                }
            }

            public void Apply(ValidationMetadataProviderContext context)
            {
                if (_key.Equals(context.Key))
                {
                    foreach (var action in _valiationActions)
                    {
                        action(context.ValidationMetadata);
                    }
                }
            }

            public IMetadataBuilder BindingDetails(Action<BindingMetadata> action)
            {
                _bindingActions.Add(action);
                return this;
            }

            public IMetadataBuilder DisplayDetails(Action<DisplayMetadata> action)
            {
                _displayActions.Add(action);
                return this;
            }

            public IMetadataBuilder ValidationDetails(Action<ValidationMetadata> action)
            {
                _valiationActions.Add(action);
                return this;
            }
        }
    }
}