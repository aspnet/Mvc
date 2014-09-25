using Microsoft.AspNet.Mvc.Core;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Rendering
{
    /// <summary>
    /// Tests the <see cref="HtmlHelper"/> class.
    /// </summary>
    public class HtmlHelperTest
    {
        [Fact]
        public void ActionLink_CallsUrlHelper_WithExpectedValues()
        {
            //Arrange
            var action = "Details";
            var controller = "Product";
            var protocol = "https";
            var hostname = "www.contoso.com";
            var fragment = "h1";
            var routeValues = new { isprint = "true", showreviews = "true" };
            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(h => h.Action(action, controller, routeValues, protocol, hostname, fragment)).Verifiable();
            var htmlHelper = DefaultTemplatesUtilities.GetHtmlHelper(urlHelper.Object);

            // Act
            var url = htmlHelper.ActionLink("Details",
                                            actionName: action,
                                            controllerName: controller,
                                            protocol: protocol,
                                            hostname: hostname,
                                            fragment: fragment,
                                            routeValues: routeValues,
                                            htmlAttributes: null);

            // Assert
            urlHelper.Verify();
        }

        [Fact]
        public void RouteLink_CallsUrlHelper_WithExpectedValues()
        {
            //Arrange
            var routeName = "default";
            var protocol = "https";
            var hostname = "www.contoso.com";
            var fragment = "h1";
            var routeValues = new { action = "Details", controller = "Product", showreviews = "true" };
            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(h => h.RouteUrl(routeName, routeValues, protocol, hostname, fragment)).Verifiable();
            var htmlHelper = DefaultTemplatesUtilities.GetHtmlHelper(urlHelper.Object);

            // Act
            var url = htmlHelper.RouteLink("Details",
                                            routeName: routeName,
                                            protocol: protocol,
                                            hostName: hostname,
                                            fragment: fragment,
                                            routeValues: routeValues,
                                            htmlAttributes: null);

            // Assert
            urlHelper.Verify();
        }
    }
}