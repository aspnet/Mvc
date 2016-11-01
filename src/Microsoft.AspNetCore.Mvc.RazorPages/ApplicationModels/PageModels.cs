// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    public class PageModel
    {
        public PageModel(string relativePath, string viewEnginePath)
        {
            if (relativePath == null)
            {
                throw new ArgumentNullException(nameof(relativePath));
            }

            if (viewEnginePath == null)
            {
                throw new ArgumentNullException(nameof(viewEnginePath));
            }

            RelativePath = relativePath;
            ViewEnginePath = viewEnginePath;

            Filters = new List<IFilterMetadata>();
            Properties = new Dictionary<object, object>();
            Selectors = new List<SelectorModel>();
        }

        public PageModel(PageModel other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            RelativePath = other.RelativePath;
            ViewEnginePath = other.ViewEnginePath;

            Filters = new List<IFilterMetadata>(other.Filters);
            Properties = new Dictionary<object, object>(other.Properties);

            Selectors = new List<SelectorModel>(other.Selectors.Select(m => new SelectorModel(m)));
        }

        public string RelativePath { get; }

        public string ViewEnginePath { get; }

        public IList<IFilterMetadata> Filters { get; }

        public IDictionary<object, object> Properties { get; }

        public IList<SelectorModel> Selectors { get; }
    }
}