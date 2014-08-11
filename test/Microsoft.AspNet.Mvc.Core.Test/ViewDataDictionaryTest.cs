// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.ModelBinding;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Core
{
    public class ViewDataDictionaryTest
    {
        [Fact]
        public void ConstructorInitalizesMembers()
        {
            // Arrange
            var metadataProvider = new EmptyModelMetadataProvider();
            var modelState = new ModelStateDictionary();

            // Act
            var viewData = new ViewDataDictionary(metadataProvider, modelState);

            // Assert
            Assert.Same(metadataProvider, viewData.MetadataProvider);
            Assert.Same(modelState, viewData.ModelState);
            Assert.NotNull(viewData.TemplateInfo);
            Assert.Null(viewData.Model);
            Assert.Null(viewData.ModelMetadata);
            Assert.Equal(0, viewData.Count);
        }

        [Fact]
        public void CopyConstructorInitalizesModelAndModelMetadataBasedOnSource()
        {
            // Arrange
            var metadataProvider = new EmptyModelMetadataProvider();
            var model = new TestModel();
            var source = new ViewDataDictionary(metadataProvider)
            {
                Model = model
            };
            source["foo"] = "bar";

            // Act
            var viewData = new ViewDataDictionary(source);

            // Assert
            Assert.Same(metadataProvider, viewData.MetadataProvider);
            Assert.NotNull(viewData.ModelState);
            Assert.NotNull(viewData.TemplateInfo);
            Assert.Same(model, viewData.Model);
            Assert.Equal(typeof(TestModel), viewData.ModelMetadata.ModelType);
            Assert.Equal("bar", viewData["foo"]);
        }

        [Fact]
        public void CopyConstructorUsesPassedInModel()
        {
            // Arrange
            var metadataProvider = new EmptyModelMetadataProvider();
            var model = new TestModel();
            var source = new ViewDataDictionary(metadataProvider)
            {
                Model = "string model"
            };
            source["key1"] = "value1";

            // Act
            var viewData = new ViewDataDictionary(source, model);

            // Assert
            Assert.Same(metadataProvider, viewData.MetadataProvider);
            Assert.NotNull(viewData.ModelState);
            Assert.NotNull(viewData.TemplateInfo);
            Assert.Same(model, viewData.Model);
            Assert.Equal(typeof(TestModel), viewData.ModelMetadata.ModelType);
            Assert.Equal("value1", viewData["key1"]);
        }

        private class TestModel
        {
        }
    }
}