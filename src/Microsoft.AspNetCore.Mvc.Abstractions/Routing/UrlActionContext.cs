// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Mvc.Routing
{
    /// <summary>
    /// Context object to be used for the URLs that <see cref="IUrlHelper.Action(UrlActionContext)"/> generates.
    /// </summary>
    public class UrlActionContext
    {
        /// <summary>
        /// The name of the action method that <see cref="IUrlHelper.Action(UrlActionContext)"/> uses to generate URLs.
        /// </summary>
        public string Action
        {
            get;
            set;
        }

        /// <summary>
        /// The name of the controller that <see cref="IUrlHelper.Action(UrlActionContext)"/> uses to generate URLs.
        /// </summary>
        public string Controller
        {
            get;
            set;
        }

        /// <summary>
        /// The object that contains the route values that <see cref="IUrlHelper.Action(UrlActionContext)"/>
        /// uses to generate URLs.
        /// </summary>
        public object Values
        {
            get;
            set;
        }

        /// <summary>
        /// The protocol for the URLs that <see cref="IUrlHelper.Action(UrlActionContext)"/> generates,
        /// such as "http" or "https"
        /// </summary>
        public string Protocol
        {
            get;
            set;
        }

        /// <summary>
        /// The host name for the URLs that <see cref="IUrlHelper.Action(UrlActionContext)"/> generates.
        /// </summary>
        public string Host
        {
            get;
            set;
        }

        /// <summary>
        /// The fragment for the URLs that <see cref="IUrlHelper.Action(UrlActionContext)"/> generates.
        /// </summary>
        public string Fragment
        {
            get;
            set;
        }
    }
}