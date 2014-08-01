// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNet.Mvc.HeaderValueAbstractions;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class TempInputFormatterProvider : IInputFormatterProvider
    {
        private IInputFormatter[] _formatters;

        public IInputFormatter GetInputFormatter(InputFormatterProviderContext context)
        {
            var request = context.HttpContext.Request;

            var formatters = _formatters;

            if (formatters == null)
            {
                formatters = context.HttpContext.RequestServices.GetService<IEnumerable<IInputFormatter>>()
                                    .ToArray();

                _formatters = formatters;
            }

            var contentType = MediaTypeHeaderValue.Parse(request.ContentType);
            if (contentType == null)
            {
                // TODO: http exception?
                throw new InvalidOperationException("400: Bad Request");
            }

            foreach (var formatter in formatters)
            {
                formatter.SupportedMediaTypes
                                  .FirstOrDefault(supportedMediaType => supportedMediaType.IsSubsetOf(contentType));
            }

            // TODO: Http exception
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, 
                                                              "415: Unsupported content type {0}", 
                                                              contentType.RawValue));
        }
    }
}
