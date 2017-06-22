// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.WebEncoders.Testing;

namespace Microsoft.AspNetCore.Mvc.FunctionalTests
{
    public class MvcEncodedTestFixture<TStartup> : MvcTestFixture<TStartup>
        where TStartup : class
    {
        protected override void ConfigureApplication(MvcWebApplicationBuilder<TStartup> builder)
        {
            base.ConfigureApplication(builder);
            builder.ConfigureBeforeStartup(services =>
            {
                services.AddTransient<HtmlEncoder, HtmlTestEncoder>();
                services.AddTransient<JavaScriptEncoder, JavaScriptTestEncoder>();
                services.AddTransient<UrlEncoder, UrlTestEncoder>();
            });
        }
    }
}
