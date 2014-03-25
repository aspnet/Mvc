
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc
{
    public interface IViewComponentResult
    {
        void Execute([NotNull] ViewComponentContext viewComponentContext);

        Task ExecuteAsync([NotNull] ViewComponentContext viewComponentContext);
    }}
