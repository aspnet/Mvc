﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.AspNet.Abstractions;
using Microsoft.AspNet.PipelineCore.Collections;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
namespace Microsoft.AspNet.Mvc.Core.Test
{
    public class AntiForgeryTokenStoreTest
    {
        private readonly string _cookieName = "cookie-name";

        [Fact]
        public void GetCookieToken_CookieDoesNotExist_ReturnsNull()
        {
            // Arrange

            var requestCookies = new Mock<IReadableStringCollection>();
            requestCookies.Setup(o => o.Get(It.IsAny<string>())).Returns(string.Empty);
            Mock<HttpContext> mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(o => o.Request.Cookies).Returns(requestCookies.Object);
            var config = new MockAntiForgeryConfig()
            {
                CookieName = _cookieName
            };

            AntiForgeryTokenStore tokenStore = new AntiForgeryTokenStore(
                config: config,
                serializer: null);

            // Act
            AntiForgeryToken token = tokenStore.GetCookieToken(mockHttpContext.Object);

            // Assert
            Assert.Null(token);
        }

        [Fact]
        public void GetCookieToken_CookieIsEmpty_ReturnsNull()
        {
            // Arrange
            var mockHttpContext = GetMockHttpContext(_cookieName, string.Empty);

            MockAntiForgeryConfig config = new MockAntiForgeryConfig()
            {
                CookieName = _cookieName
            };

            AntiForgeryTokenStore tokenStore = new AntiForgeryTokenStore(
                config: config,
                serializer: null);

            // Act
            AntiForgeryToken token = tokenStore.GetCookieToken(mockHttpContext);

            // Assert
            Assert.Null(token);
        }

        [Fact]
        public void GetCookieToken_CookieIsInvalid_PropagatesException()
        {
            // Arrange
            var mockHttpContext = GetMockHttpContext(_cookieName, "invalid-value");
            MockAntiForgeryConfig config = new MockAntiForgeryConfig()
            {
                CookieName = _cookieName
            };

            var expectedException = new InvalidOperationException("some exception");
            var mockSerializer = new Mock<MockableAntiForgeryTokenSerializer>();
            mockSerializer.Setup(o => o.Deserialize("invalid-value")).Throws(expectedException);

            AntiForgeryTokenStore tokenStore = new AntiForgeryTokenStore(
                config: config,
                serializer: mockSerializer.Object);

            // Act & assert
            var ex = Assert.Throws<InvalidOperationException>(() => tokenStore.GetCookieToken(mockHttpContext));
            Assert.Equal(expectedException, ex);
        }

        [Fact]
        public void GetCookieToken_CookieIsValid_ReturnsToken()
        {
            // Arrange
            AntiForgeryToken expectedToken = new AntiForgeryToken();
            var mockHttpContext = GetMockHttpContext(_cookieName, "valid-value");


            MockAntiForgeryConfig config = new MockAntiForgeryConfig()
            {
                CookieName = _cookieName
            };

            var mockSerializer = new Mock<MockableAntiForgeryTokenSerializer>();
            mockSerializer.Setup(o => o.Deserialize("valid-value")).Returns((object)expectedToken);

            AntiForgeryTokenStore tokenStore = new AntiForgeryTokenStore(
                config: config,
                serializer: mockSerializer.Object);

            // Act
            AntiForgeryToken retVal = tokenStore.GetCookieToken(mockHttpContext);

            // Assert
            Assert.Same(expectedToken, retVal);
        }

        [Fact]
        public async Task GetFormToken_FormFieldIsEmpty_ReturnsNull()
        {
            // Arrange
            Mock<HttpContext> mockHttpContext = new Mock<HttpContext>();
            var requestContext = new Mock<HttpRequest>();
            IReadableStringCollection formsCollection =
                new MockCookieCollection(new Dictionary<string, string>() {{"form-field-name", string.Empty}});
            requestContext.Setup(o => o.GetFormAsync())
                .Returns(Task.FromResult(formsCollection));
            mockHttpContext.Setup(o => o.Request).Returns(requestContext.Object);

            MockAntiForgeryConfig config = new MockAntiForgeryConfig()
            {
                FormFieldName = "form-field-name"
            };

            AntiForgeryTokenStore tokenStore = new AntiForgeryTokenStore(
                config: config,
                serializer: null);

            // Act
            AntiForgeryToken token = await tokenStore.GetFormTokenAsync(mockHttpContext.Object);

            // Assert
            Assert.Null(token);
        }

        [Fact]
        public async Task GetFormToken_FormFieldIsInvalid_PropagatesException()
        {
            // Arrange
            Mock<HttpContext> mockHttpContext = new Mock<HttpContext>();
            var requestContext = new Mock<HttpRequest>();
            IReadableStringCollection formsCollection =
                new MockCookieCollection(new Dictionary<string, string>() { { "form-field-name", "invalid-value" } });
            requestContext.Setup(o => o.GetFormAsync())
                .Returns(Task.FromResult(formsCollection));
            mockHttpContext.Setup(o => o.Request).Returns(requestContext.Object);

            MockAntiForgeryConfig config = new MockAntiForgeryConfig()
            {
                FormFieldName = "form-field-name"
            };

            InvalidOperationException expectedException = new InvalidOperationException("some exception");
            var mockSerializer = new Mock<MockableAntiForgeryTokenSerializer>();
            mockSerializer.Setup(o => o.Deserialize("invalid-value")).Throws(expectedException);

            AntiForgeryTokenStore tokenStore = new AntiForgeryTokenStore(
                config: config,
                serializer: mockSerializer.Object);

            // Act & assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await tokenStore.GetFormTokenAsync(mockHttpContext.Object));
            Assert.Same(expectedException, ex);
        }

