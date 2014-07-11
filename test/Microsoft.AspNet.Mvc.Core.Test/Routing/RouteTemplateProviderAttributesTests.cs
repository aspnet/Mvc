// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Mvc.Routing;
using Xunit;

namespace Microsoft.AspNet.Mvc.Core.Test
{
    public class RouteTemplateProviderAttributesTests
    {
        [Theory]
        [MemberData("RouteTemplateProvidersTestData")]
        public void ConstraintsDictionary_Defaults_ToNull(IRouteTemplateProvider routeTemplateProvider)
        {
            // Act & Assert
            Assert.Null(routeTemplateProvider.Constraints);
        }

        [Theory]
        [MemberData("RouteTemplateProvidersTestData")]
        public void DataTokensDictionary_Defaults_ToNull(IRouteTemplateProvider routeTemplateProvider)
        {
            // Act & Assert
            Assert.Null(routeTemplateProvider.DataTokens);
        }

        [Theory]
        [MemberData("RouteTemplateProvidersTestData")]
        public void DefaultValuesDictionary_Defaults_ToNull(IRouteTemplateProvider routeTemplateProvider)
        {
            // Act & Assert
            Assert.Null(routeTemplateProvider.Defaults);
        }

        [Theory]
        [MemberData("RouteTemplateProvidersTestData")]
        public void Order_Defaults_ToNull(IRouteTemplateProvider routeTemplateProvider)
        {
            // Act & Assert
            Assert.Null(routeTemplateProvider.Order);
        }

        [Theory]
        [MemberData("RouteTemplateProvidersTestData")]
        public void Name_Defaults_ToNull(IRouteTemplateProvider routeTemplateProvider)
        {
            // Act & Assert
            Assert.Null(routeTemplateProvider.Name);
        }

        public static TheoryData<IRouteTemplateProvider> RouteTemplateProvidersTestData
        {
            get
            {
                var data = new TheoryData<IRouteTemplateProvider>();
                data.Add(new HttpGetAttribute());
                data.Add(new HttpPostAttribute());
                data.Add(new HttpPutAttribute());
                data.Add(new HttpPatchAttribute());
                data.Add(new HttpDeleteAttribute());
                data.Add(new RouteAttribute(""));

                return data;
            }
        }
    }
}