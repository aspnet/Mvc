// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;
using Microsoft.AspNetCore.Mvc.Core;

namespace Microsoft.AspNetCore.Mvc.ModelBinding
{
    public class CompositeBindingSourceTest
    {
        [Fact]
        public void CompositeBindingSourceTest_CanAcceptDataFrom_ThrowsOnComposite()
        {
            // Arrange
            var expected = Resources.FormatBindingSource_CannotBeComposite("Test Source2", "CanAcceptDataFrom");

            var composite1 = CompositeBindingSource.Create(
                bindingSources: new BindingSource[] { BindingSource.Query, BindingSource.Form },
                displayName: "Test Source1");

            var composite2 = CompositeBindingSource.Create(
              bindingSources: new BindingSource[] { BindingSource.Query, BindingSource.Form },
              displayName: "Test Source2");


            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(
                () => composite1.CanAcceptDataFrom(composite2));
            Assert.StartsWith(expected, exception.Message);
        }

        [Fact]
        public void CompositeBindingSourceTest_CanAcceptDataFrom_Match()
        {
            // Arrange
            var composite = CompositeBindingSource.Create(
                bindingSources: new BindingSource[] { BindingSource.Query, BindingSource.Form },
                displayName: "Test Source1");

            // Act
            var result = composite.CanAcceptDataFrom(BindingSource.Query);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CompositeBindingSourceTest_CanAcceptDataFrom_NoMatch()
        {
            // Arrange
            var composite = CompositeBindingSource.Create(
                bindingSources: new BindingSource[] { BindingSource.Query, BindingSource.Form },
                displayName: "Test Source1");

            // Act
            var result = composite.CanAcceptDataFrom(BindingSource.Path);

            // Assert
            Assert.False(result);
        }
    }
}