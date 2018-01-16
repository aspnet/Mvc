// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Razor.Hosting;
using static Microsoft.AspNetCore.Mvc.Razor.Compilation.CompiledViewDescriptorProvider;

namespace Microsoft.AspNetCore.Mvc.Razor.Internal
{
    internal class TestableCompiledViewDescriptorProvider : DefaultCompiledViewDescriptorProvider
    {
        private readonly Dictionary<Assembly, IEnumerable<RazorViewAttribute>> _attributes;
        private readonly Dictionary<Assembly, IReadOnlyList<RazorCompiledItem>> _items;

        public TestableCompiledViewDescriptorProvider(
            Dictionary<Assembly, IReadOnlyList<RazorCompiledItem>> items,
            Dictionary<Assembly, IEnumerable<RazorViewAttribute>> attributes)
        {
            _items = items;
            _attributes = attributes;
        }

        public override IEnumerable<RazorViewAttribute> GetViewAttributes(Assembly assembly)
        {
            if (_attributes.TryGetValue(assembly, out var attributes))
            {
                return attributes;
            }

            return Enumerable.Empty<RazorViewAttribute>();
        }

        public override IReadOnlyList<RazorCompiledItem> LoadItems(Assembly assembly)
            => _items[assembly];
    }
}
