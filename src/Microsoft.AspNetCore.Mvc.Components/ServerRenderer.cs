// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.Rendering;
using Microsoft.AspNetCore.Blazor.RenderTree;

namespace Microsoft.AspNetCore.Mvc.Components
{
    public class ServerRenderer : Renderer
    {
        public static readonly HashSet<string> VoidElements = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "area", "base", "br", "col", "embed", "hr", "img", "input", "link", "meta", "param", "source", "track", "wbr",
        };

        public ServerRenderer(IServiceProvider services)
            : base(services)
        {
        }

        public (T component, int componentId) CreateComponent<T>() where T : IComponent
        {
            var component = InstantiateComponent(typeof(T));
            var componentId = AssignRootComponentId(component);
            return ((T)component, componentId);
        }

        public void RenderComponent(int componentId, TextWriter writer, HtmlEncoder encoder)
        {
            RenderRootComponent(componentId);        
            RenderComponentCore(componentId, writer, encoder);
        }

        private void RenderComponentCore(int componentId, TextWriter writer, HtmlEncoder encoder)
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

                if (openElements.Count > 0 && i >= openElements.Peek().end)
                {
                    if (state == RenderingState.InsideStartTag && VoidElements.Contains(openElements.Peek().element))
                    {
                        writer.Write(">");
                    }
                    else if (state == RenderingState.InsideStartTag)
                    {
                        writer.Write("/>");
                    }
                    else
                    {
                        writer.Write("</");
                        writer.Write(openElements.Peek().element);
                        writer.Write(">");
                    }

                    openElements.Pop();
                    state = RenderingState.Normal;
                }

                if (state == RenderingState.InsideStartTag && ShouldCloseStartTag(frame.FrameType))
                {
                    writer.Write(">");
                    state = RenderingState.Normal;
                }

                switch (frame.FrameType)
                {
                    case RenderTreeFrameType.Element:
                        {
                            Debug.Assert(state == RenderingState.Normal);

                            writer.Write("<");
                            writer.Write(frame.ElementName);
                            state = RenderingState.InsideStartTag;
                            break;
                        }

                    case RenderTreeFrameType.Component:
                        {
                            Debug.Assert(state == RenderingState.Normal);

                            RenderComponentCore(frame.ComponentId, writer, encoder);

                            openElements.Push(("$component$", i + frame.ComponentSubtreeLength));
                            state = RenderingState.Suppressed;
                            break;
                        }

                    case RenderTreeFrameType.Attribute:
                        {
                            Debug.Assert(state == RenderingState.InsideStartTag);

                            writer.Write(" ");
                            writer.Write(frame.AttributeName);
                            writer.Write("=\"");
                            encoder.Encode(writer, frame.AttributeValue?.ToString());
                            writer.Write("\"");
                            break;
                        }

                    case RenderTreeFrameType.Markup:
                        {
                            Debug.Assert(state == RenderingState.Normal);

                            writer.Write(frame.MarkupContent);
                            break;
                        }

                    case RenderTreeFrameType.Text:
                        {
                            Debug.Assert(state == RenderingState.Normal);

                            encoder.Encode(writer, frame.TextContent);
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
            while (openElements.Count > 0)
            {
                var element = openElements.Peek().element;
                if (state == RenderingState.InsideStartTag && VoidElements.Contains(element))
                {
                    writer.Write(">");
                }
                else if (state == RenderingState.InsideStartTag)
                {
                    writer.Write("/>");
                }
                else
                {
                    writer.Write("</");
                    writer.Write(element);
                    writer.Write(">");
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
