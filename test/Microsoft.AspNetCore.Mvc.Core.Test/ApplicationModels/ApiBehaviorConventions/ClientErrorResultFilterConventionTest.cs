// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    public class ClientErrorResultFilterConventionTest
    {
        [Fact]
        public void Apply_AddsFilter()
        {
            // Arrange
            var action = GetActionModel();
            var convention = GetConvention();

            // Act
            convention.Apply(action);

            // Assert
            Assert.Single(action.Filters.OfType<ClientErrorResultFilter>());
        }

        [Fact]
        public void Apply_DoesNotAddFilter_IfFeatureIsDisabled()
        {
            // Arrange
            var action = GetActionModel();
            var options = new ApiBehaviorOptions
            {
                SuppressMapClientErrors = true,
            };
            var convention = GetConvention(options);

            // Act
            convention.Apply(action);

            // Assert
            Assert.Empty(action.Filters.OfType<ClientErrorResultFilter>());
        }

        private ClientErrorResultFilterConvention GetConvention(ApiBehaviorOptions options = null)
        {
            options = options ?? new ApiBehaviorOptions();

            return new ClientErrorResultFilterConvention(
                Options.Create(options),
                Mock.Of<IClientErrorFactory>(),
                NullLoggerFactory.Instance);
        }

        private static ActionModel GetActionModel()
        {
            return new ActionModel(typeof(object).GetMethods()[0], new object[0]);
        }
    }
}
