﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Mvc.Razor.Internal
{
    public class RazorReferenceManager
    {
        private readonly ApplicationPartManager _partManager;
        private readonly IList<MetadataReference> _additionalMetadataReferences;
        private object _compilationReferencesLock = new object();
        private bool _compilationReferencesInitialized;
        private IList<MetadataReference> _compilationReferences;

        public RazorReferenceManager(
            ApplicationPartManager partManager,
            IOptions<RazorViewEngineOptions> optionsAccessor)
        {
            _partManager = partManager;
            _additionalMetadataReferences = optionsAccessor.Value.AdditionalCompilationReferences;
        }

        public IList<MetadataReference> CompilationReferences
        {
            get
            {
                return LazyInitializer.EnsureInitialized(
                    ref _compilationReferences,
                    ref _compilationReferencesInitialized,
                    ref _compilationReferencesLock,
                    GetCompilationReferences);
            }
        }

        private IList<MetadataReference> GetCompilationReferences()
        {
            var feature = new MetadataReferenceFeature();
            _partManager.PopulateFeature(feature);
            var applicationReferences = feature.MetadataReferences;

            if (_additionalMetadataReferences.Count == 0)
            {
                return applicationReferences;
            }

            var compilationReferences = new List<MetadataReference>(applicationReferences.Count + _additionalMetadataReferences.Count);
            compilationReferences.AddRange(applicationReferences);
            compilationReferences.AddRange(_additionalMetadataReferences);

            return compilationReferences;
        }
    }
}
