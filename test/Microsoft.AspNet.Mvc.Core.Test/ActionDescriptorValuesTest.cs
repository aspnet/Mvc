﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.Logging;
using Xunit;

namespace Microsoft.AspNet.Mvc.Test.Logging
{
    public class ActionDescriptorValuesTest
    {
        [Fact]
        public void AssertActionDescriptorValuesPropertiesMatchActionDescriptorProperties()
        {
            // Arrange
            var exclude = new string[] { "DisplayName", "Properties", "RouteValueDefaults" };

            // Assert
            PropertiesHelper.AssertPropertiesAreTheSame(
                typeof(ControllerActionDescriptor), 
                typeof(ActionDescriptorValues), 
                exclude);
        }
    }
}