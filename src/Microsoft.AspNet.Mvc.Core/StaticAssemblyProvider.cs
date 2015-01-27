// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// A <see cref="IAssemblyProvider"/> with a fixed set of candidate assemblies. 
    /// </summary>
    public class StaticAssemblyProvider : IAssemblyProvider
    {
        /// <summary>
        /// Initializes a new instance of <see cref="StaticAssemblyProvider"/>.
        /// </summary>
        /// <param name="assemblies">The candidate assemblies.</param>
        public StaticAssemblyProvider([NotNull] IEnumerable<Assembly> assemblies)
        {
            CandidateAssemblies = assemblies;
        }

        /// <inheritdoc />
        public IEnumerable<Assembly> CandidateAssemblies { get; }
    }
}
