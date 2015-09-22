// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNet.Mvc.ModelBinding;
using Xunit;

namespace Microsoft.AspNet.Mvc
{
    public class ModelStateDictionaryExtensionsTest
    {
        [Fact]
        public void AddsModelErrorMessage_ForSingleExpression()
        {
            // Arrange
            var modelState = new ModelStateDictionary();

            // Act
            modelState.AddModelError<TestModel>(model => model.Text, "Message");

            // Assert
            Assert.Equal("Text", modelState.Single().Key);
            Assert.Equal("Message", modelState.Single().Value.Errors.Single().ErrorMessage);
        }

        [Fact]
        public void AddsModelErrorMessage_ForRelationExpression()
        {
            // Arrange
            var modelState = new ModelStateDictionary();

            // Act
            modelState.AddModelError<TestModel>(model => model.Child.Text, "Message");

            // Assert
            Assert.Equal("Child.Text", modelState.Single().Key);
            Assert.Equal("Message", modelState.Single().Value.Errors.Single().ErrorMessage);
        }

        [Fact]
        public void AddsModelErrorMessage_ForImplicitlyCastedToObjectExpression()
        {
            // Arrange
            var modelState = new ModelStateDictionary();

            // Act
            modelState.AddModelError<TestModel>(model => model.Child.Value, "Message");

            // Assert
            Assert.Equal("Child.Value", modelState.Single().Key);
            Assert.Equal("Message", modelState.Single().Value.Errors.Single().ErrorMessage);
        }

        [Fact]
        public void AddsModelErrorMessage_ForExplicitlyCastedToObjectExpression()
        {
            // Arrange
            var modelState = new ModelStateDictionary();

            // Act
            modelState.AddModelError<TestModel>(model => (object)model.Child.Value, "Message");

            // Assert
            Assert.Equal("Child.Value", modelState.Single().Key);
            Assert.Equal("Message", modelState.Single().Value.Errors.Single().ErrorMessage);
        }

        [Fact]
        public void AddsModelErrorMessage_ForExpressionWithoutTextRepresentation()
        {
            // Arrange
            var modelState = new ModelStateDictionary();

            // Act
            modelState.AddModelError<TestModel>(model => model.ToString(), "Message");

            // Assert
            Assert.Equal("", modelState.Single().Key);
            Assert.Equal("Message", modelState.Single().Value.Errors.Single().ErrorMessage);
        }

        [Fact]
        public void AddsModelErrorException_ForSingleExpression()
        {
            // Arrange
            var exception = new Exception();
            var modelState = new ModelStateDictionary();

            // Act
            modelState.AddModelError<TestModel>(model => model.Text, exception);

            // Assert
            Assert.Equal("Text", modelState.Single().Key);
            Assert.Same(exception, modelState.Single().Value.Errors.Single().Exception);
        }

        [Fact]
        public void AddsModelErrorException_ForRelationExpression()
        {
            // Arrange
            var exception = new Exception();
            var modelState = new ModelStateDictionary();

            // Act
            modelState.AddModelError<TestModel>(model => model.Child.Text, exception);

            // Assert
            Assert.Equal("Child.Text", modelState.Single().Key);
            Assert.Same(exception, modelState.Single().Value.Errors.Single().Exception);
        }

        [Fact]
        public void AddsModelErrorException_ForImplicitlyCastedToObjectExpression()
        {
            // Arrange
            var exception = new Exception();
            var modelState = new ModelStateDictionary();

            // Act
            modelState.AddModelError<TestModel>(model => model.Child.Value, exception);

            // Assert
            Assert.Equal("Child.Value", modelState.Single().Key);
            Assert.Same(exception, modelState.Single().Value.Errors.Single().Exception);
        }

        [Fact]
        public void AddsModelErrorException_ForExplicitlyCastedToObjectExpression()
        {
            // Arrange
            var exception = new Exception();
            var modelState = new ModelStateDictionary();

            // Act
            modelState.AddModelError<TestModel>(model => (object)model.Child.Value, exception);

            // Assert
            Assert.Equal("Child.Value", modelState.Single().Key);
            Assert.Same(exception, modelState.Single().Value.Errors.Single().Exception);
        }

        [Fact]
        public void AddsModelErrorException_ForExpressionWithoutTextRepresentation()
        {
            // Arrange
            var exception = new Exception();
            var modelState = new ModelStateDictionary();

            // Act
            modelState.AddModelError<TestModel>(model => model.ToString(), exception);

            // Assert
            Assert.Equal("", modelState.Single().Key);
            Assert.Same(exception, modelState.Single().Value.Errors.Single().Exception);
        }

        [Fact]
        public void RemovesModelStateKey_ForSingleExpression()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.Add("Text", new ModelState());

            // Act
            modelState.Remove<TestModel>(model => model.Text);

            // Assert
            Assert.Empty(modelState);
        }

        [Fact]
        public void RemovesModelStateKey_ForRelationExpression()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.Add("Child.Text", new ModelState());

            // Act
            modelState.Remove<TestModel>(model => model.Child.Text);

            // Assert
            Assert.Empty(modelState);
        }

        [Fact]
        public void RemovesModelStateKey_ForImplicitlyCastedToObjectExpression()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.Add("Child.Value", new ModelState());

            // Act
            modelState.Remove<TestModel>(model => model.Child.Value);

            // Assert
            Assert.Empty(modelState);
        }

        [Fact]
        public void RemovesModelStateKey_ForExplicitlyCastedToObjectExpression()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.Add("Child.Value", new ModelState());

            // Act
            modelState.Remove<TestModel>(model => (object)model.Child.Value);

            // Assert
            Assert.Empty(modelState);
        }

        [Fact]
        public void RemovesModelStateKey_ForExpressionWithoutTextRepresentation()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.Add("", new ModelState());

            // Act
            modelState.Remove<TestModel>(model => model.ToString());

            // Assert
            Assert.Empty(modelState);
        }

        private class TestModel
        {
            public string Text { get; set; }

            public ChildModel Child { get; set; }
        }

        private class ChildModel
        {
            public int Value { get; set; }
            public string Text { get; set; }
        }
    }
}
