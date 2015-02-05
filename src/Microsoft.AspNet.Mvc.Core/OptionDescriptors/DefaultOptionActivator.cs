using System;
using System.Collections.Concurrent;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc.OptionDescriptors
{
    public class DefaultOptionActivator<TOption> : IOptionActivator<TOption>
    {
        private Func<Type, ObjectFactory> CreateFactory =
            (t) => ActivatorUtilities.CreateFactory(t, Type.EmptyTypes);
        private ConcurrentDictionary<Type, ObjectFactory> _optionActivatorCache =
               new ConcurrentDictionary<Type, ObjectFactory>();

        public TOption CreateInstance([NotNull] IServiceProvider serviceProvider, [NotNull] Type optionType)
        {
            var optionFactory = _optionActivatorCache.GetOrAdd(optionType, CreateFactory);
            return (TOption)optionFactory(serviceProvider, null);
        }
    }
}