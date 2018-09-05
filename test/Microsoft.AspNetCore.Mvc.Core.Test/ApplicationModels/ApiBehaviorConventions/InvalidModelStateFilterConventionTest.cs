// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    public class InvalidModelStateFilterConventionTest
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
            Assert.Single(action.Filters.OfType<ModelStateInvalidFilter>());
        }

        [Fact]
        public void Apply_DoesNotAddFilter_IfFeatureIsDisabled()
        {
            // Arrange
            var action = GetActionModel();
            var options = new ApiBehaviorOptions
            {
                SuppressModelStateInvalidFilter = true,
            };
            var convention = GetConvention(options);

            // Act
            convention.Apply(action);

            // Assert
            Assert.Empty(action.Filters.OfType<ModelStateInvalidFilter>());
        }

        private InvalidModelStateFilterConvention GetConvention(ApiBehaviorOptions options = null)
        {
            options = options ?? new ApiBehaviorOptions
            {
                InvalidModelStateResponseFactory = _ => null,
            };

            return new InvalidModelStateFilterConvention(
                Options.Create(options),
                NullLoggerFactory.Instance);
        }

        private static ActionModel GetActionModel()
        {
            return new ActionModel(typeof(object).GetMethods()[0], new object[0]);
        }
    }
}
