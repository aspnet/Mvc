// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class GenericModelBinder : IModelBinder
    {
        public async Task<ModelBindingResult> BindModelAsync(ModelBindingContext bindingContext)
        {
            var bindingInfo = ResolveGenericBindingInfo(bindingContext.ModelType);
            if (bindingInfo != null)
            {
                var binder = (IModelBinder)Activator.CreateInstance(bindingInfo.ModelBinderType);
                var result = await binder.BindModelAsync(bindingContext);

                if (result != null && result.IsModelSet)
                {
                    // Success - propagate the values returned by the model binder.
                    return result;
                }

                // If this is the fallback case, and we didn't bind as a top-level model, then generate a
                // default 'empty' model and return it.
                var isTopLevelObject = bindingContext.ModelMetadata.ContainerType == null;
                var hasExplicitAlias = bindingContext.BinderModelName != null;

                if (isTopLevelObject && (hasExplicitAlias || bindingContext.ModelName == string.Empty))
                {
                    object model;
                    if (bindingInfo.UnderlyingModelType.IsArray)
                    {
                        model = Array.CreateInstance(bindingInfo.UnderlyingModelType.GetElementType(), 0);
                    }
                    else
                    {
                        model = Activator.CreateInstance(bindingInfo.UnderlyingModelType);
                    }

                    var validationNode = new ModelValidationNode(
                        bindingContext.ModelName,
                        bindingContext.ModelMetadata,
                        model);

                    return new ModelBindingResult(model, bindingContext.ModelName, true, validationNode);
                }

                // We always want to return a result for model types that we handle; tell the model binding
                // system to skip other model binders by returning non-null.
                return new ModelBindingResult(model: null, key: bindingContext.ModelName, isModelSet: false);
            }

            return null;
        }

        private static GenericModelBindingInfo ResolveGenericBindingInfo(Type modelType)
        {
            return GetArrayBinderInfo(modelType) ??
                   GetCollectionBinderInfo(modelType) ??
                   GetDictionaryBinderInfo(modelType) ??
                   GetKeyValuePairBinderInfo(modelType);
        }

        private static GenericModelBindingInfo GetArrayBinderInfo(Type modelType)
        {
            if (modelType.IsArray)
            {
                var elementType = modelType.GetElementType();
                var modelBinderType = typeof(ArrayModelBinder<>).MakeGenericType(elementType);

                return new GenericModelBindingInfo()
                {
                    ModelBinderType = modelBinderType,
                    UnderlyingModelType = modelType,
                };
            }

            return null;
        }

        private static GenericModelBindingInfo GetCollectionBinderInfo(Type modelType)
        {
            return GetGenericModelBindingInfo(
                typeof(ICollection<>),
                typeof(List<>),
                typeof(CollectionModelBinder<>),
                modelType);
        }

        private static GenericModelBindingInfo GetDictionaryBinderInfo(Type modelType)
        {
            return GetGenericModelBindingInfo(
                typeof(IDictionary<,>),
                typeof(Dictionary<,>),
                typeof(DictionaryModelBinder<,>),
                modelType);
        }

        private static GenericModelBindingInfo GetKeyValuePairBinderInfo(Type modelType)
        {
            var modelTypeInfo = modelType.GetTypeInfo();
            if (modelTypeInfo.IsGenericType &&
                modelTypeInfo.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                var modelBinderType = typeof(KeyValuePairModelBinder<,>)
                    .MakeGenericType(modelTypeInfo.GenericTypeArguments);

                return new GenericModelBindingInfo()
                {
                    ModelBinderType = modelBinderType,
                    UnderlyingModelType = modelType,
                };
            }

            return null;
        }

        //
        // Example: 
        //      GetGenericBinderType(typeof(IList<T>), typeof(List<T>), typeof(ListBinder<T&>), ...)
        //
        // This means that the ListBinder<T> type can work with models that implement IList<T>, and if there is no 
        // existing model instance, the binder will create a List{T}. 
        //
        // This method will return null if the given model type isn't compatible with the combination of
        // supportedInterfaceType and modelType. If supportedInterfaceType and modelType are compatible, then
        // it will return the closed-generic newInstanceType and closed-generic openBinderType.
        // </remarks>
        private static GenericModelBindingInfo GetGenericModelBindingInfo(
            Type supportedInterfaceType,
            Type newInstanceType,
            Type openBinderType,
            Type modelType)
        {
            Debug.Assert(supportedInterfaceType != null);
            Debug.Assert(openBinderType != null);
            Debug.Assert(modelType != null);

            var modelTypeArguments = GetGenericBinderTypeArgs(supportedInterfaceType, modelType);

            if (modelTypeArguments == null)
            {
                return null;
            }

            var closedNewInstanceType = newInstanceType.MakeGenericType(modelTypeArguments);
            if (!modelType.GetTypeInfo().IsAssignableFrom(closedNewInstanceType.GetTypeInfo()))
            {
                return null;
            }

            return new GenericModelBindingInfo()
            {
                ModelBinderType = openBinderType.MakeGenericType(modelTypeArguments),
                UnderlyingModelType = closedNewInstanceType,
            };
        }

        // Get the generic arguments for the binder, based on the model type. Or null if not compatible.
        private static Type[] GetGenericBinderTypeArgs(Type supportedInterfaceType, Type modelType)
        {
            var modelTypeInfo = modelType.GetTypeInfo();
            if (!modelTypeInfo.IsGenericType || modelTypeInfo.IsGenericTypeDefinition)
            {
                // not a closed generic type
                return null;
            }

            var modelTypeArguments = modelTypeInfo.GenericTypeArguments;
            if (modelTypeArguments.Length != supportedInterfaceType.GetTypeInfo().GenericTypeParameters.Length)
            {
                // wrong number of generic type arguments
                return null;
            }

            return modelTypeArguments;
        }

        private class GenericModelBindingInfo
        {
            public Type ModelBinderType { get; set; }

            // The concrete type that should be created if needed
            public Type UnderlyingModelType { get; set; }
        }
    }
}
