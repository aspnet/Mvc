// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
#if NETSTANDARD1_3
using System.Reflection;
#endif
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Internal;

namespace Microsoft.AspNetCore.Mvc.ModelBinding
{
    /// <summary>
    /// An <see cref="IModelBinder"/> which binds models from the request headers when a model
    /// has the binding source <see cref="BindingSource.Header"/>/
    /// </summary>
    public class HeaderModelBinder : IModelBinder
    {
        /// <inheritdoc />
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            // This method is optimized to use cached tasks when possible and avoid allocating
            // using Task.FromResult. If you need to make changes of this nature, profile
            // allocations afterwards and look for Task<ModelBindingResult>.

            var allowedBindingSource = bindingContext.BindingSource;
            if (allowedBindingSource == null ||
                !allowedBindingSource.CanAcceptDataFrom(BindingSource.Header))
            {
                // Headers are opt-in. This model either didn't specify [FromHeader] or specified something
                // incompatible so let other binders run.
                return TaskCache.CompletedTask;
            }

            var request = bindingContext.OperationBindingContext.HttpContext.Request;
            var modelMetadata = bindingContext.ModelMetadata;

            // Property name can be null if the model metadata represents a type (rather than a property or parameter).
            var headerName = bindingContext.FieldName;

            object model;
            if (bindingContext.ModelType == typeof(string))
            {
                string value = request.Headers[headerName];
                model = value;
            }
            else if (typeof(IEnumerable<string>).IsAssignableFrom(bindingContext.ModelType))
            {
                if (bindingContext.ModelMetadata.IsReadOnly &&
                    (bindingContext.ModelType == typeof(string[]) || bindingContext.Model == null))
                {
                    // Silently fail and stop other model binders running if a new collection is needed (perhaps
                    // because target type is an array) but can't assign it to the property.
                    model = null;
                }
                else
                {
                    var values = request.Headers.GetCommaSeparatedValues(headerName);
                    model = GetCompatibleCollection(bindingContext, values);
                }
            }
            else
            {
                // An unsupported datatype.
                model = null;
            }

            if (model == null)
            {
                // Silently fail if unable to create an instance or use the current instance. Also reach here in the
                // typeof(string) case if the header does not exist in the request and in the
                // typeof(IEnumerable<string>) case if the header does not exist and this is not a top-level object.
                bindingContext.Result = ModelBindingResult.Failed(bindingContext.ModelName);
            }
            else
            {
                bindingContext.ModelState.SetModelValue(
                    bindingContext.ModelName,
                    request.Headers.GetCommaSeparatedValues(headerName),
                    request.Headers[headerName]);

                bindingContext.Result = ModelBindingResult.Success(bindingContext.ModelName, model);
            }

            return TaskCache.CompletedTask;
        }

        private static object GetCompatibleCollection(ModelBindingContext bindingContext, string[] values)
        {
            // Almost-always success if IsTopLevelObject.
            if (!bindingContext.IsTopLevelObject && values.Length == 0)
            {
                return null;
            }

            if (bindingContext.ModelType.IsAssignableFrom(typeof(string[])))
            {
                // Array we already have is compatible.
                return values;
            }

            ICollection<string> collection;
            if (bindingContext.Model == null)
            {
                // Note this call may return null if the model type cannot be activated.
                collection = ModelBindingHelper.CreateCompatibleCollection<string>(
                    bindingContext.ModelType,
                    values.Length);
            }
            else
            {
                // Note this cast may fail if the runtime model implements IEnumerable<IFormFile> but not
                // ICollection<IFormFile>. Give up in then: Assuming we're not in an odd corner case where
                // the property is settable and its declared type is assignable from List<IFormFile>.
                collection = bindingContext.Model as ICollection<string>;
                collection?.Clear();
            }

            if (collection == null)
            {
                return null;
            }

            for (int i = 0; i < values.Length; i++)
            {
                collection.Add(values[i]);
            }

            return collection;
        }
    }
}