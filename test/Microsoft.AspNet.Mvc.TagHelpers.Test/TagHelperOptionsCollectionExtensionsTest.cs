using System;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.OptionsModel;
using Xunit;

namespace Microsoft.AspNet.Mvc.TagHelpers.Test
{
    public class TagHelperOptionsCollectionExtensionsTest
    {
        [Fact]
        public void ConfigureForm_GetsOptionsFromConfigurationCorrectly()
        {
            // Arrange
            var config = new Configuration();
            config.Set("FormTagHelperOptions:GenerateAntiForgeryToken", "true");
            var services = new ServiceCollection();

            // Act
            services.ConfigureTagHelpers()
                .ConfigureForm(config);
            var options = services.BuildServiceProvider()
                .GetService<IOptions<FormTagHelperOptions>>()
                .Options;

            // Assert
            Assert.NotNull(options.GenerateAntiForgeryToken);
            Assert.True(options.GenerateAntiForgeryToken.Value);
        }
    }
}