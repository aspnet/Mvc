// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Core;
using Microsoft.AspNet.Http.Core.Collections;
using Microsoft.AspNet.Http.Security;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

// Not sure if this is the right location/namespace for this..
namespace Microsoft.AspNet.Mvc.Core
{
	public class MvcControllerUnitTestHelper
	{
		public MvcControllerUnitTestHelper() : this(new ServiceCollection(), new RouteData())
		{
		}

		public MvcControllerUnitTestHelper(IServiceCollection serviceCollection)
			: this(serviceCollection, new RouteData())
		{
		}

		private MvcControllerUnitTestHelper(IServiceCollection serviceCollection, RouteData routeData)
		{
			RouteData = routeData;
			ServiceProvider = serviceCollection.BuildServiceProvider();
		}

		// Not sure if this RouteData property will be usefull..
		public RouteData RouteData
		{
			get;
			set;
		}

		public IServiceProvider ServiceProvider
		{
			get;
			set;
		}

		// Not sure if this HttpContext property will be usefull..
		public HttpContext HttpContext
		{
			get;
			set;
		}

		public Controller Initialize(Controller controller, MvcControllerUnitTestHelperCallback callback)
		{
			return Initialize(controller, new ActionDescriptor(), callback);
			// Question: Can we build (fully or partially) ActionDescriptor() from tyep information??
		}

		private Controller Initialize(Controller controller, ActionDescriptor actionDescriptor, MvcControllerUnitTestHelperCallback callback)
		{
			HttpContext = new MvcControllerTestHelperHttpContext();
			HttpContext.RequestServices = ServiceProvider;
			controller.ActionContext = new ActionContext(HttpContext, RouteData, actionDescriptor);
			controller.Url = new MvcControllerTestHelpeUrlHelper();

			if (callback != null)
			{
				((MvcControllerTestHelperHttpContext)HttpContext).OnRequestAborted = callback.OnRquestAborted;
				((MvcControllerTestHelperHttpContext)HttpContext).OnRequestFormCollection = callback.OnRequestFormCollection;
				((MvcControllerTestHelpeUrlHelper)controller.Url).OnAction =
						(action, controllerName, values, protocol, host, fragment) => callback.OnUrlAction(action);
			}

			return controller;
		}

		private class MvcControllerTestHelperHttpContext : HttpContext
		{
			private HttpContext _httpContext;

			public MvcControllerTestHelperHttpContext()
			{
				_httpContext = new DefaultHttpContext();
			}

			public Func<CancellationToken> OnRequestAborted
			{
				get;
				set;
			}

			public Func<FormCollection> OnRequestFormCollection
			{
				get;
				set;
			}

			public override CancellationToken RequestAborted
			{
				get
				{
					return OnRequestAborted();
				}
			}

			public override HttpRequest Request
			{
				get
				{
					_httpContext.Request.Form = OnRequestFormCollection();
					return _httpContext.Request;
				}
			}

			public override HttpResponse Response
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public override ClaimsPrincipal User
			{
				get
				{
					throw new NotImplementedException();
				}

				set
				{
					throw new NotImplementedException();
				}
			}

			public override IDictionary<object, object> Items
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public override IServiceProvider ApplicationServices
			{
				get
				{
					throw new NotImplementedException();
				}

				set
				{
					throw new NotImplementedException();
				}
			}

			public override IServiceProvider RequestServices
			{
				get
				{
					return _httpContext.RequestServices;
				}

				set
				{
					_httpContext.RequestServices = value;
				}
			}

			public override ISessionCollection Session
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public override bool IsWebSocketRequest
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public override IList<string> WebSocketRequestedProtocols
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public override void Abort()
			{
				throw new NotImplementedException();
			}

			public override void Dispose()
			{
				throw new NotImplementedException();
			}

			public override object GetFeature(Type type)
			{
				throw new NotImplementedException();
			}

			public override void SetFeature(Type type, object instance)
			{
				throw new NotImplementedException();
			}

			public override IEnumerable<AuthenticationDescription> GetAuthenticationTypes()
			{
				throw new NotImplementedException();
			}

			public override IEnumerable<AuthenticationResult> Authenticate(IEnumerable<string> authenticationTypes)
			{
				throw new NotImplementedException();
			}

			public override Task<IEnumerable<AuthenticationResult>> AuthenticateAsync(IEnumerable<string> authenticationTypes)
			{
				throw new NotImplementedException();
			}

			public override Task<WebSocket> AcceptWebSocketAsync(string subProtocol)
			{
				throw new NotImplementedException();
			}
		}

		private class MvcControllerTestHelpeUrlHelper : IUrlHelper
		{
			public Func<string, string, object, string, string, string, string>
				OnAction
			{
				get;
				set;
			}

			public string Action(string action, string controller, object values, string protocol, string host, string fragment)
			{
				return OnAction(action, controller, values, protocol, host, fragment);
			}

			public string Content(string contentPath)
			{
				throw new NotImplementedException();
			}

			public bool IsLocalUrl(string url) // Can it be a static method or the UrlHelper class or UrlHelperExtensions??
			{
				return
					!string.IsNullOrEmpty(url) &&

					// Allows "/" or "/foo" but not "//" or "/\".
					((url[0] == '/' && (url.Length == 1 || (url[1] != '/' && url[1] != '\\'))) ||

					// Allows "~/" or "~/foo".
					(url.Length > 1 && url[0] == '~' && url[1] == '/'));
			}

			public string RouteUrl(string routeName, object values, string protocol, string host, string fragment)
			{
				throw new NotImplementedException();
			}
		}
	}
}