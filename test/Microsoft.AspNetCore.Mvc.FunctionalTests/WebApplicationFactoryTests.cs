using System;
using Microsoft.AspNetCore.Mvc.Testing;
using RazorPagesClassLibrary;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.FunctionalTests
{
    public class WebApplicationFactoryTests
    {
        /// <summary>
        /// Verifies that a non entry point class will throw an <see cref="InvalidOperationException"/> 
        /// in the constructor of <see cref="WebApplicationFactory{TEntryPoint}"/>
        /// </summary>
        [Fact]
        public void ConstructorThrowsInvalidOperationForNonEntryPoint()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => new WebApplicationFactory<ClassLibraryStartup>());
            // Use string literal as Mvc.Testing.Resources is inaccessable (marked as internal)
            Assert.Equal($"The provided Type '{typeof(ClassLibraryStartup).Name}' does not belong to an assembly with an entry point. A common cause for this error is providing a Type from a class library.",
                ex.Message);
        }
    }
}
