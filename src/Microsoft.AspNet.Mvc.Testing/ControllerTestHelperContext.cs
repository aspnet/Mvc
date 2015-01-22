// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Core;
using Microsoft.AspNet.Http.Interfaces;
using Microsoft.AspNet.Http.Interfaces.Security;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Runtime;

namespace Microsoft.AspNet.Mvc.Testing
{
    public class ControllerTestHelperContext
    {
        private HttpContext _httpContext;
        private RouteData _routeData;
        private IApplicationEnvironment _applicationEnvironment;
        private IServiceCollection _requestServices;
        private IServiceCollection _applicationServices;
        private UserContext _userContext;
        private UrlHelperContext _urlHelperContext;

        public ControllerTestHelperContext()
        {
            RequestAborted = false;
        }

        public HttpContext HttpContext
        {
            get
            {
                if (_httpContext == null)
                {
                    _httpContext = CreateHttpContext();
                }
                return _httpContext;
            }
            set
            {
                _httpContext = value;
            }
        }

        public RouteData RouteData
        {
            get
            {
                if (_routeData == null)
                {
                    _routeData = new RouteData();
                }
                return _routeData;
            }
            set
            {
                _routeData = value;
            }
        }

        public bool RequestAborted
        {
            get;
            set;
        }

        public IAuthenticationHandler ResponseAuthenticationHandler
        {
            get;
            set;
        }

        public UrlHelperContext Url
        {
            get
            {
                if (_urlHelperContext == null)
                {
                    _urlHelperContext = new UrlHelperContext();
                }

                return _urlHelperContext;
            }

            set
            {
                _urlHelperContext = value;
            }
        }

        public IApplicationEnvironment ApplicationEnvironment
        {
            get
            {
                if (_applicationEnvironment == null)
                {
                    _applicationEnvironment = new MockApplicationEnvironment();
                }
                return _applicationEnvironment;
            }
            set
            {
                _applicationEnvironment = value;
            }
        }

        public IServiceCollection RequestServices
        {
            get
            {
                if (_requestServices == null)
                {
                    _requestServices = new ServiceCollection();
                }
                return _requestServices;
            }
            set
            {
                _requestServices = value;
            }
        }

        public IServiceCollection ApplicationServices
        {
            get
            {
                if (_applicationServices == null)
                {
                    _applicationServices = new ServiceCollection();
                }
                return _applicationServices;
            }
            set
            {
                _applicationServices = value;
            }
        }

        public UserContext User
        {
            get
            {
                if (_userContext == null)
                {
                    _userContext = new UserContext();
                }
                return _userContext;
            }
            set
            {
                _userContext = value;
            }
        }

        private DefaultHttpContext CreateHttpContext()
        {
            var httpContext = new DefaultHttpContext();

            httpContext.SetFeature<IHttpRequestLifetimeFeature>(new MockHttpRequestLifetimeFeature());
            httpContext.SetFeature<IHttpAuthenticationFeature>(
                new MockAuthenticationFeature() { Handler = this?.ResponseAuthenticationHandler });

            return httpContext;
        }
    }
}