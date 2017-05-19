// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Microsoft.AspNetCore.Mvc.Razor.TagHelpers
{
    public class TagHelperComponentManager : ITagHelperComponentManager
    {
        private List<ITagHelperComponent> _tagHelperComponents;

        public TagHelperComponentManager(IEnumerable<ITagHelperComponent> tagHelperComponents)
        {
            if (tagHelperComponents == null)
            {
                throw new ArgumentNullException(nameof(tagHelperComponents));
            }

            _tagHelperComponents = new List<ITagHelperComponent>(tagHelperComponents);
        }

        public IEnumerable<ITagHelperComponent> Components => _tagHelperComponents;

        public void Add(ITagHelperComponent tagHelperComponent)
        {
            _tagHelperComponents.Add(tagHelperComponent);
        }
    }
}
