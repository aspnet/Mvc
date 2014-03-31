// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Security.Principal;

namespace Microsoft.AspNet.Mvc
{
    // Can extract unique identifers for a claims-based identity
    public interface IClaimUidExtractor
    {
        byte[] ExtractClaimUid(IIdentity identity);
    }
}