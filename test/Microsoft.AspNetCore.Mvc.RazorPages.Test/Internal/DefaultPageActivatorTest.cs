// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Testing;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Internal
{
    public class DefaultPageActivatorTest
    {
        [Fact]
        public void Create_ThrowsIfPageTypeInfoIsNull()
        {
            // Arrange
            var descriptor = new CompiledPageActionDescriptor();
            var activator = new DefaultPageActivator();

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => activator.CreateActivator(descriptor),
                "actionDescriptor",
                "The 'PageTypeInfo' property of 'actionDescriptor' must not be null.");
        }

        [Theory]
        [InlineData(typeof(TestPage))]
        [InlineData(typeof(PageWithMultipleConstructors))]
        public void Create_ReturnsFactoryForPage(Type type)
        {
            // Arrange
            var pageContext = new PageContext();
            var descriptor = new CompiledPageActionDescriptor
            {
                PageTypeInfo = type.GetTypeInfo(),
            };

            var activator = new DefaultPageActivator();

            // Act
            var factory = activator.CreateActivator(descriptor);
            var instance = factory(pageContext);

            // Assert
            Assert.NotNull(instance);
            Assert.IsType(type, instance);
        }

        [Fact]
        public void Create_ThrowsIfTypeDoesNotHaveParameterlessConstructor()
        {
            // Arrange
            var descriptor = new CompiledPageActionDescriptor
            {
                PageTypeInfo = typeof(PageWithoutParameterlessConstructor).GetTypeInfo(),
            };
            var pageContext = new PageContext();
            var activator = new DefaultPageActivator();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => activator.CreateActivator(descriptor));
        }

        [Fact]
        public void Release_WorksForSimplePageTypes()
        {
            // Arrange
            var context = new PageContext();
            var activator = new DefaultPageActivator();
            var page = new TestPage();

            // Act
            var disposer = activator.CreateDisposer(new CompiledPageActionDescriptor());
            disposer(context, page);

            // Assert
            // If we got this far, everything is fine.
        }

        [Fact]
        public void Release_DisposesDisposableTypes()
        {
            // Arrange
            var context = new PageContext();
            var activator = new DefaultPageActivator();
            var page = new DisposablePage();

            // Act
            var disposer = activator.CreateDisposer(new CompiledPageActionDescriptor());
            disposer(context, page);

            // Assert
            Assert.True(page.Disposed);
        }

        private class TestPage : Page
        {
            public override Task ExecuteAsync()
            {
                throw new NotImplementedException();
            }
        }

        private class PageWithMultipleConstructors : Page
        {
            public PageWithMultipleConstructors(int x)
            {

            }

            public PageWithMultipleConstructors()
            {

            }

            public override Task ExecuteAsync()
            {
                throw new NotImplementedException();
            }
        }

        private class PageWithoutParameterlessConstructor : Page
        {
            public PageWithoutParameterlessConstructor(ILogger logger)
            {
            }

            public override Task ExecuteAsync()
            {
                throw new NotImplementedException();
            }
        }

        private class DisposablePage : TestPage, IDisposable
        {
            public bool Disposed { get; private set; }

            public void Dispose()
            {
                Disposed = true;
            }
        }
    }
}
