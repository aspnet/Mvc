// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.ApplicationModels;
using Microsoft.AspNet.Mvc.Logging;
using Xunit;

namespace Microsoft.AspNet.Mvc.Test.Logging
{
    public class ControllerModelValuesTest
    {
        [Fact]
        public void AssertControllerModelValuesPropertiesMatchControllerModelProperties()
        {
            // Arrange
            string[] exclude = { "ApiExplorer", "Application" };

            // Assert
            PropertiesHelper.AssertPropertiesAreTheSame(
                typeof(ControllerModel), 
                typeof(ControllerModelValues), 
                exclude);
        }
    }
}