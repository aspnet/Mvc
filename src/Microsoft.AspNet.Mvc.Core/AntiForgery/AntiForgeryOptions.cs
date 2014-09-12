// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using Microsoft.AspNet.Mvc.Core;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Provides programmatic configuration for the anti-forgery token system.
    /// </summary>
    public class AntiForgeryOptions
    {
        private string _cookieName;
        private string _formFieldName = AntiForgeryTokenFieldName;
        internal const string AntiForgeryTokenFieldName = "__RequestVerificationToken";

        /// <summary>
        /// Specifies the name of the cookie that is used by the anti-forgery
        /// system.
        /// </summary>
        /// <remarks>
        /// If an explicit name is not provided, the system will automatically
        /// generate a name.
        /// </remarks>
        public string CookieName
        {
            get
            {
                return _cookieName;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value",
                                                    Resources.FormatPropertyOfTypeCannotBeNull(
                                                                "CookieName", typeof(AntiForgeryOptions)));
                }

                _cookieName = value;
            }
        }

        /// <summary>
        /// Specifies the name of the anti-forgery token field that is used by the anti-forgery system.
        /// </summary>
        public string FormFieldName
        {
            get
            {
                return _formFieldName;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value", 
                                                    Resources.FormatPropertyOfTypeCannotBeNull(
                                                                "FormFieldName", typeof(AntiForgeryOptions)));
                }

                _formFieldName = value;
            }
        }

        /// <summary>
        /// Specifies whether SSL is required for the anti-forgery system
        /// to operate. If this setting is 'true' and a non-SSL request
        /// comes into the system, all anti-forgery APIs will fail.
        /// </summary>
        public bool RequireSSL
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
        public bool SuppressXFrameOptionsHeader
        {
            get;
            set;
        }

        // If the app path is provided, we're generating a cookie name rather than using a field name, and the cookie names should
        // be unique so that a development server cookie and an IIS cookie - both running on localhost - don't stomp on
        // each other.
        internal static string GetDefaultAntiForgeryCookieName(string appPath)
        {
            if (string.IsNullOrEmpty(appPath) || appPath == "/")
            {
                return AntiForgeryTokenFieldName;
            }
            else
            {
                return AntiForgeryTokenFieldName + "_" + AntiForgeryTokenEncodingHelper.UrlTokenEncode(Encoding.UTF8.GetBytes(appPath));
            }
        }
    }
}