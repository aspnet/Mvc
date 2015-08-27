﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.Razor.Precompilation
{
    /// <summary>
    /// Caches mapping of <see cref="System.Reflection.TypeInfo"/> to <see cref="INamedTypeSymbol"/>.
    /// </summary>
    public class CodeAnalysisSymbolLookupCache
    {
        private readonly Dictionary<System.Reflection.TypeInfo, INamedTypeSymbol> _symbolLookup =
            new Dictionary<System.Reflection.TypeInfo, INamedTypeSymbol>();
        private readonly object _lookupLock = new object();
        private readonly CodeAnalysis.Compilation _compilation;

        /// <summary>
        /// Initialzes a new instance of <see cref="CodeAnalysisSymbolLookupCache"/>.
        /// </summary>
        /// <param name="compilation">The <see cref="CodeAnalysis.Compilation"/> instance.</param>
        public CodeAnalysisSymbolLookupCache([NotNull] CodeAnalysis.Compilation compilation)
        {
            _compilation = compilation;
        }

        /// <summary>
        /// Gets a <see cref="INamedTypeSymbol"/> that corresponds to <paramref name="typeInfo"/>.
        /// </summary>
        /// <param name="typeInfo">The <see cref="System.Reflection.TypeInfo"/> to lookup.</param>
        /// <returns></returns>
        public INamedTypeSymbol GetSymbol([NotNull] System.Reflection.TypeInfo typeInfo)
        {
           lock (_lookupLock)
            {
                INamedTypeSymbol typeSymbol;
                if (!_symbolLookup.TryGetValue(typeInfo, out typeSymbol))
                {
                    typeSymbol = _compilation.GetTypeByMetadataName(typeInfo.FullName);
                    _symbolLookup[typeInfo] = typeSymbol;
                }

                return typeSymbol;
            }
        }
    }
}
