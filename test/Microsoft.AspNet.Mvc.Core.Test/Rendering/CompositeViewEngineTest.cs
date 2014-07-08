// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public class CompositeViewEngineTest
    {
	    [Fact]
        public void FindView_ReturnsViewFromFirstViewEngineWithFoundResult()
        {
            // Arrange
            var viewName = "foo";
            var engine1 = new Mock<IViewEngine>();
            var engine2 = new Mock<IViewEngine>();
            var engine3 = new Mock<IViewEngine>();
            var view2 = Mock.Of<IView>();
            var view3 = Mock.Of<IView>();
            engine1.Setup(e => e.FindView(It.IsAny<IDictionary<string, object>>(), It.IsAny<string>()))
                   .Returns(ViewEngineResult.NotFound(viewName, Enumerable.Empty<string>()));
            engine2.Setup(e => e.FindView(It.IsAny<IDictionary<string, object>>(), It.IsAny<string>()))
                   .Returns(ViewEngineResult.Found(viewName, view2));
            engine2.Setup(e => e.FindView(It.IsAny<IDictionary<string, object>>(), It.IsAny<string>()))
                   .Returns(ViewEngineResult.Found(viewName, view3));

            var provider = new Mock<IViewEnginesProvider>();
            provider.SetupGet(p => p.)
            // Act


        }
    }
}