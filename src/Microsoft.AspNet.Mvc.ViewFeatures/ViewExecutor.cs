﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Framework.Internal;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Utility type for rendering a <see cref="IView"/> to the response.
    /// </summary>
    public static class ViewExecutor
    {
        private const int BufferSize = 1024;
        private static readonly MediaTypeHeaderValue DefaultContentType = new MediaTypeHeaderValue("text/html")
        {
            Encoding = Encoding.UTF8
        };

        /// <summary>
        /// Asynchronously renders the specified <paramref name="view"/> to the response body.
        /// </summary>
        /// <param name="view">The <see cref="IView"/> to render.</param>
        /// <param name="actionContext">The <see cref="ActionContext"/> for the current executing action.</param>
        /// <param name="viewData">The <see cref="ViewDataDictionary"/> for the view being rendered.</param>
        /// <param name="tempData">The <see cref="ITempDataDictionary"/> for the view being rendered.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous rendering.</returns>
        public static async Task ExecuteAsync([NotNull] IView view,
                                              [NotNull] ActionContext actionContext,
                                              [NotNull] ViewDataDictionary viewData,
                                              [NotNull] ITempDataDictionary tempData,
                                              [NotNull] HtmlHelperOptions htmlHelperOptions,
                                              MediaTypeHeaderValue contentType)
        {
            var response = actionContext.HttpContext.Response;

            var contentTypeHeader = contentType;
            Encoding encoding;
            if (contentTypeHeader == null)
            {
                contentTypeHeader = DefaultContentType;
                encoding = Encoding.UTF8;
            }
            else
            {
                if (contentTypeHeader.Encoding == null)
                {
                    // Do not modify the user supplied content type, so copy it instead
                    contentTypeHeader = contentTypeHeader.Copy();
                    contentTypeHeader.Encoding = Encoding.UTF8;

                    encoding = Encoding.UTF8;
                }
                else
                {
                    encoding = contentTypeHeader.Encoding;
                }
            }

            response.ContentType = contentTypeHeader.ToString();

            using (var writer = new HttpResponseStreamWriter(response.Body, encoding))
            {
                var viewContext = new ViewContext(
                    actionContext,
                    view,
                    viewData,
                    tempData,
                    writer,
                    htmlHelperOptions);

                await view.RenderAsync(viewContext);            }
        }
    }
}