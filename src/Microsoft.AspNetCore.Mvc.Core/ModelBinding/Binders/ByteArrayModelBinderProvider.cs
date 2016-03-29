﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders
{
    /// <summary>
    /// An <see cref="IModelBinderProvider"/> for binding base64 encoded byte arrays.
    /// </summary>
    public class ByteArrayModelBinderProvider : IModelBinderProvider
    {
        /// <inheritdoc />
        public IModelBinder Create(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.ModelType == typeof(byte[]))
            {
                return new ByteArrayModelBinder();
            }

            return null;
        }
    }
}
