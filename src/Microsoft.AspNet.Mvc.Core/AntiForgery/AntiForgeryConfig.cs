// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Text;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Provides programmatic configuration for the anti-forgery token system.
    /// </summary>
    public static class AntiForgeryConfig
    {
        internal const string AntiForgeryTokenFieldName = "__RequestVerificationToken";

        private static string _cookieName;
        private static string _uniqueClaimTypeIdentifier;

        /// <summary>
        /// Specifies an object that can provide additional data to put into all
        /// generated tokens and that can validate additional data in incoming
        /// tokens.
        /// </summary>
        public static IAntiForgeryAdditionalDataProvider AdditionalDataProvider
        {
            get;
            set;
        }

        /// <summary>
        /// Specifies the name of the cookie that is used by the anti-forgery
        /// system.
        /// </summary>
        /// <remarks>
        /// If an explicit name is not provided, the system will automatically
        /// generate a name.
        /// </remarks>
        public static string CookieName
        {
            get
            {
                if (_cookieName == null)
                {
                    _cookieName = GetAntiForgeryCookieName();
                }
                return _cookieName;
            }
            set
            {
                _cookieName = value;
            }
        }

        /// <summary>
        /// Specifies whether SSL is required for the anti-forgery system
        /// to operate. If this setting is 'true' and a non-SSL request
        /// comes into the system, all anti-forgery APIs will fail.
        /// </summary>
        public static bool RequireSsl
        {
            get;
            set;
        }

        /// <summary>
        /// Specifies whether to suppress the generation of X-Frame-Options header
        /// which is used to prevent ClickJacking. By default, the X-Frame-Options
        /// header is generated with the value SAMEORIGIN. If this setting is 'true', 
        /// the X-Frame-Options header will not be generated for the response.
        /// </summary>
        public static bool SuppressXFrameOptionsHeader
        {
            get;
            set;
        }

        /// <summary>
        /// Specifies whether the anti-forgery system should skip checking
        /// for conditions that might indicate misuse of the system. Please
        /// use caution when setting this switch, as improper use could open
        /// security holes in the application.
        /// </summary>
        /// <remarks>
        /// Setting this switch will disable several checks, including:
        /// - Identity.IsAuthenticated = true without Identity.Name being set
        /// - special-casing claims-based identities
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static bool SuppressIdentityHeuristicChecks
        {
            get;
            set;
        }

        // TODO: Replace the stub.
        private static string GetAntiForgeryCookieName()
        {
            return AntiForgeryTokenFieldName;
        }
    }
}
