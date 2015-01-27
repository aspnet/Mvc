// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Logging
{
    public class DefaultControllerTypeProviderLoggingTest
    {
        [Fact]
        public void SimpleController_AssemblyDiscovery()
        {
            // Arrange
            var sink = new TestSink();
            var loggerFactory = new TestLoggerFactory(sink);
            var assemblyProvider = new Mock<IAssemblyProvider>();
            assemblyProvider.SetupGet(p => p.CandidateAssemblies)
                            .Returns(new[] { GetType().Assembly });
            var provider = new DefaultControllerTypeProvider(assemblyProvider.Object,
                                                 loggerFactory);

            // Act
            provider.GetControllerTypes();

            // Assert
            var writeContext = Assert.Single(sink.Writes);

            var assemblyValues = Assert.IsType<AssemblyValues>(writeContext.State);
            Assert.NotNull(assemblyValues);
            Assert.True(assemblyValues.AssemblyName.Contains("Microsoft.AspNet.Mvc.Core.Test"));
        }
    }
}