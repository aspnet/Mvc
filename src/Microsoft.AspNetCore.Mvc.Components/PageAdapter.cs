// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.RenderTree;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Mvc.Components
{
    internal class PageAdapter<TComponent> : PageBase where TComponent : IComponent
    {
        public PageAdapter<TComponent> Model { get; set; }

        public override Task ExecuteAsync()
        {
            var encoder = PageContext.HttpContext.RequestServices.GetRequiredService<HtmlEncoder>();
            var renderer = new ServerRenderer(PageContext.HttpContext.RequestServices);

            var (component, componentId) = renderer.CreateComponent<WrapperComponent>();
            component.Page = this;

            renderer.RenderComponent(componentId, ViewContext.Writer, encoder);
            return Task.CompletedTask;
        }

        private class WrapperComponent : BlazorComponent
        {
            public PageBase Page { get; set; }

            protected override void BuildRenderTree(RenderTreeBuilder builder)
            {
                base.BuildRenderTree(builder);

                builder.OpenComponent<CascadingValue<PageBase>>(0);
                builder.AddAttribute(1, "Value", Page);
                builder.AddAttribute(2, "ChildContent", (RenderFragment)((b1) =>
                {
                    b1.OpenComponent<TComponent>(3);
                    b1.CloseComponent();
                }));
                builder.CloseComponent();
            }
        }
    }

    internal class PageAdapter<TComponent, TModel> : PageBase where TComponent : IComponent
    {
        public TModel Model => (TModel)ViewContext.ViewData.Model;

        public override Task ExecuteAsync()
        {
            var encoder = PageContext.HttpContext.RequestServices.GetRequiredService<HtmlEncoder>();
            var renderer = new ServerRenderer(PageContext.HttpContext.RequestServices);

            var (component, componentId) = renderer.CreateComponent<WrapperComponent>();
            component.Model = Model;
            component.Page = this;

            renderer.RenderComponent(componentId, ViewContext.Writer, encoder);
            return Task.CompletedTask;
        }

        private class WrapperComponent : BlazorComponent
        {
            public TModel Model { get; set; }

            public PageBase Page { get; set; }

            protected override void BuildRenderTree(RenderTreeBuilder builder)
            {
                base.BuildRenderTree(builder);

                builder.OpenComponent<CascadingValue<TModel>>(0);
                builder.AddAttribute(1, "Value", Model);
                builder.AddAttribute(2, "ChildContent", (RenderFragment)((b1) =>
                {
                    b1.OpenComponent<CascadingValue<PageBase>>(3);
                    b1.AddAttribute(4, "Value", Page);
                    b1.AddAttribute(5, "ChildContent", (RenderFragment)((b2) =>
                    {
                        b2.OpenComponent<TComponent>(6);
                        b2.CloseComponent();
                    }));
                    b1.CloseComponent();
                }));
                builder.CloseComponent();
            }
        }
    }
}
