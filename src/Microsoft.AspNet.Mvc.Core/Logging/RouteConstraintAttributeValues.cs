// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    public class RouteConstraintAttributeValues : ILoggerStructure
    {
        public RouteConstraintAttribute Inner { get; }

        private Dictionary<string, object> _values;

        public RouteConstraintAttributeValues(RouteConstraintAttribute inner)
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
                    { "RouteKey", Inner.RouteKey },
                    { "RouteValue", Inner.RouteValue },
                    { "RouteKeyHandling", Inner.RouteKeyHandling },
                    { "BlockNonAttributedActions", Inner.BlockNonAttributedActions }
                };
            }
            return _values;
        }
    }
}