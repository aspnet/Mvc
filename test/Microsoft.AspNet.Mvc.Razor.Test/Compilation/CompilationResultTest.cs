using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.AspNet.Mvc.Razor.Test
{
    public class CompilationResultTest
    {
        [Fact]
        public void FailedResult_ThrowsWhenAccessingCompiledType()
        {
            // Arrange
            var expected = @"Compilation for 'myfile' failed:
hello
world";
            var result = CompilationResult.Failed("myfile",
                                                 "<h1>hello world</h1>", 
                                                 new[] { new CompilationMessage("hello"), new CompilationMessage("world") },
                                                 new Dictionary<string, object> { { "key", "value" } });

            // Act
            var ex = Assert.Throws<CompilationFailedException>(() => result.CompiledType);

            // Assert
            Assert.Equal(expected, ex.Message);
            Assert.Equal("value", ex.Data["key"]);
        }
    }
}