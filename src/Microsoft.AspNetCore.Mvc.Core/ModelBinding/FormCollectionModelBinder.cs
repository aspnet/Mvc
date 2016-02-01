// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Mvc.ModelBinding
{
    /// <summary>
    /// <see cref="IModelBinder"/> implementation to bind form values to <see cref="IFormCollection"/>.
    /// </summary>
    public class FormCollectionModelBinder : IModelBinder
    {
        private static Task _complete = Task.FromResult(0);

        /// <inheritdoc />
        public Task BindModelAsync(IModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }
            Debug.Assert(bindingContext.Result == null);

            // This method is optimized to use cached tasks when possible and avoid allocating
            // using Task.FromResult. If you need to make changes of this nature, profile
            // allocations afterwards and look for Task<ModelBindingResult>.

            if (bindingContext.ModelType != typeof(IFormCollection))
            {
                return _complete;
            }

            return BindModelCoreAsync(bindingContext);
        }

        private async Task BindModelCoreAsync(IModelBindingContext bindingContext)
        {
            object model;
            var request = bindingContext.OperationBindingContext.HttpContext.Request;
            if (request.HasFormContentType)
            {
                var form = await request.ReadFormAsync();
                model = form;
            }
            else
            {
                model = new EmptyFormCollection();
            }

            bindingContext.ValidationState.Add(model, new ValidationStateEntry() { SuppressValidation = true });
            bindingContext.Result = ModelBindingResult.Success(bindingContext.ModelName, model);
        }

        private class EmptyFormCollection : IFormCollection
        {
            public StringValues this[string key]
            {
                get
                {
                    return StringValues.Empty;
                }
            }

            public int Count
            {
                get
                {
                    return 0;
                }
            }

            public IFormFileCollection Files
            {
                get
                {
                    return new EmptyFormFileCollection();
                }
            }

            public ICollection<string> Keys
            {
                get
                {
                    return new List<string>();
                }
            }

            public bool ContainsKey(string key)
            {
                return false;
            }

            public string Get(string key)
            {
                return null;
            }

            public IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator()
            {
                return Enumerable.Empty<KeyValuePair<string, StringValues>>().GetEnumerator();
            }

            public IList<StringValues> GetValues(string key)
            {
                return null;
            }

            public bool TryGetValue(string key, out StringValues value)
            {
                value = default(StringValues);
                return false;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private class EmptyFormFileCollection : List<IFormFile>, IFormFileCollection
        {
            public IFormFile this[string name]
            {
                get
                {
                    return null;
                }
            }

            public IFormFile GetFile(string name)
            {
                return null;
            }

            IReadOnlyList<IFormFile> IFormFileCollection.GetFiles(string name)
            {
                return null;
            }
        }
    }
}