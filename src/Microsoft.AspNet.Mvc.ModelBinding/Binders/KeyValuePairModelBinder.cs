using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.ModelBinding.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public sealed class KeyValuePairModelBinder<TKey, TValue> : IModelBinder
    {
        public async Task<bool> BindModelAsync(ModelBindingContext bindingContext)
        {
            ModelBindingHelper.ValidateBindingContext(bindingContext, typeof(KeyValuePair<TKey, TValue>), allowNullModel: true);

            var keyResult = await TryBindStrongModel<TKey>(bindingContext, "key");
            var keyBindingSucceeded = keyResult.Item1;
            var key = keyResult.Item2;

            var valueResult =  await TryBindStrongModel<TValue>(bindingContext, "value");
            var valueBindingSucceeded = valueResult.Item1;
            var value = valueResult.Item2;

            if (keyBindingSucceeded && valueBindingSucceeded)
            {
                bindingContext.Model = new KeyValuePair<TKey, TValue>(key, value);
            }
            return keyBindingSucceeded || valueBindingSucceeded;
        }

        // TODO: Make this internal
        public async Task<Tuple<bool, TModel>> TryBindStrongModel<TModel>(ModelBindingContext parentBindingContext,
                                                                          string propertyName)
        {
            ModelBindingContext propertyBindingContext = new ModelBindingContext(parentBindingContext)
            {
                ModelMetadata = parentBindingContext.MetadataProvider.GetMetadataForType(modelAccessor: null, modelType: typeof(TModel)),
                ModelName = ModelBindingHelper.CreatePropertyModelName(parentBindingContext.ModelName, propertyName)
            };

            if (await propertyBindingContext.ModelBinder.BindModelAsync(propertyBindingContext))
            {
                object untypedModel = propertyBindingContext.Model;
                var model = ModelBindingHelper.CastOrDefault<TModel>(untypedModel);
                parentBindingContext.ValidationNode.ChildNodes.Add(propertyBindingContext.ValidationNode);
                return Tuple.Create(true, model);
            }

            return Tuple.Create(false, default(TModel));
        }
    }
}
