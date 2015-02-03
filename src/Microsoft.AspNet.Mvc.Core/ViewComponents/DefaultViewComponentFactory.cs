using System;
using System.Collections.Concurrent;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc
{
    public class DefaultViewComponentFactory : IViewComponentFactory
    {
        private readonly IServiceProvider _provider;
        private static readonly Func<Type, ObjectFactory> CreateFactory =
            (t) => ActivatorUtilities.CreateFactory(t, Type.EmptyTypes);
        private static readonly ConcurrentDictionary<Type, ObjectFactory> _viewComponentCache =
               new ConcurrentDictionary<Type, ObjectFactory>();

        public DefaultViewComponentFactory([NotNull] IServiceProvider provider)
        {
            _provider = provider;
        }

        public object CreateInstance([NotNull] Type componentType)
        {
            var viewComponentFactory = _viewComponentCache.GetOrAdd(componentType, CreateFactory);
            return viewComponentFactory(_provider, null);
        }
    }
}