        [Fact]
        public async Task GetFormToken_FormFieldIsValid_ReturnsToken()
        {
            // Arrange
            AntiForgeryToken expectedToken = new AntiForgeryToken();

            // Arrange
            Mock<HttpContext> mockHttpContext = new Mock<HttpContext>();
            var requestContext = new Mock<HttpRequest>();
            IReadableStringCollection formsCollection =
                new MockCookieCollection(new Dictionary<string, string>() { { "form-field-name", "valid-value" } });
            requestContext.Setup(o => o.GetFormAsync())
                .Returns(Task.FromResult(formsCollection));
            mockHttpContext.Setup(o => o.Request).Returns(requestContext.Object);

            MockAntiForgeryConfig config = new MockAntiForgeryConfig()
            {
                FormFieldName = "form-field-name"
            };

            var mockSerializer = new Mock<MockableAntiForgeryTokenSerializer>();
            mockSerializer.Setup(o => o.Deserialize("valid-value")).Returns((object)expectedToken);

            AntiForgeryTokenStore tokenStore = new AntiForgeryTokenStore(
                config: config,
                serializer: mockSerializer.Object);

            // Act
            AntiForgeryToken retVal = await tokenStore.GetFormTokenAsync(mockHttpContext.Object);

            // Assert
            Assert.Same(expectedToken, retVal);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, null)]
        public void SaveCookieToken(bool requireSsl, bool? expectedCookieSecureFlag)
        {
            // Arrange
            AntiForgeryToken token = new AntiForgeryToken();
            var mockCookies = new Mock<IResponseCookies>();
            
            // TODO : Once we decide on where to pick this value from enable this.  
            bool defaultCookieSecureValue = expectedCookieSecureFlag ?? false; // pulled from config; set by ctor
            var cookies = new MockResponseCookieCollection();

            cookies.Count = 0;
            Mock<HttpContext> mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(o => o.Response.Cookies).Returns(cookies);

            var mockSerializer = new Mock<MockableAntiForgeryTokenSerializer>();
            mockSerializer.Setup(o => o.Serialize(token)).Returns("serialized-value");

            MockAntiForgeryConfig config = new MockAntiForgeryConfig()
            {
                CookieName = _cookieName,
                RequireSSL = requireSsl
            };

            AntiForgeryTokenStore tokenStore = new AntiForgeryTokenStore(
                config: config,
                serializer: mockSerializer.Object);

            // Act
            tokenStore.SaveCookieToken(mockHttpContext.Object, token);

            // Assert
            Assert.Equal(1, cookies.Count);

            Assert.NotNull(cookies);
            Assert.Equal(_cookieName, cookies.Key);
            Assert.Equal("serialized-value", cookies.Value);
            Assert.True(cookies.Options.HttpOnly);
            Assert.Equal(defaultCookieSecureValue, cookies.Options.Secure);
        }


        private HttpContext GetMockHttpContext(string cookieName, string cookieValue)
        {
            var requestCookies = new MockCookieCollection(new Dictionary<string, string>(){{cookieName, cookieValue}});
           

            var request = new Mock<HttpRequest>();
            request.Setup(o => o.Cookies).Returns(requestCookies);
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(o => o.Request).Returns(request.Object);

            return mockHttpContext.Object;
        }

        private class MockResponseCookieCollection : IResponseCookies
        {
            public string Key { get; set; }
            public string Value { get; set; }
            public CookieOptions Options { get; set; }
            public int Count { get; set; }
            public void Append(string key, string value, CookieOptions options)
            {
                this.Key = key;
                this.Value = value;
                this.Options = options;
                this.Count++;
            }

            public void Append(string key, string value)
            {
                throw new NotImplementedException();
            }

            public void Delete(string key, CookieOptions options)
            {
                throw new NotImplementedException();
            }

            public void Delete(string key)
            {
                throw new NotImplementedException();
            }
        }


        private class MockCookieCollection : IReadableStringCollection
        {
            private Dictionary<string, string> dictionary;

            public MockCookieCollection(Dictionary<string, string> dictionary)
            {
                // TODO: Complete member initialization
                this.dictionary = dictionary;
            }

            public static MockCookieCollection GetDummyInstance(string key, string value)
            {
                return new MockCookieCollection(new Dictionary<string,string> (){{key, value}});
            }
            public string Get(string key)
            {
                return this[key];
            }

            public IList<string> GetValues(string key)
            {
                throw new NotImplementedException();
            }

            public string this[string key]
            {
                get { return this.dictionary[key]; }
            }

            public IEnumerator<KeyValuePair<string, string[]>> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }
    }
}