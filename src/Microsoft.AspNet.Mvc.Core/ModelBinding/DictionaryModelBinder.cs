// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// <see cref="IModelBinder"/> implementation for binding dictionary values.
    /// </summary>
    /// <typeparam name="TKey">Type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">Type of values in the dictionary.</typeparam>
    public class DictionaryModelBinder<TKey, TValue> : CollectionModelBinder<KeyValuePair<TKey, TValue>>
    {
        /// <inheritdoc />
        public override async Task<ModelBindingResult> BindModelAsync([NotNull] ModelBindingContext bindingContext)
        {
            var result = await base.BindModelAsync(bindingContext);
            if (result == null || !result.IsModelSet)
            {
                // No match for the prefix at all.
                return result;
            }

            Debug.Assert(result.Model != null);
            var model = (Dictionary<TKey, TValue>)result.Model;
            if (model.Count != 0)
            {
                // ICollection<KeyValuePair<TKey, TValue>> approach was successful.
                return result;
            }

            var enumerableValueProvider = bindingContext.ValueProvider as IEnumerableValueProvider;
            if (enumerableValueProvider == null)
            {
                // No IEnumerableValueProvider available for the fallback approach. For example the user may have
                // replaced the ValueProvider with something other than a CompositeValueProvider.
                return result;
            }

            // Attempt to bind dictionary from a set of prefix[key]=value entries. Get the short and long keys first.
            var keys = await enumerableValueProvider.GetKeysFromPrefixAsync(bindingContext.ModelName);
            if (!keys.Any())
            {
                // No entries with the expected keys.
                return result;
            }

            // Update the existing successful but empty ModelBindingResult.
            var metadataProvider = bindingContext.OperationBindingContext.MetadataProvider;
            var valueMetadata = metadataProvider.GetMetadataForType(typeof(TValue));
            var valueBindingContext = ModelBindingContext.GetChildModelBindingContext(
                bindingContext,
                bindingContext.ModelName,
                valueMetadata);

            var modelBinder = bindingContext.OperationBindingContext.ModelBinder;
            var validationNode = result.ValidationNode;

            foreach (var key in keys)
            {
                var dictionaryKey = ConvertFromString(key.Key);
                valueBindingContext.ModelName = key.Value;

                var valueResult = await modelBinder.BindModelAsync(valueBindingContext);

                // Always add an entry to the dictionary but validate only if binding was successful.
                model[dictionaryKey] = ModelBindingHelper.CastOrDefault<TValue>(valueResult?.Model);
                if (valueResult != null && valueResult.IsModelSet)
                {
                    validationNode.ChildNodes.Add(valueResult.ValidationNode);
                }
            }

            return result;
        }

        /// <inheritdoc />
        protected override object GetModel(IEnumerable<KeyValuePair<TKey, TValue>> newCollection)
        {
            return newCollection?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /// <inheritdoc />
        protected override object CreateEmptyCollection()
        {
            return new Dictionary<TKey, TValue>();
        }

        private static TKey ConvertFromString(string keyString)
        {
            // Use InvariantCulture to convert string since ExpressionHelper.GetExpressionText() used that culture.
            var keyResult = new ValueProviderResult(keyString);
            var keyObject = keyResult.ConvertTo(typeof(TKey));

            return ModelBindingHelper.CastOrDefault<TKey>(keyObject);
        }
    }
}
