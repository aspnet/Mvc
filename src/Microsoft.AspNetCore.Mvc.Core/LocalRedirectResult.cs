// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.Core;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// An <see cref="ActionResult"/> that returns a redirect to the supplied local URL.
    /// </summary>
    public class LocalRedirectResult : ActionResult
    {
        private string _localUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalRedirectResult"/> class with the values
        /// provided.
        /// </summary>
        /// <param name="localUrl">The local URL to redirect to.</param>
        public LocalRedirectResult(string localUrl)
             : this(localUrl, permanent: false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalRedirectResult"/> class with the values
        /// provided.
        /// </summary>
        /// <param name="localUrl">The local URL to redirect to.</param>
        /// <param name="permanent">Specifies whether the redirect should be permanent (301) or temporary (302).</param>
        public LocalRedirectResult(string localUrl, bool permanent)
            : this(localUrl, permanent, preserveMethod: false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalRedirectResult"/> class with the values
        /// provided.
        /// </summary>
        /// <param name="localUrl">The local URL to redirect to.</param>
        /// <param name="permanent">Specifies whether the redirect should be permanent (301) or temporary (302).</param>
        /// <param name="preserveMethod">If set to true, make the temporary redirect (307) or permanent redirect (308) preserve the intial request's method.</param>
        public LocalRedirectResult(string localUrl, bool permanent, bool preserveMethod)
        {
            if (string.IsNullOrEmpty(localUrl))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, nameof(localUrl));
            }

            Permanent = permanent;
            PreserveMethod = preserveMethod;
            Url = localUrl;
        }

        /// <summary>
        /// Gets or sets the value that specifies that the redirect should be permanent if true or temporary if false.
        /// </summary>
        public bool Permanent { get; set; }

        /// <summary>
        /// Gets or sets an indication that the redirect preserves initial request method.
        /// </summary>
        public bool PreserveMethod { get; set; }

        /// <summary>
        /// Gets or sets the local URL to redirect to.
        /// </summary>
        public string Url
        {
            get
            {
                return _localUrl;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, nameof(value));
                }

                _localUrl = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IUrlHelper"/> for this result.
        /// </summary>
        public IUrlHelper UrlHelper { get; set; }

        /// <inheritdoc />
        public override void ExecuteResult(ActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var executor = context.HttpContext.RequestServices.GetRequiredService<LocalRedirectResultExecutor>();
            executor.Execute(context, this);
        }
    }
}
