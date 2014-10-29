// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.HeaderValueAbstractions;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// An action result which formats the given object as JSON.
    /// </summary>
    public class JsonResult : ActionResult
    {
        private static readonly MediaTypeHeaderValue[] _defaultSupportedContentTypes = new MediaTypeHeaderValue[]
        {
            MediaTypeHeaderValue.Parse("application/json"),
            MediaTypeHeaderValue.Parse("text/json"),
        };

        /// <summary>
        /// Creates a new <see cref="JsonResult"/> with the given <paramref name="data"/>.
        /// </summary>
        /// <param name="value">The value to format as JSON.</param>
        public JsonResult(object value)
            : this(value, null)
        {
        }

        /// <summary>
        /// Creates a new <see cref="JsonResult"/> with the given <paramref name="data"/>.
        /// </summary>
        /// <param name="value">The value to format as JSON.</param>
        /// <param name="formatter">The formatter to use, or <c>null</c> to choose a formatter dynamically.</param>
        public JsonResult(object value, IOutputFormatter formatter)
        {
            Value = value;
            Formatter = formatter;

            ContentTypes = new List<MediaTypeHeaderValue>();
        }

        /// <summary>
        /// Gets or sets the list of supported Content-Types.
        /// </summary>
        public IList<MediaTypeHeaderValue> ContentTypes { get; set; }

        /// <summary>
        /// Gets or sets the formatter.
        /// </summary>
        public IOutputFormatter Formatter { get; set; }

        /// <summary>
        /// Gets or sets the value to be formatted.
        /// </summary>
        public object Value { get; set; }

        /// <inheritdoc />
        public override async Task ExecuteResultAsync([NotNull] ActionContext context)
        {
            var objectResult = new ObjectResult(Value);

            // Set the content type explicitly to application/json and text/json.
            // if the user has not already set it.
            if (ContentTypes == null || ContentTypes.Count == 0)
            {
                objectResult.ContentTypes = _defaultSupportedContentTypes;
            }
            else
            {
                objectResult.ContentTypes = ContentTypes;
            }

            var formatterContext = new OutputFormatterContext()
            {
                ActionContext = context,
                Object = Value,
            };

            var formatter = SelectFormatter(objectResult, formatterContext);
            await formatter.WriteAsync(formatterContext);
        }

        private IOutputFormatter SelectFormatter(ObjectResult objectResult, OutputFormatterContext formatterContext)
        {
            if (Formatter == null)
            {
                // If no formatter was provided, then run Conneg with the formatters configured in options.
                var formatters = formatterContext
                    .ActionContext
                    .HttpContext
                    .RequestServices
                    .GetRequiredService<IOutputFormattersProvider>()
                    .OutputFormatters;

                var formatter = objectResult.SelectFormatter(formatterContext, formatters);
                if (formatter == null)
                {
                    // If the available user-configured formatters can't write this type, then fall back to the
                    // 'global' one.
                    formatter = formatterContext
                        .ActionContext
                        .HttpContext
                        .RequestServices
                        .GetRequiredService<JsonOutputFormatter>();

                    // Run SelectFormatter again to try to choose a content type that this formatter can do.
                    objectResult.SelectFormatter(formatterContext, new[] { formatter });
                }

                return formatter;
            }
            else
            {
                // Run SelectFormatter to try to choose a content type that this formatter can do.
                objectResult.SelectFormatter(formatterContext, new[] { Formatter });
                return Formatter;
            }
        }
    }
}
