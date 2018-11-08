// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Core;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// A filter that specifies the expected <see cref="System.Type"/> the action will return and the supported
    /// response content types. The <see cref="ContentTypes"/> value is used to set
    /// <see cref="ObjectResult.ContentTypes"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ProducesAttribute : Attribute, IResultFilter, IOrderedFilter, IApiResponseMetadataProvider
    {
        /// <summary>
        /// Initializes an instance of <see cref="ProducesAttribute"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> of object that is going to be written in the response.</param>
        public ProducesAttribute(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            Type = type;
            ContentTypes = new MediaTypeCollection();
        }

        /// <summary>
        /// Initializes an instance of <see cref="ProducesAttribute"/> with allowed content types.
        /// </summary>
        /// <param name="contentType">The allowed content type for a response.</param>
        /// <param name="additionalContentTypes">Additional allowed content types for a response.</param>
        public ProducesAttribute(string contentType, params string[] additionalContentTypes)
        {
            if (contentType == null)
            {
                throw new ArgumentNullException(nameof(contentType));
            }

            // We want to ensure that the given provided content types are valid values, so
            // we validate them using the semantics of MediaTypeHeaderValue.
            MediaTypeHeaderValue.Parse(contentType);

            for (var i = 0; i < additionalContentTypes.Length; i++)
            {
                MediaTypeHeaderValue.Parse(additionalContentTypes[i]);
            }

            ContentTypes = GetContentTypes(contentType, additionalContentTypes);
        }

        /// <inheritdoc />
        public Type Type { get; set; }

        /// <summary>
        /// Gets or sets the supported response content types. Used to set <see cref="ObjectResult.ContentTypes"/>.
        /// </summary>
        public MediaTypeCollection ContentTypes { get; set; }

        /// <inheritdoc />
        public int StatusCode => StatusCodes.Status200OK;

        /// <inheritdoc />
        public int Order { get; set; }

        /// <inheritdoc />
        public virtual void OnResultExecuting(ResultExecutingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Result is ObjectResult objectResult)
            {
                // Check if there are any IFormatFilter in the pipeline, and if any of them is active. If there is one,
                // do not override the content type value.
                for (var i = 0; i < context.Filters.Count; i++)
                {
                    var filter = context.Filters[i] as IFormatFilter;

                    if (filter?.GetFormat(context) != null)
                    {
                        return;
                    }
                }

                SetContentTypes(objectResult.ContentTypes);
            }
        }

        /// <inheritdoc />
        public virtual void OnResultExecuted(ResultExecutedContext context)
        {
        }

        /// <inheritdoc />
        public void SetContentTypes(MediaTypeCollection contentTypes)
        {
            contentTypes.Clear();
            foreach (var contentType in ContentTypes)
            {
                contentTypes.Add(contentType);
            }
        }

        private MediaTypeCollection GetContentTypes(string firstArg, string[] args)
        {
            var completeArgs = new List<string>();
            completeArgs.Add(firstArg);
            completeArgs.AddRange(args);
            var contentTypes = new MediaTypeCollection();
            foreach (var arg in completeArgs)
            {
                var contentType = new MediaType(arg);
                if (contentType.HasWildcard)
                {
                    throw new InvalidOperationException(
                        Resources.FormatMatchAllContentTypeIsNotAllowed(arg));
                }

                contentTypes.Add(arg);
            }

            return contentTypes;
        }
    }
}
