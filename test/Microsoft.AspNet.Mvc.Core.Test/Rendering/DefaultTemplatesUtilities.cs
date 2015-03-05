﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNet.DataProtection;
using Microsoft.AspNet.Http.Core;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.OptionsModel;
using Microsoft.Framework.WebEncoders;
using Moq;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public class DefaultTemplatesUtilities
    {
        public class ObjectTemplateModel
        {
            public ObjectTemplateModel()
            {
                ComplexInnerModel = new object();
            }

            public string Property1 { get; set; }
            public string Property2 { get; set; }
            public object ComplexInnerModel { get; set; }
        }

        public class ObjectWithScaffoldColumn
        {
            public string Property1 { get; set; }

            [ScaffoldColumn(false)]
            public string Property2 { get; set; }

            [ScaffoldColumn(true)]
            public string Property3 { get; set; }
        }

        public static HtmlHelper<ObjectTemplateModel> GetHtmlHelper()
        {
            return GetHtmlHelper<ObjectTemplateModel>(model: null);
        }

        public static HtmlHelper<IEnumerable<ObjectTemplateModel>> GetHtmlHelperForEnumerable()
        {
            return GetHtmlHelper<IEnumerable<ObjectTemplateModel>>(model: null);
        }

        public static HtmlHelper<ObjectTemplateModel> GetHtmlHelper(IUrlHelper urlHelper)
        {
            return GetHtmlHelper<ObjectTemplateModel>(
                model: null,
                urlHelper: urlHelper,
                viewEngine: CreateViewEngine(),
                provider: TestModelMetadataProvider.CreateDefaultProvider());
        }

        public static HtmlHelper<ObjectTemplateModel> GetHtmlHelper(IHtmlGenerator htmlGenerator)
        {
            var metadataProvider = TestModelMetadataProvider.CreateDefaultProvider();
            return GetHtmlHelper<ObjectTemplateModel>(
                new ViewDataDictionary<ObjectTemplateModel>(metadataProvider),
                CreateUrlHelper(),
                CreateViewEngine(),
                metadataProvider,
                innerHelperWrapper: null,
                htmlGenerator: htmlGenerator);
        }

        public static HtmlHelper<TModel> GetHtmlHelper<TModel>(ViewDataDictionary<TModel> viewData)
        {
            return GetHtmlHelper(
                viewData,
                CreateUrlHelper(),
                CreateViewEngine(),
                TestModelMetadataProvider.CreateDefaultProvider(),
                innerHelperWrapper: null,
                htmlGenerator: null);
        }

        public static HtmlHelper<TModel> GetHtmlHelper<TModel>(TModel model)
        {
            return GetHtmlHelper(model, CreateViewEngine());
        }

        public static HtmlHelper<IEnumerable<TModel>> GetHtmlHelperForEnumerable<TModel>(TModel model)
        {
            return GetHtmlHelper<IEnumerable<TModel>>(new TModel[] { model });
        }

        public static HtmlHelper<TModel> GetHtmlHelper<TModel>(IModelMetadataProvider provider)
        {
            return GetHtmlHelper<TModel>(model: default(TModel), provider: provider);
        }

        public static HtmlHelper<ObjectTemplateModel> GetHtmlHelper(IModelMetadataProvider provider)
        {
            return GetHtmlHelper<ObjectTemplateModel>(model: null, provider: provider);
        }

        public static HtmlHelper<IEnumerable<ObjectTemplateModel>> GetHtmlHelperForEnumerable(
            IModelMetadataProvider provider)
        {
            return GetHtmlHelper<IEnumerable<ObjectTemplateModel>>(model: null, provider: provider);
        }

        public static HtmlHelper<TModel> GetHtmlHelper<TModel>(TModel model, IModelMetadataProvider provider)
        {
            return GetHtmlHelper(model, CreateUrlHelper(), CreateViewEngine(), provider);
        }

        public static HtmlHelper<TModel> GetHtmlHelper<TModel>(TModel model, ICompositeViewEngine viewEngine)
        {
            return GetHtmlHelper(model, CreateUrlHelper(), viewEngine, TestModelMetadataProvider.CreateDefaultProvider());
        }

        public static HtmlHelper<TModel> GetHtmlHelper<TModel>(
            TModel model,
            ICompositeViewEngine viewEngine,
            Func<IHtmlHelper, IHtmlHelper> innerHelperWrapper)
        {
            return GetHtmlHelper(
                model,
                CreateUrlHelper(),
                viewEngine,
                TestModelMetadataProvider.CreateDefaultProvider(),
                innerHelperWrapper);
        }

        public static HtmlHelper<TModel> GetHtmlHelper<TModel>(
            TModel model,
            IUrlHelper urlHelper,
            ICompositeViewEngine viewEngine,
            IModelMetadataProvider provider)
        {
            return GetHtmlHelper(model, urlHelper, viewEngine, provider, innerHelperWrapper: null);
        }

        public static HtmlHelper<TModel> GetHtmlHelper<TModel>(
            TModel model,
            IUrlHelper urlHelper,
            ICompositeViewEngine viewEngine,
            IModelMetadataProvider provider,
            Func<IHtmlHelper, IHtmlHelper> innerHelperWrapper)
        {
            var viewData = new ViewDataDictionary<TModel>(provider);
            viewData.Model = model;

            return GetHtmlHelper(viewData, urlHelper, viewEngine, provider, innerHelperWrapper, htmlGenerator: null);
        }

        private static HtmlHelper<TModel> GetHtmlHelper<TModel>(
            ViewDataDictionary<TModel> viewData,
            IUrlHelper urlHelper,
            ICompositeViewEngine viewEngine,
            IModelMetadataProvider provider,
            Func<IHtmlHelper, IHtmlHelper> innerHelperWrapper,
            IHtmlGenerator htmlGenerator)
        {
            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            var bindingContext = new ActionBindingContext()
            {
                ValidatorProvider = new DataAnnotationsModelValidatorProvider(),
            };

            var bindingContextAccessor = new MockScopedInstance<ActionBindingContext>();
            bindingContextAccessor.Value = bindingContext;

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(s => s.GetService(typeof(ICompositeViewEngine)))
                .Returns(viewEngine);
            serviceProvider
                .Setup(s => s.GetService(typeof(IUrlHelper)))
                .Returns(urlHelper);
            serviceProvider
                .Setup(s => s.GetService(typeof(IViewComponentHelper)))
                .Returns(new Mock<IViewComponentHelper>().Object);

            httpContext.RequestServices = serviceProvider.Object;
            if (htmlGenerator == null)
            {
                htmlGenerator = new DefaultHtmlGenerator(
                    GetAntiForgeryInstance(),
                    bindingContextAccessor,
                    provider,
                    urlHelper,
                    new HtmlEncoder());
            }

            // TemplateRenderer will Contextualize this transient service.
            var innerHelper = (IHtmlHelper)new HtmlHelper(
                htmlGenerator,
                viewEngine,
                provider,
                new HtmlEncoder(),
                new UrlEncoder(),
                new JavaScriptStringEncoder());
            if (innerHelperWrapper != null)
            {
                innerHelper = innerHelperWrapper(innerHelper);
            }
            serviceProvider
                .Setup(s => s.GetService(typeof(IHtmlHelper)))
                .Returns(() => innerHelper);

            var htmlHelper = new HtmlHelper<TModel>(
                htmlGenerator,
                viewEngine,
                provider,
                new HtmlEncoder(),
                new UrlEncoder(),
                new JavaScriptStringEncoder());
            var viewContext = new ViewContext(actionContext, Mock.Of<IView>(), viewData, new StringWriter());
            htmlHelper.Contextualize(viewContext);

            return htmlHelper;
        }

        public static string FormatOutput(IHtmlHelper helper, object model)
        {
            var modelExplorer = helper.MetadataProvider.GetModelExplorerForType(model.GetType(), model);
            return FormatOutput(modelExplorer);
        }

        private static ICompositeViewEngine CreateViewEngine()
        {
            var view = new Mock<IView>();
            view
                .Setup(v => v.RenderAsync(It.IsAny<ViewContext>()))
                .Callback(async (ViewContext v) =>
                {
                    view.ToString();
                    await v.Writer.WriteAsync(FormatOutput(v.ViewData.ModelExplorer));
                })
                .Returns(Task.FromResult(0));

            var viewEngine = new Mock<ICompositeViewEngine>();
            viewEngine
                .Setup(v => v.FindPartialView(It.IsAny<ActionContext>(), It.IsAny<string>()))
                .Returns(ViewEngineResult.Found("MyView", view.Object));

            return viewEngine.Object;
        }

        private static AntiForgery GetAntiForgeryInstance()
        {
            var claimExtractor = new Mock<IClaimUidExtractor>();
            var dataProtectionProvider = new Mock<IDataProtectionProvider>();
            var additionalDataProvider = new Mock<IAntiForgeryAdditionalDataProvider>();
            var optionsAccessor = new Mock<IOptions<MvcOptions>>();
            optionsAccessor.SetupGet(o => o.Options).Returns(new MvcOptions());
            return new AntiForgery(
                claimExtractor.Object,
                dataProtectionProvider.Object,
                additionalDataProvider.Object,
                optionsAccessor.Object,
                new HtmlEncoder());
        }

        private static string FormatOutput(ModelExplorer modelExplorer)
        {
            var metadata = modelExplorer.Metadata;
            return string.Format(
                CultureInfo.InvariantCulture,
                "Model = {0}, ModelType = {1}, PropertyName = {2}, SimpleDisplayText = {3}",
                modelExplorer.Model ?? "(null)",
                metadata.ModelType == null ? "(null)" : metadata.ModelType.FullName,
                metadata.PropertyName ?? "(null)",
                modelExplorer.GetSimpleDisplayText() ?? "(null)");
        }

        private static IUrlHelper CreateUrlHelper()
        {
            return Mock.Of<IUrlHelper>();
        }
    }
}