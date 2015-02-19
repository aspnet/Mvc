// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.TestHost;
using XmlFormattersWebSite;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class XmlDataContractSerializerInputFormatterTest
    {
        private readonly IServiceProvider _services = TestHelper.CreateServices(nameof(XmlFormattersWebSite));
        private readonly Action<IApplicationBuilder> _app = new Startup().Configure;

        // Verifies that even though all the required data is posted to an action, the model
        // state has errors related to value types's Required attribute validation.
        [Fact]
        public async Task RequiredDataIsProvided_AndModelIsBound_AndHasRequiredAttributeValidationErrors()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml-dcs"));
            var input = "<Store xmlns=\"http://schemas.datacontract.org/2004/07/XmlFormattersWebSite\" " +
                        "xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\"><Address><State>WA</State><Zipcode>" +
                        "98052</Zipcode></Address><Customers><Customer><Address><State>WI</State><Zipcode>70383" +
                        "</Zipcode></Address><Id>1000</Id></Customer></Customers><Id>10</Id></Store>";
            var content = new StringContent(input, Encoding.UTF8, "application/xml-dcs");
            var propertiesCollection = new List<KeyValuePair<string, string>>();
            propertiesCollection.Add(new KeyValuePair<string, string>(nameof(Store.Id), typeof(Store).FullName));
            propertiesCollection.Add(new KeyValuePair<string, string>(nameof(Address.Zipcode), typeof(Address).FullName));
            propertiesCollection.Add(new KeyValuePair<string, string>(nameof(Customer.Id), typeof(Customer).FullName));
            var expectedErrorMessages = propertiesCollection.Select(kvp =>
            {
                return string.Format(
                    "{1}:Value type property '{0}' on type '{1}' has RequiredAttribute but " +
                    "no DataMember(IsRequired = true) attribute.", 
                    kvp.Key, kvp.Value);
            });

            // Act
            var response = await client.PostAsync("http://localhost/Validation/CreateStore", content);

            //Assert
            var dcsSerializer = new DataContractSerializer(typeof(ModelBindingInfo));
            var responseStream = await response.Content.ReadAsStreamAsync();
            var modelBindingInfo = dcsSerializer.ReadObject(responseStream) as ModelBindingInfo;
            Assert.NotNull(modelBindingInfo);
            Assert.NotNull(modelBindingInfo.Store);
            Assert.Equal(10, modelBindingInfo.Store.Id);
            Assert.NotNull(modelBindingInfo.Store.Address);
            Assert.Equal(98052, modelBindingInfo.Store.Address.Zipcode);
            Assert.Equal("WA", modelBindingInfo.Store.Address.State);
            Assert.NotNull(modelBindingInfo.Store.Customers);
            Assert.Equal(1, modelBindingInfo.Store.Customers.Count);
            Assert.Equal(1000, modelBindingInfo.Store.Customers[0].Id);
            Assert.Equal("WI", modelBindingInfo.Store.Customers[0].Address.State);
            Assert.Equal(70383, modelBindingInfo.Store.Customers[0].Address.Zipcode);
            Assert.NotNull(modelBindingInfo.ModelStateErrorMessages);
            Assert.Equal(expectedErrorMessages.Count(), modelBindingInfo.ModelStateErrorMessages.Count);
            foreach (var expectedErrorMessage in expectedErrorMessages)
            {
                Assert.Contains(
                modelBindingInfo.ModelStateErrorMessages,
                (actualErrorMessage) => actualErrorMessage.StartsWith(expectedErrorMessage));
            }
        }

        // Verifies that the model state has errors related to body model validation(for reference types) and also for
        // Required attribute validation (for value types).
        [Fact]
        public async Task DataMissingForReferneceTypeProperties_AndModelIsBound_AndHasMixedValidationErrors()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml-dcs"));
            var input = "<Store xmlns=\"http://schemas.datacontract.org/2004/07/XmlFormattersWebSite\"" +
                        " xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\"><Address><State i:nil=\"true\"/>" +
                        "<Zipcode>98052</Zipcode></Address><Customers><Customer><Address i:nil=\"true\"/><Id>1000" +
                        "</Id></Customer></Customers><Id>10</Id></Store>";
            var content = new StringContent(input, Encoding.UTF8, "application/xml-dcs");
            var propertiesCollection = new List<KeyValuePair<string, string>>();
            propertiesCollection.Add(new KeyValuePair<string, string>(nameof(Store.Id), typeof(Store).FullName));
            propertiesCollection.Add(new KeyValuePair<string, string>(nameof(Address.Zipcode), typeof(Address).FullName));
            propertiesCollection.Add(new KeyValuePair<string, string>(nameof(Customer.Id), typeof(Customer).FullName));
            var expectedErrorMessages = propertiesCollection.Select(kvp =>
            {
                return string.Format(
                    "{1}:Value type property '{0}' on type '{1}' has RequiredAttribute but " +
                    "no DataMember(IsRequired = true) attribute.",
                    kvp.Key, kvp.Value);
            }).ToList();
            expectedErrorMessages.Add("store.Address.State:The State field is required.");
            expectedErrorMessages.Add("store.Customers[0].Address:The Address field is required.");

            // Act
            var response = await client.PostAsync("http://localhost/Validation/CreateStore", content);

            //Assert
            var dcsSerializer = new DataContractSerializer(typeof(ModelBindingInfo));
            var responseStream = await response.Content.ReadAsStreamAsync();
            var modelBindingInfo = dcsSerializer.ReadObject(responseStream) as ModelBindingInfo;
            Assert.NotNull(modelBindingInfo);
            Assert.NotNull(modelBindingInfo.Store);
            Assert.Equal(10, modelBindingInfo.Store.Id);
            Assert.NotNull(modelBindingInfo.Store.Address);
            Assert.Equal(98052, modelBindingInfo.Store.Address.Zipcode);
            Assert.Null(modelBindingInfo.Store.Address.State);
            Assert.NotNull(modelBindingInfo.Store.Customers);
            Assert.Equal(1, modelBindingInfo.Store.Customers.Count);
            Assert.Equal(1000, modelBindingInfo.Store.Customers[0].Id);
            Assert.Null(modelBindingInfo.Store.Customers[0].Address);
            Assert.NotNull(modelBindingInfo.ModelStateErrorMessages);
            Assert.Equal(expectedErrorMessages.Count(), modelBindingInfo.ModelStateErrorMessages.Count);
            foreach (var expectedErrorMessage in expectedErrorMessages)
            {
                Assert.Contains(
                modelBindingInfo.ModelStateErrorMessages,
                (actualErrorMessage) => actualErrorMessage.StartsWith(expectedErrorMessage));
            }
        }
    }
}