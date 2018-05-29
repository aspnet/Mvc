// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BasicViews;
using Microsoft.AspNetCore.Hosting;

namespace Microsoft.AspNetCore.Mvc.FunctionalTests
{
    public class BasicViewsFixture : MvcTestFixture<Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);

            // In Production (the MvcTestFixture default), site uses asp-fallback-* attributes. The generated HTML then
            // includes <script> elements containing non-HTML-encoded JavaScript e.g. a && b. XDocument doesn't know
            // <script> elements are implicitly CDATA content, leading to an XmlExcetion in AntiforgeryTestHelper.
            builder.UseEnvironment("Development");
        }

        // Do not leave .db file behind.
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Startup.DropDatabase(Server.Host.Services);
            }

            base.Dispose(disposing);
        }
    }
}
