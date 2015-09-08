// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.Dnx.Compilation.CSharp;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.Razor.Precompilation
{
    /// <summary>
    /// <see cref="TagHelperDescriptorResolver"/> used during Razor precompilation.
    /// </summary>
    public class PrecompilationTagHelperTypeResolver : TagHelperTypeResolver
    {
        private static readonly string TagHelperTypeName = typeof(ITagHelper).FullName;
        private readonly BeforeCompileContext _compileContext;
        private readonly IAssemblyLoadContext _loadContext;
        private object _compilationLock = new object();
        private bool _assemblyEmited;
        private TypeInfo[] _exportedTypeInfos;

        /// <summary>
        /// Initializes a new instance of <see cref="PrecompilationTagHelperTypeResolver"/>.
        /// </summary>
        /// <param name="compileContext">The <see cref="BeforeCompileContext"/>.</param>
        /// <param name="loadContext">The <see cref="IAssemblyLoadContext"/>.</param>
        public PrecompilationTagHelperTypeResolver([NotNull] BeforeCompileContext compileContext,
                                                   [NotNull] IAssemblyLoadContext loadContext)
        {
            _compileContext = compileContext;
            _loadContext = loadContext;
        }

        /// <inheritdoc />
        protected override IEnumerable<TypeInfo> GetExportedTypes([NotNull] AssemblyName assemblyName)
        {
            var compilingAssemblyName = _compileContext.Compilation.AssemblyName;
            if (string.Equals(assemblyName.Name, compilingAssemblyName, StringComparison.Ordinal))
            {
                return LazyInitializer.EnsureInitialized(ref _exportedTypeInfos,
                                                         ref _assemblyEmited,
                                                         ref _compilationLock,
                                                         GetExportedTypesFromCompilation);
            }

            return GetExportedTypesCore(assemblyName);
        }

        private IEnumerable<TypeInfo> GetExportedTypesCore(AssemblyName assemblyName)
        {
            var assembly = _loadContext.Load(assemblyName.Name);

            return assembly.ExportedTypes.Select(type => type.GetTypeInfo());
        }

        private TypeInfo[] GetExportedTypesFromCompilation()
        {
            using (var stream = new MemoryStream())
            {
                var assemblyName = string.Join(".", _compileContext.Compilation.AssemblyName,
                                                    nameof(PrecompilationTagHelperTypeResolver),
                                                    Path.GetRandomFileName());

                var emitResult = _compileContext.Compilation
                                                .WithAssemblyName(assemblyName)
                                                .Emit(stream);
                if (!emitResult.Success)
                {
                    // Return an empty sequence. Compilation will fail once precompilation completes.
                    return new TypeInfo[0];
                }

                stream.Position = 0;
                var assembly = _loadContext.LoadStream(stream, assemblySymbols: null);
                return assembly.ExportedTypes
                               .Select(type => type.GetTypeInfo())
                               .ToArray();
            }
        }
    }
}