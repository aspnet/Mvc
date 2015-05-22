// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if DNX451
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNet.Http.Internal;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Routing;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.ViewComponents
{
    public class DefaultViewComponentActivatorTests
    {
        [Fact]
        public void DefaultViewComponentActivator_ActivatesViewComponentContext()
        {
            // Arrange
            var activator = new DefaultViewComponentActivator();

            var context = new ViewComponentContext();
            var instance = new TestViewComponent();

            // Act
            activator.Activate(instance, context);

            // Assert
            Assert.Same(context, instance.ViewComponentContext);
        }

        private class TestViewComponent : ViewComponent
        {
            public Task ExecuteAsync()
            {
                throw new NotImplementedException();
            }
        }
    }
}
#endif
