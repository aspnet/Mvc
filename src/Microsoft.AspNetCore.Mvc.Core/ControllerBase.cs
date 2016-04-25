// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc.Core;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [Controller]
    public abstract class ControllerBase
    {
        private ControllerContext _controllerContext;
        private IModelMetadataProvider _metadataProvider;
        private IModelBinderFactory _modelBinderFactory;
        private IObjectModelValidator _objectValidator;
        private IUrlHelper _url;

        /// <summary>
        /// Gets the <see cref="Http.HttpContext"/> for the executing action.
        /// </summary>
        public HttpContext HttpContext
        {
            get
            {
                return ControllerContext.HttpContext;
            }
        }

        /// <summary>
        /// Gets the <see cref="HttpRequest"/> for the executing action.
        /// </summary>
        public HttpRequest Request
        {
            get
            {
                return HttpContext?.Request;
            }
        }

        /// <summary>
        /// Gets the <see cref="HttpResponse"/> for the executing action.
        /// </summary>
        public HttpResponse Response
        {
            get
            {
                return HttpContext?.Response;
            }
        }

        /// <summary>
        /// Gets the <see cref="AspNetCore.Routing.RouteData"/> for the executing action.
        /// </summary>
        public RouteData RouteData
        {
            get
            {
                return ControllerContext.RouteData;
            }
        }

        /// <summary>
        /// Gets the <see cref="ModelStateDictionary"/> that contains the state of the model and of model-binding validation.
        /// </summary>
        public ModelStateDictionary ModelState
        {
            get
            {
                return ControllerContext?.ModelState;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Mvc.ControllerContext"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="Controllers.IControllerActivator"/> activates this property while activating controllers.
        /// If user code directly instantiates a controller, the getter returns an empty
        /// <see cref="Mvc.ControllerContext"/>.
        /// </remarks>
        [ControllerContext]
        public ControllerContext ControllerContext
        {
            get
            {
                if (_controllerContext == null)
                {
                    _controllerContext = new ControllerContext();
                }

                return _controllerContext;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _controllerContext = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IModelMetadataProvider"/>.
        /// </summary>
        public IModelMetadataProvider MetadataProvider
        {
            get
            {
                if (_metadataProvider == null)
                {
                    _metadataProvider = HttpContext?.RequestServices?.GetRequiredService<IModelMetadataProvider>();
                }

                return _metadataProvider;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _metadataProvider = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IModelBinderFactory"/>.
        /// </summary>
        public IModelBinderFactory ModelBinderFactory
        {
            get
            {
                if (_modelBinderFactory == null)
                {
                    _modelBinderFactory = HttpContext?.RequestServices?.GetRequiredService<IModelBinderFactory>();
                }

                return _modelBinderFactory;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _modelBinderFactory = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IUrlHelper"/>.
        /// </summary>
        public IUrlHelper Url
        {
            get
            {
                if (_url == null)
                {
                    var factory = HttpContext?.RequestServices?.GetRequiredService<IUrlHelperFactory>();
                    _url = factory?.GetUrlHelper(ControllerContext);
                }

                return _url;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _url = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IObjectModelValidator"/>.
        /// </summary>
        public IObjectModelValidator ObjectValidator
        {
            get
            {
                if (_objectValidator == null)
                {
                    _objectValidator = HttpContext?.RequestServices?.GetRequiredService<IObjectModelValidator>();
                }

                return _objectValidator;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _objectValidator = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="ClaimsPrincipal"/> for user associated with the executing action.
        /// </summary>
        public ClaimsPrincipal User
        {
            get
            {
                return HttpContext?.User;
            }
        }

        /// <summary>
        /// Creates a <see cref="StatusCodeResult"/> object by specifying a <paramref name="statusCode"/>.
        /// </summary>
        /// <param name="statusCode">The status code to set on the response.</param>
        /// <returns>The created <see cref="StatusCodeResult"/> object for the response.</returns>
        [NonAction]
        public virtual StatusCodeResult StatusCode(int statusCode)
        {
            return new StatusCodeResult(statusCode);
        }

        /// <summary>
        /// Creates a <see cref="ObjectResult"/> object by specifying a <paramref name="statusCode"/> and <paramref name="value"/>
        /// </summary>
        /// <param name="statusCode">The status code to set on the response.</param>
        /// <param name="value">The value to set on the <see cref="ObjectResult"/>.</param>
        /// <returns>The created <see cref="ObjectResult"/> object for the response.</returns>
        [NonAction]
        public virtual ObjectResult StatusCode(int statusCode, object value)
        {
            var result = new ObjectResult(value);
            result.StatusCode = statusCode;

            return result;
        }

        /// <summary>
        /// Creates a <see cref="ContentResult"/> object by specifying a <paramref name="content"/> string.
        /// </summary>
        /// <param name="content">The content to write to the response.</param>
        /// <returns>The created <see cref="ContentResult"/> object for the response.</returns>
        [NonAction]
        public virtual ContentResult Content(string content)
        {
            return Content(content, (MediaTypeHeaderValue)null);
        }

        /// <summary>
        /// Creates a <see cref="ContentResult"/> object by specifying a <paramref name="content"/> string
        /// and a content type.
        /// </summary>
        /// <param name="content">The content to write to the response.</param>
        /// <param name="contentType">The content type (MIME type).</param>
        /// <returns>The created <see cref="ContentResult"/> object for the response.</returns>
        [NonAction]
        public virtual ContentResult Content(string content, string contentType)
        {
            return Content(content, MediaTypeHeaderValue.Parse(contentType));
        }

        /// <summary>
        /// Creates a <see cref="ContentResult"/> object by specifying a <paramref name="content"/> string,
        /// a <paramref name="contentType"/>, and <paramref name="contentEncoding"/>.
        /// </summary>
        /// <param name="content">The content to write to the response.</param>
        /// <param name="contentType">The content type (MIME type).</param>
        /// <param name="contentEncoding">The content encoding.</param>
        /// <returns>The created <see cref="ContentResult"/> object for the response.</returns>
        /// <remarks>
        /// If encoding is provided by both the 'charset' and the <paramref name="contentEncoding"/> parameters, then
        /// the <paramref name="contentEncoding"/> parameter is chosen as the final encoding.
        /// </remarks>
        [NonAction]
        public virtual ContentResult Content(string content, string contentType, Encoding contentEncoding)
        {
            var mediaTypeHeaderValue = MediaTypeHeaderValue.Parse(contentType);
            mediaTypeHeaderValue.Encoding = contentEncoding ?? mediaTypeHeaderValue.Encoding;
            return Content(content, mediaTypeHeaderValue);
        }

        /// <summary>
        /// Creates a <see cref="ContentResult"/> object by specifying a <paramref name="content"/>
        /// string and a <paramref name="contentType"/>.
        /// </summary>
        /// <param name="content">The content to write to the response.</param>
        /// <param name="contentType">The content type (MIME type).</param>
        /// <returns>The created <see cref="ContentResult"/> object for the response.</returns>
        [NonAction]
        public virtual ContentResult Content(string content, MediaTypeHeaderValue contentType)
        {
            var result = new ContentResult
            {
                Content = content,
                ContentType = contentType?.ToString()
            };

            return result;
        }

        /// <summary>
        /// Creates a <see cref="NoContentResult"/> object that produces an empty No Content (204) response.
        /// </summary>
        /// <returns>The created <see cref="NoContentResult"/> object for the response.</returns>
        [NonAction]
        public virtual NoContentResult NoContent()
        {
            return new NoContentResult();
        }

        /// <summary>
        /// Creates a <see cref="OkResult"/> object that produces an empty OK (200) response.
        /// </summary>
        /// <returns>The created <see cref="OkResult"/> for the response.</returns>
        [NonAction]
        public virtual OkResult Ok()
        {
            return new OkResult();
        }

        /// <summary>
        /// Creates an <see cref="OkObjectResult"/> object that produces an OK (200) response.
        /// </summary>
        /// <param name="value">The content value to format in the entity body.</param>
        /// <returns>The created <see cref="OkObjectResult"/> for the response.</returns>
        [NonAction]
        public virtual OkObjectResult Ok(object value)
        {
            return new OkObjectResult(value);
        }

        /// <summary>
        /// Creates a <see cref="RedirectResult"/> object that redirects to the specified <paramref name="url"/>.
        /// </summary>
        /// <param name="url">The URL to redirect to.</param>
        /// <returns>The created <see cref="RedirectResult"/> for the response.</returns>
        [NonAction]
        public virtual RedirectResult Redirect(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, nameof(url));
            }

            return new RedirectResult(url);
        }

        /// <summary>
        /// Creates a <see cref="RedirectResult"/> object with <see cref="RedirectResult.Permanent"/> set to true
        /// using the specified <paramref name="url"/>.
        /// </summary>
        /// <param name="url">The URL to redirect to.</param>
        /// <returns>The created <see cref="RedirectResult"/> for the response.</returns>
        [NonAction]
        public virtual RedirectResult RedirectPermanent(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, nameof(url));
            }

            return new RedirectResult(url, permanent: true);
        }

        /// <summary>
        /// Creates a <see cref="LocalRedirectResult"/> object that redirects to
        /// the specified local <paramref name="localUrl"/>.
        /// </summary>
        /// <param name="localUrl">The local URL to redirect to.</param>
        /// <returns>The created <see cref="LocalRedirectResult"/> for the response.</returns>
        [NonAction]
        public virtual LocalRedirectResult LocalRedirect(string localUrl)
        {
            if (string.IsNullOrEmpty(localUrl))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, nameof(localUrl));
            }

            return new LocalRedirectResult(localUrl);
        }

        /// <summary>
        /// Creates a <see cref="LocalRedirectResult"/> object with <see cref="LocalRedirectResult.Permanent"/>
        /// set to true using the specified <paramref name="localUrl"/>.
        /// </summary>
        /// <param name="localUrl">The local URL to redirect to.</param>
        /// <returns>The created <see cref="LocalRedirectResult"/> for the response.</returns>
        [NonAction]
        public virtual LocalRedirectResult LocalRedirectPermanent(string localUrl)
        {
            if (string.IsNullOrEmpty(localUrl))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, nameof(localUrl));
            }

            return new LocalRedirectResult(localUrl, permanent: true);
        }

        /// <summary>
        /// Redirects to the specified action using the <paramref name="actionName"/>.
        /// </summary>
        /// <param name="actionName">The name of the action.</param>
        /// <returns>The created <see cref="RedirectToActionResult"/> for the response.</returns>
        [NonAction]
        public virtual RedirectToActionResult RedirectToAction(string actionName)
        {
            return RedirectToAction(actionName, routeValues: null);
        }

        /// <summary>
        /// Redirects to the specified action using the <paramref name="actionName"/>
        /// and <paramref name="routeValues"/>.
        /// </summary>
        /// <param name="actionName">The name of the action.</param>
        /// <param name="routeValues">The parameters for a route.</param>
        /// <returns>The created <see cref="RedirectToActionResult"/> for the response.</returns>
        [NonAction]
        public virtual RedirectToActionResult RedirectToAction(string actionName, object routeValues)
        {
            return RedirectToAction(actionName, controllerName: null, routeValues: routeValues);
        }

        /// <summary>
        /// Redirects to the specified action using the <paramref name="actionName"/>
        /// and the <paramref name="controllerName"/>.
        /// </summary>
        /// <param name="actionName">The name of the action.</param>
        /// <param name="controllerName">The name of the controller.</param>
        /// <returns>The created <see cref="RedirectToActionResult"/> for the response.</returns>
        [NonAction]
        public virtual RedirectToActionResult RedirectToAction(string actionName, string controllerName)
        {
            return RedirectToAction(actionName, controllerName, routeValues: null);
        }

        /// <summary>
        /// Redirects to the specified action using the specified <paramref name="actionName"/>,
        /// <paramref name="controllerName"/>, and <paramref name="routeValues"/>.
        /// </summary>
        /// <param name="actionName">The name of the action.</param>
        /// <param name="controllerName">The name of the controller.</param>
        /// <param name="routeValues">The parameters for a route.</param>
        /// <returns>The created <see cref="RedirectToActionResult"/> for the response.</returns>
        [NonAction]
        public virtual RedirectToActionResult RedirectToAction(
            string actionName,
            string controllerName,
            object routeValues)
        {
            return new RedirectToActionResult(actionName, controllerName, routeValues)
            {
                UrlHelper = Url,
            };
        }

        /// <summary>
        /// Redirects to the specified action with <see cref="RedirectToActionResult.Permanent"/> set to true
        /// using the specified <paramref name="actionName"/>.
        /// </summary>
        /// <param name="actionName">The name of the action.</param>
        /// <returns>The created <see cref="RedirectToActionResult"/> for the response.</returns>
        [NonAction]
        public virtual RedirectToActionResult RedirectToActionPermanent(string actionName)
        {
            return RedirectToActionPermanent(actionName, routeValues: null);
        }

        /// <summary>
        /// Redirects to the specified action with <see cref="RedirectToActionResult.Permanent"/> set to true
        /// using the specified <paramref name="actionName"/> and <paramref name="routeValues"/>.
        /// </summary>
        /// <param name="actionName">The name of the action.</param>
        /// <param name="routeValues">The parameters for a route.</param>
        /// <returns>The created <see cref="RedirectToActionResult"/> for the response.</returns>
        [NonAction]
        public virtual RedirectToActionResult RedirectToActionPermanent(string actionName, object routeValues)
        {
            return RedirectToActionPermanent(actionName, controllerName: null, routeValues: routeValues);
        }

        /// <summary>
        /// Redirects to the specified action with <see cref="RedirectToActionResult.Permanent"/> set to true
        /// using the specified <paramref name="actionName"/> and <paramref name="controllerName"/>.
        /// </summary>
        /// <param name="actionName">The name of the action.</param>
        /// <param name="controllerName">The name of the controller.</param>
        /// <returns>The created <see cref="RedirectToActionResult"/> for the response.</returns>
        [NonAction]
        public virtual RedirectToActionResult RedirectToActionPermanent(string actionName, string controllerName)
        {
            return RedirectToActionPermanent(actionName, controllerName, routeValues: null);
        }

        /// <summary>
        /// Redirects to the specified action with <see cref="RedirectToActionResult.Permanent"/> set to true
        /// using the specified <paramref name="actionName"/>, <paramref name="controllerName"/>,
        /// and <paramref name="routeValues"/>.
        /// </summary>
        /// <param name="actionName">The name of the action.</param>
        /// <param name="controllerName">The name of the controller.</param>
        /// <param name="routeValues">The parameters for a route.</param>
        /// <returns>The created <see cref="RedirectToActionResult"/> for the response.</returns>
        [NonAction]
        public virtual RedirectToActionResult RedirectToActionPermanent(
            string actionName,
            string controllerName,
            object routeValues)
        {
            return new RedirectToActionResult(
                actionName,
                controllerName,
                routeValues,
                permanent: true)
            {
                UrlHelper = Url,
            };
        }

        /// <summary>
        /// Redirects to the specified route using the specified <paramref name="routeName"/>.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <returns>The created <see cref="RedirectToRouteResult"/> for the response.</returns>
        [NonAction]
        public virtual RedirectToRouteResult RedirectToRoute(string routeName)
        {
            return RedirectToRoute(routeName, routeValues: null);
        }

        /// <summary>
        /// Redirects to the specified route using the specified <paramref name="routeValues"/>.
        /// </summary>
        /// <param name="routeValues">The parameters for a route.</param>
        /// <returns>The created <see cref="RedirectToRouteResult"/> for the response.</returns>
        [NonAction]
        public virtual RedirectToRouteResult RedirectToRoute(object routeValues)
        {
            return RedirectToRoute(routeName: null, routeValues: routeValues);
        }

        /// <summary>
        /// Redirects to the specified route using the specified <paramref name="routeName"/>
        /// and <paramref name="routeValues"/>.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="routeValues">The parameters for a route.</param>
        /// <returns>The created <see cref="RedirectToRouteResult"/> for the response.</returns>
        [NonAction]
        public virtual RedirectToRouteResult RedirectToRoute(string routeName, object routeValues)
        {
            return new RedirectToRouteResult(routeName, routeValues)
            {
                UrlHelper = Url,
            };
        }

        /// <summary>
        /// Redirects to the specified route with <see cref="RedirectToRouteResult.Permanent"/> set to true
        /// using the specified <paramref name="routeName"/>.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <returns>The created <see cref="RedirectToRouteResult"/> for the response.</returns>
        [NonAction]
        public virtual RedirectToRouteResult RedirectToRoutePermanent(string routeName)
        {
            return RedirectToRoutePermanent(routeName, routeValues: null);
        }

        /// <summary>
        /// Redirects to the specified route with <see cref="RedirectToRouteResult.Permanent"/> set to true
        /// using the specified <paramref name="routeValues"/>.
        /// </summary>
        /// <param name="routeValues">The parameters for a route.</param>
        /// <returns>The created <see cref="RedirectToRouteResult"/> for the response.</returns>
        [NonAction]
        public virtual RedirectToRouteResult RedirectToRoutePermanent(object routeValues)
        {
            return RedirectToRoutePermanent(routeName: null, routeValues: routeValues);
        }

        /// <summary>
        /// Redirects to the specified route with <see cref="RedirectToRouteResult.Permanent"/> set to true
        /// using the specified <paramref name="routeName"/> and <paramref name="routeValues"/>.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="routeValues">The parameters for a route.</param>
        /// <returns>The created <see cref="RedirectToRouteResult"/> for the response.</returns>
        [NonAction]
        public virtual RedirectToRouteResult RedirectToRoutePermanent(string routeName, object routeValues)
        {
            return new RedirectToRouteResult(routeName, routeValues, permanent: true)
            {
                UrlHelper = Url,
            };
        }

        /// <summary>
        /// Returns a file with the specified <paramref name="fileContents" /> as content and the
        /// specified <paramref name="contentType" /> as the Content-Type.
        /// </summary>
        /// <param name="fileContents">The file contents.</param>
        /// <param name="contentType">The Content-Type of the file.</param>
        /// <returns>The created <see cref="FileContentResult"/> for the response.</returns>
        [NonAction]
        public virtual FileContentResult File(byte[] fileContents, string contentType)
        {
            return File(fileContents, contentType, fileDownloadName: null);
        }

        /// <summary>
        /// Returns a file with the specified <paramref name="fileContents" /> as content, the
        /// specified <paramref name="contentType" /> as the Content-Type and the
        /// specified <paramref name="fileDownloadName" /> as the suggested file name.
        /// </summary>
        /// <param name="fileContents">The file contents.</param>
        /// <param name="contentType">The Content-Type of the file.</param>
        /// <param name="fileDownloadName">The suggested file name.</param>
        /// <returns>The created <see cref="FileContentResult"/> for the response.</returns>
        [NonAction]
        public virtual FileContentResult File(byte[] fileContents, string contentType, string fileDownloadName)
        {
            return new FileContentResult(fileContents, contentType) { FileDownloadName = fileDownloadName };
        }

        /// <summary>
        /// Returns a file in the specified <paramref name="fileStream" /> with the
        /// specified <paramref name="contentType" /> as the Content-Type.
        /// </summary>
        /// <param name="fileStream">The <see cref="Stream"/> with the contents of the file.</param>
        /// <param name="contentType">The Content-Type of the file.</param>
        /// <returns>The created <see cref="FileStreamResult"/> for the response.</returns>
        [NonAction]
        public virtual FileStreamResult File(Stream fileStream, string contentType)
        {
            return File(fileStream, contentType, fileDownloadName: null);
        }

        /// <summary>
        /// Returns a file in the specified <paramref name="fileStream" /> with the
        /// specified <paramref name="contentType" /> as the Content-Type and the
        /// specified <paramref name="fileDownloadName" /> as the suggested file name.
        /// </summary>
        /// <param name="fileStream">The <see cref="Stream"/> with the contents of the file.</param>
        /// <param name="contentType">The Content-Type of the file.</param>
        /// <param name="fileDownloadName">The suggested file name.</param>
        /// <returns>The created <see cref="FileStreamResult"/> for the response.</returns>
        [NonAction]
        public virtual FileStreamResult File(Stream fileStream, string contentType, string fileDownloadName)
        {
            return new FileStreamResult(fileStream, contentType) { FileDownloadName = fileDownloadName };
        }

        /// <summary>
        /// Returns the file specified by <paramref name="virtualPath" /> with the
        /// specified <paramref name="contentType" /> as the Content-Type.
        /// </summary>
        /// <param name="virtualPath">The virtual path of the file to be returned.</param>
        /// <param name="contentType">The Content-Type of the file.</param>
        /// <returns>The created <see cref="VirtualFileResult"/> for the response.</returns>
        [NonAction]
        public virtual VirtualFileResult File(string virtualPath, string contentType)
        {
            return File(virtualPath, contentType, fileDownloadName: null);
        }

        /// <summary>
        /// Returns the file specified by <paramref name="virtualPath" /> with the
        /// specified <paramref name="contentType" /> as the Content-Type and the
        /// specified <paramref name="fileDownloadName" /> as the suggested file name.
        /// </summary>
        /// <param name="virtualPath">The virtual path of the file to be returned.</param>
        /// <param name="contentType">The Content-Type of the file.</param>
        /// <param name="fileDownloadName">The suggested file name.</param>
        /// <returns>The created <see cref="VirtualFileResult"/> for the response.</returns>
        [NonAction]
        public virtual VirtualFileResult File(string virtualPath, string contentType, string fileDownloadName)
        {
            return new VirtualFileResult(virtualPath, contentType) { FileDownloadName = fileDownloadName };
        }

        /// <summary>
        /// Returns the file specified by <paramref name="physicalPath" /> with the
        /// specified <paramref name="contentType" /> as the Content-Type.
        /// </summary>
        /// <param name="physicalPath">The physical path of the file to be returned.</param>
        /// <param name="contentType">The Content-Type of the file.</param>
        /// <returns>The created <see cref="PhysicalFileResult"/> for the response.</returns>
        [NonAction]
        public virtual PhysicalFileResult PhysicalFile(string physicalPath, string contentType)
        {
            return PhysicalFile(physicalPath, contentType, fileDownloadName: null);
        }

        /// <summary>
        /// Returns the file specified by <paramref name="physicalPath" /> with the
        /// specified <paramref name="contentType" /> as the Content-Type and the
        /// specified <paramref name="fileDownloadName" /> as the suggested file name.
        /// </summary>
        /// <param name="physicalPath">The physical path of the file to be returned.</param>
        /// <param name="contentType">The Content-Type of the file.</param>
        /// <param name="fileDownloadName">The suggested file name.</param>
        /// <returns>The created <see cref="PhysicalFileResult"/> for the response.</returns>
        [NonAction]
        public virtual PhysicalFileResult PhysicalFile(
            string physicalPath,
            string contentType,
            string fileDownloadName)
        {
            return new PhysicalFileResult(physicalPath, contentType) { FileDownloadName = fileDownloadName };
        }

        /// <summary>
        /// Creates an <see cref="UnauthorizedResult"/> that produces an Unauthorized (401) response.
        /// </summary>
        /// <returns>The created <see cref="UnauthorizedResult"/> for the response.</returns>
        [NonAction]
        public virtual UnauthorizedResult Unauthorized()
        {
            return new UnauthorizedResult();
        }

        /// <summary>
        /// Creates an <see cref="NotFoundResult"/> that produces a Not Found (404) response.
        /// </summary>
        /// <returns>The created <see cref="NotFoundResult"/> for the response.</returns>
        [NonAction]
        public virtual NotFoundResult NotFound()
        {
            return new NotFoundResult();
        }

        /// <summary>
        /// Creates an <see cref="NotFoundObjectResult"/> that produces a Not Found (404) response.
        /// </summary>
        /// <returns>The created <see cref="NotFoundObjectResult"/> for the response.</returns>
        [NonAction]
        public virtual NotFoundObjectResult NotFound(object value)
        {
            return new NotFoundObjectResult(value);
        }

        /// <summary>
        /// Creates an <see cref="BadRequestResult"/> that produces a Bad Request (400) response.
        /// </summary>
        /// <returns>The created <see cref="BadRequestResult"/> for the response.</returns>
        [NonAction]
        public virtual BadRequestResult BadRequest()
        {
            return new BadRequestResult();
        }

        /// <summary>
        /// Creates an <see cref="BadRequestObjectResult"/> that produces a Bad Request (400) response.
        /// </summary>
        /// <returns>The created <see cref="BadRequestObjectResult"/> for the response.</returns>
        [NonAction]
        public virtual BadRequestObjectResult BadRequest(object error)
        {
            return new BadRequestObjectResult(error);
        }

        /// <summary>
        /// Creates an <see cref="BadRequestObjectResult"/> that produces a Bad Request (400) response.
        /// </summary>
        /// <returns>The created <see cref="BadRequestObjectResult"/> for the response.</returns>
        [NonAction]
        public virtual BadRequestObjectResult BadRequest(ModelStateDictionary modelState)
        {
            if (modelState == null)
            {
                throw new ArgumentNullException(nameof(modelState));
            }

            return new BadRequestObjectResult(modelState);
        }

        /// <summary>
        /// Creates a <see cref="CreatedResult"/> object that produces a Created (201) response.
        /// </summary>
        /// <param name="uri">The URI at which the content has been created.</param>
        /// <param name="value">The content value to format in the entity body.</param>
        /// <returns>The created <see cref="CreatedResult"/> for the response.</returns>
        [NonAction]
        public virtual CreatedResult Created(string uri, object value)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            return new CreatedResult(uri, value);
        }

        /// <summary>
        /// Creates a <see cref="CreatedResult"/> object that produces a Created (201) response.
        /// </summary>
        /// <param name="uri">The URI at which the content has been created.</param>
        /// <param name="value">The content value to format in the entity body.</param>
        /// <returns>The created <see cref="CreatedResult"/> for the response.</returns>
        [NonAction]
        public virtual CreatedResult Created(Uri uri, object value)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            return new CreatedResult(uri, value);
        }

        /// <summary>
        /// Creates a <see cref="CreatedAtActionResult"/> object that produces a Created (201) response.
        /// </summary>
        /// <param name="actionName">The name of the action to use for generating the URL.</param>
        /// <param name="value">The content value to format in the entity body.</param>
        /// <returns>The created <see cref="CreatedAtActionResult"/> for the response.</returns>
        [NonAction]
        public virtual CreatedAtActionResult CreatedAtAction(string actionName, object value)
        {
            return CreatedAtAction(actionName, routeValues: null, value: value);
        }

        /// <summary>
        /// Creates a <see cref="CreatedAtActionResult"/> object that produces a Created (201) response.
        /// </summary>
        /// <param name="actionName">The name of the action to use for generating the URL.</param>
        /// <param name="routeValues">The route data to use for generating the URL.</param>
        /// <param name="value">The content value to format in the entity body.</param>
        /// <returns>The created <see cref="CreatedAtActionResult"/> for the response.</returns>
        [NonAction]
        public virtual CreatedAtActionResult CreatedAtAction(string actionName, object routeValues, object value)
        {
            return CreatedAtAction(actionName, controllerName: null, routeValues: routeValues, value: value);
        }

        /// <summary>
        /// Creates a <see cref="CreatedAtActionResult"/> object that produces a Created (201) response.
        /// </summary>
        /// <param name="actionName">The name of the action to use for generating the URL.</param>
        /// <param name="controllerName">The name of the controller to use for generating the URL.</param>
        /// <param name="routeValues">The route data to use for generating the URL.</param>
        /// <param name="value">The content value to format in the entity body.</param>
        /// <returns>The created <see cref="CreatedAtActionResult"/> for the response.</returns>
        [NonAction]
        public virtual CreatedAtActionResult CreatedAtAction(
            string actionName,
            string controllerName,
            object routeValues,
            object value)
        {
            return new CreatedAtActionResult(actionName, controllerName, routeValues, value);
        }

        /// <summary>
        /// Creates a <see cref="CreatedAtRouteResult"/> object that produces a Created (201) response.
        /// </summary>
        /// <param name="routeName">The name of the route to use for generating the URL.</param>
        /// <param name="value">The content value to format in the entity body.</param>
        /// <returns>The created <see cref="CreatedAtRouteResult"/> for the response.</returns>
        [NonAction]
        public virtual CreatedAtRouteResult CreatedAtRoute(string routeName, object value)
        {
            return CreatedAtRoute(routeName, routeValues: null, value: value);
        }

        /// <summary>
        /// Creates a <see cref="CreatedAtRouteResult"/> object that produces a Created (201) response.
        /// </summary>
        /// <param name="routeValues">The route data to use for generating the URL.</param>
        /// <param name="value">The content value to format in the entity body.</param>
        /// <returns>The created <see cref="CreatedAtRouteResult"/> for the response.</returns>
        [NonAction]
        public virtual CreatedAtRouteResult CreatedAtRoute(object routeValues, object value)
        {
            return CreatedAtRoute(routeName: null, routeValues: routeValues, value: value);
        }

        /// <summary>
        /// Creates a <see cref="CreatedAtRouteResult"/> object that produces a Created (201) response.
        /// </summary>
        /// <param name="routeName">The name of the route to use for generating the URL.</param>
        /// <param name="routeValues">The route data to use for generating the URL.</param>
        /// <param name="value">The content value to format in the entity body.</param>
        /// <returns>The created <see cref="CreatedAtRouteResult"/> for the response.</returns>
        [NonAction]
        public virtual CreatedAtRouteResult CreatedAtRoute(string routeName, object routeValues, object value)
        {
            return new CreatedAtRouteResult(routeName, routeValues, value);
        }

        /// <summary>
        /// Creates a <see cref="ChallengeResult"/>.
        /// </summary>
        /// <returns>The created <see cref="ChallengeResult"/> for the response.</returns>
        [NonAction]
        public virtual ChallengeResult Challenge()
            => new ChallengeResult();

        /// <summary>
        /// Creates a <see cref="ChallengeResult"/> with the specified authentication schemes.
        /// </summary>
        /// <param name="authenticationSchemes">The authentication schemes to challenge.</param>
        /// <returns>The created <see cref="ChallengeResult"/> for the response.</returns>
        [NonAction]
        public virtual ChallengeResult Challenge(params string[] authenticationSchemes)
            => new ChallengeResult(authenticationSchemes);

        /// <summary>
        /// Creates a <see cref="ChallengeResult"/> with the specified <paramref name="properties" />.
        /// </summary>
        /// <param name="properties"><see cref="AuthenticationProperties"/> used to perform the authentication
        /// challenge.</param>
        /// <returns>The created <see cref="ChallengeResult"/> for the response.</returns>
        [NonAction]
        public virtual ChallengeResult Challenge(AuthenticationProperties properties)
            => new ChallengeResult(properties);

        /// <summary>
        /// Creates a <see cref="ChallengeResult"/> with the specified specified authentication schemes and
        /// <paramref name="properties" />.
        /// </summary>
        /// <param name="properties"><see cref="AuthenticationProperties"/> used to perform the authentication
        /// challenge.</param>
        /// <param name="authenticationSchemes">The authentication schemes to challenge.</param>
        /// <returns>The created <see cref="ChallengeResult"/> for the response.</returns>
        [NonAction]
        public virtual ChallengeResult Challenge(
            AuthenticationProperties properties,
            params string[] authenticationSchemes)
            => new ChallengeResult(authenticationSchemes, properties);

        /// <summary>
        /// Creates a <see cref="ForbidResult"/>.
        /// </summary>
        /// <returns>The created <see cref="ForbidResult"/> for the response.</returns>
        [NonAction]
        public virtual ForbidResult Forbid()
            => new ForbidResult();

        /// <summary>
        /// Creates a <see cref="ForbidResult"/> with the specified authentication schemes.
        /// </summary>
        /// <param name="authenticationSchemes">The authentication schemes to challenge.</param>
        /// <returns>The created <see cref="ForbidResult"/> for the response.</returns>
        [NonAction]
        public virtual ForbidResult Forbid(params string[] authenticationSchemes)
            => new ForbidResult(authenticationSchemes);

        /// <summary>
        /// Creates a <see cref="ForbidResult"/> with the specified <paramref name="properties" />.
        /// </summary>
        /// <param name="properties"><see cref="AuthenticationProperties"/> used to perform the authentication
        /// challenge.</param>
        /// <returns>The created <see cref="ForbidResult"/> for the response.</returns>
        [NonAction]
        public virtual ForbidResult Forbid(AuthenticationProperties properties)
            => new ForbidResult(properties);

        /// <summary>
        /// Creates a <see cref="ForbidResult"/> with the specified specified authentication schemes and
        /// <paramref name="properties" />.
        /// </summary>
        /// <param name="properties"><see cref="AuthenticationProperties"/> used to perform the authentication
        /// challenge.</param>
        /// <param name="authenticationSchemes">The authentication schemes to challenge.</param>
        /// <returns>The created <see cref="ForbidResult"/> for the response.</returns>
        [NonAction]
        public virtual ForbidResult Forbid(AuthenticationProperties properties, params string[] authenticationSchemes)
            => new ForbidResult(authenticationSchemes, properties);

        /// <summary>
        /// Creates a <see cref="SignInResult"/> with the specified authentication scheme.
        /// </summary>
        /// <param name="principal">The <see cref="ClaimsPrincipal"/> containing the user claims.</param>
        /// <param name="authenticationScheme">The authentication scheme to use for the sign-in operation.</param>
        /// <returns>The created <see cref="SignInResult"/> for the response.</returns>
        [NonAction]
        public virtual SignInResult SignIn(ClaimsPrincipal principal, string authenticationScheme)
            => new SignInResult(authenticationScheme, principal);

        /// <summary>
        /// Creates a <see cref="SignInResult"/> with the specified specified authentication scheme and
        /// <paramref name="properties" />.
        /// </summary>
        /// <param name="principal">The <see cref="ClaimsPrincipal"/> containing the user claims.</param>
        /// <param name="properties"><see cref="AuthenticationProperties"/> used to perform the sign-in operation.</param>
        /// <param name="authenticationScheme">The authentication scheme to use for the sign-in operation.</param>
        /// <returns>The created <see cref="SignInResult"/> for the response.</returns>
        [NonAction]
        public virtual SignInResult SignIn(
            ClaimsPrincipal principal,
            AuthenticationProperties properties,
            string authenticationScheme)
            => new SignInResult(authenticationScheme, principal, properties);

        /// <summary>
        /// Creates a <see cref="SignOutResult"/> with the specified authentication schemes.
        /// </summary>
        /// <param name="authenticationSchemes">The authentication schemes to use for the sign-out operation.</param>
        /// <returns>The created <see cref="SignOutResult"/> for the response.</returns>
        [NonAction]
        public virtual SignOutResult SignOut(params string[] authenticationSchemes)
            => new SignOutResult(authenticationSchemes);

        /// <summary>
        /// Creates a <see cref="SignOutResult"/> with the specified specified authentication schemes and
        /// <paramref name="properties" />.
        /// </summary>
        /// <param name="properties"><see cref="AuthenticationProperties"/> used to perform the sign-out operation.</param>
        /// <param name="authenticationSchemes">The authentication scheme to use for the sign-out operation.</param>
        /// <returns>The created <see cref="SignOutResult"/> for the response.</returns>
        [NonAction]
        public virtual SignOutResult SignOut(AuthenticationProperties properties, params string[] authenticationSchemes)
            => new SignOutResult(authenticationSchemes, properties);

        /// <summary>
        /// Updates the specified <paramref name="model"/> instance using values from the controller's current
        /// <see cref="IValueProvider"/>.
        /// </summary>
        /// <typeparam name="TModel">The type of the model object.</typeparam>
        /// <param name="model">The model instance to update.</param>
        /// <returns>A <see cref="Task"/> that on completion returns <c>true</c> if the update is successful.</returns>
        [NonAction]
        public virtual Task<bool> TryUpdateModelAsync<TModel>(
            TModel model)
            where TModel : class
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            return TryUpdateModelAsync(model, prefix: string.Empty);
        }

        /// <summary>
        /// Updates the specified <paramref name="model"/> instance using values from the controller's current
        /// <see cref="IValueProvider"/> and a <paramref name="prefix"/>.
        /// </summary>
        /// <typeparam name="TModel">The type of the model object.</typeparam>
        /// <param name="model">The model instance to update.</param>
        /// <param name="prefix">The prefix to use when looking up values in the current <see cref="IValueProvider"/>.
        /// </param>
        /// <returns>A <see cref="Task"/> that on completion returns <c>true</c> if the update is successful.</returns>
        [NonAction]
        public virtual Task<bool> TryUpdateModelAsync<TModel>(
            TModel model,
            string prefix)
            where TModel : class
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (prefix == null)
            {
                throw new ArgumentNullException(nameof(prefix));
            }

            return TryUpdateModelAsync(model, prefix, new CompositeValueProvider(ControllerContext.ValueProviders));
        }

        /// <summary>
        /// Updates the specified <paramref name="model"/> instance using the <paramref name="valueProvider"/> and a
        /// <paramref name="prefix"/>.
        /// </summary>
        /// <typeparam name="TModel">The type of the model object.</typeparam>
        /// <param name="model">The model instance to update.</param>
        /// <param name="prefix">The prefix to use when looking up values in the <paramref name="valueProvider"/>.
        /// </param>
        /// <param name="valueProvider">The <see cref="IValueProvider"/> used for looking up values.</param>
        /// <returns>A <see cref="Task"/> that on completion returns <c>true</c> if the update is successful.</returns>
        [NonAction]
        public virtual Task<bool> TryUpdateModelAsync<TModel>(
            TModel model,
            string prefix,
            IValueProvider valueProvider)
            where TModel : class
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (prefix == null)
            {
                throw new ArgumentNullException(nameof(prefix));
            }

            if (valueProvider == null)
            {
                throw new ArgumentNullException(nameof(valueProvider));
            }

            return ModelBindingHelper.TryUpdateModelAsync(
                model,
                prefix,
                ControllerContext,
                MetadataProvider,
                ModelBinderFactory,
                valueProvider,
                ObjectValidator);
        }

        /// <summary>
        /// Updates the specified <paramref name="model"/> instance using values from the controller's current
        /// <see cref="IValueProvider"/> and a <paramref name="prefix"/>.
        /// </summary>
        /// <typeparam name="TModel">The type of the model object.</typeparam>
        /// <param name="model">The model instance to update.</param>
        /// <param name="prefix">The prefix to use when looking up values in the current <see cref="IValueProvider"/>.
        /// </param>
        /// <param name="includeExpressions"> <see cref="Expression"/>(s) which represent top-level properties
        /// which need to be included for the current model.</param>
        /// <returns>A <see cref="Task"/> that on completion returns <c>true</c> if the update is successful.</returns>
        [NonAction]
        public Task<bool> TryUpdateModelAsync<TModel>(
            TModel model,
            string prefix,
            params Expression<Func<TModel, object>>[] includeExpressions)
           where TModel : class
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (includeExpressions == null)
            {
                throw new ArgumentNullException(nameof(includeExpressions));
            }

            return ModelBindingHelper.TryUpdateModelAsync(
                model,
                prefix,
                ControllerContext,
                MetadataProvider,
                ModelBinderFactory,
                new CompositeValueProvider(ControllerContext.ValueProviders),
                ObjectValidator,
                includeExpressions);
        }

        /// <summary>
        /// Updates the specified <paramref name="model"/> instance using values from the controller's current
        /// <see cref="IValueProvider"/> and a <paramref name="prefix"/>.
        /// </summary>
        /// <typeparam name="TModel">The type of the model object.</typeparam>
        /// <param name="model">The model instance to update.</param>
        /// <param name="prefix">The prefix to use when looking up values in the current <see cref="IValueProvider"/>.
        /// </param>
        /// <param name="propertyFilter">A predicate which can be used to filter properties at runtime.</param>
        /// <returns>A <see cref="Task"/> that on completion returns <c>true</c> if the update is successful.</returns>
        [NonAction]
        public Task<bool> TryUpdateModelAsync<TModel>(
            TModel model,
            string prefix,
            Func<ModelMetadata, bool> propertyFilter)
            where TModel : class
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (propertyFilter == null)
            {
                throw new ArgumentNullException(nameof(propertyFilter));
            }

            return ModelBindingHelper.TryUpdateModelAsync(
                model,
                prefix,
                ControllerContext,
                MetadataProvider,
                ModelBinderFactory,
                new CompositeValueProvider(ControllerContext.ValueProviders),
                ObjectValidator,
                propertyFilter);
        }

        /// <summary>
        /// Updates the specified <paramref name="model"/> instance using the <paramref name="valueProvider"/> and a
        /// <paramref name="prefix"/>.
        /// </summary>
        /// <typeparam name="TModel">The type of the model object.</typeparam>
        /// <param name="model">The model instance to update.</param>
        /// <param name="prefix">The prefix to use when looking up values in the <paramref name="valueProvider"/>.
        /// </param>
        /// <param name="valueProvider">The <see cref="IValueProvider"/> used for looking up values.</param>
        /// <param name="includeExpressions"> <see cref="Expression"/>(s) which represent top-level properties
        /// which need to be included for the current model.</param>
        /// <returns>A <see cref="Task"/> that on completion returns <c>true</c> if the update is successful.</returns>
        [NonAction]
        public Task<bool> TryUpdateModelAsync<TModel>(
            TModel model,
            string prefix,
            IValueProvider valueProvider,
            params Expression<Func<TModel, object>>[] includeExpressions)
           where TModel : class
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (valueProvider == null)
            {
                throw new ArgumentNullException(nameof(valueProvider));
            }

            if (includeExpressions == null)
            {
                throw new ArgumentNullException(nameof(includeExpressions));
            }

            return ModelBindingHelper.TryUpdateModelAsync(
                model,
                prefix,
                ControllerContext,
                MetadataProvider,
                ModelBinderFactory,
                valueProvider,
                ObjectValidator,
                includeExpressions);
        }

        /// <summary>
        /// Updates the specified <paramref name="model"/> instance using the <paramref name="valueProvider"/> and a
        /// <paramref name="prefix"/>.
        /// </summary>
        /// <typeparam name="TModel">The type of the model object.</typeparam>
        /// <param name="model">The model instance to update.</param>
        /// <param name="prefix">The prefix to use when looking up values in the <paramref name="valueProvider"/>.
        /// </param>
        /// <param name="valueProvider">The <see cref="IValueProvider"/> used for looking up values.</param>
        /// <param name="propertyFilter">A predicate which can be used to filter properties at runtime.</param>
        /// <returns>A <see cref="Task"/> that on completion returns <c>true</c> if the update is successful.</returns>
        [NonAction]
        public Task<bool> TryUpdateModelAsync<TModel>(
            TModel model,
            string prefix,
            IValueProvider valueProvider,
            Func<ModelMetadata, bool> propertyFilter)
            where TModel : class
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (valueProvider == null)
            {
                throw new ArgumentNullException(nameof(valueProvider));
            }

            if (propertyFilter == null)
            {
                throw new ArgumentNullException(nameof(propertyFilter));
            }

            return ModelBindingHelper.TryUpdateModelAsync(
                model,
                prefix,
                ControllerContext,
                MetadataProvider,
                ModelBinderFactory,
                valueProvider,
                ObjectValidator,
                propertyFilter);
        }

        /// <summary>
        /// Updates the specified <paramref name="model"/> instance using values from the controller's current
        /// <see cref="IValueProvider"/> and a <paramref name="prefix"/>.
        /// </summary>
        /// <param name="model">The model instance to update.</param>
        /// <param name="modelType">The type of model instance to update.</param>
        /// <param name="prefix">The prefix to use when looking up values in the current <see cref="IValueProvider"/>.
        /// </param>
        /// <returns>A <see cref="Task"/> that on completion returns <c>true</c> if the update is successful.</returns>
        [NonAction]
        public virtual Task<bool> TryUpdateModelAsync(
            object model,
            Type modelType,
            string prefix)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (modelType == null)
            {
                throw new ArgumentNullException(nameof(modelType));
            }

            return ModelBindingHelper.TryUpdateModelAsync(
                model,
                modelType,
                prefix,
                ControllerContext,
                MetadataProvider,
                ModelBinderFactory,
                new CompositeValueProvider(ControllerContext.ValueProviders),
                ObjectValidator);
        }

        /// <summary>
        /// Updates the specified <paramref name="model"/> instance using the <paramref name="valueProvider"/> and a
        /// <paramref name="prefix"/>.
        /// </summary>
        /// <param name="model">The model instance to update.</param>
        /// <param name="modelType">The type of model instance to update.</param>
        /// <param name="prefix">The prefix to use when looking up values in the <paramref name="valueProvider"/>.
        /// </param>
        /// <param name="valueProvider">The <see cref="IValueProvider"/> used for looking up values.</param>
        /// <param name="propertyFilter">A predicate which can be used to filter properties at runtime.</param>
        /// <returns>A <see cref="Task"/> that on completion returns <c>true</c> if the update is successful.</returns>
        [NonAction]
        public Task<bool> TryUpdateModelAsync(
            object model,
            Type modelType,
            string prefix,
            IValueProvider valueProvider,
            Func<ModelMetadata, bool> propertyFilter)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (modelType == null)
            {
                throw new ArgumentNullException(nameof(modelType));
            }

            if (valueProvider == null)
            {
                throw new ArgumentNullException(nameof(valueProvider));
            }

            if (propertyFilter == null)
            {
                throw new ArgumentNullException(nameof(propertyFilter));
            }

            return ModelBindingHelper.TryUpdateModelAsync(
                model,
                modelType,
                prefix,
                ControllerContext,
                MetadataProvider,
                ModelBinderFactory,
                valueProvider,
                ObjectValidator,
                propertyFilter);
        }

        /// <summary>
        /// Validates the specified <paramref name="model"/> instance.
        /// </summary>
        /// <param name="model">The model to validate.</param>
        /// <returns><c>true</c> if the <see cref="ModelState"/> is valid; <c>false</c> otherwise.</returns>
        [NonAction]
        public virtual bool TryValidateModel(
            object model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            return TryValidateModel(model, prefix: null);
        }

        /// <summary>
        /// Validates the specified <paramref name="model"/> instance.
        /// </summary>
        /// <param name="model">The model to validate.</param>
        /// <param name="prefix">The key to use when looking up information in <see cref="ModelState"/>.
        /// </param>
        /// <returns><c>true</c> if the <see cref="ModelState"/> is valid;<c>false</c> otherwise.</returns>
        [NonAction]
        public virtual bool TryValidateModel(
            object model,
            string prefix)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            ObjectValidator.Validate(
                ControllerContext,
                validationState: null
                prefix: prefix ?? string.Empty,
                model: model);
            return ModelState.IsValid;
        }
    }
}
