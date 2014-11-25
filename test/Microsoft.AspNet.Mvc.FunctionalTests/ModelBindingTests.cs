﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.TestHost;
using ModelBindingWebSite;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class ModelBindingTests
    {
        private readonly IServiceProvider _services = TestHelper.CreateServices("ModelBindingWebSite");
        private readonly Action<IApplicationBuilder> _app = new ModelBindingWebSite.Startup().Configure;

        [Theory]
        [InlineData("RestrictValueProvidersUsingFromRoute", "valueFromRoute")]
        [InlineData("RestrictValueProvidersUsingFromQuery", "valueFromQuery")]
        [InlineData("RestrictValueProvidersUsingFromForm", "valueFromForm")]
        public async Task CompositeModelBinder_Restricts_ValueProviders(string actionName, string expectedValue)
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Provide all three values, it should bind based on the attribute on the action method.
            var request = new HttpRequestMessage(HttpMethod.Post,
                string.Format("http://localhost/CompositeTest/{0}/valueFromRoute?param=valueFromQuery", actionName));
            var nameValueCollection = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string,string>("param", "valueFromForm"),
            };

            request.Content = new FormUrlEncodedContent(nameValueCollection);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedValue, await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task MultipleParametersMarkedWithFromBody_Throws()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
               client.GetAsync("http://localhost/MultipleParametersFromBody/MultipleParametersFromBodyThrows"));

            Assert.Equal("More than one parameter is bound to the HTTP request's content.",
                         ex.Message);
        }

        [Fact]
        public async Task ParametersWithNoValueProviderMetadataUseTheAvailableValueProviders()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var response = await
                     client.GetAsync("http://localhost/WithMetadata" +
                     "/ParametersWithNoValueProviderMetadataUseTheAvailableValueProviders" +
                     "?Name=somename&Age=12");

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var emp = JsonConvert.DeserializeObject<Employee>(
                            await response.Content.ReadAsStringAsync());
            Assert.Null(emp.Department);
            Assert.Equal("somename", emp.Name);
            Assert.Equal(12, emp.Age);
        }

        [Fact]
        public async Task ParametersAreAlwaysCreated()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var response = await
                     client.GetAsync("http://localhost/WithoutMetadata" +
                     "/GetPersonParameter" +
                     "?Name=somename&Age=12");

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var person = JsonConvert.DeserializeObject<Person>(
                            await response.Content.ReadAsStringAsync());
            Assert.NotNull(person);
            Assert.Equal("somename", person.Name);
            Assert.Equal(12, person.Age);
        }

        [Theory]
        [InlineData("http://localhost/Home/ActionWithPersonFromUrlWithPrefix/Javier/26")]
        [InlineData("http://localhost/Home/ActionWithPersonFromUrlWithoutPrefix/Javier/26")]
        public async Task CanBind_ComplexData_FromRouteData(string url)
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var response = await
                     client.GetAsync(url);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            Assert.NotNull(body);

            var person = JsonConvert.DeserializeObject<Person>(body);
            Assert.NotNull(person);
            Assert.Equal("Javier", person.Name);
            Assert.Equal(26, person.Age);
        }

        [Fact]
        public async Task ModelBindCancellationTokenParameteres()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/Home/ActionWithCancellationToken");

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("true", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task ModelBindCancellationToken_ForProperties()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync(
                "http://localhost/Home/ActionWithCancellationTokenModel?wrapper=bogusValue");

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("true", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task ModelBindingBindsBase64StringsToByteArrays()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/Home/Index?byteValues=SGVsbG9Xb3JsZA==");

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("HelloWorld", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task ModelBindingBindsEmptyStringsToByteArrays()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/Home/Index?byteValues=");

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("\0", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task ModelBinding_LimitsErrorsToMaxErrorCount()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();
            var queryString = string.Join("=&", Enumerable.Range(0, 10).Select(i => "field" + i));

            // Act
            var response = await client.GetStringAsync("http://localhost/Home/ModelWithTooManyValidationErrors?" + queryString);

            //Assert
            var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
            // 8 is the value of MaxModelValidationErrors for the application being tested.
            Assert.Equal(8, json.Count);
            Assert.Equal("The Field1 field is required.", json["Field1.Field1"]);
            Assert.Equal("The Field2 field is required.", json["Field1.Field2"]);
            Assert.Equal("The Field3 field is required.", json["Field1.Field3"]);
            Assert.Equal("The Field1 field is required.", json["Field2.Field1"]);
            Assert.Equal("The Field2 field is required.", json["Field2.Field2"]);
            Assert.Equal("The Field3 field is required.", json["Field2.Field3"]);
            Assert.Equal("The Field1 field is required.", json["Field3.Field1"]);
            Assert.Equal("The maximum number of allowed model errors has been reached.", json[""]);
        }

        [Fact]
        public async Task ModelBinding_ValidatesAllPropertiesInModel()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetStringAsync("http://localhost/Home/ModelWithFewValidationErrors?model=");

            //Assert
            var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
            Assert.Equal(3, json.Count);
            Assert.Equal("The Field1 field is required.", json["model.Field1"]);
            Assert.Equal("The Field2 field is required.", json["model.Field2"]);
            Assert.Equal("The Field3 field is required.", json["model.Field3"]);
        }

        [Fact]
        public async Task BindAttribute_AppliesAtBothParameterAndTypeLevelTogether_BlacklistedAtEitherLevelIsNotBound()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetStringAsync("http://localhost/BindAttribute/" +
                "BindAtParamterLevelAndBindAtTypeLevelAreBothEvaluated_BlackListingAtEitherLevelDoesNotBind" +
                "?param1.IncludedExplicitlyAtTypeLevel=someValue&param2.ExcludedExplicitlyAtTypeLevel=someValue");

            // Assert
            var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
            Assert.Equal(2, json.Count);
            Assert.Null(json["param1.IncludedExplicitlyAtTypeLevel"]);
            Assert.Null(json["param2.ExcludedExplicitlyAtTypeLevel"]);
        }

        [Fact]
        public async Task BindAttribute_AppliesAtBothParameterAndTypeLevelTogether_WhitelistedAtBothLevelsIsBound()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetStringAsync("http://localhost/BindAttribute/" +
                "BindAtParamterLevelAndBindAtTypeLevelAreBothEvaluated_WhiteListingAtBothLevelBinds" +
                "?param1.IncludedExplicitlyAtTypeLevel=someValue&param2.ExcludedExplicitlyAtTypeLevel=someValue");

            // Assert
            var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
            Assert.Equal(1, json.Count);
            Assert.Equal("someValue", json["param1.IncludedExplicitlyAtTypeLevel"]);
        }

        [Fact]
        public async Task BindAttribute_AppliesAtBothParameterAndTypeLevelTogether_WhitelistingAtOneLevelIsNotBound()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetStringAsync("http://localhost/BindAttribute/" +
                "BindAtParamterLevelAndBindAtTypeLevelAreBothEvaluated_WhiteListingAtOnlyOneLevelDoesNotBind" +
                "?param1.IncludedExplicitlyAtTypeLevel=someValue&param1.IncludedExplicitlyAtParameterLevel=someValue");

            // Assert
            var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
            Assert.Equal(2, json.Count);
            Assert.Null(json["param1.IncludedExplicitlyAtParameterLevel"]);
            Assert.Null(json["param1.IncludedExplicitlyAtTypeLevel"]);
        }

        [Fact]
        public async Task BindAttribute_BindsUsingParameterPrefix()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetStringAsync("http://localhost/BindAttribute/" +
                "BindParameterUsingParameterPrefix" +
                "?randomPrefix.Value=someValue");

            // Assert
            Assert.Equal("someValue", response);
        }

        [Fact]
        public async Task BindAttribute_DoesNotUseTypePrefix()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetStringAsync("http://localhost/BindAttribute/" +
                "TypePrefixIsNeverUsed" +
                "?param.Value=someValue");

            // Assert
            Assert.Equal("someValue", response);
        }

        [Fact]
        public async Task BindAttribute_FallsBackOnEmptyPrefixIfNoParameterPrefixIsProvided()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetStringAsync("http://localhost/BindAttribute/" +
                "TypePrefixIsNeverUsed" +
                "?Value=someValue");

            // Assert
            Assert.Equal("someValue", response);
        }

        [Fact]
        public async Task BindAttribute_DoesNotFallBackOnEmptyPrefixIfParameterPrefixIsProvided()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/BindAttribute/" +
                "BindParameterUsingParameterPrefix" +
                "?Value=someValue");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task TryUpdateModel_IncludeTopLevelProperty_IncludesAllSubProperties()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetStringAsync("http://localhost/TryUpdateModel/" +
                "GetUserAsync_IncludesAllSubProperties" +
                "?id=123&Key=34&RegistrationMonth=March&Address.Street=123&Address.Country.Name=USA&" +
                "Address.State=WA&Address.Country.Cities[0].CityName=Seattle&Address.Country.Cities[0].CityCode=SEA");

            // Assert
            var user = JsonConvert.DeserializeObject<User>(response);

            // Should update everything under Address.
            Assert.Equal(123, user.Address.Street); // Included by default as sub properties are included.
            Assert.Equal("WA", user.Address.State); // Included by default as sub properties of address are included.
            Assert.Equal("USA", user.Address.Country.Name); // Included by default.
            Assert.Equal("Seattle", user.Address.Country.Cities[0].CityName); // Included by default.
            Assert.Equal("SEA", user.Address.Country.Cities[0].CityCode); // Included by default.

            // Should not update Any property at the same level as address.
            // Key is id + 20.
            Assert.Equal(143, user.Key);
            Assert.Null(user.RegisterationMonth);
        }

        [Fact]
        public async Task TryUpdateModel_ChainedPropertyExpression_Throws()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                          client.GetAsync("http://localhost/TryUpdateModel/GetUserAsync_WithChainedProperties?id=123"));

            Assert.Equal("Chained member access expression is not supported for include property expression.",
                         ex.Message);
        }

        [Fact]
        public async Task TryUpdateModel_FailsToUpdateProperties()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetStringAsync("http://localhost/TryUpdateModel/" +
                "TryUpdateModelFails" +
                "?id=123&RegisterationMonth=March&Key=123&UserName=SomeName");

            // Assert
            var result = JsonConvert.DeserializeObject<bool>(response);

            // Act
            Assert.False(result);
        }

        [Fact]
        public async Task TryUpdateModel_IncludeExpression_WorksOnlyAtTopLevel()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetStringAsync("http://localhost/TryUpdateModel/" +
                "GetPerson" +
                "?Parent.Name=fatherName&Parent.Parent.Name=grandFatherName");

            // Assert
            var person = JsonConvert.DeserializeObject<Person>(response);

            // Act
            Assert.Equal("fatherName", person.Parent.Name);

            // Includes this as there is data from value providers, the include filter
            // only works for top level objects.
            Assert.Equal("grandFatherName", person.Parent.Parent.Name);
        }

        [Fact]
        public async Task TryUpdateModel_Validates_ForTopLevelNotIncludedProperties()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetStringAsync("http://localhost/TryUpdateModel/" +
                "CreateAndUpdateUser" +
                "?RegisterationMonth=March");

            // Assert
            var result = JsonConvert.DeserializeObject<bool>(response);
            Assert.False(result);
        }

        [Fact]
        public async Task TryUpdateModelExcludeSpecific_Properties()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetStringAsync("http://localhost/TryUpdateModel/" +
                "GetUserAsync_ExcludeSpecificProperties" +
                "?id=123&RegisterationMonth=March&Key=123&UserName=SomeName");

            // Assert
            var user = JsonConvert.DeserializeObject<User>(response);

            // Should not update excluded properties.
            Assert.NotEqual(123, user.Key);

            // Should Update all explicitly included properties.
            Assert.Equal("March", user.RegisterationMonth);
            Assert.Equal("SomeName", user.UserName);
        }

        [Fact]
        public async Task TryUpdateModelIncludeSpecific_Properties()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetStringAsync("http://localhost/TryUpdateModel/" +
                "GetUserAsync_IncludeSpecificProperties" +
                "?id=123&RegisterationMonth=March&Key=123&UserName=SomeName");

            // Assert
            var user = JsonConvert.DeserializeObject<User>(response);

            // Should not update any not explicitly mentioned properties. 
            Assert.NotEqual("SomeName", user.UserName);
            Assert.NotEqual(123, user.Key);

            // Should Update all included properties.
            Assert.Equal("March", user.RegisterationMonth);
        }

        [Fact]
        public async Task TryUpdateModelIncludesAllProperties_ByDefault()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetStringAsync("http://localhost/TryUpdateModel/" +
                "GetUserAsync_IncludeAllByDefault" +
                "?id=123&RegisterationMonth=March&Key=123&UserName=SomeName");

            // Assert
            var user = JsonConvert.DeserializeObject<User>(response);

            // Should not update any not explicitly mentioned properties. 
            Assert.Equal("SomeName", user.UserName);
            Assert.Equal(123, user.Key);

            // Should Update all included properties.
            Assert.Equal("March", user.RegisterationMonth);
        }
    }
}