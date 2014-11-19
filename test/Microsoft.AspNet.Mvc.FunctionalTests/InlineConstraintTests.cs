﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Threading.Tasks;
using InlineConstraints;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.TestHost;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class InlineConstraintTests
    {
        private readonly IServiceCollection _services = TestHelper.CreateServices("InlineConstraintsWebSite");
        private readonly Action<IApplicationBuilder> _app = new Startup().Configure;

        [Fact]
        public async Task RoutingToANonExistantArea_WithExistConstraint_RoutesToCorrectAction()
        {
            var svc = new DefaultCommandLineArgumentBuilder();
            svc.AddArgument("--TemplateCollection:areaRoute:TemplateValue=" +
                            "{area:exists}/{controller=Home}/{action=Index}");
            svc.AddArgument("--TemplateCollection:actionAsMethod:TemplateValue=" +
                            "{controller=Home}/{action=Index}");

            _services.AddInstance<ICommandLineArgumentBuilder>(svc);

            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/Users");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var returnValue = await response.Content.ReadAsStringAsync();
            Assert.Equal("Users.Index", returnValue);
        }

        [Fact]
        public async Task RoutingToANonExistantArea_WithoutExistConstraint_RoutesToIncorrectAction()
        {
            // Arrange
            var svc = new DefaultCommandLineArgumentBuilder();
            svc.AddArgument("--TemplateCollection:areaRoute:TemplateValue=" +
                            "{area}/{controller=Home}/{action=Index}");
            svc.AddArgument("--TemplateCollection:actionAsMethod:TemplateValue" +
                            "={controller=Home}/{action=Index}");
            _services.AddInstance<ICommandLineArgumentBuilder>(svc);

            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => client.GetAsync("http://localhost/Users"));

            Assert.Equal("The view 'Index' was not found." +
                         " The following locations were searched:\r\n/Areas/Users/Views/Home/Index.cshtml\r\n" +
                         "/Areas/Users/Views/Shared/Index.cshtml\r\n/Views/Shared/Index.cshtml.",
                         ex.Message);
        }
    }
}