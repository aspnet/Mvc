// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Mvc.ApplicationParts
{
    public class CompiledPageInfo
    {
        public CompiledPageInfo(string path, Type compiledType, string routePrefix)
        {
            Path = path;
            CompiledType = compiledType;
            RoutePrefix = routePrefix;
        }

        public CompiledPageInfo(string path, Type compiledType, string routePrefix, string name)
        {
            Path = path;
            CompiledType = compiledType;
            RoutePrefix = routePrefix;
            Name = name;
        }

        public string Path { get; }

        public string RoutePrefix { get; }

        public string Name { get; }

        public Type CompiledType { get; }
    }
}
