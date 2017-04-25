// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Buffers;
#if NET451
using System.Configuration;
#endif
using System.Globalization;
using System.IO;
#if NET451
using System.Linq;
#endif
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures
{
    /// <summary>
    /// Default implementation of <see cref="IJsonHelper"/>.
    /// </summary>
    public class JsonHelper : IJsonHelper
    {
        private const string AllowJsonHtml = "Switch.Microsoft.AspNetCore.Mvc.AllowJsonHtml";
        private readonly JsonOutputFormatter _jsonOutputFormatter;
        private readonly ArrayPool<char> _charPool;

        /// <summary>
        /// Initializes a new instance of <see cref="JsonHelper"/> that is backed by <paramref name="jsonOutputFormatter"/>.
        /// </summary>
        /// <param name="jsonOutputFormatter">The <see cref="JsonOutputFormatter"/> used to serialize JSON.</param>
        /// <param name="charPool">
        /// The <see cref="ArrayPool{Char}"/> for use with custom <see cref="JsonSerializerSettings"/> (see
        /// <see cref="Serialize(object, JsonSerializerSettings)"/>).
        /// </param>
        public JsonHelper(JsonOutputFormatter jsonOutputFormatter, ArrayPool<char> charPool)
        {
            if (jsonOutputFormatter == null)
            {
                throw new ArgumentNullException(nameof(jsonOutputFormatter));
            }
            if (charPool == null)
            {
                throw new ArgumentNullException(nameof(charPool));
            }

            _jsonOutputFormatter = jsonOutputFormatter;
            _charPool = charPool;
        }

        /// <inheritdoc />
        public IHtmlContent Serialize(object value)
        {
            var allowJsonHtml = false;
#if NET451
            var switchValue = ConfigurationManager.AppSettings.GetValues(AllowJsonHtml)?.FirstOrDefault();
            bool.TryParse(switchValue, out allowJsonHtml);
#else
            AppContext.TryGetSwitch(AllowJsonHtml, out allowJsonHtml);
#endif

            if (allowJsonHtml)
            {
                return SerializeInternal(_jsonOutputFormatter, value);
            }

            var settings = ShallowCopy(_jsonOutputFormatter.PublicSerializerSettings);
            settings.StringEscapeHandling = StringEscapeHandling.EscapeHtml;

            return Serialize(value, settings);
        }

        /// <inheritdoc />
        public IHtmlContent Serialize(object value, JsonSerializerSettings serializerSettings)
        {
            if (serializerSettings == null)
            {
                throw new ArgumentNullException(nameof(serializerSettings));
            }

            var jsonOutputFormatter = new JsonOutputFormatter(serializerSettings, _charPool);

            return SerializeInternal(jsonOutputFormatter, value);
        }

        private IHtmlContent SerializeInternal(JsonOutputFormatter jsonOutputFormatter, object value)
        {
            var stringWriter = new StringWriter(CultureInfo.InvariantCulture);
            jsonOutputFormatter.WriteObject(stringWriter, value);

            return new HtmlString(stringWriter.ToString());
        }

        private static JsonSerializerSettings ShallowCopy(JsonSerializerSettings settings)
        {
            var copiedSettings = new JsonSerializerSettings
            {
                Binder = settings.Binder,
                CheckAdditionalContent = settings.CheckAdditionalContent,
                ConstructorHandling = settings.ConstructorHandling,
                Context = settings.Context,
                ContractResolver = settings.ContractResolver,
                Converters = settings.Converters,
                Culture = settings.Culture,
                DateFormatHandling = settings.DateFormatHandling,
                DateFormatString = settings.DateFormatString,
                DateParseHandling = settings.DateParseHandling,
                DateTimeZoneHandling = settings.DateTimeZoneHandling,
                DefaultValueHandling = settings.DefaultValueHandling,
                EqualityComparer = settings.EqualityComparer,
                Error = settings.Error,
                FloatFormatHandling = settings.FloatFormatHandling,
                FloatParseHandling = settings.FloatParseHandling,
                Formatting = settings.Formatting,
                MaxDepth = settings.MaxDepth,
                MetadataPropertyHandling = settings.MetadataPropertyHandling,
                MissingMemberHandling = settings.MissingMemberHandling,
                NullValueHandling = settings.NullValueHandling,
                ObjectCreationHandling = settings.ObjectCreationHandling,
                PreserveReferencesHandling = settings.PreserveReferencesHandling,
                ReferenceLoopHandling = settings.ReferenceLoopHandling,
                ReferenceResolverProvider = settings.ReferenceResolverProvider,
                StringEscapeHandling = settings.StringEscapeHandling,
                TraceWriter = settings.TraceWriter,
                TypeNameAssemblyFormat = settings.TypeNameAssemblyFormat,
                TypeNameHandling = settings.TypeNameHandling,
            };

            return copiedSettings;
        }
    }
}