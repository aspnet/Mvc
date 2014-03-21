
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc
{
    public interface IViewComponentResult
    {
        void Execute([NotNull] ComponentContext componentContext);

        Task ExecuteAsync([NotNull] ComponentContext componentContext);
    }}
