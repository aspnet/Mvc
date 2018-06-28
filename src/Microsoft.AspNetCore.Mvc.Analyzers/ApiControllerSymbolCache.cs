// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;

namespace Microsoft.AspNetCore.Mvc.Analyzers
{
    internal readonly struct ApiControllerSymbolCache
    {
        public ApiControllerSymbolCache(Compilation compilation)
        {
            ApiConventionTypeAttribute = compilation.GetTypeByMetadataName(SymbolNames.ApiConventionTypeAttribute);
            ApiConventionNameMatchAttribute = compilation.GetTypeByMetadataName(SymbolNames.ApiConventionNameMatchAttribute);
            ApiConventionTypeMatchAttribute = compilation.GetTypeByMetadataName(SymbolNames.ApiConventionTypeMatchAttribute);
            ProducesResponseTypeAttribute = compilation.GetTypeByMetadataName(SymbolNames.ProducesResponseTypeAttribute);
        }

        public INamedTypeSymbol ApiConventionNameMatchAttribute { get; }

        public INamedTypeSymbol ApiConventionTypeMatchAttribute { get; }

        public INamedTypeSymbol ApiConventionTypeAttribute { get; }

        public INamedTypeSymbol ProducesResponseTypeAttribute { get; }
    }
}
