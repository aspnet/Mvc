// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Text;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.AspNet.Mvc.Razor.Compilation;
using Microsoft.AspNet.Razor.CodeGenerators;

namespace RazorPageExecutionInstrumentationWebSite
{
    public class TestRazorCompilationService : RazorCompilationService
    {
        public TestRazorCompilationService(
            ICompilationService compilationService,
            IMvcRazorHost razorHost,
            IRazorViewEngineFileProviderAccessor fileProviderAccessor)
            : base(compilationService, razorHost, fileProviderAccessor)
        {
        }

        protected override GeneratorResults GenerateCode(string relativePath, Stream inputStream)
        {
            // Normalize line endings to '\r\n' (CRLF). This removes core.autocrlf, core.eol, core.safecrlf, and
            // .gitattributes from the equation and treats "\r\n" and "\n" as equivalent. Does not handle
            // some line endings like "\r" but otherwise ensures checksums and line mappings are consistent.
            string text;
            using (var streamReader = new StreamReader(inputStream))
            {
                text = streamReader.ReadToEnd().Replace("\r", "").Replace("\n", "\r\n");
            }

            var bytes = Encoding.UTF8.GetBytes(text);
            inputStream = new MemoryStream(bytes);

            return base.GenerateCode(relativePath, inputStream);
        }
    }
}