using System;
using System.Collections.Concurrent;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Encapsulates information that creates a ViewComponent.
    /// </summary>
    public class DefaultViewComponentFactory : IViewComponentFactory
    {
        private readonly ConcurrentDictionary<Type, ObjectFactory> _viewComponentCache =
               new ConcurrentDictionary<Type, ObjectFactory>();

        /// <summary>
        /// Creates an instance of ViewComponent.
        /// </summary>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> instance that retrieves services from the
        /// service collection.</param>
        /// <param name="componentType">The <see cref="Type"/> of the <see cref="ViewComponent"/> to create.</param>
        public object CreateInstance([NotNull]IServiceProvider serviceProvider, [NotNull] Type componentType)
        {
            var typeActivatorCache = serviceProvider.GetRequiredService<ITypeActivatorCache>();
            return typeActivatorCache.CreateInstance<object>(serviceProvider, componentType);
        }
    }
}