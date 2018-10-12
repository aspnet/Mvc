﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.AspNetCore.Mvc.Formatters.Xml
{
    public class ProblemDetailsWrapperProviderFactoryTest
    {
        [Fact]
        public void GetProvider_ReturnsNull_IfTypeDoesNotMatch()
        {
            // Arrange
            var xmlOptions = new MvcXmlOptions();
            var providerFactory = new ProblemDetailsWrapperProviderFactory(xmlOptions);
            var context = new WrapperProviderContext(typeof(SerializableError), isSerialization: true);

            // Act
            var provider = providerFactory.GetProvider(context);

            // Assert
            Assert.Null(provider);
        }

        [Fact]
        public void GetProvider_ReturnsWrapper_ForProblemDetails()
        {
            // Arrange
            var xmlOptions = new MvcXmlOptions { AllowRfc7807CompliantProblemDetailsFormat = true };
            var providerFactory = new ProblemDetailsWrapperProviderFactory(xmlOptions);
            var instance = new ProblemDetails();
            var context = new WrapperProviderContext(instance.GetType(), isSerialization: true);

            // Act
            var provider = providerFactory.GetProvider(context);

            // Assert
            var result = provider.Wrap(instance);
            var wrapper = Assert.IsType<ProblemDetailsWrapper>(result);
            Assert.Same(instance, wrapper.ProblemDetails);
        }

        [Fact]
        public void GetProvider_Returns21CompatibleWrapper_ForProblemDetails()
        {
            // Arrange
            var xmlOptions = new MvcXmlOptions();
            var providerFactory = new ProblemDetailsWrapperProviderFactory(xmlOptions);
            var instance = new ProblemDetails();
            var context = new WrapperProviderContext(instance.GetType(), isSerialization: true);

            // Act
            var provider = providerFactory.GetProvider(context);

            // Assert
            var result = provider.Wrap(instance);
#pragma warning disable CS0618 // Type or member is obsolete
            var wrapper = Assert.IsType<ProblemDetails21Wrapper>(result);
#pragma warning restore CS0618 // Type or member is obsolete
            Assert.Same(instance, wrapper.ProblemDetails);
        }

        [Fact]
        public void GetProvider_ReturnsWrapper_ForValidationProblemDetails()
        {
            // Arrange
            var xmlOptions = new MvcXmlOptions { AllowRfc7807CompliantProblemDetailsFormat = true };
            var providerFactory = new ProblemDetailsWrapperProviderFactory(xmlOptions);
            var instance = new ValidationProblemDetails();
            var context = new WrapperProviderContext(instance.GetType(), isSerialization: true);

            // Act
            var provider = providerFactory.GetProvider(context);

            // Assert
            var result = provider.Wrap(instance);
            var wrapper = Assert.IsType<ValidationProblemDetailsWrapper>(result);
            Assert.Same(instance, wrapper.ProblemDetails);
        }

        [Fact]
        public void GetProvider_Returns21CompatibleWrapper_ForValidationProblemDetails()
        {
            // Arrange
            var xmlOptions = new MvcXmlOptions();
            var providerFactory = new ProblemDetailsWrapperProviderFactory(xmlOptions);
            var instance = new ValidationProblemDetails();
            var context = new WrapperProviderContext(instance.GetType(), isSerialization: true);

            // Act
            var provider = providerFactory.GetProvider(context);

            // Assert
            var result = provider.Wrap(instance);
#pragma warning disable CS0618 // Type or member is obsolete
            var wrapper = Assert.IsType<ValidationProblemDetails21Wrapper>(result);
#pragma warning restore CS0618 // Type or member is obsolete
            Assert.Same(instance, wrapper.ProblemDetails);
        }

        [Fact]
        public void GetProvider_ReturnsNull_ForCustomProblemDetails()
        {
            // Arrange
            var xmlOptions = new MvcXmlOptions();
            var providerFactory = new ProblemDetailsWrapperProviderFactory(xmlOptions);
            var instance = new CustomProblemDetails();
            var context = new WrapperProviderContext(instance.GetType(), isSerialization: true);

            // Act
            var provider = providerFactory.GetProvider(context);

            // Assert
            Assert.Null(provider);
        }

        private class CustomProblemDetails : ProblemDetails { }
    }
}
