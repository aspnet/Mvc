
namespace Microsoft.AspNet.Mvc.Rendering
{
    public interface IHtmlHelper<TModel> : IHtmlHelper
    {
        /// <summary>
        /// Gets the current view data.
        /// </summary>
        ViewData<TModel> ViewData { get; }
    }
}
