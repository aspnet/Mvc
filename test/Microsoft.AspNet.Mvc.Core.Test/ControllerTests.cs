using Microsoft.AspNet.Mvc.Rendering;
using Xunit;

namespace Microsoft.AspNet.Mvc.Core
{
    public class ControllerTests
    {
        [Fact]
        public void SettingViewData_AlsoUpdatesViewBag()
        {
            // Arrange (eventually passing null to these consturctors will throw)
            var controller = new Controller();
            var originalViewData = controller.ViewData = new ViewDataDictionary<object>(metadataProvider: null);
            var replacementViewData = new ViewDataDictionary<object>(metadataProvider: null);

            // Act
            controller.ViewBag.Hello = "goodbye";
            controller.ViewData = replacementViewData;
            controller.ViewBag.Another = "property";

            // Assert
            Assert.NotSame(originalViewData, controller.ViewData);
            Assert.Same(replacementViewData, controller.ViewData);
            Assert.Null(controller.ViewBag.Hello);
            Assert.Equal("property", controller.ViewBag.Another);
            Assert.Equal("property", controller.ViewData["Another"]);
        }
    }
}
