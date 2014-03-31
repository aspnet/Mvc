// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;

namespace Microsoft.AspNet.Mvc
{
    // Can extract unique identifers for a claims-based identity
    public class DefaultClaimUidExtractor : IClaimUidExtractor
    {
        private const string NameIdentifierClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

        private readonly ClaimsIdentityConverter _claimsIdentityConverter;
        private readonly IAntiForgeryConfig _config;

        public DefaultClaimUidExtractor(IAntiForgeryConfig config)
        {
            _config = config;
        }

        public byte[] ExtractClaimUid(IIdentity identity)
        {
            if (identity == null || !identity.IsAuthenticated || _config.SuppressIdentityHeuristicChecks)
            {
                // Skip anonymous users
                // Skip when claims-based checks are disabled
                return null;
            }

            var claimsIdentity = identity as ClaimsIdentity;
            if (claimsIdentity == null)
            {
                // not a claims-based identity
                return null;
            }

            string[] uniqueIdentifierParameters = GetUniqueIdentifierParameters(claimsIdentity);
            byte[] claimUidBytes = ComputeSHA256(uniqueIdentifierParameters);
            return claimUidBytes;
        }

        private static string[] GetUniqueIdentifierParameters(ClaimsIdentity claimsIdentity)
        {
            // TODO: We need to select a single claim based on the Authentication Type of the claim.
            var claims = claimsIdentity.Claims;

            // TODO: Need to check with vittorio for acs.
            // For a correctly configured ACS consumer, this tuple will uniquely
            // identify a user of the application. We assume that a well-behaved
            // identity provider will never assign the same name identifier to multiple
            // users within its security realm, and we assume that ACS has been
            // configured so that each identity provider has a unique 'identityProvider'
            // claim.
            // By default, we look for 'nameIdentifier' claim.
            Claim nameIdentifierClaim = claims.SingleOrDefault(claim => String.Equals(NameIdentifierClaimType, claim.ValueType, StringComparison.Ordinal));
            if (nameIdentifierClaim == null || String.IsNullOrEmpty(nameIdentifierClaim.Value))
            {
                // TODO: Update the exception message.
                throw new InvalidOperationException("DefaultClaimsNotPresent");
            }

            return new string[]
            {
                NameIdentifierClaimType,
                nameIdentifierClaim.Value
            };
        }

     
        private static byte[] ComputeSHA256(IList<string> parameters)
        {
            // TODO: find the right api to compute the hash.
            throw new NotImplementedException();
        }
    }
}