
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Rendering;

namespace Microsoft.AspNet.Mvc
{
    public interface IViewComponentResult
    {
        void Execute(ViewContext viewContext, TextWriter writer);

        Task ExecuteAsync(ViewContext viewContext, TextWriter writer);
    }
}
