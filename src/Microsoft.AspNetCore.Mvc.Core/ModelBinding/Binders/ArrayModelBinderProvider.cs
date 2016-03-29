﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders
{
    /// <summary>
    /// An <see cref="IModelBinderProvider"/> for arrays.
    /// </summary>
    public class ArrayModelBinderProvider : IModelBinderProvider
    {
        /// <inheritdoc />
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // We don't support binding readonly properties of arrays because we can't resize the
            // existing value.
            if (context.Metadata.ModelType.IsArray && !context.Metadata.IsReadOnly)
            {
                var elementType = context.Metadata.ElementMetadata.ModelType;
                var elementBinder = context.CreateBinder(context.Metadata.ElementMetadata);

                var binderType = typeof(ArrayModelBinder<>).MakeGenericType(elementType);
                return (IModelBinder)Activator.CreateInstance(binderType, elementBinder);
            }

            return null;
        }
    }
}
