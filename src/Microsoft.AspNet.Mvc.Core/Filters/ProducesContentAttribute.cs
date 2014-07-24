// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Mvc.HeaderValueAbstractions;
using System.Linq;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Specifies the allowed content types which can be used to select a formatter
    /// while executing <see cref="ObjectResult"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ProducesContentAttribute : ResultFilterAttribute
    {
        private List<MediaTypeHeaderValue> _contentTypes;

        public ProducesContentAttribute(string contentType, params string[] additionalContentTypes)
        {
            _contentTypes = GetContentTypes(contentType, additionalContentTypes);
        }

        public override void OnResultExecuting([NotNull]ResultExecutingContext context)
        {
            base.OnResultExecuting(context);
            var objectResult = context.Result as ObjectResult;

            // Do not override if there is a value that is already present. 
            // This is because filters are executed in the order of 
            // Action->Controller->Global, if something has been set by a lower filter
            // We should not override. 
            if (objectResult != null && 
                (objectResult.ContentTypes == null || objectResult.ContentTypes.Count == 0))
            {
                objectResult.ContentTypes = _contentTypes;
            }
        }

        private List<MediaTypeHeaderValue> GetContentTypes(string firstArg, IEnumerable<string> args)
        {
            var contentTypes = new List<MediaTypeHeaderValue>();
            contentTypes.Add(MediaTypeHeaderValue.Parse(firstArg));
            foreach (var item in args)
            {
                var contentType = MediaTypeHeaderValue.Parse(item);
                contentTypes.Add(contentType);
            }

            return contentTypes;
        }
    }
}
