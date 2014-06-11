﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;

namespace Microsoft.AspNet.Mvc.Core.Test
{
    public class MvcOptionsTests
    {
        [Fact]
        public void AntiForgeryConfig_SettingNullValue_Throws()
        {
            // Arrange
            var options = new MvcOptions();

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => options.AntiForgeryConfig = null);
            Assert.Equal("The 'AntiForgeryConfig' property of 'Microsoft.AspNet.Mvc.MvcOptions' must not be null." + 
                         "\r\nParameter name: value", ex.Message);
        }
    }
}