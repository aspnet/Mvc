using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    /// <summary>
    /// Represents the state of a <see cref="ViewDataDictionary"/>. 
    /// Logged when a view is executed.
    /// </summary>
    public class ViewDataValues : LoggerStructureBase
    {
        public ViewDataValues([NotNull] ViewDataDictionary viewDataDictionary, Type pageType)
        {
            ViewDataTypes = viewDataDictionary.Data.ToDictionary(d => d.Key, d => d.GetType());
            Model = viewDataDictionary.Model?.GetType();
            PageType = pageType;
        }

        /// <summary>
        /// The <see cref="Type"/> of the page being rendered.
        /// </summary>
        public Type PageType { get; }

        /// <summary>
        /// A dictionary mapping the view data to its <see cref="Type"/>.
        /// </summary>
        public IDictionary<string, Type> ViewDataTypes { get; }

        /// <summary>
        /// The <see cref="Type"/> of the model object on the <see cref="ViewDataDictionary"/>.
        /// </summary>
        public Type Model { get; }

        public override string Format()
        {
            return LogFormatter.FormatStructure(this);
        }
    }
}