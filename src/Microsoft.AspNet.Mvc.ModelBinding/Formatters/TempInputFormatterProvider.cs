﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNet.Mvc.ModelBinding.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class TempInputFormatterProvider : IInputFormatterProvider
    {
        private readonly IInputFormatter[] _formatters;

        public TempInputFormatterProvider(IEnumerable<IInputFormatter> formatters)
        {
            _formatters = formatters.ToArray();
        }

        public IInputFormatter GetInputFormatter(InputFormatterProviderContext context)
        {
            var request = context.HttpContext.Request;
            var contentType = request.GetContentType();
            if (contentType == null)
            {
                // TODO: http exception?
                throw new InvalidOperationException("400: Bad Request");
            }

            for (var i = 0; i < _formatters.Length; i++)
            {
                var formatter = _formatters[i];
                if (formatter.SupportedMediaTypes.Contains(contentType.ContentType, StringComparer.OrdinalIgnoreCase))
                {
                    return formatter;
                }
            }

            // TODO: Http exception
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "415: Unsupported content type {0}", contentType));
        }
    }
}
