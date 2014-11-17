// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Framework.Logging;
using Microsoft.AspNet.Mvc.ApplicationModels;

namespace Microsoft.AspNet.Mvc.Logging
{
    public class AttributeRouteModelValues : ILoggerStructure
    {
        public AttributeRouteModel Inner { get; }

        public Dictionary<string, object> _values { get; private set; }

        public AttributeRouteModelValues(AttributeRouteModel inner)
        {
            Inner = inner;
        }

        public string Format()
        {
            return LogFormatter.FormatStructure(this);
        }

        public IEnumerable<KeyValuePair<string, object>> GetValues()
        {
            if (_values == null && Inner != null)
            {
                _values = new Dictionary<string, object> {
                    { "Template", Inner.Template },
                    { "Order", Inner.Order },
                    { "Name", Inner.Name },
                    { "IsAbsoluteTemplate", Inner.IsAbsoluteTemplate }
                };
            }
            return _values;
        }
    }
}