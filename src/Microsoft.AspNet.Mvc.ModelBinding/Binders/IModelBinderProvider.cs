using System;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public interface IModelBinderProvider
    {
        /// <summary>
        /// Find a binder for the given type
        /// </summary>
        /// <returns>a binder, which can attempt to bind this type. Or null if the binder knows statically that it will never be able to bind the type.</returns>
        IModelBinder GetBinder(ActionContext actionContext, Type modelType);
    }
}
