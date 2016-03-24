using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Core.Test.ApplicationParts
{
    public class ApplicationPartManagerTest
    {

        [Fact]
        public void PopulateFeature_InvokesAllProvidersSequentially_ForAGivenFeature()
        {
            // Arrange
            var manager = new ApplicationPartManager();
            manager.ApplicationParts.Add(new TestPart());
            manager.ApplicationParts.Add(new OtherPartType());
            manager.ApplicationParts.Add(new TestPart());
            manager.Providers.Add(new FeatureProvider(f => f.Value++));
            manager.Providers.Add(new FeatureProvider(f => f.Value = f.Value * 3));

            var feature = new Feature();
            // Act
            manager.PopulateFeature(feature);

            // Assert
            Assert.Equal(18, feature.Value);
        }

        private class TestPart : ApplicationPart
        {
            public override string Name => "Test";
        }

        private class OtherPartType: ApplicationPart
        {
            public override string Name => "Other";
        }

        private class Feature
        {
            public int Value { get; set; }
        }

        private class FeatureProvider : IApplicationFeatureProvider<Feature>
        {
            private readonly Action<Feature> _operation;

            public FeatureProvider(Action<Feature> operation)
            {
                _operation = operation;
            }

            public void GetFeature(IEnumerable<ApplicationPart> parts, Feature feature)
            {
                foreach (var part in parts.OfType<TestPart>())
                {
                    _operation(feature);
                }
            }
        }
    }
}
