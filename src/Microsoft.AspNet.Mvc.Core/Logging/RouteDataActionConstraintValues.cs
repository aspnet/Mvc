using System;
using System.Collections.Generic;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    public class RouteDataActionConstraintValues : ILoggerStructure
    {
        public RouteDataActionConstraint Inner { get; }

        private Dictionary<string, object> _values;

        public RouteDataActionConstraintValues(RouteDataActionConstraint inner)
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
                    { "RouteKeyHandling", Inner.KeyHandling }
                };
            }
            return _values;
        }
    }
}