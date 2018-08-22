using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Microsoft.AspNetCore.Mvc.Analyzers.TestFiles.TopLevelParameterAttributeAnalyzerTest
{
   public class DiagnosticsAreReturned_ForControllerActionsWithParametersWithRequiredAttributeForIEnumerable : Controller
    {
        public IActionResult Index([Required]IEnumerable<int> /*MM*/collection) => null;
    }
}
