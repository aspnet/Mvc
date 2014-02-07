using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.ModelBinding.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public sealed class DictionaryModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ActionContext actionContext, Type modelType)
        {
            return CollectionModelBinderUtil.GetGenericBinder(typeof(IDictionary<,>), typeof(Dictionary<,>), typeof(DictionaryModelBinder<,>), modelType);
        }
    }
}
