using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Security.DataProtection;
using Microsoft.Framework.OptionsModel;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Core.Test
{
    /// <summary>
    /// Summary description for DefaultEditorTemplatesTest
    /// </summary>
    public class DefaultDisplayTemplatesTest
    {
        [Fact]
        public void ObjectTemplate_IgnoresPropertiesWith_ScaffoldColumnFalse()
        {
            // Arrange
            var expected =
@"<div class=""display-label"">Property1</div>
<div class=""display-field""></div>
<div class=""display-label"">Property2</div>
<div class=""display-field""></div>
<div class=""display-label"">Property3</div>
<div class=""display-field""></div>
";
            var model = new ObjectWithScaffoldColumn();
            var htmlHelper = CreateHtmlHelper(model);

            // Act
            var result = DefaultDisplayTemplates.ObjectTemplate(htmlHelper);

            // Assert
            Assert.Equal(expected, result);
        }

        private static HtmlHelper CreateHtmlHelper(object model)
        {
            var httpContext = new Mock<HttpContext>();
            var routeContext = new RouteContext(httpContext.Object);
            var actionContext = new ActionContext(routeContext, new ActionDescriptor());
            var modelMetadataProvider = new DataAnnotationsModelMetadataProvider();
            var options = new Mock<IOptionsAccessor<MvcOptions>>();
            options.SetupGet(o => o.Options)
                   .Returns(new MvcOptions());
            var antiForgery = new AntiForgery(Mock.Of<IClaimUidExtractor>(),
                                              Mock.Of<IDataProtectionProvider>(),
                                              Mock.Of<IAntiForgeryAdditionalDataProvider>(),
                                              options.Object);
            var actionBindingContextProvider = new Mock<IActionBindingContextProvider>();
            actionBindingContextProvider.Setup(c => c.GetActionBindingContextAsync(It.IsAny<ActionContext>()))
                                    .Returns(Task.FromResult(new ActionBindingContext(actionContext,
                                                                        modelMetadataProvider,
                                                                        Mock.Of<IModelBinder>(),
                                                                        Mock.Of<IValueProvider>(),
                                                                        Mock.Of<IInputFormatterProvider>(),
                                                                        Enumerable.Empty<IModelValidatorProvider>())));
            var htmlHelper = new HtmlHelper(Mock.Of<IViewEngine>(),
                                            modelMetadataProvider,
                                            Mock.Of<IUrlHelper>(),
                                            antiForgery,
                                            actionBindingContextProvider.Object);
            var viewEngine = new Mock<IViewEngine>();
            viewEngine.Setup(v => v.FindPartialView(It.IsAny<IDictionary<string, object>>(), It.IsAny<string>()))
                      .Returns(ViewEngineResult.NotFound("", Enumerable.Empty<string>()));
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(s => s.GetService(typeof(IViewEngine)))
                           .Returns(viewEngine.Object);
            serviceProvider.Setup(s => s.GetService(typeof(IHtmlHelper)))
                           .Returns(htmlHelper);

            httpContext.SetupGet(c => c.RequestServices)
                       .Returns(serviceProvider.Object);

            var viewContext = new ViewContext(actionContext,
                                              Mock.Of<IView>(),
                                              new ViewDataDictionary(modelMetadataProvider) { Model = model },
                                              Mock.Of<TextWriter>());
            htmlHelper.Contextualize(viewContext);
            return htmlHelper;
        }

        private class ObjectWithScaffoldColumn
        {
            public string Property1 { get; set; }

            [ScaffoldColumn(false)]
            public string Property2 { get; set; }

            [ScaffoldColumn(true)]
            public string Property3 { get; set; }
        }
    }
}