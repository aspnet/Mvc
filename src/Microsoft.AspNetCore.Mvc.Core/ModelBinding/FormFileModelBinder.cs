// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
#if NETSTANDARD1_3
using System.Reflection;
#endif
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Microsoft.AspNetCore.Mvc.ModelBinding
{
    /// <summary>
    /// <see cref="IModelBinder"/> implementation to bind posted files to <see cref="IFormFile"/>.
    /// </summary>
    public class FormFileModelBinder : IModelBinder
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

            if (bindingContext.ModelType != typeof(IFormFile) &&
                !typeof(IEnumerable<IFormFile>).IsAssignableFrom(bindingContext.ModelType))
            {
                // Not a type this model binder supports. Let other binders run.
                return TaskCache.CompletedTask;
            }

            ICollection<IFormFile> postedFiles = CreateCompatibleCollection(bindingContext);
            if (postedFiles == null)
            {
                // Silently fail and stop other model binders running if unable to create an instance or use the
                // current instance.
                bindingContext.Result = ModelBindingResult.Failed(bindingContext.ModelName);
                return TaskCache.CompletedTask;
            }

            return BindModelCoreAsync(bindingContext, postedFiles);
        }

        private static ICollection<IFormFile> CreateCompatibleCollection(ModelBindingContext bindingContext)
        {

            ICollection<IFormFile> collection;
            if (bindingContext.ModelType == typeof(IFormFile))
            {
                // Create an intermediate list. Will throw it away later.
                collection = new List<IFormFile>();
            }
            else if (bindingContext.ModelType == typeof(IFormFile[]))
            {
                if (bindingContext.ModelMetadata.IsReadOnly)
                {
                    // Can't change the length of an existing array or replace it.
                    return null;
                }

                // Use a List<IFormFile> for now. Will create an array later.
                collection = new List<IFormFile>();
            }
            else
            {
                if (bindingContext.Model == null)
                {
                    if (bindingContext.ModelMetadata.IsReadOnly)
                    {
                        // Need a new collection instance but unable to assign it to the property.
                        return null;
                    }

                    if (bindingContext.ModelType == typeof(IFormFileCollection))
                    {
                        // Special-case creating a custom IFormFileCollection if the property is settable and
                        // currently null. Use a List<IFormFile> for now. Will create a FileCollection later.
                        collection = new List<IFormFile>();
                    }
                    else
                    {
                        // Note this call may return null if the model type cannot be activated.
                        collection = ModelBindingHelper.CreateCompatibleCollection<IFormFile>(
                            bindingContext.ModelType,
                            capacity: null);
                    }
                }
                else
                {
                    // Note this cast may fail if the runtime model implements IEnumerable<IFormFile> but not
                    // ICollection<IFormFile>. Give up in then: Assuming we're not in an odd corner case where the
                    // property is settable and its declared type is actually assignable from List<IFormFile>.
                    collection = bindingContext.Model as ICollection<IFormFile>;
                    collection?.Clear();
                }
            }

            return collection;
        }

        private async Task BindModelCoreAsync(ModelBindingContext bindingContext, ICollection<IFormFile> postedFiles)
        {
            Debug.Assert(postedFiles != null);

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
                    bindingContext.Result = ModelBindingResult.Failed(bindingContext.ModelName);
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
                    bindingContext.Result = ModelBindingResult.Failed(bindingContext.ModelName);
                    return;
                }

                // Perform any final type mangling needed.
                if (bindingContext.ModelType == typeof(IFormFile[]))
                {
                    Debug.Assert(postedFiles is List<IFormFile>);
                    value = ((List<IFormFile>)postedFiles).ToArray();
                }
                else if (bindingContext.Model == null && bindingContext.ModelType == typeof(IFormFileCollection))
                {
                    Debug.Assert(postedFiles is List<IFormFile>);
                    value = new FileCollection((List<IFormFile>)postedFiles);
                }
                else
                {
                    value = postedFiles;
                }
            }

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
        }

        private async Task GetFormFilesAsync(
            string modelName,
            ModelBindingContext bindingContext,
            ICollection<IFormFile> postedFiles)
        {
            var request = bindingContext.OperationBindingContext.HttpContext.Request;
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