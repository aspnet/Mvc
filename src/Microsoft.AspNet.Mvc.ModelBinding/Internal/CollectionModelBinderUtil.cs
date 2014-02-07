using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Microsoft.AspNet.Mvc.ModelBinding.Internal
{
    public static class CollectionModelBinderUtil
    {
        public static IEnumerable<string> GetIndexNamesFromValueProviderResult(ValueProviderResult valueProviderResultIndex)
        {
            IEnumerable<string> indexNames = null;
            if (valueProviderResultIndex != null)
            {
                string[] indexes = (string[])valueProviderResultIndex.ConvertTo(typeof(string[]));
                if (indexes != null && indexes.Length > 0)
                {
                    indexNames = indexes;
                }
            }
            return indexNames;
        }

        public static void CreateOrReplaceCollection<TElement>(ModelBindingContext bindingContext,
                                                               IEnumerable<TElement> incomingElements,
                                                               Func<ICollection<TElement>> creator)
        {
            var collection = bindingContext.Model as ICollection<TElement>;
            if (collection == null || collection.IsReadOnly)
            {
                collection = creator();
                bindingContext.Model = collection;
            }

            collection.Clear();
            foreach (TElement element in incomingElements)
            {
                collection.Add(element);
            }
        }

        /// <summary>
        /// Instantiate a generic binder.
        /// </summary>
        /// <param name="supportedInterfaceType">Type that is updatable by this binder.</param>
        /// <param name="newInstanceType">Type that will be created by the binder if necessary.</param>
        /// <param name="openBinderType">Model binder type.</param>
        /// <param name="modelType">Model type.</param>
        /// <returns></returns>
        // Example: GetGenericBinder(typeof(IList<>), typeof(List<>), typeof(ListBinder<>), ...) means that the ListBinder<T>
        // type can update models that implement IList<T>, and if for some reason the existing model instance is not
        // updatable the binder will create a List<T> object and bind to that instead. This method will return a ListBinder<T>
        // or null, depending on whether the type and updatability checks succeed.
        internal static IModelBinder GetGenericBinder(Type supportedInterfaceType, Type newInstanceType, Type openBinderType, Type modelType)
        {
            Contract.Assert(supportedInterfaceType != null);
            Contract.Assert(openBinderType != null);
            Contract.Assert(modelType != null);

            Type[] modelTypeArguments = GetGenericBinderTypeArgs(supportedInterfaceType, modelType);

            if (modelTypeArguments == null)
            {
                return null;
            }

            Type closedNewInstanceType = newInstanceType.MakeGenericType(modelTypeArguments);
            if (!modelType.IsAssignableFrom(closedNewInstanceType))
            {
                return null;
            }

            Type closedBinderType = openBinderType.MakeGenericType(modelTypeArguments);
            var binder = (IModelBinder)Activator.CreateInstance(closedBinderType);
            return binder;
        }

        // Get the generic arguments for the binder, based on the model type. Or null if not compatible.
        private static Type[] GetGenericBinderTypeArgs(Type supportedInterfaceType, Type modelType)
        {
            TypeInfo modelTypeInfo = modelType.GetTypeInfo();
            if (!modelTypeInfo.IsGenericType || modelTypeInfo.IsGenericTypeDefinition)
            {
                // not a closed generic type
                return null;
            }

            Type[] modelTypeArguments = modelTypeInfo.GenericTypeArguments;
            if (modelTypeArguments.Length != supportedInterfaceType.GetTypeInfo().GenericTypeParameters.Length)
            {
                // wrong number of generic type arguments
                return null;
            }

            return modelTypeArguments;
        }
    }
}
