// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Core;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders
{
    /// <summary>
    /// An <see cref="IModelBinder"/> which binds models from the request body using an <see cref="IInputFormatter"/>
    /// when a model has the binding source <see cref="BindingSource.Body"/>.
    /// </summary>
    public class BodyModelBinder : IModelBinder
    {
        private readonly IList<IInputFormatter> _formatters;
        private readonly Func<Stream, Encoding, TextReader> _readerFactory;
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new <see cref="BodyModelBinder"/>.
        /// </summary>
        /// <param name="formatters">The list of <see cref="IInputFormatter"/>.</param>
        /// <param name="readerFactory">
        /// The <see cref="IHttpRequestStreamReaderFactory"/>, used to create <see cref="System.IO.TextReader"/>
        /// instances for reading the request body.
        /// </param>
        public BodyModelBinder(IList<IInputFormatter> formatters, IHttpRequestStreamReaderFactory readerFactory)
            : this(formatters, readerFactory, null)
        {
        }

        /// <summary>
        /// Creates a new <see cref="BodyModelBinder"/>.
        /// </summary>
        /// <param name="formatters">The list of <see cref="IInputFormatter"/>.</param>
        /// <param name="readerFactory">
        /// The <see cref="IHttpRequestStreamReaderFactory"/>, used to create <see cref="System.IO.TextReader"/>
        /// instances for reading the request body.
        /// </param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public BodyModelBinder(IList<IInputFormatter> formatters, IHttpRequestStreamReaderFactory readerFactory, ILogger logger)
        {
            if (formatters == null)
            {
                throw new ArgumentNullException(nameof(formatters));
            }

            if (readerFactory == null)
            {
                throw new ArgumentNullException(nameof(readerFactory));
            }

            _formatters = formatters;
            _readerFactory = readerFactory.CreateReader;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            // Special logic for body, treat the model name as string.Empty for the top level
            // object, but allow an override via BinderModelName. The purpose of this is to try
            // and be similar to the behavior for POCOs bound via traditional model binding.
            string modelBindingKey;
            if (bindingContext.IsTopLevelObject)
            {
                modelBindingKey = bindingContext.BinderModelName ?? string.Empty;
            }
            else
            {
                modelBindingKey = bindingContext.ModelName;
            }

            var httpContext = bindingContext.HttpContext;

            var formatterContext = new InputFormatterContext(
                httpContext,
                modelBindingKey,
                bindingContext.ModelState,
                bindingContext.ModelMetadata,
                _readerFactory);

            var formatter = (IInputFormatter)null;
            _logger.IsEnabled(LogLevel.Debug);
            for (var i = 0; i < _formatters.Count; i++)
            {
                _logger.LogDebug("Checking formatter: '{0}'", _formatters[i]);
                if (_formatters[i].CanRead(formatterContext))
                {
                    formatter = _formatters[i];
                    //_logger.LogDebug("Selected formatter: '{0}'", formatter);
                    _logger.InputFormatterSelected(formatter,formatterContext);
                    break;
                }
            }

            if (formatter == null)
            {
                var message = Resources.FormatUnsupportedContentType(httpContext.Request.ContentType);

                // move to UnsupportedContentTypeFilter?
                //_logger.LogDebug("No formatter could be found to support the content type '{0}' for use with the [FromBody] attribute.",
                // httpContext.Request.ContentType);
                _logger.NoInputFormatter(formatterContext);
                //var isForm = httpContext.Request.HasFormContentType;
                //if (isForm)
                //{
                //    _logger.LogDebug("To use model binding, remove the [FromBody] attribute from the action method's parameter.");
                //}

                var exception = new UnsupportedContentTypeException(message);
                bindingContext.ModelState.AddModelError(modelBindingKey, exception, bindingContext.ModelMetadata);
                return;
            }

            try
            {
                var previousCount = bindingContext.ModelState.ErrorCount;
                var result = await formatter.ReadAsync(formatterContext);
                var model = result.Model;

                if (result.HasError)
                {
                    // Formatter encountered an error. Do not use the model it returned.
                    return;
                }

                bindingContext.Result = ModelBindingResult.Success(model);
                return;
            }
            catch (Exception ex)
            {
                bindingContext.ModelState.AddModelError(modelBindingKey, ex, bindingContext.ModelMetadata);
                return;
            }
        }
    }
}
