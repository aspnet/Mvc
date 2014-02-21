
namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class RouteValueValueProviderFactory : IValueProviderFactory
    {
        public IValueProvider GetValueProvider(ActionContext actionContext)
        {
            return new DictionaryBasedValueProvider(actionContext.RouteValues);
        }
    }
}
