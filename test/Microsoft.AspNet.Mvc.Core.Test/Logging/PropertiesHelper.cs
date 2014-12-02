using System;
using System.Linq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Test.Logging
{
    public class PropertiesHelper
    {
        /// <summary>
        /// Given two types, compares their properties and asserts true if they have the same property names
        /// </summary>
        /// <param name="original">The original type to compare against</param>
        /// <param name="shadow">The shadow type whose properties will be compared against the original</param>
        /// <param name="exclude">Any properties that should be ignored (exists in the original type but not the shadow)</param>
        public static void AssertPropertiesAreTheSame(Type original, Type shadow, string[] exclude = null)
        {
            var originalProperties = original.GetProperties().Where(p => !exclude?.Contains(p.Name) ?? true)
                .Select(p => p.Name).OrderBy(n => n);
            var shadowProperties = shadow.GetProperties()
                .Select(p => p.Name).OrderBy(n => n);

            Assert.True(originalProperties.SequenceEqual(shadowProperties));
        }
    }
}