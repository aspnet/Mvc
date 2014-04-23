﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Microsoft.AspNet.Mvc
{
    // Can extract unique identifers for a claims-based identity
    public class DefaultClaimUidExtractor : IClaimUidExtractor
    {
        public string ExtractClaimUid(ClaimsIdentity claimsIdentity)
        {
            if (claimsIdentity == null || !claimsIdentity.IsAuthenticated)
            {
                // Skip anonymous users
                return null;
            }

            var uniqueIdentifierParameters = GetUniqueIdentifierParameters(claimsIdentity);
            byte[] claimUidBytes = ComputeSHA256(uniqueIdentifierParameters);
            return Convert.ToBase64String(claimUidBytes);
        }

        private static IEnumerable<string> GetUniqueIdentifierParameters(ClaimsIdentity claimsIdentity)
        {
            // TODO: Need to enable support for special casing acs identities.
            var nameIdentifierClaim = claimsIdentity.FindFirst(claim =>
                                                            String.Equals(ClaimTypes.NameIdentifier,
                                                                        claim.Type, StringComparison.Ordinal));
            if (nameIdentifierClaim != null && !string.IsNullOrEmpty(nameIdentifierClaim.Value))
            {
                return new string[]
                {
                    ClaimTypes.NameIdentifier,
                    nameIdentifierClaim.Value
                };
            }

            // We Do not understand this claimsIdentity, fallback on serializing the entire claims Identity.
            var claims = claimsIdentity.Claims.ToList();
            claims.Sort((a, b) => string.Compare(a.Type, b.Type, StringComparison.Ordinal));
            var identifierParameters = new List<string>();
            foreach (var claim in claims)
            {
                identifierParameters.Add(claim.Type);
                identifierParameters.Add(claim.Value);
            }

            return identifierParameters;
        }

        private static byte[] ComputeSHA256(IEnumerable<string> parameters)
        {
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    foreach (string parameter in parameters)
                    {
                        bw.Write(parameter); // also writes the length as a prefix; unambiguous
                    }

                    bw.Flush();

                    using (var sha256 = SHA256.Create())
                    {
                        byte[] retVal = sha256.ComputeHash(ms.ToArray(), 0, checked((int)ms.Length));
                        return retVal;
                    }
                }
            }
        }
    }
}