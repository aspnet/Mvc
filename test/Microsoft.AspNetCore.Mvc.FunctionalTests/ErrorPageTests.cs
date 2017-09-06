// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.FunctionalTests
{
    /// <summary>
    /// Functional test to verify the error reporting of Razor compilation by diagnostic middleware.
    /// </summary>
    public class ErrorPageTests : IClassFixture<MvcTestFixture<ErrorPageMiddlewareWebSite.Startup>>
    {
        public ErrorPageTests(MvcTestFixture<ErrorPageMiddlewareWebSite.Startup> fixture)
        {
            Client = fixture.Client;
        }

        public HttpClient Client { get; }

        [Theory]
        [InlineData("CompilationFailure", "Cannot implicitly convert type &#x27;int&#x27; to &#x27;string&#x27;")]
        [InlineData("ParserError",
            "The code block is missing a closing &quot;}&quot; character.  Make sure you " +
            "have a matching &quot;}&quot; character for all the &quot;{&quot; characters " +
            "within this block, and that none of the &quot;}&quot; characters are being " +
            "interpreted as markup.")]
        public async Task CompilationFailuresAreListedByErrorPageMiddleware(string action, string expected)
        {
            // Arrange
            var expectedMediaType = MediaTypeHeaderValue.Parse("text/html; charset=utf-8");

            // Act
            var response = await Client.GetAsync("http://localhost/" + action);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal(expectedMediaType, response.Content.Headers.ContentType);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains($"/Views/ErrorPageMiddleware/{action}.cshtml", content);
            Assert.Contains(expected, content);
        }

        [Fact]
        public async Task CompilationFailuresFromViewImportsAreListed()
        {
            // Arrange
            var expectedMessage = "The type or namespace name &#x27;NamespaceDoesNotExist&#x27; could not be found ("
                + "are you missing a using directive or an assembly reference?)";
            var expectedMediaType = MediaTypeHeaderValue.Parse("text/html; charset=utf-8");

            // Act
            var response = await Client.GetAsync("http://localhost/ErrorFromViewImports");

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal(expectedMediaType, response.Content.Headers.ContentType);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("/Views/ErrorFromViewImports/_ViewImports.cshtml", content);
            Assert.Contains(expectedMessage, content);
        }

        [Fact]
        public async Task RuntimeErrorAreListedByErrorPageMiddleware()
        {
            // The desktop CLR does not correctly read the stack trace from portable PDBs. However generating full pdbs
            // is only supported on machines with CLSID_CorSymWriter available. On desktop, we'll skip this test on 
            // machines without this component.
#if NET451
            if (!SymbolsUtility.SupportsFullPdbGeneration())
            {
                return;
            }
#endif

            // Arrange
            var expectedMessage = HtmlEncoder.Default.Encode("throw new Exception(\"Error from view\");");
            var expectedMediaType = MediaTypeHeaderValue.Parse("text/html; charset=utf-8");

            // Act
            var response = await Client.GetAsync("http://localhost/RuntimeError");

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal(expectedMediaType, response.Content.Headers.ContentType);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("/Views/ErrorPageMiddleware/RuntimeError.cshtml", content);
            Assert.Contains(expectedMessage, content);
        }
    }
}