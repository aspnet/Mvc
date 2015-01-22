// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Mvc.Internal;

namespace Microsoft.AspNet.Mvc.Testing
{
    internal class MockUrlHelper : IUrlHelper
    {
        public Func<UrlHelperContext, string> OnAction
        {
            get;
            set;
        }

        public string Action
            (string action, string controller, object values, string protocol, string host, string fragment)
        {
            if (OnAction != null)
            {
                var context = new UrlHelperContext()
                {
                    Action = action,
                    Controller = controller,
                    Values = values,
                    Protocol = protocol,
                    Host = host,
                    Fragment = fragment
                };

                return OnAction(context);
            }

            return null;
        }

        public string Content(string contentPath)
        {
            throw new NotImplementedException();
        }

        public bool IsLocalUrl(string url)
        {
            return UrlUtility.IsLocalUrl(url);
        }

        public string RouteUrl(string routeName, object values, string protocol, string host, string fragment)
        {
            throw new NotImplementedException();
        }
    }
}