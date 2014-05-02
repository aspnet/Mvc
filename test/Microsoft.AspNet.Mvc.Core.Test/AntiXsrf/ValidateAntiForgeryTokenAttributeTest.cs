using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.DependencyInjection.Fallback;
using Microsoft.AspNet.Security.DataProtection;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Core.Test
{
    public class ValidateAntiForgeryTokenAttributeTest
    {
        [Fact]
        public void ValidationAttribute_ForwardsCallToValidateAntiForgeryTokenAuthorizationFilter()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddInstance<AntiForgery>(GetAntiForgeryInstance());
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var attribute = new ValidateAntiForgeryTokenAttribute();

            // Act
            var filter = attribute.CreateInstance(serviceProvider);

            // Assert
            var validationFilter = filter as ValidateAntiForgeryTokenAuthorizationFilter;
            Assert.NotNull(validationFilter);
        }

        private AntiForgery GetAntiForgeryInstance()
        {
            var claimExtractor = new Mock<IClaimUidExtractor>();
            var dataProtectionProvider = new Mock<IDataProtectionProvider>();
            var additionalDataProvider = new Mock<IAntiForgeryAdditionalDataProvider>();
            return new AntiForgery(claimExtractor.Object,
                                   dataProtectionProvider.Object,
                                   additionalDataProvider.Object);
        }
    }
}
