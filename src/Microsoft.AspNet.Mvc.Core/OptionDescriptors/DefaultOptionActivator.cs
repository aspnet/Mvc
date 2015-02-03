using System;
using System.Collections.Concurrent;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc.OptionDescriptors
{
    public class DefaultOptionActivator<TOption> : IOptionActivator<TOption>
    {
        private readonly IServiceProvider _provider;
        private static readonly Func<Type, ObjectFactory> CreateFactory =
            (t) => ActivatorUtilities.CreateFactory(t, Type.EmptyTypes);
        private static readonly ConcurrentDictionary<Type, ObjectFactory> _optionActivatorCache =
               new ConcurrentDictionary<Type, ObjectFactory>();

        public DefaultOptionActivator([NotNull] IServiceProvider provider)
        {
            _provider = provider;
        }

        public TOption CreateInstance([NotNull] Type optionType)
        {
            var optionFactory = _optionActivatorCache.GetOrAdd(optionType, CreateFactory);
            return (TOption)optionFactory(_provider, null);
        }
    }
}