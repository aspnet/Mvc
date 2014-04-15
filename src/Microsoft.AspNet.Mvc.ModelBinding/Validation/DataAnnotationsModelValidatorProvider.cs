﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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
        private static bool _addImplicitRequiredAttributeForValueTypes = true;
        private readonly Dictionary<Type, DataAnnotationsModelValidationFactory> _attributeFactories =
            BuildAttributeFactoriesDictionary();

        // Factories for validation attributes
        private static readonly DataAnnotationsModelValidationFactory _defaultAttributeFactory =
            (attribute) => new DataAnnotationsModelValidator(attribute);

        // Factories for IValidatableObject models
        private static readonly DataAnnotationsValidatableObjectAdapterFactory _defaultValidatableFactory =
            () => new ValidatableObjectAdapter();

        private static bool AddImplicitRequiredAttributeForValueTypes
        {
            get { return _addImplicitRequiredAttributeForValueTypes; }
            set { _addImplicitRequiredAttributeForValueTypes = value; }
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
                results.Add(_defaultValidatableFactory());
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
                                                          Type validationAttributeType,
                                                          DataAnnotationsModelValidationFactory factory)
        {
            if (validationAttributeType != null)
            {
                dictionary.Add(validationAttributeType, factory);
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
    }
}
