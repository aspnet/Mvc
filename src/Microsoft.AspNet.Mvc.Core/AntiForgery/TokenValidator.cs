// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Security.Principal;
using Microsoft.AspNet.Abstractions;

namespace Microsoft.AspNet.Mvc
{
    internal sealed class TokenValidator : ITokenValidator, ITokenGenerator
    {
        private readonly IClaimUidExtractor _claimUidExtractor;
        private readonly IAntiForgeryConfig _config;

        internal TokenValidator(IAntiForgeryConfig config, IClaimUidExtractor claimUidExtractor)
        {
            _config = config;
            _claimUidExtractor = claimUidExtractor;
        }

        public AntiForgeryToken GenerateCookieToken()
        {
            return new AntiForgeryToken()
            {
                // SecurityToken will be populated automatically.
                IsSessionToken = true
            };
        }

        public AntiForgeryToken GenerateFormToken(HttpContext httpContext, IIdentity identity, AntiForgeryToken cookieToken)
        {
            Contract.Assert(IsCookieTokenValid(cookieToken));

            AntiForgeryToken formToken = new AntiForgeryToken()
            {
                SecurityToken = cookieToken.SecurityToken,
                IsSessionToken = false
            };

            bool requireAuthenticatedUserHeuristicChecks = false;
            // populate Username and ClaimUid
            if (identity != null && identity.IsAuthenticated)
            {
                if (!_config.SuppressIdentityHeuristicChecks)
                {
                    // If the user is authenticated and heuristic checks are not suppressed,
                    // then Username, ClaimUid, or AdditionalData must be set.
                    requireAuthenticatedUserHeuristicChecks = true;
                }

                formToken.ClaimUid = GetClaimUid(_claimUidExtractor.ExtractClaimUid(identity));
                if (formToken.ClaimUid == null)
                {
                    formToken.Username = identity.Name;
                }
            }

            // populate AdditionalData
            if (_config.AdditionalDataProvider != null)
            {
                formToken.AdditionalData = _config.AdditionalDataProvider.GetAdditionalData(httpContext);
            }

            if (requireAuthenticatedUserHeuristicChecks
                && String.IsNullOrEmpty(formToken.Username)
                && formToken.ClaimUid == null
                && String.IsNullOrEmpty(formToken.AdditionalData))
            {
                // TODO: Throw properException
                // Application says user is authenticated, but we have no identifier for the user.
                throw new InvalidOperationException("TokenValidator_AuthenticatedUserWithoutUsername");
            }

            return formToken;
        }

        public bool IsCookieTokenValid(AntiForgeryToken cookieToken)
        {
            return (cookieToken != null && cookieToken.IsSessionToken);
        }

        // TODO: Sweep the exceptions.
        public void ValidateTokens(HttpContext httpContext, IIdentity identity, AntiForgeryToken sessionToken, AntiForgeryToken fieldToken)
        {
            // Were the tokens even present at all?
            if (sessionToken == null)
            {
                // TODO: Throw properException
                //throw HttpAntiForgeryException.CreateCookieMissingException(_config.CookieName);
            }
            if (fieldToken == null)
            {
                // TODO: Throw properException
                //throw HttpAntiForgeryException.CreateFormFieldMissingException(_config.FormFieldName);
            }

            // Do the tokens have the correct format?
            if (!sessionToken.IsSessionToken || fieldToken.IsSessionToken)
            {
                // TODO: Throw properException
                //throw HttpAntiForgeryException.CreateTokensSwappedException(_config.CookieName, _config.FormFieldName);
            }

            // Are the security tokens embedded in each incoming token identical?
            if (!Equals(sessionToken.SecurityToken, fieldToken.SecurityToken))
            {
                // TODO: Throw properException
                //throw HttpAntiForgeryException.CreateSecurityTokenMismatchException();
            }

            // Is the incoming token meant for the current user?
            string currentUsername = String.Empty;
            BinaryBlob currentClaimUid = null;

            if (identity != null && identity.IsAuthenticated)
            {
                currentClaimUid = GetClaimUid( _claimUidExtractor.ExtractClaimUid(identity));
                if (currentClaimUid == null)
                {
                    currentUsername = identity.Name ?? String.Empty;
                }
            }

            // OpenID and other similar authentication schemes use URIs for the username.
            // These should be treated as case-sensitive.
            bool useCaseSensitiveUsernameComparison = currentUsername.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || currentUsername.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

            if (!String.Equals(fieldToken.Username, currentUsername, (useCaseSensitiveUsernameComparison) ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase))
            {
                // TODO: Throw properException
                //throw HttpAntiForgeryException.CreateUsernameMismatchException(fieldToken.Username, currentUsername);
            }
            if (!Equals(fieldToken.ClaimUid, currentClaimUid))
            {
                // TODO: Throw properException
                //throw HttpAntiForgeryException.CreateClaimUidMismatchException();
            }

            // Is the AdditionalData valid?
            if (_config.AdditionalDataProvider != null && !_config.AdditionalDataProvider.ValidateAdditionalData(httpContext, fieldToken.AdditionalData))
            {
                // TODO: Throw properException
                //throw HttpAntiForgeryException.CreateAdditionalDataCheckFailedException();
            }
        }

        private static BinaryBlob GetClaimUid(byte[] claimUidBytes)
        {
            return new BinaryBlob(256, claimUidBytes);
        }
    }
}