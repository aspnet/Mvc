// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using Microsoft.AspNet.Mvc.Core;
using Newtonsoft.Json;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.Rendering
{
    /// <summary>
    /// Default implementation of <see cref="IJsonHelper"/>.
    /// </summary>
    public class JsonHelper : IJsonHelper, ICanHasViewContext
    {
        private ViewContext _viewContext;
        private JsonOutputFormatter _jsonOutputFormatter;

        /// <inheritdoc />
        public ViewContext ViewContext
        {
            get
            {
                if (_viewContext == null)
                {
                    throw new InvalidOperationException(Resources.HtmlHelper_NotContextualized);
                }

                return _viewContext;
            }
            private set
            {
                _viewContext = value;
            }
        }

        private JsonOutputFormatter GetJsonOutputFormatter()
        {
            if (_jsonOutputFormatter == null)
            {
                var services = ViewContext.HttpContext.RequestServices;
                _jsonOutputFormatter = services.GetRequiredService<JsonOutputFormatter>();
            }

            return _jsonOutputFormatter;
        }

        /// <inheritdoc />
        public HtmlString Serialize(object value)
        {
            var jsonOutputFormatter = GetJsonOutputFormatter();

            return SerializeInternal(jsonOutputFormatter, value);
        }

        /// <inheritdoc />
        public HtmlString Serialize(object value, JsonSerializerSettings serializerSettings)
        {
            JsonOutputFormatter jsonOutputFormatter = new JsonOutputFormatter
            {
                SerializerSettings = serializerSettings
            };

            return SerializeInternal(jsonOutputFormatter, value);
        }

        private HtmlString SerializeInternal(JsonOutputFormatter jsonOutputFormatter, object value)
        {
            StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
            jsonOutputFormatter.WriteObject(stringWriter, value);

            return new HtmlString(stringWriter.ToString());
        }

        /// <inheritdoc />
        public virtual void Contextualize([NotNull] ViewContext viewContext)
        {
            ViewContext = viewContext;
        }
    }
}