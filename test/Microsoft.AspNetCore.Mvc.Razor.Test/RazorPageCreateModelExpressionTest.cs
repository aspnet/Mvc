// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Razor
{
    public class RazorPageCreateModelExpressionTest
    {
        public static TheoryData IdentityExpressions
        {
            get
            {
                return new TheoryData<Func<IdentityRazorPage, ModelExpression>, string>
                {
                    // m => m
                    { page => page.CreateModelExpression1(), string.Empty },
                    // m => Model
                    { page => page.CreateModelExpression2(), string.Empty },
                    // m => ViewData.Model (ExpressionHelper can only special-case a leftmost Model node)
                    { page => page.CreateModelExpression3(), "ViewData.Model" },
                };
            }
        }

        public static TheoryData<Expression<Func<RazorPageCreateModelExpressionModel, int>>, string> IntExpressions
        {
            get
            {
                var somethingElse = 23;
                return new TheoryData<Expression<Func<RazorPageCreateModelExpressionModel, int>>, string>
                {
                    { model => somethingElse, "somethingElse" },
                    { model => model.Id, "Id" },
                    { model => model.SubModel.Id, "SubModel.Id" },
                    { model => model.SubModel.SubSubModel.Id, "SubModel.SubSubModel.Id" },
                };
            }
        }

        public static TheoryData<Expression<Func<RazorPageCreateModelExpressionModel, string>>, string> StringExpressions
        {
            get
            {
                var somethingElse = "This is something else";
                return new TheoryData<Expression<Func<RazorPageCreateModelExpressionModel, string>>, string>
                {
                    { model => somethingElse, "somethingElse" },
                    { model => model.Name, "Name" },
                    { model => model.SubModel.Name, "SubModel.Name" },
                    { model => model.SubModel.SubSubModel.Name, "SubModel.SubSubModel.Name" },
                };
            }
        }

        [Theory]
        [MemberData(nameof(IdentityExpressions))]
        public void CreateModelExpression_ReturnsExpectedMetadata_IdentityExpressions(
            Func<IdentityRazorPage, ModelExpression> createModelExpression,
            string expectedName)
        {
            // Arrange
            var viewContext = CreateViewContext();
            var modelExplorer = viewContext.ViewData.ModelExplorer.GetExplorerForProperty(
                nameof(RazorPageCreateModelExpressionModel.Name));
            var viewData = new ViewDataDictionary<string>(viewContext.ViewData)
            {
                ModelExplorer = modelExplorer,
            };
            viewContext.ViewData = viewData;

            var page = CreateIdentityPage(viewContext);

            // Act
            var modelExpression = createModelExpression(page);

            // Assert
            Assert.NotNull(modelExpression);
            Assert.Equal(expectedName, modelExpression.Name);
            Assert.Same(modelExplorer, modelExpression.ModelExplorer);
        }

        [Theory]
        [MemberData(nameof(IntExpressions))]
        public void CreateModelExpression_ReturnsExpectedMetadata_IntExpressions(
            Expression<Func<RazorPageCreateModelExpressionModel, int>> expression,
            string expectedName)
        {
            // Arrange
            var viewContext = CreateViewContext();
            var page = CreatePage(viewContext);

            // Act
            var result = page.CreateModelExpression(expression);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Metadata);
            Assert.Equal(typeof(int), result.Metadata.ModelType);
            Assert.Equal(expectedName, result.Name);
        }

        [Theory]
        [MemberData(nameof(StringExpressions))]
        public void CreateModelExpression_ReturnsExpectedMetadata_StringExpressions(
            Expression<Func<RazorPageCreateModelExpressionModel, string>> expression,
            string expectedName)
        {
            // Arrange
            var viewContext = CreateViewContext();
            var page = CreatePage(viewContext);

            // Act
            var result = page.CreateModelExpression(expression);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Metadata);
            Assert.Equal(typeof(string), result.Metadata.ModelType);
            Assert.Equal(expectedName, result.Name);
        }

        private static IdentityRazorPage CreateIdentityPage(ViewContext viewContext)
        {
            return new IdentityRazorPage
            {
                ViewContext = viewContext,
                ViewData = (ViewDataDictionary<string>)viewContext.ViewData,
            };
        }

        private static TestRazorPage CreatePage(ViewContext viewContext)
        {
            return new TestRazorPage
            {
                ViewContext = viewContext,
                ViewData = (ViewDataDictionary<RazorPageCreateModelExpressionModel>)viewContext.ViewData,
            };
        }

        private static ViewContext CreateViewContext()
        {
            var provider = new TestModelMetadataProvider();
            var viewData = new ViewDataDictionary<RazorPageCreateModelExpressionModel>(provider);
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IModelMetadataProvider>(provider);
            serviceCollection.AddSingleton<ExpressionTextCache, ExpressionTextCache>();

            var httpContext = new DefaultHttpContext
            {
                RequestServices = serviceCollection.BuildServiceProvider(),
            };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            return new ViewContext(
                actionContext,
                NullView.Instance,
                viewData,
                Mock.Of<ITempDataDictionary>(),
                new StringWriter(),
                new HtmlHelperOptions());
        }

        public class IdentityRazorPage : RazorPage<string>
        {
            public ModelExpression CreateModelExpression1()
            {
                return CreateModelExpression(m => m);
            }

            public ModelExpression CreateModelExpression2()
            {
                return CreateModelExpression(m => Model);
            }

            public ModelExpression CreateModelExpression3()
            {
                return CreateModelExpression(m => ViewData.Model);
            }

            public override Task ExecuteAsync()
            {
                throw new NotImplementedException();
            }
        }

        private class TestRazorPage : RazorPage<RazorPageCreateModelExpressionModel>
        {
            public override Task ExecuteAsync()
            {
                throw new NotImplementedException();
            }
        }

        public class RazorPageCreateModelExpressionModel
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public RazorPageCreateModelExpressionSubModel SubModel { get; set; }
        }

        public class RazorPageCreateModelExpressionSubModel
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public RazorPageCreateModelExpressionSubSubModel SubSubModel { get; set; }
        }

        public class RazorPageCreateModelExpressionSubSubModel
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}