// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Testing;
using Microsoft.AspNet.Testing.xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.ModelBinding.Validation
{
    public class CompareAttributeAdapterTest
    {
        [Fact]
        [ReplaceCulture]
        public void ClientRulesWithCompareAttribute_ErrorMessageUsesDisplayName()
        {
            // Arrange
            var metadataProvider = TestModelMetadataProvider.CreateDefaultProvider();
            var metadata = metadataProvider.GetMetadataForProperty(typeof(PropertyDisplayNameModel), "MyProperty");

            var attribute = new CompareAttribute("OtherProperty");
            var adapter = new CompareAttributeAdapter(attribute, stringLocalizer: null);

            var serviceCollection = new ServiceCollection();
            var requestServices = serviceCollection.BuildServiceProvider();

            var context = new ClientModelValidationContext(metadata, metadataProvider, requestServices);

            // Act
            var rules = adapter.GetClientValidationRules(context);

            // Assert
            var rule = Assert.Single(rules);
            // Mono issue - https://github.com/aspnet/External/issues/19
            Assert.Equal(
                PlatformNormalizer.NormalizeContent(
                    "'MyPropertyDisplayName' and 'OtherPropertyDisplayName' do not match."),
                rule.ErrorMessage);
        }

        [Fact]
        [ReplaceCulture]
        public void ClientRulesWithCompareAttribute_ErrorMessageUsesPropertyName()
        {
            // Arrange
            var metadataProvider = TestModelMetadataProvider.CreateDefaultProvider();
            var metadata = metadataProvider.GetMetadataForProperty(typeof(PropertyNameModel), "MyProperty");
            var attribute = new CompareAttribute("OtherProperty");
            var serviceCollection = new ServiceCollection();
            var requestServices = serviceCollection.BuildServiceProvider();
            var context = new ClientModelValidationContext(metadata, metadataProvider, requestServices);
            var adapter = new CompareAttributeAdapter(attribute, stringLocalizer: null);

            // Act
            var rules = adapter.GetClientValidationRules(context);

            // Assert
            var rule = Assert.Single(rules);
            // Mono issue - https://github.com/aspnet/External/issues/19
            Assert.Equal(
                PlatformNormalizer.NormalizeContent("'MyProperty' and 'OtherProperty' do not match."),
                rule.ErrorMessage);
        }

        [Fact]
        public void ClientRulesWithCompareAttribute_ErrorMessageUsesOverride()
        {
            // Arrange
            var metadataProvider = TestModelMetadataProvider.CreateDefaultProvider();
            var metadata = metadataProvider.GetMetadataForProperty(typeof(PropertyNameModel), "MyProperty");
            var attribute = new CompareAttribute("OtherProperty")
            {
                ErrorMessage = "Hello '{0}', goodbye '{1}'."
            };
            var serviceCollection = new ServiceCollection();
            var requestServices = serviceCollection.BuildServiceProvider();
            var context = new ClientModelValidationContext(metadata, metadataProvider, requestServices);
            var adapter = new CompareAttributeAdapter(attribute, stringLocalizer: null);

            // Act
            var rules = adapter.GetClientValidationRules(context);

            // Assert
            var rule = Assert.Single(rules);
            Assert.Equal("Hello 'MyProperty', goodbye 'OtherProperty'.", rule.ErrorMessage);
        }

        [ConditionalFact]
        // ValidationAttribute in Mono does not read non-public resx properties.
        [FrameworkSkipCondition(RuntimeFrameworks.Mono)]
        public void ClientRulesWithCompareAttribute_ErrorMessageUsesResourceOverride()
        {
            // Arrange
            var metadataProvider = TestModelMetadataProvider.CreateDefaultProvider();
            var metadata = metadataProvider.GetMetadataForProperty(typeof(PropertyNameModel), "MyProperty");
            var attribute = new CompareAttribute("OtherProperty")
            {
                ErrorMessageResourceName = "CompareAttributeTestResource",
                ErrorMessageResourceType = typeof(DataAnnotations.Test.Resources),
            };
            var serviceCollection = new ServiceCollection();
            var requestServices = serviceCollection.BuildServiceProvider();
            var context = new ClientModelValidationContext(metadata, metadataProvider, requestServices);
            var adapter = new CompareAttributeAdapter(attribute, stringLocalizer: null);

            // Act
            var rules = adapter.GetClientValidationRules(context);

            // Assert
            var rule = Assert.Single(rules);
            Assert.Equal("Comparing MyProperty to OtherProperty.", rule.ErrorMessage);
        }
#if DNX451
        [Fact]
        [ReplaceCulture]
        public void ClientRulesWithCompareAttribute_StringLocalizer_ReturnsLocalizedErrorString()
        {
            // Arrange
            var provider = TestModelMetadataProvider.CreateDefaultProvider();
            var metadata = provider.GetMetadataForProperty(typeof(PropertyNameModel), "MyProperty");
            var errorKey = "Error field {0} parameter {1}";
            var attribute = new CompareAttribute("OtherProperty") { ErrorMessage = errorKey };

            var localizedString = new LocalizedString(errorKey, "Longueur est invalide");
            var stringLocalizer = new Mock<IStringLocalizer>();
            stringLocalizer.Setup(s => s[errorKey, "MyProperty", "OtherProperty" ]).Returns(localizedString);

            var serviceCollection = new ServiceCollection();
            var requestServices = serviceCollection.BuildServiceProvider();
            var context = new ClientModelValidationContext(metadata, provider, requestServices);
            var adapter = new CompareAttributeAdapter(attribute, stringLocalizer.Object);

            // Act
            var rules = adapter.GetClientValidationRules(context);

            // Assert
            var rule = Assert.Single(rules);
            Assert.Equal("Longueur est invalide", rule.ErrorMessage);
        }
#endif


        private class PropertyDisplayNameModel
        {
            [Display(Name = "MyPropertyDisplayName")]
            public string MyProperty { get; set; }

            [Display(Name = "OtherPropertyDisplayName")]
            public string OtherProperty { get; set; }
        }

        private class PropertyNameModel
        {
            public string MyProperty { get; set; }

            public string OtherProperty { get; set; }
        }
    }
}