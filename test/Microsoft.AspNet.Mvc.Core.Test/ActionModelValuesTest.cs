// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.ApplicationModels;
using Xunit;

namespace Microsoft.AspNet.Mvc.Logging
{
    public class ActionModelValuesTest
    {
        [Fact]
        public void ActionModelValues_IncludesAllProperties()
        {
            // Arrange
            string[] exclude = { "Controller", "Attributes", "IsActionNameMatchRequired" };

            // Assert
            PropertiesHelper.AssertPropertiesAreTheSame(
                typeof(ActionModel), 
                typeof(ActionModelValues), 
                exclude);
        }
    }
}