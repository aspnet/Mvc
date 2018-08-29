using System.ComponentModel.DataAnnotations;

namespace Microsoft.AspNetCore.Mvc.Analyzers.TestFiles.TopLevelParameterAttributeAnalyzerTest
{
    public class DiagnosticsAreReturned_ForControllerActionsWithParametersWithRequiredAttributeForArray: Controller
    {
        public IActionResult Index([Required]int[] /*MM*/collection) => null; 
    }
}
