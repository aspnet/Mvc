using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Microsoft.AspNetCore.Mvc.Analyzers.TestFiles.ModelPropertyRequiredAttribueBindRequiredAttributeAnalyzerTest
{
    public class DiagnosticsAreReturned_ForNonNullableTypeProperty
    {
        [Required]
        public int /*MM*/id { get; set; }

        protected static int static_x { get; set; }

        protected int x { get; set; }

        [Required]
        public int? y { get; set; }
    }
}
