﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNet.Mvc.Infrastructure
{
    public class HttpResponseStreamReaderTest
    {
        private static readonly char[] CharData = new char[]
        {
            char.MinValue,
            char.MaxValue,
            '\t',
            ' ',
            '$',
            '@',
            '#',
            '\0',
            '\v',
            '\'',
            '\u3190',
            '\uC3A0',
            'A',
            '5',
            '\r',
            '\uFE70',
            '-',
            ';',
            '\r',
            '\n',
            'T',
            '3',
            '\n',
            'K',
            '\u00E6',
        };
        
        [Fact]
        public static async Task ReadToEndAsync()
        {
            // Arrange
            var reader = new HttpRequestStreamReader(GetLargeStream(), Encoding.UTF8);

            var result = await reader.ReadToEndAsync();

            Assert.Equal(5000, result.Length);
        }

        [Fact]
        public static void TestRead()
        {
            // Arrange
            var reader = CreateReader();

            // Act & Assert
            for (var i = 0; i < CharData.Length; i++)
            {
                var tmp = reader.Read();
                Assert.Equal((int)CharData[i], tmp);
            }
        }

        [Fact]
        public static void TestPeek()
        {
            // Arrange
            var reader = CreateReader();

            // Act & Assert
            for (var i = 0; i < CharData.Length; i++)
            {
                var peek = reader.Peek();
                Assert.Equal((int)CharData[i], peek);

                reader.Read();
            }
        }

        [Fact]
        public static void EmptyStream()
        {
            // Arrange
            var reader = new HttpRequestStreamReader(new MemoryStream(), Encoding.UTF8);
            var buffer = new char[10];

            // Act
            var read = reader.Read(buffer, 0, 1);

            // Assert
            Assert.Equal(0, read);
        }

        [Fact]
        public static void Read_ReadAllCharactersAtOnce()
        {
            // Arrange
            var reader = CreateReader();
            var chars = new char[CharData.Length];

            // Act
            var read = reader.Read(chars, 0, chars.Length);

            // Assert
            Assert.Equal(chars.Length, read);
            for (var i = 0; i < CharData.Length; i++)
            {
                Assert.Equal(CharData[i], chars[i]);
            }
        }

        [Fact]
        public static async Task Read_ReadInTwoChunks()
        {
            // Arrange
            var reader = CreateReader();
            var chars = new char[CharData.Length];

            // Act
            var read = await reader.ReadAsync(chars, 4, 3);

            // Assert
            Assert.Equal(read, 3);
            for (var i = 0; i < 3; i++)
            {
                Assert.Equal(CharData[i], chars[i + 4]);
            }
        }

        [Fact]
        public static void ReadLine_ReadMultipleLines()
        {
            // Arrange
            var reader = CreateReader();
            var valueString = new string(CharData);

            // Act & Assert
            var data = reader.ReadLine();
            Assert.Equal(valueString.Substring(0, valueString.IndexOf('\r')), data);

            data = reader.ReadLine();
            Assert.Equal(valueString.Substring(valueString.IndexOf('\r') + 1, 3), data);

            data = reader.ReadLine();
            Assert.Equal(valueString.Substring(valueString.IndexOf('\n') + 1, 2), data);

            data = reader.ReadLine();
            Assert.Equal((valueString.Substring(valueString.LastIndexOf('\n') + 1)), data);
        }

        [Fact]
        public static void ReadLine_ReadWithNoNewlines()
        {
            // Arrange
            var reader = CreateReader();
            var valueString = new string(CharData);
            var temp = new char[10];

            // Act
            reader.Read(temp, 0, 1);
            var data = reader.ReadLine();

            // Assert
            Assert.Equal(valueString.Substring(1, valueString.IndexOf('\r') - 1), data);
        }

        [Fact]
        public static async Task ReadLineAsync_MultipleContinuousLines()
        {
            // Arrange
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write("\n\n\r\r\n");
            writer.Flush();
            stream.Position = 0;

            var reader = new HttpRequestStreamReader(stream, Encoding.UTF8);

            // Act & Assert
            for (var i = 0; i < 4; i++)
            {
                var data = await reader.ReadLineAsync();
                Assert.Equal(string.Empty, data);
            }

            var eol = await reader.ReadLineAsync();
            Assert.Null(eol);
        }

        private static HttpRequestStreamReader CreateReader()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(CharData);
            writer.Flush();
            stream.Position = 0;

            return new HttpRequestStreamReader(stream, Encoding.UTF8);
        }

        private static MemoryStream GetSmallStream()
        {
            var testData = new byte[] { 72, 69, 76, 76, 79 };
            return new MemoryStream(testData);
        }

        private static MemoryStream GetLargeStream()
        {
            var testData = new byte[] { 72, 69, 76, 76, 79 };
            // System.Collections.Generic.

            var data = new List<byte>();
            for (var i = 0; i < 1000; i++)
            {
                data.AddRange(testData);
            }

            return new MemoryStream(data.ToArray());
        }
    }
}
