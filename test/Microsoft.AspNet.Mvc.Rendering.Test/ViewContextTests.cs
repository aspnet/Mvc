using Xunit;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public class ViewContextTests
    {
        [Fact]
        public void SettingViewData_AlsoUpdatesViewBag()
        {
            // Arrange (eventually passing null to these consturctors will throw)
            var context = new ViewContext(null, null, null);
            var originalViewData = context.ViewData = new ViewData(metadataProvider: null);
            var replacementViewData = new ViewData(metadataProvider: null);

            // Act
            context.ViewBag.Hello = "goodbye";
            context.ViewData = replacementViewData;
            context.ViewBag.Another = "property";

            // Assert
            Assert.NotSame(originalViewData, context.ViewData);
            Assert.Same(replacementViewData, context.ViewData);
            Assert.Null(context.ViewBag.Hello);
            Assert.Equal("property", context.ViewBag.Another);
        }
    }
}
