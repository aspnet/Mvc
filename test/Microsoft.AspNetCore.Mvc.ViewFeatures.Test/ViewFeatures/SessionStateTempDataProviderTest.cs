// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Internal;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures
{
    public class SessionStateTempDataProviderTest
    {
        [Fact]
        public void Load_ThrowsException_WhenSessionIsNotEnabled()
        {
            // Arrange
            var testProvider = new SessionStateTempDataProvider();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                testProvider.LoadTempData(GetHttpContext(sessionEnabled: false));
            });
        }

        [Fact]
        public void Save_ThrowsException_WhenSessionIsNotEnabled()
        {
            // Arrange
            var testProvider = new SessionStateTempDataProvider();
            var values = new Dictionary<string, object>();
            values.Add("key1", "value1");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                testProvider.SaveTempData(GetHttpContext(sessionEnabled: false), values);
            });
        }

        [Fact]
        public void Load_ReturnsEmptyDictionary_WhenNoSessionDataIsAvailable()
        {
            // Arrange
            var testProvider = new SessionStateTempDataProvider();

            // Act
            var tempDataDictionary = testProvider.LoadTempData(GetHttpContext());

            // Assert
            Assert.Empty(tempDataDictionary);
        }

        public static TheoryData<object, Type> InvalidTypes
        {
            get
            {
                return new TheoryData<object, Type>
                {
                    { new object(), typeof(object) },
                    { new object[3], typeof(object) },
                    { new TestItem(), typeof(TestItem) },
                    { new List<TestItem>(), typeof(TestItem) },
                    { new Dictionary<string, TestItem>(), typeof(TestItem) },
                };
            }
        }

        [Theory]
        [MemberData(nameof(InvalidTypes))]
        public void EnsureObjectCanBeSerialized_ThrowsException_OnInvalidType(object value, Type type)
        {
            // Arrange
            var testProvider = new SessionStateTempDataProvider();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                testProvider.EnsureObjectCanBeSerialized(value);
            });
            Assert.Equal($"The '{typeof(SessionStateTempDataProvider).FullName}' cannot serialize " +
                $"an object of type '{type}' to session state.",
                exception.Message);
        }

        public static TheoryData<object, Type> InvalidDictionaryTypes
        {
            get
            {
                return new TheoryData<object, Type>
                {
                    { new Dictionary<int, string>(), typeof(int) },
                    { new Dictionary<Uri, Guid>(), typeof(Uri) },
                    { new Dictionary<object, string>(), typeof(object) },
                    { new Dictionary<TestItem, TestItem>(), typeof(TestItem) }
                };
            }
        }

        [Theory]
        [MemberData(nameof(InvalidDictionaryTypes))]
        public void EnsureObjectCanBeSerialized_ThrowsException_OnInvalidDictionaryType(object value, Type type)
        {
            // Arrange
            var testProvider = new SessionStateTempDataProvider();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                testProvider.EnsureObjectCanBeSerialized(value);
            });
            Assert.Equal($"The '{typeof(SessionStateTempDataProvider).FullName}' cannot serialize a dictionary " +
                $"with a key of type '{type}' to session state.",
                exception.Message);
        }

        public static TheoryData<object> ValidTypes
        {
            get
            {
                return new TheoryData<object>
                {
                    { 10 },
                    { new int[]{ 10, 20 } },
                    { "FooValue" },
                    { new Uri("http://Foo") },
                    { Guid.NewGuid() },
                    { new List<string> { "foo", "bar" } },
                    { new DateTimeOffset() },
                    { 100.1m },
                    { new Dictionary<string, int>() },
                    { new Uri[] { new Uri("http://Foo"), new Uri("http://Bar") } },
                    { DayOfWeek.Sunday },
                };
            }
        }

        [Theory]
        [MemberData(nameof(ValidTypes))]
        public void EnsureObjectCanBeSerialized_DoesNotThrow_OnValidType(object value)
        {
            // Arrange
            var testProvider = new SessionStateTempDataProvider();

            // Act & Assert (Does not throw)
            testProvider.EnsureObjectCanBeSerialized(value);
        }

        [Fact]
        public void SaveAndLoad_StringCanBeStoredAndLoaded()
        {
            // Arrange
            var testProvider = new SessionStateTempDataProvider();
            var input = new Dictionary<string, object>
            {
                { "string", "value" }
            };
            var context = GetHttpContext();

            // Act
            testProvider.SaveTempData(context, input);
            var TempData = testProvider.LoadTempData(context);

            // Assert
            var stringVal = Assert.IsType<string>(TempData["string"]);
            Assert.Equal("value", stringVal);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void SaveAndLoad_IntCanBeStoredAndLoaded(int expected)
        {
            // Arrange
            var testProvider = new SessionStateTempDataProvider();
            var input = new Dictionary<string, object>
            {
                { "int", expected }
            };
            var context = GetHttpContext();

            // Act
            testProvider.SaveTempData(context, input);
            var TempData = testProvider.LoadTempData(context);

            // Assert
            var intVal = Assert.IsType<int>(TempData["int"]);
            Assert.Equal(expected, intVal);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void SaveAndLoad_BoolCanBeStoredAndLoaded(bool value)
        {
            // Arrange
            var testProvider = new SessionStateTempDataProvider();
            var input = new Dictionary<string, object>
            {
                { "bool", value }
            };
            var context = GetHttpContext();

            // Act
            testProvider.SaveTempData(context, input);
            var TempData = testProvider.LoadTempData(context);

            // Assert
            var boolVal = Assert.IsType<bool>(TempData["bool"]);
            Assert.Equal(value, boolVal);
        }

        [Fact]
        public void SaveAndLoad_DateTimeCanBeStoredAndLoaded()
        {
            // Arrange
            var testProvider = new SessionStateTempDataProvider();
            var inputDatetime = new DateTime(2010, 12, 12, 1, 2, 3, DateTimeKind.Local);
            var input = new Dictionary<string, object>
            {
                { "DateTime", inputDatetime }
            };
            var context = GetHttpContext();

            // Act
            testProvider.SaveTempData(context, input);
            var TempData = testProvider.LoadTempData(context);

            // Assert
            var datetime = Assert.IsType<DateTime>(TempData["DateTime"]);
            Assert.Equal(inputDatetime, datetime);
        }

        [Fact]
        public void SaveAndLoad_GuidCanBeStoredAndLoaded()
        {
            // Arrange
            var testProvider = new SessionStateTempDataProvider();
            var inputGuid = Guid.NewGuid();
            var input = new Dictionary<string, object>
            {
                { "Guid", inputGuid }
            };
            var context = GetHttpContext();

            // Act
            testProvider.SaveTempData(context, input);
            var TempData = testProvider.LoadTempData(context);

            // Assert
            var guidVal = Assert.IsType<Guid>(TempData["Guid"]);
            Assert.Equal(inputGuid, guidVal);
        }

        [Fact]
        public void SaveAndLoad_EnumCanBeSavedAndLoaded()
        {
            // Arrange
            var key = "EnumValue";
            var testProvider = new SessionStateTempDataProvider();
            var expected = DayOfWeek.Friday;
            var input = new Dictionary<string, object>
            {
                { key, expected }
            };
            var context = GetHttpContext();

            // Act
            testProvider.SaveTempData(context, input);
            var TempData = testProvider.LoadTempData(context);
            var result = TempData[key];

            // Assert
            var actual = (DayOfWeek)result;
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(3100000000L)]
        [InlineData(-3100000000L)]
        public void SaveAndLoad_LongCanBeSavedAndLoaded(long expected)
        {
            // Arrange
            var key = "LongValue";
            var testProvider = new SessionStateTempDataProvider();
            var input = new Dictionary<string, object>
            {
                { key, expected }
            };
            var context = GetHttpContext();

            // Act
            testProvider.SaveTempData(context, input);
            var TempData = testProvider.LoadTempData(context);
            var result = TempData[key];

            // Assert
            var actual = Assert.IsType<long>(result);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SaveAndLoad_ListCanBeStoredAndLoaded()
        {
            // Arrange
            var testProvider = new SessionStateTempDataProvider();
            var input = new Dictionary<string, object>
            {
                { "List`string", new List<string> { "one", "two" } }
            };
            var context = GetHttpContext();

            // Act
            testProvider.SaveTempData(context, input);
            var TempData = testProvider.LoadTempData(context);

            // Assert
            var list = (IList<string>)TempData["List`string"];
            Assert.Equal(2, list.Count);
            Assert.Equal("one", list[0]);
            Assert.Equal("two", list[1]);
        }

        [Fact]
        public void SaveAndLoad_DictionaryCanBeStoredAndLoaded()
        {
            // Arrange
            var testProvider = new SessionStateTempDataProvider();
            var inputDictionary = new Dictionary<string, string>
            {
                { "Hello", "World" },
            };
            var input = new Dictionary<string, object>
            {
                { "Dictionary", inputDictionary }
            };
            var context = GetHttpContext();

            // Act
            testProvider.SaveTempData(context, input);
            var TempData = testProvider.LoadTempData(context);

            // Assert
            var dictionary = Assert.IsType<Dictionary<string, string>>(TempData["Dictionary"]);
            Assert.Equal("World", dictionary["Hello"]);
        }

        [Fact]
        public void SaveAndLoad_EmptyDictionary_RoundTripsAsNull()
        {
            // Arrange
            var testProvider = new SessionStateTempDataProvider();
            var input = new Dictionary<string, object>
            {
                { "EmptyDictionary", new Dictionary<string, int>() }
            };
            var context = GetHttpContext();

            // Act
            testProvider.SaveTempData(context, input);
            var TempData = testProvider.LoadTempData(context);

            // Assert
            var emptyDictionary = (IDictionary<string, int>)TempData["EmptyDictionary"];
            Assert.Null(emptyDictionary);
        }

        private class TestItem
        {
            public int DummyInt { get; set; }
        }

        private HttpContext GetHttpContext(bool sessionEnabled = true)
        {
            var httpContext = new DefaultHttpContext();
            if (sessionEnabled)
            {
                httpContext.Features.Set<ISessionFeature>(new SessionFeature() { Session = new TestSession() });
            }
            return httpContext;
        }

        private class SessionFeature : ISessionFeature
        {
            public ISession Session { get; set; }
        }

        private class TestSession : ISession
        {
            private Dictionary<string, byte[]> _innerDictionary = new Dictionary<string, byte[]>();

            public IEnumerable<string> Keys { get { return _innerDictionary.Keys; } }

            public Task LoadAsync()
            {
                return Task.FromResult(0);
            }

            public Task CommitAsync()
            {
                return Task.FromResult(0);
            }

            public void Clear()
            {
                _innerDictionary.Clear();
            }

            public void Remove(string key)
            {
                _innerDictionary.Remove(key);
            }

            public void Set(string key, byte[] value)
            {
                _innerDictionary[key] = value.ToArray();
            }

            public bool TryGetValue(string key, out byte[] value)
            {
                return _innerDictionary.TryGetValue(key, out value);
            }
        }
    }
}