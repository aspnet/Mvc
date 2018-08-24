// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Resources = Microsoft.AspNetCore.Mvc.ViewFeatures.Resources;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// A <see cref="RemoteAttribute"/> for razor page handler which configures Unobtrusive validation 
    /// to send an Ajax request to the web site. The invoked handler should return JSON indicating 
    /// whether the value is valid.
    /// </summary>
    /// <remarks>Does no server-side validation of the final form submission.</remarks>
    public class PageRemoteAttribute : RemoteAttribute
    {
        private readonly string _pageHandler;
        private readonly string _pageName;

        /// <summary>
        /// Initializes a new instance of the <see cref="PageRemoteAttribute"/> class.
        /// </summary>
        /// <remarks>
        /// Will use ambient page name and handler name when generating the URL where client
        /// should send a validation request.
        /// </remarks>
        public PageRemoteAttribute()
            : this(pageHandler: null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageRemoteAttribute"/> class.
        /// </summary>
        /// <param name="pageHandler">
        /// The handler name used when generating the URL where client should send a validation request.
        /// </param>
        public PageRemoteAttribute(string pageHandler)
            : this(pageHandler, pageName: null)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageRemoteAttribute"/> class.
        /// </summary>
        /// <param name="pageHandler">
        /// The page handler name used when generating the URL where client should send a validation request.
        /// </param>
        /// <param name="pageName">
        /// The page name used when generating the URL where client should send a validation request.
        /// </param>
        /// <remarks>
        /// <para>
        /// If <paramref name="pageName"/> or <paramref name="pageHandler"/> is <c>null</c>, uses the corresponding
        /// ambient value.
        /// </para>
        /// </remarks>
        public PageRemoteAttribute(string pageHandler, string pageName)
            : base()
        {
            _pageHandler = pageHandler;
            _pageName = pageName;
        }

        /// <inheritdoc />
        protected override string GetUrl(ClientModelValidationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            
            var services = context.ActionContext.HttpContext.RequestServices;
            var factory = services.GetRequiredService<IUrlHelperFactory>();
            var urlHelper = factory.GetUrlHelper(context.ActionContext);

            var url = urlHelper.Page(_pageName, _pageHandler);

            if (url == null)
            {
                throw new InvalidOperationException(Resources.RemoteAttribute_NoUrlFound);
            }

            return url;
        }
    }
}
