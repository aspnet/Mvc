using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc
{
    public class DefaultModelBindingConfigProvider : IModelBindingConfigProvider
    {
        private static readonly object _requestCacheKey = new object();
        private readonly IModelMetadataProvider _modelMetadataProvider;
        private readonly IEnumerable<IModelBinder> _modelBinders;
        private readonly IEnumerable<IValueProviderFactory> _valueProviderFactories;

        public DefaultModelBindingConfigProvider(IModelMetadataProvider modelMetadataProvider,
                                                 IEnumerable<IModelBinder> modelBinders,
                                                 IEnumerable<IValueProviderFactory> valueProviderFactories)
        {
            _modelMetadataProvider = modelMetadataProvider;
            _modelBinders = modelBinders.OrderBy(binder => binder.GetType() == typeof(ComplexModelDtoModelBinder) ? 1 : 0);
            _valueProviderFactories = valueProviderFactories;
        }

        public ModelBinderConfig GetConfig(ActionContext actionContext)
        {
            var requestContext = new RequestContext(actionContext.HttpContext, actionContext.RouteValues);
            var valueProviders = _valueProviderFactories.Select(factory => factory.GetValueProvider(requestContext))
                                                        .Where(valueProvider => valueProvider != null)
                                                        .ToList();
            
            return new ModelBinderConfig
            {
                MetadataProvider = _modelMetadataProvider,
                ModelBinder = new CompositeModelBinder(_modelBinders),
                ValueProvider = new CompositeValueProvider(valueProviders)
            };
        }
    }
}
