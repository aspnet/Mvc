// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.TestConfiguration;

namespace Microsoft.AspNet.Builder
{
    public static class BuilderExtensions
    {
        public static IApplicationBuilder GetTestConfiguration(this IApplicationBuilder app)
        {
            // Unconditionally place CultureReplacerMiddleware as early as possible in the pipeline.
            return app.UseMiddleware<CultureReplacerMiddleware>();
        }

        public static IApplicationBuilder UseErrorReporter(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ErrorReporterMiddleware>();
        }
    }
}