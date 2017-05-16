// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders
{
    /// <summary>
    /// <see cref="IModelBinder"/> implementation to bind posted files to <see cref="IFormFile"/>.
    /// </summary>
    public class FormFileModelBinder : IModelBinder
    {
        /// <inheritdoc />
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var createFileCollection = bindingContext.ModelType == typeof(IFormFileCollection);
            if (!createFileCollection && !ModelBindingHelper.CanGetCompatibleCollection<IFormFile>(bindingContext))
            {
                // Silently fail if unable to create an instance or use the current instance.
                return;
            }

            ICollection<IFormFile> postedFiles;
            if (createFileCollection)
            {
                postedFiles = new List<IFormFile>();
            }
            else
            {
                postedFiles = ModelBindingHelper.GetCompatibleCollection<IFormFile>(bindingContext);
            }

            // If we're at the top level, then use the FieldName (parameter or property name).
            // This handles the fact that there will be nothing in the ValueProviders for this parameter
            // and so we'll do the right thing even though we 'fell-back' to the empty prefix.
            var modelName = bindingContext.IsTopLevelObject
                ? bindingContext.BinderModelName ?? bindingContext.FieldName
                : bindingContext.ModelName;

            await GetFormFilesAsync(modelName, bindingContext, postedFiles);

            object value;
            if (bindingContext.ModelType == typeof(IFormFile))
            {
                if (postedFiles.Count == 0)
                {
                    // Silently fail if the named file does not exist in the request.
                    return;
                }

                value = postedFiles.First();
            }
            else
            {
                if (postedFiles.Count == 0 && !bindingContext.IsTopLevelObject)
                {
                    // Silently fail if no files match. Will bind to an empty collection (treat empty as a success
                    // case and not reach here) if binding to a top-level object.
                    return;
                }

                // Perform any final type mangling needed.
                var modelType = bindingContext.ModelType;
                if (modelType == typeof(IFormFile[]))
                {
                    Debug.Assert(postedFiles is List<IFormFile>);
                    value = ((List<IFormFile>)postedFiles).ToArray();
                }
                else if (modelType == typeof(IFormFileCollection))
                {
                    Debug.Assert(postedFiles is List<IFormFile>);
                    value = new FileCollection((List<IFormFile>)postedFiles);
                }
                else
                {
                    value = postedFiles;
                }
            }

            // We need to add a ValidationState entry because the modelName might be non-standard. Otherwise
            // the entry we create in model state might not be marked as valid.
            bindingContext.ValidationState.Add(value, new ValidationStateEntry()
            {
                Key = modelName,
            });

            bindingContext.ModelState.SetModelValue(
                modelName,
                rawValue: null,
                attemptedValue: null);

            bindingContext.Result = ModelBindingResult.Success(value);
        }

        private async Task GetFormFilesAsync(
            string modelName,
            ModelBindingContext bindingContext,
            ICollection<IFormFile> postedFiles)
        {
            var request = bindingContext.HttpContext.Request;
            if (request.HasFormContentType)
            {
                var form = await request.ReadFormAsync();

                foreach (var file in form.Files)
                {
                    // If there is an <input type="file" ... /> in the form and is left blank.
                    if (file.Length == 0 && string.IsNullOrEmpty(file.FileName))
                    {
                        continue;
                    }

                    if (file.Name.Equals(modelName, StringComparison.OrdinalIgnoreCase))
                    {
                        postedFiles.Add(file);
                    }
                }
            }
        }

        private class FileCollection : ReadOnlyCollection<IFormFile>, IFormFileCollection
        {
            public FileCollection(List<IFormFile> list)
                : base(list)
            {
            }

            public IFormFile this[string name] => GetFile(name);

            public IFormFile GetFile(string name)
            {
                for (var i = 0; i < Items.Count; i++)
                {
                    var file = Items[i];
                    if (string.Equals(name, file.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        return file;
                    }
                }

                return null;
            }

            public IReadOnlyList<IFormFile> GetFiles(string name)
            {
                var files = new List<IFormFile>();
                for (var i = 0; i < Items.Count; i++)
                {
                    var file = Items[i];
                    if (string.Equals(name, file.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        files.Add(file);
                    }
                }

                return files;
            }
        }
    }
}