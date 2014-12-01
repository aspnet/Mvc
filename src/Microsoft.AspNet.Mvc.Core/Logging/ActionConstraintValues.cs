using System;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    /// <summary>
    /// Represents an <see cref="IActionConstraintMetadata"/>. 
    /// </summary>
    public class ActionConstraintValues : LoggerStructureBase
    {
        public ActionConstraintValues(IActionConstraintMetadata inner)
        {
            var constraint = inner as IActionConstraint;
            if (constraint != null)
            {
                Order = constraint.Order;
            }
            ActionConstraintMetadataType = inner.GetType();
        }

        public Type ActionConstraintMetadataType { get; }

        public int Order { get; }

        public override string Format()
        {
            return LogFormatter.FormatStructure(this);
        }
    }
}