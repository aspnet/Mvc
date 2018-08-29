namespace Microsoft.AspNetCore.Mvc.Analyzers.TestFiles.TopLevelParameterAttributeAnalyzerTest
{
    [Controller]
    public class DiagnosticsAreReturned_ForControllerActionsWithParametersWithOutRequiredAttribute
    {
        public IActionResult Index(int[] collection) => null;
    }
}
