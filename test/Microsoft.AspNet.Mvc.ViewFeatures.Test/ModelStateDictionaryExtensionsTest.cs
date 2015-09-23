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
        public void AddModelError_ForSingleExpression_AddsExpectedMessage()
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
        public void AddModelError_ForRelationExpression_AddsExpectedMessage()
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
        public void AddModelError_ForImplicitlyCastedToObjectExpression_AddsExpectedMessage()
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
        public void AddModelError_ForExplicitlyCastedToObjectExpression_AddsExpectedMessage()
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
        public void AddModelError_ForExpressionWithoutStringRepresentation_AddsExpectedMessage()
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
        public void AddModelError_ForSingleExpression_AddsExpectedException()
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
        public void AddModelError_ForRelationExpression_AddsExpectedException()
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
        public void AddModelError_ForImplicitlyCastedToObjectExpression_AddsExpectedException()
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
        public void AddModelError_ForExplicitlyCastedToObjectExpression_AddsExpectedException()
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
        public void AddModelError_ForExpressionWithoutStringRepresentation_AddsExpectedException()
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
        public void Remove_ForSingleExpression_RemovesModelStateKey()
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
        public void Remove_ForRelationExpression_RemovesModelStateKey()
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
        public void Remove_ForImplicitlyCastedToObjectExpression_RemovesModelStateKey()
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
        public void Remove_ForExplicitlyCastedToObjectExpression_RemovesModelStateKey()
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
        public void Remove_ForExpressionWithoutStringRepresentation_RemovesModelStateKey()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.Add("", new ModelState());

            // Act
            modelState.Remove<TestModel>(model => model.ToString());

            // Assert
            Assert.Empty(modelState);
        }

        [Fact]
        public void RemoveAll_ForSingleExpression_RemovesModelStateKeys()
        {
            // Arrange
            var state = new ModelState();
            var modelState = new ModelStateDictionary();

            modelState.Add("Key", state);
            modelState.Add("Text", new ModelState());
            modelState.Add("Text.Length", new ModelState());

            // Act
            modelState.RemoveAll<TestModel>(model => model.Text);

            // Assert
            Assert.Equal("Key", modelState.Single().Key);
            Assert.Same(state, modelState.Single().Value);
        }

        [Fact]
        public void RemoveAll_ForRelationExpression_RemovesModelStateKeys()
        {
            // Arrange
            var state = new ModelState();
            var modelState = new ModelStateDictionary();

            modelState.Add("Key", state);
            modelState.Add("Child", new ModelState());
            modelState.Add("Child.Text", new ModelState());

            // Act
            modelState.RemoveAll<TestModel>(model => model.Child);

            // Assert
            Assert.Equal("Key", modelState.Single().Key);
            Assert.Same(state, modelState.Single().Value);
        }

        [Fact]
        public void RemoveAll_ForImplicitlyCastedToObjectExpression_RemovesModelStateKeys()
        {
            // Arrange
            var state = new ModelState();
            var modelState = new ModelStateDictionary();

            modelState.Add("Child", state);
            modelState.Add("Child.Value", new ModelState());

            // Act
            modelState.RemoveAll<TestModel>(model => model.Child.Value);

            // Assert
            Assert.Equal("Child", modelState.Single().Key);
            Assert.Same(state, modelState.Single().Value);
        }

        [Fact]
        public void RemoveAll_ForExplicitlyCastedToObjectExpression_RemovesModelStateKeys()
        {
            // Arrange
            var state = new ModelState();
            var modelState = new ModelStateDictionary();

            modelState.Add("Child", state);
            modelState.Add("Child.Value", new ModelState());

            // Act
            modelState.RemoveAll<TestModel>(model => (object)model.Child.Value);

            // Assert
            Assert.Equal("Child", modelState.Single().Key);
            Assert.Same(state, modelState.Single().Value);
        }

        [Fact]
        public void RemoveAll_ForExpressionWithoutStringRepresentation_RemovesModelPropertyKeys()
        {
            // Arrange
            var state = new ModelState();
            var modelState = new ModelStateDictionary();

            modelState.Add("Key", state);
            modelState.Add("Text", new ModelState());
            modelState.Add("Child", new ModelState());
            modelState.Add("Child.Text", new ModelState());
            modelState.Add("Child.NoValue", new ModelState());

            // Act
            modelState.RemoveAll<TestModel>(model => model.ToString());

            // Assert
            Assert.Equal("Key", modelState.Single().Key);
            Assert.Same(state, modelState.Single().Value);
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
