// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc
{
    public class ActionDescriptorsCollection
    {
        public ActionDescriptorsCollection([NotNull] IReadOnlyList<ActionDescriptor> items, int version)
        {
            Items = items;
            Version = version;
        }

        public IReadOnlyList<ActionDescriptor> Items { get; private set; }
        public int Version { get; private set; }
    }
}