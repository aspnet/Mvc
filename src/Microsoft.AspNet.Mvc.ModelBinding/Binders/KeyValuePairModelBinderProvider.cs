using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.ModelBinding.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public sealed class KeyValuePairModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ActionContext actionContext, Type modelType)
        {
            return ModelBindingHelper.GetPossibleBinderInstance(
                closedModelType: modelType, 
                openModelType: typeof(KeyValuePair<,>), 
                openBinderType: typeof(KeyValuePairModelBinder<,>));
        }
    }
}
