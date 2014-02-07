using System;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    // Returns a binder that can extract a ValueProviderResult.RawValue and return it directly.
    public sealed class TypeMatchModelBinderProvider : IModelBinderProvider
    {
        private static readonly TypeMatchModelBinder _binder = new TypeMatchModelBinder();

        public IModelBinder GetBinder(ActionContext actionContext, Type modelType)
        {
            return _binder;
        }
    }
}
