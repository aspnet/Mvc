// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.Cci;
using Microsoft.Cci.MutableCodeModel;

namespace Microsoft.AspNetCore.Mvc.Razor.Precompilation.Internal
{
    public class AddResourcesRewriter : MetadataRewriter
    {
        private readonly List<CompileOutputs> _outputs;
        private IAssembly _assembly;

        public AddResourcesRewriter(IMetadataHost host, List<CompileOutputs> outputs)
            : base(host)
        {
            _outputs = outputs;
        }

        public override IAssembly Rewrite(IAssembly assembly)
        {
            _assembly = assembly;
            return _assembly;
        }

        public override List<IResourceReference> Rewrite(List<IResourceReference> resourceReferences)
        {
            if (resourceReferences == null)
            {
                resourceReferences = new List<IResourceReference>();
            }

            foreach (var output in _outputs)
            {
                var dllResource = $"{PrecompiledViewsFeatureProvider.PrecompiledResourcePrefix}{output.RelativePath}.dll";
                resourceReferences.Add(new ResourceReference
                {
                    Name = host.NameTable.GetNameFor(dllResource),
                    DefiningAssembly = _assembly,
                    IsPublic = true,
                    Resource = new Resource
                    {
                        Data = output.AssemblyStream.ToArray().ToList(),
                        IsPublic = true,
                        DefiningAssembly = _assembly,
                    }
                });

                var pdbResource = $"{PrecompiledViewsFeatureProvider.PrecompiledResourcePrefix}{output.RelativePath}.pdb";
                resourceReferences.Add(new ResourceReference
                {
                    Name = host.NameTable.GetNameFor(pdbResource),
                    DefiningAssembly = _assembly,
                    IsPublic = true,
                    Resource = new Resource
                    {
                        Data = output.PdbStream.ToArray().ToList(),
                        IsPublic = true,
                        DefiningAssembly = _assembly,
                    }
                });
            }

            return resourceReferences;
        }
    }
}
