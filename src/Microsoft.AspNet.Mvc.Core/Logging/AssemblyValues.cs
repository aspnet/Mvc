// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    /// <summary>
    /// Represents the state of an <see cref="Assembly"/>. Logged during Assembly discovery in startup
    /// </summary>
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

        public string AssemblyName { get; }

#if ASPNET50
        public string Location { get; }

        public bool IsFullyTrusted { get; }
#endif
        public bool IsDynamic { get; }

        public override string Format()
        {
            return LogFormatter.FormatStructure(this);
        }
    }
}