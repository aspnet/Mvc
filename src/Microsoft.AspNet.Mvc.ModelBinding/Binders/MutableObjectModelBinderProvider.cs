using System;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public sealed class MutableObjectModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ActionContext actionContext, Type modelType)
        {
            if (!MutableObjectModelBinder.CanBindType(modelType))
            {
                return null;
            }

            return new MutableObjectModelBinder();
        }
    }
}
