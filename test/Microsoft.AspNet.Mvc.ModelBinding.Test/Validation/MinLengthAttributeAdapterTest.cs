// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Testing;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.AspNet.Mvc.ModelBinding.Validation
{
    public class MinLengthAttributeAdapterTest
    {
        [Fact]
        [ReplaceCulture]
        public void ClientRulesWithMinLengthAttribute()
        {
            // Arrange
            var provider = TestModelMetadataProvider.CreateDefaultProvider();
            var metadata = provider.GetMetadataForProperty(typeof(string), "Length");
            var attribute = new MinLengthAttribute(6);
            var adapter = new MinLengthAttributeAdapter(attribute);
            var serviceCollection = new ServiceCollection();
            var requestServices = serviceCollection.BuildServiceProvider();
            var context = new ClientModelValidationContext(metadata, provider, requestServices);

            // Act
            var rules = adapter.GetClientValidationRules(context);

            // Assert
            var rule = Assert.Single(rules);
            Assert.Equal("minlength", rule.ValidationType);
            Assert.Equal(1, rule.ValidationParameters.Count);
            Assert.Equal(6, rule.ValidationParameters["min"]);
            Assert.Equal(attribute.FormatErrorMessage("Length"), rule.ErrorMessage);
        }

        [Fact]
        [ReplaceCulture]
        public void ClientRulesWithMinLengthAttributeAndCustomMessage()
        {
            // Arrange
            var propertyName = "Length";
            var message = "Array must have at least {1} items.";
            var provider = TestModelMetadataProvider.CreateDefaultProvider();
            var metadata = provider.GetMetadataForProperty(typeof(string), propertyName);
            var attribute = new MinLengthAttribute(2) { ErrorMessage = message };
            var adapter = new MinLengthAttributeAdapter(attribute);
            var serviceCollection = new ServiceCollection();
            var requestServices = serviceCollection.BuildServiceProvider();
            var context = new ClientModelValidationContext(metadata, provider, requestServices);

            // Act
            var rules = adapter.GetClientValidationRules(context);

            // Assert
            var rule = Assert.Single(rules);
            Assert.Equal("minlength", rule.ValidationType);
            Assert.Equal(1, rule.ValidationParameters.Count);
            Assert.Equal(2, rule.ValidationParameters["min"]);
            Assert.Equal("Array must have at least 2 items.", rule.ErrorMessage);
        }
    }
}