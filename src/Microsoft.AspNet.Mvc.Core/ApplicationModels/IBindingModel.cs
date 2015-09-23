using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc.ApplicationModels
{
    public interface IBindingModel
    {
        BindingInfo BindingInfo { get; set; }
    }
}