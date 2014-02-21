using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc
{
    public interface IModelBindingConfigProvider
    {
        ModelBinderConfig GetConfig(ActionContext actionContext);
    }
}
