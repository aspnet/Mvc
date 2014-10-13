﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.FileSystems;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.Framework.OptionsModel;
using Microsoft.Framework.Runtime;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Sets up default options for <see cref="RazorViewEngineOptions"/>.
    /// </summary>
    public class RazorViewEngineOptionsSetup : OptionsAction<RazorViewEngineOptions>
    {
        public RazorViewEngineOptionsSetup(IApplicationEnvironment applicationEnvironment)
            : base(options => ConfigureRazor(options, applicationEnvironment))
        {
            Order = -1;
        }

        private static void ConfigureRazor(RazorViewEngineOptions razorOptions,
                                           IApplicationEnvironment applicationEnvironment)
        {
            razorOptions.FileSystem = new PhysicalFileSystem(applicationEnvironment.ApplicationBasePath);
        }
    }
}