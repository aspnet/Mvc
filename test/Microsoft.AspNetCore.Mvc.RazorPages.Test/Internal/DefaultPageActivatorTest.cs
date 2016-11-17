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
        public void Create_ThrowsIfActionDescriptorIsNull()
        {
            // Arrange
            var context = new PageContext();
            var activator = new DefaultPageActivator();

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => activator.Create(context),
                "pageContext",
                "The 'ActionDescriptor' property of 'pageContext' must not be null.");
        }

        [Fact]
        public void Create_ThrowsIfPageTypeInfoIsNull()
        {
            // Arrange
            var context = new PageContext()
            {
                ActionDescriptor = new CompiledPageActionDescriptor(),
            };
            var activator = new DefaultPageActivator();

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => activator.Create(context),
                "pageContext",
                "The 'PageTypeInfo' property of 'ActionDescriptor' must not be null.");
        }

        [Theory]
        [InlineData(typeof(TestPage))]
        [InlineData(typeof(PageWithMultipleConstructors))]
        public void Create_ReturnsInstanceOfPage(Type type)
        {
            // Arrange
            var context = new PageContext
            {
                ActionDescriptor = new CompiledPageActionDescriptor
                {
                    PageTypeInfo = type.GetTypeInfo(),
                }
            };
            var activator = new DefaultPageActivator();

            // Act
            var instance = activator.Create(context);

            // Assert
            Assert.NotNull(instance);
            Assert.IsType(type, instance);
        }

        [Fact]
        public void Create_ThrowsIfTypeDoesNotHaveParameterlessConstructor()
        {
            // Arrange
            var context = new PageContext
            {
                ActionDescriptor = new CompiledPageActionDescriptor
                {
                    PageTypeInfo = typeof(PageWithoutParameterlessConstructor).GetTypeInfo(),
                }
            };
            var activator = new DefaultPageActivator();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => activator.Create(context));
        }

        [Fact]
        public void Release_WorksForSimplePageTypes()
        {
            // Arrange
            var context = new PageContext();
            var activator = new DefaultPageActivator();
            var page = new TestPage();

            // Act
            activator.Release(context, page);

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
            activator.Release(context, page);

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
