// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class ValueProviderResultTest
    {
        [Fact]
        public void ConvertTo_ReturnsNullForReferenceTypes_WhenValueIsNull()
        {
            var valueProviderResult = new ValueProviderResult(rawValue: null);

            var convertedValue = valueProviderResult.ConvertTo(typeof(string));

            Assert.Null(convertedValue);
        }

        [Fact]
        public void ConvertTo_ReturnsDefaultForValueTypes_WhenValueIsNull()
        {
            var valueProviderResult = new ValueProviderResult(rawValue: null);

            var convertedValue = valueProviderResult.ConvertTo(typeof(int));

            Assert.Equal(0, convertedValue);
        }

        [Fact]
        public void ConvertToCanConvertArraysToSingleElements()
        {
            // Arrange
            var valueProviderResult = new ValueProviderResult(
                new int[] { 1, 20, 42 },
                string.Empty,
                CultureInfo.InvariantCulture);

            // Act
            var converted = (string)valueProviderResult.ConvertTo(typeof(string));

            // Assert
            Assert.Equal("1", converted);
        }

        [Fact]
        public void ConvertToCanConvertSingleElementsToArrays()
        {
            // Arrange
            var valueProviderResult = new ValueProviderResult(42, string.Empty, CultureInfo.InvariantCulture);

            // Act
            var converted = (string[])valueProviderResult.ConvertTo(typeof(string[]));

            // Assert
            Assert.NotNull(converted);
            var result = Assert.Single(converted);
            Assert.Equal("42", result);
        }

        [Fact]
        public void ConvertToCanConvertSingleElementsToSingleElements()
        {
            // Arrange
            var valueProviderResult = new ValueProviderResult(42, string.Empty, CultureInfo.InvariantCulture);

            // Act
            var converted = (string)valueProviderResult.ConvertTo(typeof(string));

            // Assert
            Assert.NotNull(converted);
            Assert.Equal("42", converted);
        }

        [Fact]
        public void ConvertingNullStringToNullableIntReturnsNull()
        {
            // Arrange
            object original = null;
            var valueProviderResult = new ValueProviderResult(original, string.Empty, CultureInfo.InvariantCulture);

            // Act
            var returned = (int?)valueProviderResult.ConvertTo(typeof(int?));

            // Assert
            Assert.Equal(returned, null);
        }

        [Fact]
        public void ConvertingWhiteSpaceStringToNullableIntReturnsNull()
        {
            // Arrange
            var original = " ";
            var valueProviderResult = new ValueProviderResult(original, string.Empty, CultureInfo.InvariantCulture);

            // Act
            var returned = (int?)valueProviderResult.ConvertTo(typeof(int?));

            // Assert
            Assert.Equal(returned, null);
        }

        [Fact]
        public void ConvertToReturnsNullIfArrayElementValueIsNull()
        {
            // Arrange
            var valueProviderResult = new ValueProviderResult(rawValue: new string[] { null });

            // Act
            var outValue = valueProviderResult.ConvertTo(typeof(int));

            // Assert
            Assert.Null(outValue);
        }

        [Fact]
        public void ConvertToReturnsNullIfTryingToConvertEmptyArrayToSingleElement()
        {
            // Arrange
            var valueProviderResult = new ValueProviderResult(new int[0], string.Empty, CultureInfo.InvariantCulture);

            // Act
            var outValue = valueProviderResult.ConvertTo(typeof(int));

            // Assert
            Assert.Null(outValue);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" \t \r\n ")]
        public void ConvertToReturnsNullIfTrimmedValueIsEmptyString(object value)
        {
            // Arrange
            var valueProviderResult = new ValueProviderResult(rawValue: value);

            // Act
            var outValue = valueProviderResult.ConvertTo(typeof(int));

            // Assert
            Assert.Null(outValue);
        }

        [Fact]
        public void ConvertToReturnsNullIfTrimmedValueIsEmptyString()
        {
            // Arrange
            var valueProviderResult = new ValueProviderResult(rawValue: null);

            // Act
            var outValue = valueProviderResult.ConvertTo(typeof(int[]));

            // Assert
            Assert.Null(outValue);
        }

        [Fact]
        public void ConvertToReturnsValueIfArrayElementIsIntegerAndDestinationTypeIsEnum()
        {
            // Arrange
            var result = new ValueProviderResult(rawValue: new object[] { 1 });

            // Act
            var outValue = result.ConvertTo(typeof(IntEnum));

            // Assert
            Assert.Equal(outValue, IntEnum.Value1);
        }

        [Theory]
        [InlineData(1, typeof(IntEnum), IntEnum.Value1)]
        [InlineData(1L, typeof(LongEnum), LongEnum.Value1)]
        [InlineData(long.MaxValue, typeof(LongEnum), LongEnum.MaxValue)]
        [InlineData(1U, typeof(UnsignedIntEnum), UnsignedIntEnum.Value1)]
        [InlineData(1UL, typeof(IntEnum), IntEnum.Value1)]
        [InlineData((byte)1, typeof(ByteEnum), ByteEnum.Value1)]
        [InlineData(byte.MaxValue, typeof(ByteEnum), ByteEnum.MaxValue)]
        [InlineData((sbyte)1, typeof(ByteEnum), ByteEnum.Value1)]
        [InlineData((short)1, typeof(IntEnum), IntEnum.Value1)]
        [InlineData((ushort)1, typeof(IntEnum), IntEnum.Value1)]
        [InlineData(int.MaxValue, typeof(IntEnum?), IntEnum.MaxValue)]
        [InlineData(null, typeof(IntEnum?), null)]
        [InlineData(1L, typeof(LongEnum?), LongEnum.Value1)]
        [InlineData(null, typeof(LongEnum?), null)]
        [InlineData(uint.MaxValue, typeof(UnsignedIntEnum?), UnsignedIntEnum.MaxValue)]
        [InlineData((byte)1, typeof(ByteEnum?), ByteEnum.Value1)]
        [InlineData(null, typeof(ByteEnum?), null)]
        [InlineData((ushort)1, typeof(LongEnum?), LongEnum.Value1)]
        public void ConvertToReturnsValueIfArrayElementIsAnyIntegerTypeAndDestinationTypeIsEnum(
            object input,
            Type enumType,
            object expected)
        {
            // Arrange
            var result = new ValueProviderResult(rawValue: new object[] { input });

            // Act
            var outValue = result.ConvertTo(enumType);

            // Assert
            Assert.Equal(expected, outValue);
        }

        [Fact]
        public void ConvertToReturnsValueIfArrayElementIsStringValueAndDestinationTypeIsEnum()
        {
            // Arrange
            var valueProviderResult = new ValueProviderResult(rawValue: new object[] { "1" });

            // Act
            var outValue = valueProviderResult.ConvertTo(typeof(IntEnum));

            // Assert
            Assert.Equal(outValue, IntEnum.Value1);
        }

        [Fact]
        public void ConvertToReturnsValueIfArrayElementIsStringKeyAndDestinationTypeIsEnum()
        {
            // Arrange
            var valueProviderResult = new ValueProviderResult(rawValue: new object[] { "Value1" });

            // Act
            var outValue = valueProviderResult.ConvertTo(typeof(IntEnum));

            // Assert
            Assert.Equal(outValue, IntEnum.Value1);
        }

        [Fact]
        public void ConvertToReturnsValueIfElementIsStringAndDestinationIsNullableInteger()
        {
            // Arrange
            var valueProviderResult = new ValueProviderResult(rawValue: "12");

            // Act
            var outValue = valueProviderResult.ConvertTo(typeof(int?));

            // Assert
            Assert.Equal(12, outValue);
        }

        [Fact]
        public void ConvertToReturnsValueIfElementIsStringAndDestinationIsNullableDouble()
        {
            // Arrange
            var valueProviderResult = new ValueProviderResult(rawValue: "12.5");

            // Act
            var outValue = valueProviderResult.ConvertTo(typeof(double?));

            // Assert
            Assert.Equal(12.5, outValue);
        }

        [Fact]
        public void ConvertToReturnsValueIfElementIsDecimalAndDestinationIsNullableInteger()
        {
            // Arrange
            var valueProviderResult = new ValueProviderResult(rawValue: 12M);

            // Act
            var outValue = valueProviderResult.ConvertTo(typeof(int?));

            // Assert
            Assert.Equal(12, outValue);
        }

        [Fact]
        public void ConvertToReturnsValueIfElementIsDecimalAndDestinationIsNullableDouble()
        {
            // Arrange
            var valueProviderResult = new ValueProviderResult(rawValue: 12.5M);

            // Act
            var outValue = valueProviderResult.ConvertTo(typeof(double?));

            // Assert
            Assert.Equal(12.5, outValue);
        }

        [Fact]
        public void ConvertToReturnsValueIfElementIsDecimalDoubleAndDestinationIsNullableInteger()
        {
            // Arrange
            var valueProviderResult = new ValueProviderResult(rawValue: 12.5M);

            // Act
            var outValue = valueProviderResult.ConvertTo(typeof(int?));

            // Assert
            Assert.Equal(12, outValue);
        }

        [Fact]
        public void ConvertToReturnsValueIfElementIsDecimalDoubleAndDestinationIsNullableLong()
        {
            // Arrange
            var valueProviderResult = new ValueProviderResult(rawValue: 12.5M);

            // Act
            var outValue = valueProviderResult.ConvertTo(typeof(long?));

            // Assert
            Assert.Equal(12L, outValue);
        }

        [Fact]
        public void ConvertToReturnsValueIfArrayElementInstanceOfDestinationType()
        {
            // Arrange
            var valueProviderResult = new ValueProviderResult(rawValue: new object[] { "some string" });

            // Act
            var outValue = valueProviderResult.ConvertTo(typeof(string));

            // Assert
            Assert.Equal("some string", outValue);
        }

        [Theory]
        [InlineData(new object[] { new[] { 1, 0 } })]
        [InlineData(new object[] { new[] { "Value1", "Value0" } })]
        [InlineData(new object[] { new[] { "Value1", "value0" } })]
        public void ConvertTo_ConvertsEnumArrays(object value)
        {
            // Arrange
            var valueProviderResult = new ValueProviderResult(rawValue: value);

            // Act
            var outValue = valueProviderResult.ConvertTo(typeof(IntEnum[]));

            // Assert
            var result = Assert.IsType<IntEnum[]>(outValue);
            Assert.Equal(2, result.Length);
            Assert.Equal(IntEnum.Value1, result[0]);
            Assert.Equal(IntEnum.Value0, result[1]);
        }

        [Theory]
        [InlineData(new object[] { new[] { 1, 2 }, new[] { FlagsEnum.Value1, FlagsEnum.Value2 } })]
        [InlineData(new object[] { new[] { "Value1", "Value2" }, new[] { FlagsEnum.Value1, FlagsEnum.Value2 } })]
        [InlineData(new object[] { new[] { 5, 2 }, new[] { FlagsEnum.Value1 | FlagsEnum.Value4, FlagsEnum.Value2 } })]
        public void ConvertTo_ConvertsFlagsEnumArrays(object value, FlagsEnum[] expected)
        {
            // Arrange
            var valueProviderResult = new ValueProviderResult(rawValue: value);

            // Act
            var outValue = valueProviderResult.ConvertTo(typeof(FlagsEnum[]));

            // Assert
            var result = Assert.IsType<FlagsEnum[]>(outValue);
            Assert.Equal(2, result.Length);
            Assert.Equal(expected[0], result[0]);
            Assert.Equal(expected[1], result[1]);
        }

        [Fact]
        public void ConvertToReturnsValueIfInstanceOfDestinationType()
        {
            // Arrange
            var original = new[] { "some string" };
            var valueProviderResult = new ValueProviderResult(rawValue: original);

            // Act
            var outValue = valueProviderResult.ConvertTo(typeof(string[]));

            // Assert
            Assert.Same(original, outValue);
        }

        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(double?))]
        [InlineData(typeof(IntEnum?))]
        public void ConvertToThrowsIfConverterThrows(Type destinationType)
        {
            // Arrange
            var valueProviderResult = new ValueProviderResult(rawValue: "this-is-not-a-valid-value");

            // Act & Assert
            var ex = Assert.Throws(typeof(FormatException), () => valueProviderResult.ConvertTo(destinationType));
        }

        [Fact]
        public void ConvertToThrowsIfNoConverterExists()
        {
            // Arrange
            var valueProviderResult = new ValueProviderResult(rawValue: "x");
            var destinationType = typeof(MyClassWithoutConverter);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => valueProviderResult.ConvertTo(destinationType));
            Assert.Equal("The parameter conversion from type 'System.String' to type " +
                        "'Microsoft.AspNet.Mvc.ModelBinding.ValueProviderResultTest+MyClassWithoutConverter' " +
                        "failed because no type converter can convert between these types.",
                         ex.Message);
        }

        [Fact]
        public void ConvertToUsesProvidedCulture()
        {
            // Arrange
            var original = "12,5";
            var valueProviderResult = new ValueProviderResult(
                rawValue: original,
                attemptedValue: null,
                culture: new CultureInfo("en-GB"));
            var frCulture = new CultureInfo("fr-FR");

            // Act
            var cultureResult = valueProviderResult.ConvertTo(typeof(decimal), frCulture);

            // Assert
            Assert.Equal(12.5M, cultureResult);
            Assert.Throws<FormatException>(() => valueProviderResult.ConvertTo(typeof(decimal)));
        }

        [Fact]
        public void CulturePropertyDefaultsToInvariantCulture()
        {
            // Arrange
            var result = new ValueProviderResult(rawValue: null, attemptedValue: null, culture: null);

            // Act & assert
            Assert.Same(CultureInfo.InvariantCulture, result.Culture);
        }

        [Theory]
        [MemberData(nameof(IntrinsicConversionData))]
        public void ConvertToCanConvertIntrinsics<T>(object initialValue, T expectedValue)
        {
            // Arrange
            var result = new ValueProviderResult(initialValue, string.Empty, CultureInfo.InvariantCulture);

            // Act & Assert
            Assert.Equal(expectedValue, result.ConvertTo(typeof(T)));
        }

        public static IEnumerable<object[]> IntrinsicConversionData
        {
            get
            {
                yield return new object[] { 42, 42L };
                yield return new object[] { 42, (short)42 };
                yield return new object[] { 42, (float)42.0 };
                yield return new object[] { 42, (double)42.0 };
                yield return new object[] { 42M, 42 };
                yield return new object[] { 42L, 42 };
                yield return new object[] { 42, (byte)42 };
                yield return new object[] { (short)42, 42 };
                yield return new object[] { (float)42.0, 42 };
                yield return new object[] { (double)42.0, 42 };
                yield return new object[] { (byte)42, 42 };
                yield return new object[] { "2008-01-01", new DateTime(2008, 01, 01) };
                yield return new object[] { "00:00:20", TimeSpan.FromSeconds(20) };
                yield return new object[]
                {
                    "c6687d3a-51f9-4159-8771-a66d2b7d7038",
                    Guid.Parse("c6687d3a-51f9-4159-8771-a66d2b7d7038")
                };
            }
        }

        [Theory]
        [InlineData(typeof(TimeSpan))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(DateTimeOffset))]
        [InlineData(typeof(Guid))]
        [InlineData(typeof(IntEnum))]
        public void ConvertTo_Throws_IfValueIsNotStringData(Type destinationType)
        {
            // Arrange
            var result = new ValueProviderResult(
                new MyClassWithoutConverter(),
                string.Empty,
                CultureInfo.InvariantCulture);

            // Act
            var ex = Assert.Throws<InvalidOperationException>(() => result.ConvertTo(destinationType));

            // Assert
            var expectedMessage = string.Format("The parameter conversion from type '{0}' to type '{1}' " +
                                                "failed because no type converter can convert between these types.",
                                                typeof(MyClassWithoutConverter), destinationType);
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void ConvertTo_Throws_IfDestinationTypeIsNotConvertible()
        {
            // Arrange
            var value = "Hello world";
            var destinationType = typeof(MyClassWithoutConverter);
            var result = new ValueProviderResult(value, string.Empty, CultureInfo.InvariantCulture);

            // Act
            var ex = Assert.Throws<InvalidOperationException>(() => result.ConvertTo(destinationType));

            // Assert
            var expectedMessage = string.Format("The parameter conversion from type '{0}' to type '{1}' " +
                                                "failed because no type converter can convert between these types.",
                                                value.GetType(), typeof(MyClassWithoutConverter));
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Theory]
        [InlineData(new object[] { 2, FlagsEnum.Value2 })]
        [InlineData(new object[] { 5, FlagsEnum.Value1 | FlagsEnum.Value4 })]
        [InlineData(new object[] { 15, FlagsEnum.Value1 | FlagsEnum.Value2 | FlagsEnum.Value4 | FlagsEnum.Value8 })]
        [InlineData(new object[] { 16, (FlagsEnum)16 })]
        [InlineData(new object[] { 0, (FlagsEnum)0 })]
        [InlineData(new object[] { null, (FlagsEnum)0 })]
        [InlineData(new object[] { "Value1,Value2", (FlagsEnum)3 })]
        [InlineData(new object[] { "Value1,Value2,value4, value8", (FlagsEnum)15 })]
        public void ConvertTo_ConvertsEnumFlags(object value, object expected)
        {
            // Arrange
            var valueProviderResult = new ValueProviderResult(rawValue: value);

            // Act
            var outValue = (FlagsEnum)valueProviderResult.ConvertTo(typeof(FlagsEnum));

            // Assert
            Assert.Equal(expected, outValue);
        }

        private class MyClassWithoutConverter
        {
        }

        private enum IntEnum
        {
            Value0 = 0,
            Value1 = 1,
            MaxValue = int.MaxValue
        }

        private enum LongEnum : long
        {
            Value0 = 0L,
            Value1 = 1L,
            MaxValue = long.MaxValue
        }

        private enum UnsignedIntEnum : uint
        {
            Value0 = 0U,
            Value1 = 1U,
            MaxValue = uint.MaxValue
        }

        private enum ByteEnum : byte
        {
            Value0 = 0,
            Value1 = 1,
            MaxValue = byte.MaxValue
        }

        [Flags]
        public enum FlagsEnum
        {
            Value1 = 1,
            Value2 = 2,
            Value4 = 4,
            Value8 = 8
        }
    }
}