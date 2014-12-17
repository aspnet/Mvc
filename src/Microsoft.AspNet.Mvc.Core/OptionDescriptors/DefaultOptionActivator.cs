using System;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc.OptionDescriptors
{
    public class DefaultOptionActivator<TOption> : IOptionActivator<TOption>
    {
        private readonly IServiceProvider _provider;

        public DefaultOptionActivator([NotNull] IServiceProvider provider)
        {
            _provider = provider;
        }

        public TOption CreateInstance([NotNull] Type optionType)
        {
            return (TOption)ActivatorUtilities.CreateInstance(_provider, optionType);
        }
    }
}