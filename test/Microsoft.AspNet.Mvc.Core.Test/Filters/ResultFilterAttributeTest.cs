// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NET45
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Test
{
    // These tests share code with the ActionFilterAttribute tests because the I{Async}ResultFilter
    // implementations need to behave the same way.
    public class ResultFilterAttributeTest
    {
        [Fact]
        public async Task ResultFilterAttribute_ResultFilter_CallsBothMethods()
        {
            await ActionFilterAttributeTests.ResultFilter_CallsBothMethods(
                new Mock<ResultFilterAttribute>());
        }

        [Fact]
        public async Task ResultFilterAttribute_ResultFilter_SettingResult_DoesNotShortCircuit()
        {
            await ActionFilterAttributeTests.ResultFilter_SettingResult_DoesNotShortCircuit(
                new Mock<ResultFilterAttribute>());
        }

        [Fact]
        public async Task ResultFilterAttribute_ResultFilter_SettingCancel_ShortCircuits()
        {
            await ActionFilterAttributeTests.ResultFilter_SettingCancel_ShortCircuits(
                new Mock<ResultFilterAttribute>());
        }
    }
}
#endif