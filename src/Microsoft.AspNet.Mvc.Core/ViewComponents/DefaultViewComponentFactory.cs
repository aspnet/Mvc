using System;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc
{
    public class DefaultViewComponentFactory : IViewComponentFactory
    {
        private readonly IServiceProvider _provider;

        public DefaultViewComponentFactory([NotNull] IServiceProvider provider)
        {
            _provider = provider;
        }

        public object CreateInstance([NotNull] Type componentType)
        {
            return ActivatorUtilities.CreateInstance(_provider, componentType);
        }
    }
}