// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc
{
    public class ViewComponentContext
    {
        public ViewComponentContext(
            [NotNull] ViewComponentDescriptor viewComponentDescriptor,
            [NotNull] object[] arguments,
            [NotNull] ViewContext viewContext,
            [NotNull] TextWriter writer)
        {
            ViewComponentDescriptor = viewComponentDescriptor;
            Arguments = arguments;
            ViewContext = viewContext;
            Writer = writer;
        }

        public object[] Arguments { get; }

        public ViewComponentDescriptor ViewComponentDescriptor { get; }

        public ViewContext ViewContext { get; }

        public TextWriter Writer { get; }
    }
}
