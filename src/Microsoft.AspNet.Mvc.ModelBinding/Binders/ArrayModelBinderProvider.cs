using System;
using Microsoft.AspNet.Mvc.ModelBinding.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding.Binders
{
    public class ArrayModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ActionContext actionContext, Type modelType)
        {
            if (modelType == null)
            {
                throw Error.ArgumentNull("modelType");
            }

            if (!modelType.IsArray)
            {
                return null;
            }

            Type elementType = modelType.GetElementType();
            return (IModelBinder)Activator.CreateInstance(typeof(ArrayModelBinder<>).MakeGenericType(elementType));
        }
    }
}
