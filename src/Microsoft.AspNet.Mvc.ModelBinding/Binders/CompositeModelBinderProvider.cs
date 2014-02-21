using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc.ModelBinding.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public sealed class CompositeModelBinderProvider : IModelBinderProvider
    {
        private readonly IModelBinderProvider[] _providers;

        public CompositeModelBinderProvider(IEnumerable<IModelBinderProvider> providers)
        {
            if (providers == null)
            {
                throw Error.ArgumentNull("providers");
            }

            _providers = providers.ToArray();
        }

        public IEnumerable<IModelBinderProvider> Providers
        {
            get { return _providers; }
        }

        public IModelBinder GetBinder(ActionContext actionContext, Type modelType)
        {
            // Pre-filter out any binders that we know can't match. 
            IEnumerable<IModelBinder> binders = from provider in _providers 
                                                let binder = provider.GetBinder(actionContext, modelType) 
                                                where binder != null 
                                                select binder;
            return new CompositeModelBinder(binders);
        }
    }
}
