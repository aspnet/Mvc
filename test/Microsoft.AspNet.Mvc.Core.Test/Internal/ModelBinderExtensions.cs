using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public static class ModelBinderExtensions
    {
        public static async Task<ModelBindingResult> BindModelResultAsync(
            this IModelBinder binder, 
            IModelBindingContext context)
        {
            await binder.BindModelAsync(context);
            return context.Result ?? default(ModelBindingResult);
        }
    }
}
