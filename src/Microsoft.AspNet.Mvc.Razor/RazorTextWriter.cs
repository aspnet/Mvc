// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// A <see cref="TextWriter"/> that represents individual write operations as a sequence of strings.
    /// </summary>
    /// <remarks>
    /// This is primarily designed to avoid creating large in-memory strings.
    /// Refer to https://aspnetwebstack.codeplex.com/workitem/585 for more details.
    /// </remarks>
    public class RazorTextWriter : TextWriter
    {
        private static readonly Task _completedTask = Task.FromResult(0);
        private readonly Encoding _encoding;

        public RazorTextWriter(Encoding encoding)
        {
            _encoding = encoding;
            Buffer = new List<ListOrValue>();
        }

        public override Encoding Encoding
        {
            get { return _encoding; }
        }

        internal List<ListOrValue> Buffer { get; private set; }

        /// <inheritdoc />
        public override void Write(char value)
        {
            Buffer.Add(new ListOrValue { StringValue = "" + value });
        }

        /// <inheritdoc />
        public override void Write([NotNull] char[] buffer, int index, int count)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (index + count > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            var result = new ArraySegment<char>(buffer, index, count);
            Buffer.Add(new ListOrValue { CharArrayValue = result });
        }

        /// <inheritdoc />
        public override void Write(string value)
        {
            if (value != null)
            {
                Buffer.Add(new ListOrValue { StringValue = value });
            }
        }

        /// <inheritdoc />
        public override Task WriteAsync(char value)
        {
            Buffer.Add(new ListOrValue { StringValue = "" + value });
            return _completedTask;
        }

        /// <inheritdoc />
        public override Task WriteAsync([NotNull] char[] buffer, int index, int count)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (index + count > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            var result = new ArraySegment<char>(buffer, index, count);
            Buffer.Add(new ListOrValue { CharArrayValue = result });
            return _completedTask;
        }

        /// <inheritdoc />
        public override Task WriteAsync(string value)
        {
            if (value != null)
            {
                Buffer.Add(new ListOrValue { StringValue = value });
            }
            return _completedTask;
        }

        /// <inheritdoc />
        public override void WriteLine()
        {
            Buffer.Add(new ListOrValue { StringValue = Environment.NewLine });
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
            Buffer.Add(new ListOrValue { StringValue = "" + value });
            return _completedTask;
        }

        /// <inheritdoc />
        public override async Task WriteLineAsync(char[] value, int start, int offset)
        {
            await WriteAsync(value, start, offset);
            await WriteLineAsync();
        }

        /// <inheritdoc />
        public override async Task WriteLineAsync(string value)
        {
            await WriteAsync(value);
            await WriteLineAsync();
        }

        /// <inheritdoc />
        public override Task WriteLineAsync()
        {
            Buffer.Add(new ListOrValue { StringValue = Environment.NewLine });
            return _completedTask;
        }

        /// <summary>
        /// Copies the content of the <see cref="RazorTextWriter"/> to the <see cref="TextWriter"/> instance.
        /// </summary>
        /// <param name="writer">The writer to copy contents to.</param>
        public void CopyTo(TextWriter writer)
        {
            var targetRazorTextWriter = writer as RazorTextWriter;
            if (targetRazorTextWriter != null)
            {
                targetRazorTextWriter.Buffer.Add(new ListOrValue { List = Buffer });
            }
            else
            {
                WriteList(writer, Buffer);
            }
        }

        /// <summary>
        /// Copies the content of the <see cref="RazorTextWriter"/> to the specified <see cref="TextWriter"/> instance.
        /// </summary>
        /// <param name="writer">The writer to copy contents to.</param>
        /// <returns>A task that represents the asynchronous copy operation.</returns>
        public Task CopyToAsync(TextWriter writer)
        {
            var targetRazorTextWriter = writer as RazorTextWriter;
            if (targetRazorTextWriter != null)
            {
                targetRazorTextWriter.Buffer.Add(new ListOrValue { List = Buffer });
            }
            else
            {
                return WriteListAsync(writer, Buffer);
            }

            return _completedTask;
        }

        private static void WriteList(TextWriter writer, List<ListOrValue> values)
        {
            foreach (var value in values)
            {
                if (value.List != null)
                {
                    WriteList(writer, value.List);
                }
                else if (value.StringValue != null)
                {
                    writer.Write(value.StringValue);
                }
                else
                {
                    var arrayValue = value.CharArrayValue;
                    writer.Write(arrayValue.Array, arrayValue.Offset, arrayValue.Count);
                }
            }
        }

        private static async Task WriteListAsync(TextWriter writer, List<ListOrValue> values)
        {
            foreach (var value in values)
            {
                if (value.List != null)
                {
                    await WriteListAsync(writer, value.List);
                }
                else if (value.StringValue != null)
                {
                    await writer.WriteAsync(value.StringValue);
                }
                else
                {
                    var arrayValue = value.CharArrayValue;
                    await writer.WriteAsync(arrayValue.Array, arrayValue.Offset, arrayValue.Count);
                }
            }
        }

        internal sealed class ListOrValue
        {
            public string StringValue { get; set; }

            public ArraySegment<char> CharArrayValue { get; set; }

            public List<ListOrValue> List { get; set; }
        }
    }
}