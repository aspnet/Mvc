// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.HeaderValueAbstractions;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.Core;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Writes an object to the output stream.
    /// </summary>
    public abstract class OutputFormatter
    {
        public List<Encoding> SupportedEncodings { get; private set; }
        public List<MediaTypeHeaderValue> SupportedMediaTypes { get; private set; }

        protected OutputFormatter()
        {
            SupportedEncodings = new List<Encoding>();
            SupportedMediaTypes = new List<MediaTypeHeaderValue>();
        }

        public virtual Encoding SelectCharacterEncoding(MediaTypeHeaderValue contentTypeHeader)
        {
            // Performance-sensitive
            Encoding encoding = null;
            if (contentTypeHeader != null)
            {
                // Find encoding based on content type charset parameter
                string charset = contentTypeHeader.Charset;
                if (!String.IsNullOrWhiteSpace(charset))
                {
                    foreach (var supportedEncoding in SupportedEncodings)
                    {
                        if (charset.Equals(supportedEncoding.WebName, StringComparison.OrdinalIgnoreCase))
                        {
                            encoding = supportedEncoding;
                            break;
                        }
                    }
                }
            }

            if (encoding == null)
            {
                // We didn't find a character encoding match based on the content headers.
                // Instead we try getting the default character encoding.
                if (SupportedEncodings.Count > 0)
                {
                    encoding = SupportedEncodings[0];
                }
            }

            if (encoding == null)
            {
                // No supported encoding was found so there is no way for us to start reading or writing.
                throw new InvalidOperationException(Resources.FormatMediaTypeFormatterNoEncoding(GetType().Name));
            }

            return encoding;
        }

        public abstract bool CanWriteResult(ObjectResult result,
                                            Type declaredType,
                                            HttpContext context);

        public abstract Task WriteAsync(object value,
                                        Type declaredType,
                                        HttpContext context,
                                        CancellationToken cancellationToken);
    }
}
