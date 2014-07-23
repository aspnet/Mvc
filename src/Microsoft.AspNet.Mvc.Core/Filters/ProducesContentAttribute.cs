// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Mvc.HeaderValueAbstractions;

namespace Microsoft.AspNet.Mvc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ProducesContentAttribute : ResultFilterAttribute
    {
        private List<MediaTypeHeaderValue> _contentTypes;

        public ProducesContentAttribute(params string[] args)
        {
            _contentTypes = GetContentTypes(args);
        }

        public override void OnResultExecuting([NotNull]ResultExecutingContext context)
        {
            base.OnResultExecuting(context);
            var objectContentResult = context.Result as ObjectResult;
            if (objectContentResult != null)
            {
                objectContentResult.ContentTypes = _contentTypes;
            }
        }

        private List<MediaTypeHeaderValue> GetContentTypes(string[] args)
        {
            var contentTypes = new List<MediaTypeHeaderValue>();
            foreach (var item in args)
            {
                var contentType = MediaTypeHeaderValue.Parse(item);
                if(contentType == null)
                {
                    throw new ArgumentException(Resources.FormatProducesContentArgumentCannotBeParsed(item));
                }

                contentTypes.Add(contentType);
            }

            return contentTypes;
        }
    }
}
