// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.TestCommon
{
    public static class AssertLines
    {
        public static IEnumerable<string> ReadLines(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    yield return reader.ReadLine();
                }
            }
        }

        public static void Equal(Stream a, Stream b)
        {
            Assert.Equal(ReadLines(a), ReadLines(b)); 
        }

        public static void Equal(string a, string b)
        {
            using (var astream = new MemoryStream(Encoding.UTF8.GetBytes(a)))
            using (var bstream = new MemoryStream(Encoding.UTF8.GetBytes(b)))
            {
                Assert.Equal(ReadLines(astream), ReadLines(bstream));
            }
        }
    }
}
