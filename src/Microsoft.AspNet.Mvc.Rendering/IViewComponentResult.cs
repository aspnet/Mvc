
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc
{
    public interface IViewComponentResult
    {
        Task ExecuteAsync(TextWriter writer);
    }
}
