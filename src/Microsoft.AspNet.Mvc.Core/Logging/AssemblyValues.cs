// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    public class AssemblyValues : ILoggerStructure
    {
        public Assembly Inner { get; }

        private Dictionary<string, object> _values;

        public AssemblyValues(Assembly inner)
        {
            Inner = inner;
        }

        public string Format()
        {
            return LogFormatter.FormatStructure(this);
        }

        public IEnumerable<KeyValuePair<string, object>> GetValues()
        {
            if (_values == null)
            {
                _values = new Dictionary<string, object> {
                    { "AssemblyName", Inner.FullName },
#if ASPNET50
                    { "Location", Inner.Location },
                    { "IsFullyTrusted", Inner.IsFullyTrusted },
#endif
                    { "IsDynamic", Inner.IsDynamic }
                };
            }
            return _values;
        }
    }
}