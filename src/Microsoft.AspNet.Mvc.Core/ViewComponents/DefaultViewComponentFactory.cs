using System;
using System.Collections.Concurrent;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc
{
    public class DefaultViewComponentFactory : IViewComponentFactory
    {
        private readonly IServiceProvider _provider;
        private static readonly ConcurrentDictionary<Type, ObjectFactory> _viewComponentCache =
               new ConcurrentDictionary<Type, ObjectFactory>();

        public DefaultViewComponentFactory([NotNull] IServiceProvider provider)
        {
            _provider = provider;
        }

        public object CreateInstance([NotNull] Type componentType)
        {
            var viewComponentFactory = _viewComponentCache.GetOrAdd(componentType, ActivatorUtilities.CreateFactory(componentType, Type.EmptyTypes));
            return viewComponentFactory(_provider, null);
          //  return ActivatorUtilities.CreateInstance(_provider, componentType);
        }
    }
}