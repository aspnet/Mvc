﻿using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Abstractions;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc.Razor
{
    public abstract class RazorView : IView
    {
        public HttpContext Context { get; set; }

        public string Layout { get; set; }

        protected TextWriter Output { get; set; }

        private string BodyContent { get; set; }

        public virtual async Task RenderAsync(ViewContext context, TextWriter writer)
        {
            var contentBuilder = new StringBuilder(1024);
            using (var bodyWriter = new StringWriter(contentBuilder))
            {
                Output = bodyWriter;
                Execute();
            }

            string bodyContent = contentBuilder.ToString();
            if (!String.IsNullOrEmpty(Layout))
            {
                await RenderLayoutAsync(context, writer, bodyContent);
            }
            else
            {
                await writer.WriteAsync(bodyContent);
            }
        }

        private async Task RenderLayoutAsync(ViewContext context, TextWriter writer, string bodyContent)
        {
            var virtualPathFactory = context.ServiceProvider.GetService<IVirtualPathViewFactory>();
            RazorView layoutView = (RazorView)(await virtualPathFactory.CreateInstance(Layout));
            if (layoutView == null)
            {
                string message = String.Format(CultureInfo.CurrentCulture, "The layout view '{0}' could not be located.", Layout);
                throw new InvalidOperationException(message);
            }

            layoutView.BodyContent = bodyContent;
            await layoutView.RenderAsync(context, writer);
        }

        public abstract void Execute();

        public virtual void Write(object value)
        {
            WriteTo(Output, value);
        }

        public virtual void WriteTo(TextWriter writer, object content)
        {
            if (content != null)
            {
                var htmlString = content as HtmlString;
                if (htmlString != null)
                {
                    writer.Write(content.ToString());
                }
                else
                {
#if NET45
                    WebUtility.HtmlEncode(content.ToString(), writer);
#endif
                }
            }
        }

        public void WriteLiteral(object value)
        {
            WriteLiteralTo(Output, value);
        }

        public virtual void WriteLiteralTo(TextWriter writer, object text)
        {
            if (text != null)
            {
                writer.Write(text.ToString());
            }
        }

        public virtual void WriteAttribute(string name,
                                           PositionTagged<string> prefix,
                                           PositionTagged<string> suffix,
                                           params AttributeValue[] values)
        {
            WriteAttributeTo(Output, name, prefix, suffix, values);
        }

        public virtual void WriteAttributeTo(TextWriter writer,
                                             string name,
                                             PositionTagged<string> prefix,
                                             PositionTagged<string> suffix,
                                             params AttributeValue[] values)
        {
            bool first = true;
            bool wroteSomething = false;
            if (values.Length == 0)
            {
                // Explicitly empty attribute, so write the prefix and suffix
                WritePositionTaggedLiteral(writer, prefix);
                WritePositionTaggedLiteral(writer, suffix);
            }
            else
            {
                for (int i = 0; i < values.Length; i++)
                {
                    AttributeValue attrVal = values[i];
                    PositionTagged<object> val = attrVal.Value;
                    PositionTagged<string> next = i == values.Length - 1 ?
                        suffix : // End of the list, grab the suffix
                        values[i + 1].Prefix; // Still in the list, grab the next prefix

                    if (val.Value == null)
                    {
                        // Nothing to write
                        continue;
                    }

                    // The special cases here are that the value we're writing might already be a string, or that the 
                    // value might be a bool. If the value is the bool 'true' we want to write the attribute name instead
                    // of the string 'true'. If the value is the bool 'false' we don't want to write anything.
                    //
                    // Otherwise the value is another object (perhaps an IHtmlString), and we'll ask it to format itself.
                    string stringValue;
                    bool? boolValue = val.Value as bool?;
                    if (boolValue == true)
                    {
                        stringValue = name;
                    }
                    else if (boolValue == false)
                    {
                        continue;
                    }
                    else
                    {
                        stringValue = val.Value as string;
                    }

                    if (first)
                    {
                        WritePositionTaggedLiteral(writer, prefix);
                        first = false;
                    }
                    else
                    {
                        WritePositionTaggedLiteral(writer, attrVal.Prefix);
                    }

                    // Calculate length of the source span by the position of the next value (or suffix)
                    int sourceLength = next.Position - attrVal.Value.Position;

                    if (attrVal.Literal)
                    {
                        WriteLiteralTo(writer, stringValue ?? val.Value);
                    }
                    else
                    {
                        WriteTo(writer, stringValue ?? val.Value); // Write value
                    }
                    wroteSomething = true;
                }
                if (wroteSomething)
                {
                    WritePositionTaggedLiteral(writer, suffix);
                }
            }
        }

        private void WritePositionTaggedLiteral(TextWriter writer, string value, int position)
        {
            WriteLiteralTo(writer, value);
        }

        private void WritePositionTaggedLiteral(TextWriter writer, PositionTagged<string> value)
        {
            WritePositionTaggedLiteral(writer, value.Value, value.Position);
        }

        protected virtual HtmlString RenderBody()
        {
            if (BodyContent == null)
            {
                throw new InvalidOperationException("RenderBody cannot be called at this point because you're not executing a layout");
            }
            return new HtmlString(BodyContent);
        }
    }
}
