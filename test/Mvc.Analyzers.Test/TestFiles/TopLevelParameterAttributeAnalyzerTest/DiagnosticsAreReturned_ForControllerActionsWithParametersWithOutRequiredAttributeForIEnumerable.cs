using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNetCore.Mvc.Analyzers.TestFiles.TopLevelParameterAttributeAnalyzerTest
{
    public class DiagnosticsAreReturned_ForControllerActionsWithParametersWithOutRequiredAttributeForIEnumerable : Controller
    {
        public IActionResult Index(IEnumerable<int> /*MM*/collection) => null;
    }
}
