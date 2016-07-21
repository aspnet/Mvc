// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Cci;
using Microsoft.Cci.MutableCodeModel;

namespace Microsoft.AspNetCore.Mvc.Razor.Precompilation.Internal
{
    public class RemoveStrongNameRewriter : MetadataRewriter
    {
        public RemoveStrongNameRewriter(IMetadataHost host)
            : base(host)
        {
        }

        public override IAssembly Rewrite(IAssembly assembly)
        {
            var mutableAssembly = base.Rewrite(assembly) as Assembly;
            if (mutableAssembly == null)
            {
                return assembly;
            }

            mutableAssembly.PublicKey = null;
            mutableAssembly.StrongNameSigned = false;
            return mutableAssembly;
        }
    }
}
