using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    public class ViewDataValues : LoggerStructureBase
    {
        public ViewDataValues([NotNull] ViewDataDictionary viewDataDictionary)
        {
            ViewDataDictionary = viewDataDictionary.Data.ToDictionary(d => d.Key, d => d.GetType());
            Model = viewDataDictionary.Model?.GetType();
        }

        IDictionary<string, Type> ViewDataDictionary { get; }

        Type Model { get; }

        public override string Format()
        {
            return LogFormatter.FormatStructure(this);
        }
    }
}