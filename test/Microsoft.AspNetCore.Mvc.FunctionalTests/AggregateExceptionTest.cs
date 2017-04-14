// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.FunctionalTests
{
    public class AggregateExceptionTest : IClassFixture<MvcTestFixture<ErrorPageMiddlewareWebSite.Startup>>
    {
        public AggregateExceptionTest(MvcTestFixture<ErrorPageMiddlewareWebSite.Startup> fixture)
        {
            Client = fixture.Client;
        }

        public HttpClient Client { get; }

        [Fact]
        public async void AggregateExceptionFlattensInnerExceptions()
        {
            // Arrange
            var aggregateException = "AggregateException: One or more errors occurred.";
            var nullReferenceException = "NullReferenceException: Foo cannot be null";
            var indexOutOfRangeException = "IndexOutOfRangeException: Index is out of range";

            // Act
            var response = await Client.GetAsync("http://localhost/AggregateException");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains(aggregateException, content);
            Assert.Contains(nullReferenceException, content);
            Assert.Contains(indexOutOfRangeException, content);
        }
    }
}
