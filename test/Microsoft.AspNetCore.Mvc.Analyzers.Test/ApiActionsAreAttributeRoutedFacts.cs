// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Analyzers.Infrastructure;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Analyzers
{
    public class ApiActionsAreAttributeRoutedFacts : CodeFixVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
            => new ApiActionsAreAttributeRoutedAnalyzer();

        protected override CodeFixProvider GetCSharpCodeFixProvider()
            => new ApiActionsAreAttributeRoutedFixProvider();

        [Fact]
        public void NoDiagnosticsAreReturned_FoEmptyScenarios()
        {
            // Arrange
            var test = @"";

            // Act & Assert
            VerifyCSharpDiagnostic(test);
        }

        [Fact]
        public void NoDiagnosticsAreReturned_WhenTypeIsNotApiController()
        {
            // Arrange
            var test =
@"
using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    public IActionResult Index() => View();
}
";

            // Act & Assert
            VerifyCSharpDiagnostic(test);
        }

        [Fact]
        public void NoDiagnosticsAreReturned_WhenApiControllerActionHasAttribute()
        {
            // Arrange
            var test =
@"
using Microsoft.AspNetCore.Mvc;

[ApiController]
public class PetController : Controller
{
    [HttpGet]
    public int GetPetId() => 0;
}
";

            // Act & Assert
            VerifyCSharpDiagnostic(test);
        }

        [Fact]
        public void NoDiagnosticsAreReturned_ForNonActions()
        {
            // Arrange
            var test =
@"
using Microsoft.AspNetCore.Mvc;

[ApiController]
public class PetController : Controller
{
    private int GetPetIdPrivate() => 0;
    protected int GetPetIdProtected() => 0;
    public static IActionResult FindPetByStatus(int status) => null;
    [NonAction]
    public object Reset(int state) => null;
}
";

            // Act & Assert
            VerifyCSharpDiagnostic(test);
        }

        [Fact]
        public async Task DiagnosticsAndCodeFixes_WhenApiControllerActionDoesNotHaveAttribute()
        {
            // Arrange
            var expectedDiagnostic = new DiagnosticResult
            {
                Id = "MVC1000",
                Message = "Actions on types annotated with ApiControllerAttribute must be attribute routed.",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test.cs", 8, 16) }
            };
            var test =
@"
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route]
public class PetController : Controller
{
    public int GetPetId() => 0;
}
";
            var expectedFix =
@"
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route]
public class PetController : Controller
{
    [HttpGet]
    public int GetPetId() => 0;
}
";

            // Act & Assert
            VerifyCSharpDiagnostic(test, expectedDiagnostic);
            await VerifyCSharpFixAsync(test, expectedFix);
        }

        [Fact]
        public async Task CodeFixes_ApplyFullyQualifiedNames()
        {
            // Arrange
            var test =
@"
[Microsoft.AspNetCore.Mvc.ApiController]
[Microsoft.AspNetCore.Mvc.Route]
public class PetController
{
    public object GetPet() => null;
}
";
            var expectedFix =
@"
[Microsoft.AspNetCore.Mvc.ApiController]
[Microsoft.AspNetCore.Mvc.Route]
public class PetController
{
    [Microsoft.AspNetCore.Mvc.HttpGet]
    public object GetPet() => null;
}
";

            // Act & Assert
            await VerifyCSharpFixAsync(test, expectedFix);
        }

        [Theory]
        [InlineData("id")]
        [InlineData("petId")]
        public async Task CodeFixes_WithIdParameter(string idParameter)
        {
            // Arrange
            var test =
$@"
using Microsoft.AspNetCore.Mvc;
[ApiController]
[Route]
public class PetController
{{
    public IActionResult Post(string notid, int {idParameter}) => null;
}}";
            var expectedFix =
$@"
using Microsoft.AspNetCore.Mvc;
[ApiController]
[Route]
public class PetController
{{
    [HttpPost(""{{{idParameter}}}"")]
    public IActionResult Post(string notid, int {idParameter}) => null;
}}";

            // Act & Assert
            await VerifyCSharpFixAsync(test, expectedFix);
        }

        [Fact]
        public async Task CodeFixes_WithRouteParameter()
        {
            // Arrange
            var test =
@"
using Microsoft.AspNetCore.Mvc;
[ApiController]
[Route]
public class PetController
{
    public IActionResult DeletePetByStatus([FromRoute] Status status, [FromRoute] Category category) => null;
}";
            var expectedFix =
@"
using Microsoft.AspNetCore.Mvc;
[ApiController]
[Route]
public class PetController
{
    [HttpDelete(""{status}/{category}"")]
    public IActionResult DeletePetByStatus([FromRoute] Status status, [FromRoute] Category category) => null;
}";

            // Act & Assert
            await VerifyCSharpFixAsync(test, expectedFix);
        }

        [Fact]
        public async Task CodeFixes_WhenAttributeCannotBeInferred()
        {
            // Arrange
            var test =
@"
using Microsoft.AspNetCore.Mvc;
[ApiController]
[Route]
public class PetController
{
    public IActionResult ModifyPet() => null;
}";
            var expectedFix =
@"
using Microsoft.AspNetCore.Mvc;
[ApiController]
[Route]
public class PetController
{
    [HttpPut]
    public IActionResult ModifyPet() => null;
}";

            // Act & Assert
            // There isn't a good way to test all fixes simultaneously. We'll pick the last one to verify we
            // have 4
            await VerifyCSharpFixAsync(test, expectedFix, codeFixIndex: 3);
        }
    }
}