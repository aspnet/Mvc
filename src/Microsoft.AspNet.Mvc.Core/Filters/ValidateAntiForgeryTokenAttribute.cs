using System;
using Microsoft.AspNet.DependencyInjection;

namespace Microsoft.AspNet.Mvc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ValidateAntiForgeryTokenAttribute : Attribute, IFilterFactory, IOrderedFilter
    {
        public int Order { get; set; }

        public IFilter CreateInstance(IServiceProvider serviceProvider)
        {
            var antiForgery = serviceProvider.GetService<AntiForgery>();
            return new ValidateAntiForgeryTokenAuthorizationFilter(antiForgery);
        }
    }
}