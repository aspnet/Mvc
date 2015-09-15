// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Html.Abstractions;
using Microsoft.Framework.Internal;
using Microsoft.Framework.WebEncoders;
using Microsoft.AspNet.Mvc.Razor;

namespace Microsoft.AspNet.Mvc.Rendering
{
    /// <summary>
    /// A <see cref="TextWriter"/> that represents individual write operations as a sequence of strings.
    /// </summary>
    /// <remarks>
    /// This is primarily designed to avoid creating large in-memory strings.
    /// Refer to https://aspnetwebstack.codeplex.com/workitem/585 for more details.
    /// </remarks>
    public class StringCollectionTextWriter : HtmlTextWriter
    {
        private const int MaxCharToStringLength = 1024;
        private static readonly Task _completedTask = Task.FromResult(0);

        private readonly Encoding _encoding;
        private readonly StringCollectionTextWriterContent _content;

        /// <summary>
        /// Creates a new instance of <see cref="StringCollectionTextWriter"/>.
        /// </summary>
        /// <param name="encoding">The character <see cref="Encoding"/> in which the output is written.</param>
        public StringCollectionTextWriter(Encoding encoding)
        {
            _encoding = encoding;
            Entries = new List<object>();
            _content = new StringCollectionTextWriterContent(Entries);
        }

        /// <inheritdoc />
        public override Encoding Encoding
        {
            get { return _encoding; }
        }

        /// <summary>
        /// Gets the content written to the writer as an <see cref="IHtmlContent"/>.
        /// </summary>
        public IHtmlContent Content => _content;

        // internal for testing purposes.
        internal List<object> Entries { get; }

        /// <inheritdoc />
        public override void Write(char value)
        {
            _content.Append(value.ToString());
        }

        /// <inheritdoc />
        public override void Write([NotNull] char[] buffer, int index, int count)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (count < 0 || (buffer.Length - index < count))
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            while (count > 0)
            {
                // Split large char arrays into 1KB strings.
                var currentCount = count;
                if (MaxCharToStringLength < currentCount)
                {
                    currentCount = MaxCharToStringLength;
                }

                _content.Append(new string(buffer, index, currentCount));
                index += currentCount;
                count -= currentCount;
            }
        }

        /// <inheritdoc />
        public override void Write(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            _content.Append(value);
        }

        /// <inheritdoc />
        public override void Write(IHtmlContent content)
        {
            _content.Append(content);
        }

        /// <inheritdoc />
        public override Task WriteAsync(char value)
        {
            Write(value);
            return _completedTask;
        }

        /// <inheritdoc />
        public override Task WriteAsync([NotNull] char[] buffer, int index, int count)
        {
            Write(buffer, index, count);
            return _completedTask;
        }

        /// <inheritdoc />
        public override Task WriteAsync(string value)
        {
            Write(value);
            return _completedTask;
        }

        /// <inheritdoc />
        public override void WriteLine()
        {
            _content.Append(Environment.NewLine);
        }

        /// <inheritdoc />
        public override void WriteLine(string value)
        {
            Write(value);
            WriteLine();
        }

        /// <inheritdoc />
        public override Task WriteLineAsync(char value)
        {
            WriteLine(value);
            return _completedTask;
        }

        /// <inheritdoc />
        public override Task WriteLineAsync(char[] value, int start, int offset)
        {
            WriteLine(value, start, offset);
            return _completedTask;
        }

        /// <inheritdoc />
        public override Task WriteLineAsync(string value)
        {
            WriteLine(value);
            return _completedTask;
        }

        /// <inheritdoc />
        public override Task WriteLineAsync()
        {
            WriteLine();
            return _completedTask;
        }

        /// <summary>
        /// If the specified <paramref name="writer"/> is a <see cref="StringCollectionTextWriter"/> the contents
        /// are copied. It is just written to the <paramref name="writer"/> otherwise.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to which the content must be copied/written.</param>
        /// <param name="encoder">The <see cref="IHtmlEncoder"/> to encode the copied/written content.</param>
        public void CopyTo(TextWriter writer, IHtmlEncoder encoder)
        {
            var targetStringCollectionWriter = writer as StringCollectionTextWriter;
            if (targetStringCollectionWriter != null)
            {
                targetStringCollectionWriter._content.Append(Content);
            }
            else
            {
                Content.WriteTo(writer, encoder);
            }
        }

        /// <summary>
        /// If the specified <paramref name="writer"/> is a <see cref="StringCollectionTextWriter"/> the contents
        /// are copied. It is just written to the <paramref name="writer"/> otherwise.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to which the content must be copied/written.</param>
        /// <param name="encoder">The <see cref="IHtmlEncoder"/> to encode the copied/written content.</param>
        public Task CopyToAsync(TextWriter writer, IHtmlEncoder encoder)
        {
            CopyTo(writer, encoder);
            return _completedTask;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            using (var writer = new StringWriter())
            {
                Content.WriteTo(writer, HtmlEncoder.Default);
                return writer.ToString();
            }
        }

        private class StringCollectionTextWriterContent : IHtmlContent
        {
            private readonly List<object> _entries;

            public StringCollectionTextWriterContent(List<object> entries)
            {
                _entries = entries;
            }

            public void Append(string value)
            {
                _entries.Add(value);
            }

            public void Append(IHtmlContent content)
            {
                _entries.Add(content);
            }

            public void WriteTo(TextWriter writer, IHtmlEncoder encoder)
            {
                foreach (var item in _entries)
                {
                    if (item == null)
                    {
                        continue;
                    }

                    var itemAsString = item as string;
                    if (itemAsString != null)
                    {
                        writer.Write(itemAsString);
                    }
                    else
                    {
                        ((IHtmlContent)item).WriteTo(writer, encoder);
                    }
                }
            }
        }
    }
}