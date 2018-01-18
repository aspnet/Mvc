using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace Microsoft.AspNetCore.Mvc.Core.ApplicationParts
{
    /// <summary>
    /// An <see cref="AssemblyPart"/> that was added by an assembly that referenced it through the use
    /// of an assembly metadata attribute.
    /// </summary>
    public class AdditionalAssemblyPart : AssemblyPart
    {
        /// <inheritdoc />
        public AdditionalAssemblyPart(Assembly assembly) : base(assembly)
        {
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetReferencePaths()
        {
            return Array.Empty<string>();
        }
    }
}
