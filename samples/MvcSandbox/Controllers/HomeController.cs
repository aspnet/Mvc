// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.Rendering;
using Microsoft.AspNetCore.Blazor.RenderTree;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace MvcSandbox.Controllers
{
    public class HomeController : Controller
    {
        [ModelBinder]
        public string Id { get; set; }

        public IActionResult Index()
        {
            return ViewComponent<SomeComponents.Components.Home.Index>();
        }

        public ViewComponentResult<TComponent> ViewComponent<TComponent>() where TComponent : IComponent
        {
            return new ViewComponentResult<TComponent>();
        }
    }

    public class ViewComponentResult<TComponent> : ActionResult where TComponent : IComponent
    {
        public override Task ExecuteResultAsync(ActionContext context)
        {
            var encoder = context.HttpContext.RequestServices.GetRequiredService<HtmlEncoder>();
            var renderer = new ServerRenderer(context.HttpContext.RequestServices, encoder);
            var text = renderer.RenderComponent(typeof(TComponent));
            return context.HttpContext.Response.WriteAsync(text);
        }
    }

    public class ServerRenderer : Renderer
    {
        public static readonly HashSet<string> VoidElements = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "area", "base", "br", "col", "embed", "hr", "img", "input", "link", "meta", "param", "source", "track", "wbr",
        };

        private readonly HtmlEncoder _encoder;

        public ServerRenderer(IServiceProvider services, HtmlEncoder encoder)
            : base(services)
        {
            _encoder = encoder;
        }

        public string RenderComponent(Type componentType)
        {
            var component = InstantiateComponent(componentType);
            var componentId = AssignRootComponentId(component);
            RenderRootComponent(componentId);

            var text = new StringBuilder();
            RenderComponentCore(text, componentId);
            return text.ToString();
        }

        private void RenderComponentCore(StringBuilder text, int componentId)
        {
            var componentState = GetRequiredComponentState(componentId);
            var builder = GetCurrentBuilder(componentState);

            var openElements = new Stack<(string element, int end)>();
            
            var state = RenderingState.Normal;
            var frames = builder.GetFrames();
            for (var i = 0; i < frames.Count; i++)
            {
                ref var frame = ref frames.Array[i];

                if (state == RenderingState.Suppressed)
                {
                    // We're 'inside' a component. We want to skip all of the frames that initialize the component.

                    var openComponent = openElements.Peek();
                    if (i < openComponent.end)
                    {
                        continue;
                    }

                    openElements.Pop();
                    state = RenderingState.Normal;
                }

                if (openElements.TryPeek(out var openElement) && i >= openElement.end)
                {
                    if (state == RenderingState.InsideStartTag && VoidElements.Contains(openElement.element))
                    {
                        text.Append(">");
                    }
                    else if (state == RenderingState.InsideStartTag)
                    {
                        text.Append("/>");
                    }
                    else
                    {
                        text.Append("</");
                        text.Append(openElement.element);
                        text.Append(">");
                    }

                    openElements.Pop();
                    state = RenderingState.Normal;
                }

                if (state == RenderingState.InsideStartTag && ShouldCloseStartTag(frame.FrameType))
                {
                    text.Append(">");
                    state = RenderingState.Normal;
                }

                switch (frame.FrameType)
                {
                    case RenderTreeFrameType.Element:
                        {
                            Debug.Assert(state == RenderingState.Normal);

                            text.Append("<");
                            text.Append(frame.ElementName);
                            state = RenderingState.InsideStartTag;
                            break;
                        }

                    case RenderTreeFrameType.Component:
                        {
                            Debug.Assert(state == RenderingState.Normal);

                            RenderComponentCore(text, frame.ComponentId);

                            openElements.Push(("$component$", i + frame.ComponentSubtreeLength));
                            state = RenderingState.Suppressed;
                            break;
                        }

                    case RenderTreeFrameType.Attribute:
                        {
                            Debug.Assert(state == RenderingState.InsideStartTag);

                            text.Append(" ");
                            text.Append(frame.AttributeName);
                            text.Append("=\"");
                            text.Append(_encoder.Encode(frame.AttributeValue?.ToString()));
                            text.Append("\"");
                            break;
                        }

                    case RenderTreeFrameType.Markup:
                        {
                            Debug.Assert(state == RenderingState.Normal);

                            text.Append(frame.MarkupContent);
                            break;
                        }

                    case RenderTreeFrameType.Text:
                        {
                            Debug.Assert(state == RenderingState.Normal);

                            text.Append(_encoder.Encode(frame.TextContent));
                            break;
                        }

                    case RenderTreeFrameType.ComponentReferenceCapture:
                    case RenderTreeFrameType.ElementReferenceCapture:
                    case RenderTreeFrameType.Region:
                        {
                            break;
                        }

                    default:
                        {
                            throw new InvalidOperationException($"Unsupported frame type {frame.FrameType}");
                        }
                }
            }

            // Residue
            while (openElements.TryPeek(out var openElement))
            {
                if (state == RenderingState.InsideStartTag && VoidElements.Contains(openElement.element))
                {
                    text.Append(">");
                }
                else if (state == RenderingState.InsideStartTag)
                {
                    text.Append("/>");
                }
                else
                {
                    text.Append("</");
                    text.Append(openElement.element);
                    text.Append(">");
                }

                openElements.Pop();
            }

            bool ShouldCloseStartTag(RenderTreeFrameType frameType)
            {
                switch (frameType)
                {
                    case RenderTreeFrameType.Attribute:
                    case RenderTreeFrameType.ComponentReferenceCapture:
                    case RenderTreeFrameType.ElementReferenceCapture:
                        {
                            return false;
                        }

                    default:
                        {
                            return true;
                        }
                }
            }
        }

        protected override void UpdateDisplay(in RenderBatch renderBatch)
        {
            // Do nothing
        }

        protected int AssignRootComponentId2(IComponent component)
        {
            var method = typeof(Renderer).GetMethod(nameof(AssignRootComponentId), BindingFlags.Instance | BindingFlags.NonPublic);
            return (int)method.Invoke(this, new object[] { component, });
        }

        protected void RenderRootComponent2(int componentId)
        {
            var method = typeof(Renderer).GetMethod(nameof(RenderRootComponent), BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(this, new object[] { componentId, });
        }

        private object GetRequiredComponentState(int componentId)
        {
            var field = typeof(Renderer).GetField("_componentStateById", BindingFlags.Instance | BindingFlags.NonPublic);
            var entries = field.GetValue(this);

            var method = entries.GetType().GetProperties().Where(p => p.GetIndexParameters().Length > 0).Single().GetMethod;
            return method.Invoke(entries, new object[] { componentId, });
        }

        private RenderTreeBuilder GetCurrentBuilder(object state)
        {
            var field = state.GetType().GetField("_renderTreeBuilderCurrent", BindingFlags.Instance | BindingFlags.NonPublic);
            return (RenderTreeBuilder)field.GetValue(state);
        }

        private enum RenderingState
        {
            InsideStartTag,
            Normal,
            Suppressed,
        }
    }
}
