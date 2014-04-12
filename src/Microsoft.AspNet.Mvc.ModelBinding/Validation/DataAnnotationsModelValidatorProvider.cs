// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// An implementation of <see cref="ModelValidatorProvider"/> which providers validators
    /// for attributes which derive from <see cref="ValidationAttribute"/>. It also provides
    /// a validator for types which implement <see cref="IValidatableObject"/>. To support
    /// client side validation, you can either register adapters through the static methods
    /// on this class, or by having your validation attributes implement
    /// <see cref="IClientValidatable"/>. The logic to support IClientValidatable
    /// is implemented in <see cref="DataAnnotationsModelValidator"/>.
    /// </summary>
    public class DataAnnotationsModelValidatorProvider : AssociatedValidatorProvider
    {
        private readonly Dictionary<Type, DataAnnotationsValidatableObjectAdapterFactory> _validatableFactories =
            new Dictionary<Type, DataAnnotationsValidatableObjectAdapterFactory>();

        private readonly Dictionary<Type, DataAnnotationsModelValidationFactory> _attributeFactories =
            BuildAttributeFactoriesDictionary();

        private bool _addImplicitRequiredAttributeForValueTypes = true;

        // Factories for validation attributes
        private DataAnnotationsModelValidationFactory _defaultAttributeFactory =
            (attribute) => new DataAnnotationsModelValidator(attribute);

        // Factories for IValidatableObject models
        private DataAnnotationsValidatableObjectAdapterFactory _defaultValidatableFactory =
            () => new ValidatableObjectAdapter();

        public bool AddImplicitRequiredAttributeForValueTypes
        {
            get { return _addImplicitRequiredAttributeForValueTypes; }
            set { _addImplicitRequiredAttributeForValueTypes = value; }
        }

        public void RegisterAdapter([NotNull] Type attributeType,
                                    [NotNull] Type adapterType)
        {
            EnsureTypeDerivesFrom(typeof(ValidationAttribute), attributeType);
            EnsureTypeDerivesFrom(typeof(IModelValidator), adapterType);
            var constructor = GetAttributeAdapterConstructor(attributeType, adapterType);
            _attributeFactories[attributeType] = (attribute) => (IModelValidator)constructor.Invoke(new[] { attribute });
        }

        public void RegisterAdapterFactory([NotNull] Type attributeType,
                                           [NotNull] DataAnnotationsModelValidationFactory factory)
        {
            EnsureTypeDerivesFrom(typeof(ValidationAttribute), attributeType);
            _attributeFactories[attributeType] = factory;
        }

        public void RegisterDefaultAdapter(Type adapterType)
        {
            EnsureTypeDerivesFrom(typeof(IModelValidator), adapterType);
            ConstructorInfo constructor = GetAttributeAdapterConstructor(typeof(ValidationAttribute), adapterType);

            _defaultAttributeFactory = (attribute) => (IModelValidator)constructor.Invoke(new[] { attribute });
        }

        public void RegisterDefaultAdapterFactory([NotNull] DataAnnotationsModelValidationFactory factory)
        {
            _defaultAttributeFactory = factory;
        }

        protected override IEnumerable<IModelValidator> GetValidators(ModelMetadata metadata, IEnumerable<Attribute> attributes)
        {
            var results = new List<IModelValidator>();

            // Produce a validator for each validation attribute we find
            foreach (var attribute in attributes.OfType<ValidationAttribute>())
            {
                DataAnnotationsModelValidationFactory factory;
                if (!_attributeFactories.TryGetValue(attribute.GetType(), out factory))
                {
                    factory = _defaultAttributeFactory;
                }
                results.Add(factory(attribute));
            }

            // Produce a validator if the type supports IValidatableObject
            if (typeof(IValidatableObject).IsAssignableFrom(metadata.ModelType))
            {
                DataAnnotationsValidatableObjectAdapterFactory factory;
                if (!_validatableFactories.TryGetValue(metadata.ModelType, out factory))
                {
                    factory = _defaultValidatableFactory;
                }
                results.Add(factory());
            }

            return results;
        }

        private static Dictionary<Type, DataAnnotationsModelValidationFactory> BuildAttributeFactoriesDictionary()
        {
            var dict = new Dictionary<Type, DataAnnotationsModelValidationFactory>();
            AddValidationAttributeAdapter(dict, typeof(RegularExpressionAttribute),
                (attribute) => new RegularExpressionAttributeAdapter((RegularExpressionAttribute)attribute));

            AddDataTypeAttributeAdapter(dict, typeof(UrlAttribute), "url");

            return dict;
        }

        private static void AddValidationAttributeAdapter(Dictionary<Type, DataAnnotationsModelValidationFactory> dictionary,
                                                          Type validataionAttributeType,
                                                          DataAnnotationsModelValidationFactory factory)
        {
            if (validataionAttributeType != null)
            {
                dictionary.Add(validataionAttributeType, factory);
            }
        }

        private static void AddDataTypeAttributeAdapter(Dictionary<Type, DataAnnotationsModelValidationFactory> dictionary,
                                                        Type attributeType,
                                                        string ruleName)
        {
            AddValidationAttributeAdapter(
                dictionary,
                attributeType,
                (attribute) => new DataTypeAttributeAdapter((DataTypeAttribute)attribute, ruleName));
        }

        private static void EnsureTypeDerivesFrom(Type expectedType, Type type)
        {
            if (!expectedType.IsAssignableFrom(type))
            {
                throw new ArgumentException(
                    Resources.FormatTypeMustDeriveFromType(
                        type.FullName,
                        expectedType.FullName),
                    "attributeType");
            }
        }

        private static ConstructorInfo GetAttributeAdapterConstructor(Type attributeType, Type adapterType)
        {
            var constructor = adapterType.GetConstructor(new[] { attributeType });
            if (constructor == null)
            {
                throw new ArgumentException(
                    Resources.FormatDataAnnotationsModelValidatorProvider_ConstructorRequirements(
                        adapterType.FullName,
                        attributeType.FullName),
                    "adapterType");
            }

            return constructor;
        }
    }
}
