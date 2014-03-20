
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc
{
    public interface IViewComponentResult
    {
        void Execute(TextWriter writer);

        Task ExecuteAsync(TextWriter writer);
    }
}
