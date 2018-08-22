using System.ComponentModel.DataAnnotations;

namespace Microsoft.AspNetCore.Mvc.Analyzers.TestFiles.TopLevelParameterAttributeAnalyzerTest
{
    public class RunNoDiagnosticsAreReturnedForNonCollectionsParameterWithRequired : Controller
    {
        public IActionResult Index([Required] int id) => null;
    }
}
