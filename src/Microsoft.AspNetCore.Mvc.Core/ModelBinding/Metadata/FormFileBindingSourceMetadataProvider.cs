// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Metadata
{
    public class FormFileBindingSourceMetadataProvider : IBindingMetadataProvider
    {
        public Type Type { get; }

        /// <summary>
        /// Creates a new <see cref="FormFileBindingSourceMetadataProvider"/> for the given <paramref name="type"/>. 
        /// </summary>
        /// <param name="type">
        /// The <see cref="Type"/>. The provider sets <see cref="BindingSource"/> to <see cref="BindingSource.FormFile"/>
        /// for properties of <see cref="Type"/> <see cref="IFormFile"/> and <see cref="IFormCollection"/>.
        /// </param>
        public FormFileBindingSourceMetadataProvider(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            Type = type;
        }

        /// <inheritdoc />
        public void CreateBindingMetadata(BindingMetadataProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (Type.IsAssignableFrom(context.Key.ModelType))
            {
                context.BindingMetadata.BindingSource = BindingSource.FormFile;
            }
        }
    }
}
