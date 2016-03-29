﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders
{
    /// <summary>
    /// An <see cref="IModelBinderProvider"/> for <see cref="ICollection{T}"/>.
    /// </summary>
    public class CollectionModelBinderProvider : IModelBinderProvider
    {
        /// <inheritdoc />
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var modelType = context.Metadata.ModelType;
            
            // Arrays are handled by another binder.
            if (modelType.IsArray)
            {
                return null;
            }

            // If the model type is ICollection<> then we can call its Add method, so we can always support it.
            var collectionType = ClosedGenericMatcher.ExtractGenericInterface(modelType, typeof(ICollection<>));
            if (collectionType != null)
            {
                var elementType = collectionType.GenericTypeArguments[0];
                var elementBinder = context.CreateBinder(context.MetadataProvider.GetMetadataForType(elementType));

                var binderType = typeof(CollectionModelBinder<>).MakeGenericType(collectionType.GenericTypeArguments);
                return (IModelBinder)Activator.CreateInstance(binderType, elementBinder);
            }

            // If the model type is IEnumerable<> then we need to know if we can assign a List<> to it, since
            // that's what we would create. (The cases handled here are IEnumerable<>, IReadOnlyColection<> and
            // IReadOnlyList<>).
            //
            // We need to check IsReadOnly because we need to know if we can SET the property.
            var enumerableType = ClosedGenericMatcher.ExtractGenericInterface(modelType, typeof(IEnumerable<>));
            if (enumerableType != null && !context.Metadata.IsReadOnly)
            {
                var listType = typeof(List<>).MakeGenericType(enumerableType.GenericTypeArguments);
                if (modelType.GetTypeInfo().IsAssignableFrom(listType.GetTypeInfo()))
                {
                    var elementType = enumerableType.GenericTypeArguments[0];
                    var elementBinder = context.CreateBinder(context.MetadataProvider.GetMetadataForType(elementType));

                    var binderType = typeof(CollectionModelBinder<>).MakeGenericType(enumerableType.GenericTypeArguments);
                    return (IModelBinder)Activator.CreateInstance(binderType, elementBinder);
                }
            }

            return null;
        }
    }
}
