﻿// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

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
            Assert.Equal("The 'CookieName' property cannot be null.\r\nParameter name: value", ex.Message);
        }

        [Fact]
        public void FormFieldName_SettingNullValue_Throws()
        {
            // Arrange
            var config = new AntiForgeryConfig();

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => config.FormFieldName = null);
            Assert.Equal("The 'FormFieldName' property cannot be null.\r\nParameter name: value", ex.Message);
        }
    }
}