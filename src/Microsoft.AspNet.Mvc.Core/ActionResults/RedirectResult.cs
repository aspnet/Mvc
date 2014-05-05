// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc
{
    public class RedirectResult : ActionResult
    {
        public RedirectResult(string url)
            : this(url, permanent: false)
        {
        }

        public RedirectResult(string url, bool permanent)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, "url");
            }

            Permanent = permanent;
            Url = url;
        }

        public bool Permanent { get; private set; }

        public string Url { get; private set; }

        public override void ExecuteResult([NotNull] ActionContext context)
        {
            var urlHelper = context.HttpContext.RequestServices.GetService<IUrlHelper>();
            var destinationUrl = Url;
            if(urlHelper.IsLocalUrl(Url))
            {
                destinationUrl = urlHelper.Content(Url);
            }

            context.HttpContext.Response.Redirect(destinationUrl, Permanent);
        }
    }
}