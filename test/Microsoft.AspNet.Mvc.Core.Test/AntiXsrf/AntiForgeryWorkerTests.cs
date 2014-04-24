using Microsoft.AspNet.Abstractions;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.PipelineCore.Collections;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNet.Mvc.Core.Test
{
    public class AntiForgeryWorkerTest
    {
        [Fact]
        public async Task ChecksSSL()
        {
            // Arrange
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(o => o.Request.IsSecure)
                           .Returns(false);

            var config = new MockAntiForgeryConfig()
            {
                RequireSSL = true
            };

            AntiForgeryWorker worker = new AntiForgeryWorker(
                config: config,
                serializer: null,
                tokenStore: null,
                generator: null,
                validator: null);

            // Act & assert
            var ex =
                await
                    Assert.ThrowsAsync<InvalidOperationException>(
                        async () => await worker.ValidateAsync(mockHttpContext.Object));
            Assert.Equal(
                @"The anti-forgery system has the configuration value AntiForgeryConfig.RequireSsl = true, but the current request is not an SSL request.",
                ex.Message);

            ex =
                await
                    Assert.ThrowsAsync<InvalidOperationException>(
                        async () => await worker.ValidateAsync(mockHttpContext.Object));
            Assert.Equal(
                @"The anti-forgery system has the configuration value AntiForgeryConfig.RequireSsl = true, but the current request is not an SSL request.",
                ex.Message);

            ex = Assert.Throws<InvalidOperationException>(() => worker.GetFormInputElement(mockHttpContext.Object));
            Assert.Equal(
                @"The anti-forgery system has the configuration value AntiForgeryConfig.RequireSsl = true, but the current request is not an SSL request.",
                ex.Message);

            ex = Assert.Throws<InvalidOperationException>(() => worker.GetTokens(mockHttpContext.Object, "cookie-token"));
            Assert.Equal(
                @"The anti-forgery system has the configuration value AntiForgeryConfig.RequireSsl = true, but the current request is not an SSL request.",
                ex.Message);
        }

        [Fact]
        public void GetFormInputElement_ExistingInvalidCookieToken()
        {
            // Arrange
            var identity = new GenericIdentity("some-user");
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(o => o.User)
                           .Returns(new GenericPrincipal(identity, new string[0]));

            var mockResponse = new Mock<HttpResponse>();
            mockResponse.Setup(r => r.Headers)
                        .Returns(new HeaderDictionary(new Dictionary<string, string[]>()));
            mockHttpContext.Setup(o => o.Response)
                           .Returns(mockResponse.Object);

            var oldCookieToken = new AntiForgeryToken() { IsSessionToken = true };
            var newCookieToken = new AntiForgeryToken() { IsSessionToken = true };
            var formToken = new AntiForgeryToken();

            var config = new MockAntiForgeryConfig()
            {
                FormFieldName = "form-field-name"
            };

            var mockSerializer = new Mock<MockableAntiForgeryTokenSerializer>(MockBehavior.Strict);
            mockSerializer.Setup(o => o.Serialize(formToken))
                          .Returns("serialized-form-token");

            var mockTokenStore = new Mock<MockableTokenStore>(MockBehavior.Strict);
            mockTokenStore.Setup(o => o.GetCookieToken(mockHttpContext.Object))
                          .Returns(oldCookieToken);
            mockTokenStore.Setup(o => o.SaveCookieToken(mockHttpContext.Object, newCookieToken))
                          .Verifiable();

            var mockValidator = new Mock<MockableTokenProvider>(MockBehavior.Strict);
            mockValidator.Setup(o => o.GenerateFormToken(mockHttpContext.Object, identity, newCookieToken))
                         .Returns(formToken);
            mockValidator.Setup(o => o.IsCookieTokenValid(oldCookieToken))
                         .Returns(false);
            mockValidator.Setup(o => o.IsCookieTokenValid(newCookieToken))
                         .Returns(true);
            mockValidator.Setup(o => o.GenerateCookieToken())
                         .Returns(newCookieToken);

            var worker = new AntiForgeryWorker(
                config: config,
                serializer: mockSerializer.Object,
                tokenStore: mockTokenStore.Object,
                generator: mockValidator.Object,
                validator: mockValidator.Object);

            // Act
            var retVal = worker.GetFormInputElement(mockHttpContext.Object);

            // Assert
            Assert.Equal(@"<input name=""form-field-name"" type=""hidden"" value=""serialized-form-token"" />",
                retVal.ToString(TagRenderMode.SelfClosing));
            mockTokenStore.Verify();
        }

        [Fact]
        public void GetFormInputElement_ExistingInvalidCookieToken_SwallowsExceptions()
        {
            // Arrange
            var identity = new GenericIdentity("some-user");
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(o => o.User)
                           .Returns(new GenericPrincipal(identity, new string[0]));

            var mockResponse = new Mock<HttpResponse>();
            mockResponse.Setup(r => r.Headers)
                        .Returns(new HeaderDictionary(new Dictionary<string, string[]>()));
            mockHttpContext.Setup(o => o.Response)
                           .Returns(mockResponse.Object);

            var oldCookieToken = new AntiForgeryToken() { IsSessionToken = true };
            var newCookieToken = new AntiForgeryToken() { IsSessionToken = true };
            var formToken = new AntiForgeryToken();

            var config = new MockAntiForgeryConfig()
            {
                FormFieldName = "form-field-name"
            };

            var mockSerializer = new Mock<MockableAntiForgeryTokenSerializer>(MockBehavior.Strict);
            mockSerializer.Setup(o => o.Serialize(formToken))
                          .Returns("serialized-form-token");

            var mockTokenStore = new Mock<MockableTokenStore>(MockBehavior.Strict);
            mockTokenStore.Setup(o => o.GetCookieToken(mockHttpContext.Object))
                          .Throws(new Exception("should be swallowed"));
            mockTokenStore.Setup(o => o.SaveCookieToken(mockHttpContext.Object, newCookieToken))
                          .Verifiable();

            var mockValidator = new Mock<MockableTokenProvider>(MockBehavior.Strict);
            mockValidator.Setup(o => o.GenerateFormToken(mockHttpContext.Object, identity, newCookieToken))
                         .Returns(formToken);
            mockValidator.Setup(o => o.IsCookieTokenValid(null))
                         .Returns(false);
            mockValidator.Setup(o => o.IsCookieTokenValid(newCookieToken))
                         .Returns(true);
            mockValidator.Setup(o => o.GenerateCookieToken())
                         .Returns(newCookieToken);

            var worker = new AntiForgeryWorker(
                config: config,
                serializer: mockSerializer.Object,
                tokenStore: mockTokenStore.Object,
                generator: mockValidator.Object,
                validator: mockValidator.Object);

            // Act
            var retVal = worker.GetFormInputElement(mockHttpContext.Object);

            // Assert
            Assert.Equal(@"<input name=""form-field-name"" type=""hidden"" value=""serialized-form-token"" />",
                retVal.ToString(TagRenderMode.SelfClosing));
            mockTokenStore.Verify();
        }

        [Fact]
        public void GetFormInputElement_ExistingValidCookieToken()
        {
            // Arrange
            var identity = new GenericIdentity("some-user");
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(o => o.User)
                           .Returns(new GenericPrincipal(identity, new string[0]));

            var mockResponse = new Mock<HttpResponse>();
            mockResponse.Setup(r => r.Headers)
                        .Returns(new HeaderDictionary(new Dictionary<string, string[]>()));
            mockHttpContext.Setup(o => o.Response)
                           .Returns(mockResponse.Object);

            var cookieToken = new AntiForgeryToken() { IsSessionToken = true };
            var formToken = new AntiForgeryToken();

            var config = new MockAntiForgeryConfig()
            {
                FormFieldName = "form-field-name"
            };

            var mockSerializer = new Mock<MockableAntiForgeryTokenSerializer>(MockBehavior.Strict);
            mockSerializer.Setup(o => o.Serialize(formToken))
                          .Returns("serialized-form-token");

            var mockTokenStore = new Mock<MockableTokenStore>(MockBehavior.Strict);
            mockTokenStore.Setup(o => o.GetCookieToken(mockHttpContext.Object))
                          .Returns(cookieToken);

            var mockValidator = new Mock<MockableTokenProvider>(MockBehavior.Strict);
            mockValidator.Setup(o => o.GenerateFormToken(mockHttpContext.Object, identity, cookieToken))
                         .Returns(formToken);
            mockValidator.Setup(o => o.IsCookieTokenValid(cookieToken))
                         .Returns(true);

            var worker = new AntiForgeryWorker(
                config: config,
                serializer: mockSerializer.Object,
                tokenStore: mockTokenStore.Object,
                generator: mockValidator.Object,
                validator: mockValidator.Object);

            // Act
            var retVal = worker.GetFormInputElement(mockHttpContext.Object);

            // Assert
            Assert.Equal(@"<input name=""form-field-name"" type=""hidden"" value=""serialized-form-token"" />",
                retVal.ToString(TagRenderMode.SelfClosing));
        }

        [Theory]
        [InlineData(false, "SAMEORIGIN")]
        [InlineData(true, null)]
        public void GetFormInputElement_AddsXFrameOptionsHeader(bool suppressXFrameOptions, string expectedHeaderValue)
        {
            // Arrange
            var identity = new GenericIdentity("some-user");
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(o => o.User)
                           .Returns(new GenericPrincipal(identity, new string[0]));

            var headers = new HeaderDictionary(new Dictionary<string, string[]>());
            var mockResponse = new Mock<HttpResponse>();
            mockResponse.Setup(r => r.Headers)
                        .Returns(headers);
            mockHttpContext.Setup(o => o.Response)
                           .Returns(mockResponse.Object);

            var oldCookieToken = new AntiForgeryToken() { IsSessionToken = true };
            var newCookieToken = new AntiForgeryToken() { IsSessionToken = true };
            var formToken = new AntiForgeryToken();

            var config = new MockAntiForgeryConfig()
            {
                FormFieldName = "form-field-name",
                SuppressXFrameOptionsHeader = suppressXFrameOptions
            };

            var mockSerializer = new Mock<MockableAntiForgeryTokenSerializer>(MockBehavior.Strict);
            mockSerializer.Setup(o => o.Serialize(formToken))
                          .Returns("serialized-form-token");

            var mockTokenStore = new Mock<MockableTokenStore>(MockBehavior.Strict);
            mockTokenStore.Setup(o => o.GetCookieToken(mockHttpContext.Object))
                          .Returns(oldCookieToken);
            mockTokenStore.Setup(o => o.SaveCookieToken(mockHttpContext.Object, newCookieToken))
                          .Verifiable();

            var mockValidator = new Mock<MockableTokenProvider>(MockBehavior.Strict);
            mockValidator.Setup(o => o.GenerateFormToken(mockHttpContext.Object, identity, newCookieToken))
                         .Returns(formToken);
            mockValidator.Setup(o => o.IsCookieTokenValid(oldCookieToken))
                         .Returns(false);
            mockValidator.Setup(o => o.IsCookieTokenValid(newCookieToken))
                         .Returns(true);
            mockValidator.Setup(o => o.GenerateCookieToken())
                         .Returns(newCookieToken);

            var worker = new AntiForgeryWorker(
                config: config,
                serializer: mockSerializer.Object,
                tokenStore: mockTokenStore.Object,
                generator: mockValidator.Object,
                validator: mockValidator.Object);
            var context = mockHttpContext.Object;

            // Act
            var retVal = worker.GetFormInputElement(context);

            // Assert
            string xFrameOptions = context.Response.Headers["X-Frame-Options"];
            Assert.Equal(expectedHeaderValue, xFrameOptions);
        }

        [Fact]
        public void GetTokens_ExistingInvalidCookieToken()
        {
            // Arrange
            var identity = new GenericIdentity("some-user");
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(o => o.User)
                           .Returns(new GenericPrincipal(identity, new string[0]));

            var oldCookieToken = new AntiForgeryToken() { IsSessionToken = true };
            var newCookieToken = new AntiForgeryToken() { IsSessionToken = true };
            var formToken = new AntiForgeryToken();

            var mockSerializer = new Mock<MockableAntiForgeryTokenSerializer>(MockBehavior.Strict);
            mockSerializer.Setup(o => o.Deserialize("serialized-old-cookie-token"))
                          .Returns(oldCookieToken);
            mockSerializer.Setup(o => o.Serialize(newCookieToken))
                          .Returns("serialized-new-cookie-token");
            mockSerializer.Setup(o => o.Serialize(formToken))
                          .Returns("serialized-form-token");

            var mockValidator = new Mock<MockableTokenProvider>(MockBehavior.Strict);
            mockValidator.Setup(o => o.GenerateFormToken(mockHttpContext.Object, identity, newCookieToken))
                         .Returns(formToken);
            mockValidator.Setup(o => o.IsCookieTokenValid(oldCookieToken))
                         .Returns(false);
            mockValidator.Setup(o => o.IsCookieTokenValid(newCookieToken))
                         .Returns(true);
            mockValidator.Setup(o => o.GenerateCookieToken())
                         .Returns(newCookieToken);

            var worker = new AntiForgeryWorker(
                config: new MockAntiForgeryConfig(),
                serializer: mockSerializer.Object,
                tokenStore: null,
                generator: mockValidator.Object,
                validator: mockValidator.Object);

            // Act
            var tokenset = worker.GetTokens(mockHttpContext.Object, "serialized-old-cookie-token");

            // Assert
            Assert.Equal("serialized-new-cookie-token", tokenset.CookieToken);
            Assert.Equal("serialized-form-token", tokenset.FormToken);
        }

        [Fact]
        public void GetTokens_ExistingInvalidCookieToken_SwallowsExceptions()
        {
            // Arrange
            var identity = new GenericIdentity("some-user");
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(o => o.User)
                           .Returns(new GenericPrincipal(identity, new string[0]));

            var oldCookieToken = new AntiForgeryToken() { IsSessionToken = true };
            var newCookieToken = new AntiForgeryToken() { IsSessionToken = true };
            var formToken = new AntiForgeryToken();

            var mockSerializer = new Mock<MockableAntiForgeryTokenSerializer>(MockBehavior.Strict);
            mockSerializer.Setup(o => o.Deserialize("serialized-old-cookie-token"))
                          .Throws(new Exception("should be swallowed"));
            mockSerializer.Setup(o => o.Serialize(newCookieToken))
                          .Returns("serialized-new-cookie-token");
            mockSerializer.Setup(o => o.Serialize(formToken))
                          .Returns("serialized-form-token");

            var mockValidator = new Mock<MockableTokenProvider>(MockBehavior.Strict);
            mockValidator.Setup(o => o.GenerateFormToken(mockHttpContext.Object, identity, newCookieToken))
                         .Returns(formToken);
            mockValidator.Setup(o => o.IsCookieTokenValid(null))
                         .Returns(false);
            mockValidator.Setup(o => o.IsCookieTokenValid(newCookieToken))
                         .Returns(true);
            mockValidator.Setup(o => o.GenerateCookieToken())
                         .Returns(newCookieToken);

            var worker = new AntiForgeryWorker(
                config: new MockAntiForgeryConfig(),
                serializer: mockSerializer.Object,
                tokenStore: null,
                generator: mockValidator.Object,
                validator: mockValidator.Object);

            // Act
            var tokenset = worker.GetTokens(mockHttpContext.Object, "serialized-old-cookie-token");

            // Assert
            Assert.Equal("serialized-new-cookie-token", tokenset.CookieToken);
            Assert.Equal("serialized-form-token", tokenset.FormToken);
        }

        [Fact]
        public void GetTokens_ExistingValidCookieToken()
        {
            // Arrange
            var identity = new GenericIdentity("some-user");
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(o => o.User)
                           .Returns(new GenericPrincipal(identity, new string[0]));

            var cookieToken = new AntiForgeryToken() { IsSessionToken = true };
            var formToken = new AntiForgeryToken();

            var mockSerializer = new Mock<MockableAntiForgeryTokenSerializer>(MockBehavior.Strict);
            mockSerializer.Setup(o => o.Deserialize("serialized-old-cookie-token"))
                          .Returns(cookieToken);
            mockSerializer.Setup(o => o.Serialize(formToken))
                          .Returns("serialized-form-token");

            var mockValidator = new Mock<MockableTokenProvider>(MockBehavior.Strict);
            mockValidator.Setup(o => o.GenerateFormToken(mockHttpContext.Object, identity, cookieToken))
                         .Returns(formToken);
            mockValidator.Setup(o => o.IsCookieTokenValid(cookieToken))
                         .Returns(true);

            var worker = new AntiForgeryWorker(
                config: new MockAntiForgeryConfig(),
                serializer: mockSerializer.Object,
                tokenStore: null,
                generator: mockValidator.Object,
                validator: mockValidator.Object);

            // Act
            var tokenset = worker.GetTokens(mockHttpContext.Object, "serialized-old-cookie-token");

            // Assert
            Assert.Null(tokenset.CookieToken);
            Assert.Equal("serialized-form-token", tokenset.FormToken);
        }

        [Fact]
        public void Validate_FromStrings_Failure()
        {
            // Arrange
            var identity = new GenericIdentity("some-user");
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(o => o.User)
                           .Returns(new GenericPrincipal(identity, new string[0]));

            var cookieToken = new AntiForgeryToken();
            var formToken = new AntiForgeryToken();

            var mockSerializer = new Mock<MockableAntiForgeryTokenSerializer>();
            mockSerializer.Setup(o => o.Deserialize("cookie-token"))
                          .Returns(cookieToken);
            mockSerializer.Setup(o => o.Deserialize("form-token"))
                          .Returns(formToken);

            var mockValidator = new Mock<MockableTokenProvider>();
            mockValidator.Setup(o => o.ValidateTokens(mockHttpContext.Object, identity, cookieToken, formToken))
                         .Throws(new InvalidOperationException("my-message"));

            var worker = new AntiForgeryWorker(
                config: new MockAntiForgeryConfig(),
                serializer: mockSerializer.Object,
                tokenStore: null,
                generator: mockValidator.Object,
                validator: mockValidator.Object);

            // Act & assert
            var ex =
                Assert.Throws<InvalidOperationException>(
                    () => worker.Validate(mockHttpContext.Object, "cookie-token", "form-token"));
            Assert.Equal("my-message", ex.Message);
        }

        [Fact]
        public void Validate_FromStrings_Success()
        {
            // Arrange
            var identity = new GenericIdentity("some-user");
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(o => o.User)
                           .Returns(new GenericPrincipal(identity, new string[0]));

            var cookieToken = new AntiForgeryToken();
            var formToken = new AntiForgeryToken();

            var mockSerializer = new Mock<MockableAntiForgeryTokenSerializer>();
            mockSerializer.Setup(o => o.Deserialize("cookie-token"))
                          .Returns(cookieToken);
            mockSerializer.Setup(o => o.Deserialize("form-token"))
                          .Returns(formToken);

            var mockValidator = new Mock<MockableTokenProvider>();
            mockValidator.Setup(o => o.ValidateTokens(mockHttpContext.Object, identity, cookieToken, formToken))
                         .Verifiable();

            var worker = new AntiForgeryWorker(
                config: new MockAntiForgeryConfig(),
                serializer: mockSerializer.Object,
                tokenStore: null,
                generator: mockValidator.Object,
                validator: mockValidator.Object);

            // Act
            worker.Validate(mockHttpContext.Object, "cookie-token", "form-token");

            // Assert
            mockValidator.Verify();
        }

        [Fact]
        public async Task Validate_FromStore_Failure()
        {
            // Arrange
            var identity = new GenericIdentity("some-user");
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(o => o.User)
                           .Returns(new GenericPrincipal(identity, new string[0]));

            var cookieToken = new AntiForgeryToken();
            var formToken = new AntiForgeryToken();

            var mockTokenStore = new Mock<MockableTokenStore>();
            mockTokenStore.Setup(o => o.GetCookieToken(mockHttpContext.Object))
                          .Returns(cookieToken);
            mockTokenStore.Setup(o => o.GetFormToken(mockHttpContext.Object))
                          .Returns(formToken);

            var mockValidator = new Mock<MockableTokenProvider>();
            mockValidator.Setup(o => o.ValidateTokens(mockHttpContext.Object, identity, cookieToken, formToken))
                         .Throws(new InvalidOperationException("my-message"));

            var worker = new AntiForgeryWorker(
                config: new MockAntiForgeryConfig(),
                serializer: null,
                tokenStore: mockTokenStore.Object,
                generator: mockValidator.Object,
                validator: mockValidator.Object);

            // Act & assert
            var ex =
                await
                    Assert.ThrowsAsync<InvalidOperationException>(
                        async () => await worker.ValidateAsync(mockHttpContext.Object));
            Assert.Equal("my-message", ex.Message);
        }

        [Fact]
        public async Task Validate_FromStore_Success()
        {
            // Arrange
            var identity = new GenericIdentity("some-user");
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(o => o.User)
                           .Returns(new GenericPrincipal(identity, new string[0]));

            var cookieToken = new AntiForgeryToken();
            var formToken = new AntiForgeryToken();

            var mockTokenStore = new Mock<MockableTokenStore>();
            mockTokenStore.Setup(o => o.GetCookieToken(mockHttpContext.Object))
                          .Returns(cookieToken);
            mockTokenStore.Setup(o => o.GetFormToken(mockHttpContext.Object))
                          .Returns(formToken);

            var mockValidator = new Mock<MockableTokenProvider>();
            mockValidator.Setup(o => o.ValidateTokens(mockHttpContext.Object, identity, cookieToken, formToken))
                         .Verifiable();

            var worker = new AntiForgeryWorker(
                config: new MockAntiForgeryConfig(),
                serializer: null,
                tokenStore: mockTokenStore.Object,
                generator: mockValidator.Object,
                validator: mockValidator.Object);

            // Act
            await worker.ValidateAsync(mockHttpContext.Object);

            // Assert
            mockValidator.Verify();
        }
    }
}