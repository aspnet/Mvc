// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    public class ParameterDescriptorValues : ILoggerStructure
    {
        public ParameterDescriptor Inner { get; }

        public Dictionary<string, object> _values { get; private set; }

        public ParameterDescriptorValues(ParameterDescriptor inner)
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
                    { "ParameterName", Inner.Name },
                    { "ParameterType", Inner.ParameterType.FullName },
                    { "Optional", Inner.IsOptional }
                };
            }
            return _values;
        }
    }
}