﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc
{
    public class DefaultActionBindingContextProvider : IActionBindingContextProvider
    {
        private static readonly object _requestCacheKey = new object();
        private readonly IModelMetadataProvider _modelMetadataProvider;
        private readonly IEnumerable<IModelBinder> _modelBinders;
        private readonly IEnumerable<IValueProviderFactory> _valueProviderFactories;
        private readonly IEnumerable<IInputFormatter> _bodyReaders;

        public DefaultActionBindingContextProvider(IModelMetadataProvider modelMetadataProvider,
                                                   IEnumerable<IModelBinder> modelBinders,
                                                   IEnumerable<IValueProviderFactory> valueProviderFactories,
                                                   IEnumerable<IInputFormatter> bodyReaders)
        {
            _modelMetadataProvider = modelMetadataProvider;
            _modelBinders = modelBinders.OrderBy(binder => binder.GetType() == typeof(ComplexModelDtoModelBinder) ? 1 : 0);
            _valueProviderFactories = valueProviderFactories;
            _bodyReaders = bodyReaders;
        }

        public async Task<ActionBindingContext> GetActionBindingContextAsync(ActionContext actionContext)
        {
            var requestContext = new RequestContext(actionContext.HttpContext, actionContext.RouteValues);
            var valueProviders = await Task.WhenAll(_valueProviderFactories.Select(factory => factory.GetValueProviderAsync(requestContext)));
            valueProviders = valueProviders.Where(vp => vp != null)
                                            .ToArray();

            return new ActionBindingContext(
                actionContext,
                _modelMetadataProvider,
                new CompositeModelBinder(_modelBinders),
                new CompositeValueProvider(valueProviders),
                new CompositeInputFormatter(_bodyReaders)
            );
        }
    }
}
