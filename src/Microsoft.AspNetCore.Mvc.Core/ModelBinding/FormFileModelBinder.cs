// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
#if DOTNET5_4
using System.Reflection;
#endif
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Mvc.ModelBinding
{
    /// <summary>
    /// <see cref="IModelBinder"/> implementation to bind posted files to <see cref="IFormFile"/>.
    /// </summary>
    public class FormFileModelBinder : IModelBinder
    {
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

            if (bindingContext.ModelType != typeof(IFormFile) &&
                !typeof(IEnumerable<IFormFile>).IsAssignableFrom(bindingContext.ModelType))
            {
                return ModelBindingResult.NoResultAsync;
            }

            return BindModelCoreAsync(bindingContext);
        }

        private async Task BindModelCoreAsync(IModelBindingContext bindingContext)
        {
            // If we're at the top level, then use the FieldName (paramter or property name).
            // This handles the fact that there will be nothing in the ValueProviders for this parameter
            // and so we'll do the right thing even though we 'fell-back' to the empty prefix.
            var modelName = bindingContext.IsTopLevelObject
                ? bindingContext.BinderModelName ?? bindingContext.FieldName
                : bindingContext.ModelName;

            object value;
            if (bindingContext.ModelType == typeof(IFormFile))
            {
                var postedFiles = await GetFormFilesAsync(modelName, bindingContext);
                value = postedFiles.FirstOrDefault();
            }
            else if (typeof(IEnumerable<IFormFile>).IsAssignableFrom(bindingContext.ModelType))
            {
                var postedFiles = await GetFormFilesAsync(modelName, bindingContext);
                value = ModelBindingHelper.ConvertValuesToCollectionType(bindingContext.ModelType, postedFiles);
            }
            else
            {
                // This binder does not support the requested type.
                Debug.Fail("We shouldn't be called without a matching type.");
                return;
            }

            if (value == null)
            {
                bindingContext.Result = ModelBindingResult.Failed(bindingContext.ModelName);
                return;
            }
            else
            {
                bindingContext.ValidationState.Add(value, new ValidationStateEntry()
                {
                    Key = modelName,
                    SuppressValidation = true
                });

                bindingContext.ModelState.SetModelValue(
                    modelName,
                    rawValue: null,
                    attemptedValue: null);

                bindingContext.Result = ModelBindingResult.Success(bindingContext.ModelName, value);
                return;
            }
        }

        private async Task<List<IFormFile>> GetFormFilesAsync(string modelName, IModelBindingContext bindingContext)
        {
            var request = bindingContext.OperationBindingContext.HttpContext.Request;
            var postedFiles = new List<IFormFile>();
            if (request.HasFormContentType)
            {
                var form = await request.ReadFormAsync();

                foreach (var file in form.Files)
                {
                    ContentDispositionHeaderValue parsedContentDisposition;
                    ContentDispositionHeaderValue.TryParse(file.ContentDisposition, out parsedContentDisposition);

                    // If there is an <input type="file" ... /> in the form and is left blank.
                    if (parsedContentDisposition == null ||
                        (file.Length == 0 &&
                         string.IsNullOrEmpty(HeaderUtilities.RemoveQuotes(parsedContentDisposition.FileName))))
                    {
                        continue;
                    }

                    var fileName = HeaderUtilities.RemoveQuotes(parsedContentDisposition.Name);
                    if (fileName.Equals(modelName, StringComparison.OrdinalIgnoreCase))
                    {
                        postedFiles.Add(file);
                    }
                }
            }

            return postedFiles;
        }
    }
}