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
                    // Success - propegate the values returned by the model binder.
                    return new ModelBindingResult(result.Model, result.Key, result.IsModelSet, result.ValidationNode);
                }

                // If this is the fallback case, and we didn't bind as a top-level model, then generate a
                // default 'empty' model and return it.
                var isTopLevelObject = bindingContext.ModelMetadata.ContainerType == null;
                var hasExplicitAlias = bindingContext.BinderModelName != null;

                if (isTopLevelObject && (hasExplicitAlias || bindingContext.ModelName == string.Empty))
                {

                    var model = result?.Model;
                    if (model == null && bindingInfo.UnderlyingModelType.IsArray)
                    {
                        model = Array.CreateInstance(bindingInfo.UnderlyingModelType.GetElementType(), 0);
                    }
                    else if (model == null)
                    {
                        model = Activator.CreateInstance(bindingInfo.UnderlyingModelType);
                    }

                    var validationNode = new ModelValidationNode(
                        bindingContext.ModelName,
                        bindingContext.ModelMetadata,
                        model);

                    return new ModelBindingResult(model, bindingContext.ModelName, true, validationNode);
                }

                // We always want to return a result for model-types that we handle.
                //
                // Always tell the model binding system to skip other model binders i.e. return non-null.
                return new ModelBindingResult(model: null, key: bindingContext.ModelName, isModelSet: false);
            }

            return null;
        }

        private static GenericModelBindingInfo ResolveGenericBindingInfo(Type modelType)
        {
            return GetArrayBinder(modelType) ??
                   GetCollectionBinder(modelType) ??
                   GetDictionaryBinder(modelType) ??
                   GetKeyValuePairBinder(modelType);
        }

        private static GenericModelBindingInfo GetArrayBinder(Type modelType)
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

        private static GenericModelBindingInfo GetCollectionBinder(Type modelType)
        {
            return GetGenericModelBindingInfo(
                typeof(ICollection<>),
                typeof(List<>),
                typeof(CollectionModelBinder<>),
                modelType);
        }

        private static GenericModelBindingInfo GetDictionaryBinder(Type modelType)
        {
            return GetGenericModelBindingInfo(
                typeof(IDictionary<,>),
                typeof(Dictionary<,>),
                typeof(DictionaryModelBinder<,>),
                modelType);
        }

        private static GenericModelBindingInfo GetKeyValuePairBinder(Type modelType)
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
