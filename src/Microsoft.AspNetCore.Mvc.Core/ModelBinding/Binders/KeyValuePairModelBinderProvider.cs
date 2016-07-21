﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders
{
    /// <summary>
    /// An <see cref="IModelBinderProvider"/> for <see cref="KeyValuePair{TKey, TValue}"/>.
    /// </summary>
    public class KeyValuePairModelBinderProvider : IModelBinderProvider
    {
        /// <inheritdoc />
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var modelTypeInfo = context.Metadata.ModelType.GetTypeInfo();
            if (modelTypeInfo.IsGenericType && 
                modelTypeInfo.GetGenericTypeDefinition().GetTypeInfo() == typeof(KeyValuePair<,>).GetTypeInfo())
            {
                var typeArguments = modelTypeInfo.GenericTypeArguments;

                var keyMetadata = context.MetadataProvider.GetMetadataForType(typeArguments[0]);
                var keyBinder = context.CreateBinder(keyMetadata);

                var valueMetadata = context.MetadataProvider.GetMetadataForType(typeArguments[1]);
                var valueBinder = context.CreateBinder(valueMetadata);

                var binderType = typeof(KeyValuePairModelBinder<,>).MakeGenericType(typeArguments);
                return (IModelBinder)Activator.CreateInstance(binderType, keyBinder, valueBinder);
            }

            return null;
        }
    }
}
