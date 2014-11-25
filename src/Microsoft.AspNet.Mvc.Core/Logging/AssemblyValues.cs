// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    public class AssemblyValues : LoggerStructureBase
    {
        public AssemblyValues([NotNull] Assembly inner)
        {
            AssemblyName = inner.FullName;
#if ASPNET50
            Location = inner.Location;
            IsFullyTrusted = inner.IsFullyTrusted;
#endif
            IsDynamic = inner.IsDynamic;
        }

        public string AssemblyName { get; set; }

#if ASPNET50
        public string Location { get; set; }

        public bool IsFullyTrusted { get; set; }
#endif
        public bool IsDynamic { get; set; }

        public override string Format()
        {
            return LogFormatter.FormatStructure(this);
        }
    }
}