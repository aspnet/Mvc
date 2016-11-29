// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Formatters.Json.Test
{
    public class JsonPatchOperationsArrayProviderTests
    {
        [Fact]
        public void OnProvidersExecuting_FindsJsonPatchDocuments_ProvidesOperationsArray()
        {
            // Arrange
            var provider = new JsonPatchOperationsArrayProvider();
            var jsonPatchParameterDescription = new ApiParameterDescription
            {
                Type = typeof(JsonPatchDocument)
            };

            var stringParameterDescription = new ApiParameterDescription
            {
                Type = typeof(string)
            };

            var apiDescription = new ApiDescription();
            apiDescription.ParameterDescriptions.Clear();
            apiDescription.ParameterDescriptions.Add(jsonPatchParameterDescription);
            apiDescription.ParameterDescriptions.Add(stringParameterDescription);

            var actionDescriptorList = new List<ActionDescriptor>();
            var apiDescriptionProviderContext = new ApiDescriptionProviderContext(actionDescriptorList);
            apiDescriptionProviderContext.Results.Add(apiDescription);

            // Act
            provider.OnProvidersExecuting(apiDescriptionProviderContext);

            // Assert
            Assert.Equal(2, apiDescription.ParameterDescriptions.Count);
            Assert.True(apiDescription.ParameterDescriptions.Any(d => d.Type == typeof(Operation[])));
            Assert.False(apiDescription.ParameterDescriptions.Any(d => d.Type == typeof(IJsonPatchDocument)));
        }
    }
}
