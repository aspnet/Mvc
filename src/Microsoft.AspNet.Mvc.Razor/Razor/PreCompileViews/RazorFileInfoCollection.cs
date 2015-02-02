// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Framework.Runtime;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Specifies metadata about precompiled views.
    /// </summary>
    public abstract class RazorFileInfoCollection
    {
        /// <summary>
        /// Gets or sets the name of the resource containing the precompiled binary.
        /// </summary>
        public string AssemblyResourceName { get; protected set; }

        /// <summary>
        /// Gets or sets the name of the resource that contains the symbols (pdb).
        /// </summary>
        public string SymbolsResourceName { get; protected set; }

        /// <summary>
        /// Gets the <see cref="IReadOnlyList{T}{T}"/> of <see cref="RazorFileInfo"/>s.
        /// </summary>
        public IReadOnlyList<RazorFileInfo> FileInfos { get; protected set; }

        public virtual Assembly LoadAssembly(IAssemblyLoadContext loadContext)
        {
            var viewCollectionAssembly = GetType().GetTypeInfo().Assembly;

            using (var assemblyStream = viewCollectionAssembly.GetManifestResourceStream(AssemblyResourceName))
            {
                if (assemblyStream == null)
                {
                    throw new InvalidOperationException("The resource '{0}' specified by '{1}' could not be found.");
                }

                Stream symbolsStream = null;
                if (!string.IsNullOrEmpty(SymbolsResourceName))
                {
                    symbolsStream = viewCollectionAssembly.GetManifestResourceStream(SymbolsResourceName);
                }

                using (symbolsStream)
                {
                    return loadContext.LoadStream(assemblyStream, symbolsStream);
                }
            }
        }
    }
}