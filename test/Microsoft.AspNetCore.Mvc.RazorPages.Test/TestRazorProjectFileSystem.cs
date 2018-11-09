﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.FileProviders;
using Moq;

namespace Microsoft.AspNetCore.Mvc.RazorPages
{
    internal class TestRazorProjectFileSystem : FileProviderRazorProjectFileSystem
    {
        public TestRazorProjectFileSystem(IFileProvider fileProvider, IHostingEnvironment hostingEnvironment)
            :base(GetAccessor(fileProvider), hostingEnvironment)
        {
        }

        private static IRazorViewEngineFileProviderAccessor GetAccessor(IFileProvider fileProvider)
        {
            var fileProviderAccessor = new Mock<IRazorViewEngineFileProviderAccessor>();
            fileProviderAccessor.SetupGet(f => f.FileProvider)
                .Returns(fileProvider);

            return fileProviderAccessor.Object;
        }
    }
}
