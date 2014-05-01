using System;
using Microsoft.AspNet.DependencyInjection;

namespace Microsoft.AspNet.Mvc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ValidateAntiForgeryTokenAttribute : Attribute, IFilterFactory
    {
        public IFilter CreateInstance(IServiceProvider serviceProvider)
        {
            var antiForgeryInstance = serviceProvider.GetService<AntiForgery>();
            return new ValidateAntiForgeryTokenAuthorizationFilter(antiForgeryInstance);
        }
    }
}