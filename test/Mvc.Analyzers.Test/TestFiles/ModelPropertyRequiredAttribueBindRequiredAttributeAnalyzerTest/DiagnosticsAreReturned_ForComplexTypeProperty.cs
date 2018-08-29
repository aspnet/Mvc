using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Microsoft.AspNetCore.Mvc.Analyzers.TestFiles.ModelPropertyRequiredAttribueBindRequiredAttributeAnalyzerTest
{
    public class DiagnosticsAreReturned_ForComplexTypeProperty
    {
        [BindRequired]
        public DiagnosticsAreReturned_ForComplexTypeProperty /*MM*/MyProperty { get; set; }

        [BindRequired]
        public string name { get; set; }
    }
}
