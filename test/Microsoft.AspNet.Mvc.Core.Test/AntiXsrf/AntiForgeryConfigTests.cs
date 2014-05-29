// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;

namespace Microsoft.AspNet.Mvc.Core.Test
{
    public class AntiForgeryConfigTests
    {
        [Fact]
        public void CookieName_SettingNullValue_Throws()
        {
            // Arrange
            var config = new AntiForgeryConfig();

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => config.CookieName = null);
            Assert.Equal("The 'CookieName' property of 'Microsoft.AspNet.Mvc.AntiForgeryConfig' must not be null." + 
                         "\r\nParameter name: value", ex.Message);
        }

        [Fact]
        public void FormFieldName_SettingNullValue_Throws()
        {
            // Arrange
            var config = new AntiForgeryConfig();

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => config.FormFieldName = null);
            Assert.Equal("The 'FormFieldName' property of 'Microsoft.AspNet.Mvc.AntiForgeryConfig' must not be null." +
                         "\r\nParameter name: value", ex.Message);
        }
    }
}