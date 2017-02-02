// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Metadata
{
    public class SpecialParametersBindingMetadataProvider : IBindingMetadataProvider
    {
        public Type Type { get; }

        /// <summary>
        /// Creates a new <see cref="SpecialParametersBindingMetadataProvider"/> for the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">
        /// The <see cref="Type"/>. All properties with this <see cref="Type"/> will have
        /// <see cref="BindingSource"/> set to <see cref="BindingSource.Special"/>.
        /// </param>
        public SpecialParametersBindingMetadataProvider(Type type)
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
                context.BindingMetadata.BindingSource = BindingSource.Special;
            }
        }
    }
}
