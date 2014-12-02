﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.ApplicationModels;
using Microsoft.AspNet.Mvc.Logging;
using Xunit;

namespace Microsoft.AspNet.Mvc.Test.Logging
{
    public class ActionModelValuesTest
    {
        [Fact]
        public void AssertActionModelValuesPropertiesMatchActionModelProperties()
        {
            // Arrange
            string[] exclude = { "ApiExplorer", "Controller", "Attributes" };

            // Assert
            PropertiesHelper.AssertPropertiesAreTheSame(
                typeof(ActionModel), 
                typeof(ActionModelValues), 
                exclude);
        }
    }
}