// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.Logging;
using Microsoft.AspNet.Mvc.Routing;
using Xunit;

namespace Microsoft.AspNet.Mvc.Test.Logging
{
    public class AttributeRouteModelValuesTest
    {
        [Fact]
        public void AssertAttributeRouteInfoValuesPropertiesMatchAttributeRouteInfoProperties()
        {
            // Assert
            PropertiesHelper.AssertPropertiesAreTheSame(
                typeof(AttributeRouteInfo), 
                typeof(AttributeRouteInfoValues));
        }
    }
}