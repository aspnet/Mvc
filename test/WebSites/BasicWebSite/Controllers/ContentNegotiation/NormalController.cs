// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Buffers;
using BasicWebSite.Formatters;
using BasicWebSite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace BasicWebSite.Controllers.ContentNegotiation
{
    public class NormalController : Controller
    {
        private JsonOutputFormatter _indentingFormatter;

        public NormalController(IOptions<MvcJsonOptions> jsonOptions, ArrayPool<char> charPool)
        {
            var defaultSettings = jsonOptions.Value.SerializerSettings;
            var settings = new JsonSerializerSettings
            {
                Binder = defaultSettings.Binder,
                CheckAdditionalContent = defaultSettings.CheckAdditionalContent,
                ConstructorHandling = defaultSettings.ConstructorHandling,
                Context = defaultSettings.Context,
                ContractResolver = defaultSettings.ContractResolver,
                Converters = defaultSettings.Converters,
                Culture = defaultSettings.Culture,
                DateFormatHandling = defaultSettings.DateFormatHandling,
                DateFormatString = defaultSettings.DateFormatString,
                DateParseHandling = defaultSettings.DateParseHandling,
                DateTimeZoneHandling = defaultSettings.DateTimeZoneHandling,
                DefaultValueHandling = defaultSettings.DefaultValueHandling,
                EqualityComparer = defaultSettings.EqualityComparer,
                Error = defaultSettings.Error,
                FloatFormatHandling = defaultSettings.FloatFormatHandling,
                FloatParseHandling = defaultSettings.FloatParseHandling,
                // Just one change from the global defaults.
                Formatting = Formatting.Indented,
                MaxDepth = defaultSettings.MaxDepth,
                MetadataPropertyHandling = defaultSettings.MetadataPropertyHandling,
                MissingMemberHandling = defaultSettings.MissingMemberHandling,
                NullValueHandling = defaultSettings.NullValueHandling,
                ObjectCreationHandling = defaultSettings.ObjectCreationHandling,
                PreserveReferencesHandling = defaultSettings.PreserveReferencesHandling,
                ReferenceLoopHandling = defaultSettings.ReferenceLoopHandling,
                // ReferenceResolver property is obsolete; use only ReferenceResolverProvider.
                ReferenceResolverProvider = defaultSettings.ReferenceResolverProvider,
                StringEscapeHandling = defaultSettings.StringEscapeHandling,
                TraceWriter = defaultSettings.TraceWriter,
                TypeNameAssemblyFormat = defaultSettings.TypeNameAssemblyFormat,
                TypeNameHandling = defaultSettings.TypeNameHandling,
            };

            _indentingFormatter = new JsonOutputFormatter(settings, charPool);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var result = context.Result as ObjectResult;
            if (result != null)
            {
                result.Formatters.Add(new PlainTextFormatter());
                result.Formatters.Add(new CustomFormatter("application/custom"));
                result.Formatters.Add(_indentingFormatter);
            }

            base.OnActionExecuted(context);
        }

        public string ReturnClassName()
        {
            return "NormalController";
        }

        public User ReturnUser()
        {
            return CreateUser();
        }

        [Produces("application/NoFormatter")]
        public User ReturnUser_NoMatchingFormatter()
        {
            return CreateUser();
        }

        [Produces("application/custom", "application/json", "text/json")]
        public User MultipleAllowedContentTypes()
        {
            return CreateUser();
        }

        [Produces("application/custom")]
        public string WriteUserUsingCustomFormat()
        {
            return "Written using custom format.";
        }

        [NonAction]
        public User CreateUser()
        {
            User user = new User()
            {
                Name = "My name",
                Address = "My address",
            };

            return user;
        }
    }
}