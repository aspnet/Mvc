namespace Microsoft.AspNet.Mvc.Rendering
{
    public interface INeedViewContext
    {
        void Contextualize([NotNull] ViewContext viewContext);
    }
}
