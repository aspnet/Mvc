using System;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    // Returns a binder that can bind ComplexModelDto objects.
    public sealed class ComplexModelDtoModelBinderProvider : IModelBinderProvider
    {
        // This is really just a simple binder.
        private static readonly SimpleModelBinderProvider _underlyingProvider = GetUnderlyingProvider();

        public IModelBinder GetBinder(ActionContext configuration, Type modelType)
        {
            return _underlyingProvider.GetBinder(configuration, modelType);
        }

        private static SimpleModelBinderProvider GetUnderlyingProvider()
        {
            return new SimpleModelBinderProvider(typeof(ComplexModelDto), new ComplexModelDtoModelBinder())
            {
                SuppressPrefixCheck = true
            };
        }
    }
}
