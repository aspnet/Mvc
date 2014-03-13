
using System;
using Microsoft.AspNet.DependencyInjection;

namespace Microsoft.AspNet.Mvc
{
    public class DefaultViewComponentInvokerFactory : IViewComponentInvokerFactory
    {

        private readonly INestedProviderManager<ViewComponentInvokerProviderContext> _providerManager;

        public DefaultViewComponentInvokerFactory(INestedProviderManager<ViewComponentInvokerProviderContext> providerManager)
        {
            _providerManager = providerManager;
        }

        public IViewComponentInvoker CreateInstance([NotNull] Type componentType, [NotNull] object[] args)
        {
            var context = new ViewComponentInvokerProviderContext(componentType, args);
            _providerManager.Invoke(context);
            return context.Result;
        }
    }
}
