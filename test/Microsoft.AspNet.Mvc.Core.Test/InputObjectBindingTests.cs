// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.OptionDescriptors;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.OptionsModel;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Core.Test
{
    public class InputObjectBindingTests
    {
        [Fact]
        public async Task GetArguments_UsingInputFormatter_DeserializesWithoutErrors_WhenValidationAttributesAreAbsent()
        {
            // Arrange
            var sampleName = "SampleName";
            var input = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                            "<Person><Name>" + sampleName + "</Name></Person>";
            var modelStateDictionary = new ModelStateDictionary();
            var invoker = GetReflectedActionInvoker(
                input, typeof(Person), new XmlSerializerInputFormatter(), "application/xml");

            // Act
            var result = await invoker.GetActionArguments(modelStateDictionary);

            // Assert
            Assert.True(modelStateDictionary.IsValid);
            Assert.Equal(0, modelStateDictionary.ErrorCount);
            var model = result["foo"] as Person;
            Assert.Equal(sampleName, model.Name);
        }

        [Fact]
        public async Task GetArguments_UsingInputFormatter_DeserializesWithOneValidationError()
        {
            // Arrange
            var sampleName = "SampleName";
            var sampleUserName = "No5";
            var input = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                            "<User><Name>" + sampleName + "</Name><UserName>" + sampleUserName + "</UserName></User>";
            var modelStateDictionary = new ModelStateDictionary();
            var invoker = GetReflectedActionInvoker(input, typeof(User), new XmlSerializerInputFormatter(), "application/xml");

            // Act
            var result = await invoker.GetActionArguments(modelStateDictionary);

            // Assert
            Assert.False(modelStateDictionary.IsValid);
            Assert.Equal(1, modelStateDictionary.ErrorCount);
            Assert.Equal(
                "The field UserName must be a string or array type with a minimum length of '5'.",
                Assert.Single(Assert.Single(modelStateDictionary.Values).Errors).ErrorMessage);
            var model = result["foo"] as User;
            Assert.Equal(sampleName, model.Name);
            Assert.Equal(sampleUserName, model.UserName);
        }

        [Fact]
        public async Task GetArguments_UsingInputFormatter_DeserializesWithMultipleValidationError()
        {
            // Arrange
            var sampleName = "SampleName";
            var sampleUserName = "No5";
            var sampleSuperUserId = 1;
            var input = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                            "<SuperUser><Name>" + sampleName + "</Name><UserName>" + sampleUserName + "</UserName>" +
                            "<SuperUserId>" + sampleSuperUserId + "</SuperUserId></SuperUser>";
            var modelStateDictionary = new ModelStateDictionary();
            var invoker = GetReflectedActionInvoker(input, typeof(SuperUser), new XmlSerializerInputFormatter(), "application/xml");

            // Act
            var result = await invoker.GetActionArguments(modelStateDictionary);

            // Assert
            Assert.False(modelStateDictionary.IsValid);
            Assert.Equal(2, modelStateDictionary.ErrorCount);
            Assert.Equal(
                "The field UserName must be a string or array type with a minimum length of '5'.",
                modelStateDictionary["foo.UserName"].Errors[0].ErrorMessage);
            Assert.Equal(
                "The field SuperUserId must be between 10 and 1000.",
                modelStateDictionary["foo.SuperUserId"].Errors[0].ErrorMessage);
            var model = result["foo"] as SuperUser;
            Assert.Equal(sampleName, model.Name);
            Assert.Equal(sampleUserName, model.UserName);
            Assert.Equal(sampleSuperUserId, model.SuperUserId);
        }

        [Fact]
        public async Task GetArguments_UsingInputFormatter_DeserializesWhenThereAreNoErrors_WhenValidationAttributesArePresent()
        {
            // Arrange
            var sampleName = "SampleName";
            var sampleUserName = "UserNo5";
            var sampleSuperUserId = 21;
            var input = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                            "<SuperUser><Name>" + sampleName + "</Name><UserName>" + sampleUserName + "</UserName>" +
                            "<SuperUserId>" + sampleSuperUserId + "</SuperUserId></SuperUser>";
            var modelStateDictionary = new ModelStateDictionary();
            var invoker = GetReflectedActionInvoker(input, typeof(SuperUser), new XmlSerializerInputFormatter(), "application/xml");

            // Act
            var result = await invoker.GetActionArguments(modelStateDictionary);

            // Assert
            Assert.True(modelStateDictionary.IsValid);
            Assert.Equal(0, modelStateDictionary.ErrorCount);
            var model = result["foo"] as SuperUser;
            Assert.Equal(sampleName, model.Name);
            Assert.Equal(sampleUserName, model.UserName);
            Assert.Equal(sampleSuperUserId, model.SuperUserId);
        }

        [Fact]
        public async Task GetArguments_UsingInputFormatter_DeserializesWhenThereAreMultipleAttributesInAProperty()
        {
            // Arrange
            var sampleId = 10;
            var sampleTeamName = "HelloWorldTeam";
            var sampleLead = "SampleLead";
            var sampleTeamSize = 15;
            var input = "{Id : " + sampleId + ", TeamName : '" + sampleTeamName +
                "', Lead : '" + sampleLead + "', TeamSize : " + sampleTeamSize +", TeamDescription : 'Test Team'}";
            var modelStateDictionary = new ModelStateDictionary();
            var invoker = GetReflectedActionInvoker(input, typeof(Team), new JsonInputFormatter(), "application/json");

            // Act
            var result = await invoker.GetActionArguments(modelStateDictionary);

            // Assert
            Assert.True(modelStateDictionary.IsValid);
            Assert.Equal(0, modelStateDictionary.ErrorCount);
            var model = result["foo"] as Team;
            Assert.Equal(sampleId, model.Id);
            Assert.Equal(sampleTeamName, model.TeamName);
            Assert.Equal(sampleLead, model.Lead);
            Assert.Equal(sampleTeamSize, model.TeamSize);
        }

        [Fact]
        public async Task GetArguments_UsingInputFormatter_DeserializesWhenThereAreErrors_WithPropertiesHavingMultipleAttributes()
        {
            // Arrange
            var sampleId = 10;
            var sampleTeamName = "HWT";
            var sampleLead = "SampleLead";
            var sampleTeamSize = 15;
            var input = "{Id : " + sampleId + ", TeamName : '" + sampleTeamName +
                "', Lead : '" + sampleLead + "', TeamSize : " + sampleTeamSize + ", TeamDescription : 'Test Team'}";
            var modelStateDictionary = new ModelStateDictionary();
            var invoker = GetReflectedActionInvoker(input, typeof(Team), new JsonInputFormatter(), "application/json");

            // Act
            var result = await invoker.GetActionArguments(modelStateDictionary);

            // Assert
            Assert.False(modelStateDictionary.IsValid);
            Assert.Equal(1, modelStateDictionary.ErrorCount);
            Assert.Equal(
                "The field TeamName must be a string with a minimum length of 4 and a maximum length of 20.",
                Assert.Single(Assert.Single(modelStateDictionary.Values).Errors).ErrorMessage);
            var model = result["foo"] as Team;
            Assert.Equal(sampleId, model.Id);
            Assert.Equal(sampleTeamName, model.TeamName);
            Assert.Equal(sampleLead, model.Lead);
            Assert.Equal(sampleTeamSize, model.TeamSize);
        }

        [Fact]
        public async Task GetArguments_UsingInputFormatter_DeserializesNestedObjects()
        {
            // Arrange
            var sampleOrgId = 1;
            var sampleOrgName = "TestOrg";
            var sampleDevTeamId = 10;
            var sampleTeamName = "HelloWorldTeam";
            var sampleLead = "SampleLead";
            var sampleTeamSize = 15;
            var input = "{Id: " + sampleOrgId + ", OrgName: '" + sampleOrgName + 
                "', Dev: {Id : " + sampleDevTeamId + ", TeamName : '" + sampleTeamName +
                "', Lead : '" + sampleLead + "', TeamSize : " + sampleTeamSize + ", TeamDescription : 'Test Team'}}";
            var modelStateDictionary = new ModelStateDictionary();
            var invoker = GetReflectedActionInvoker(input, typeof(Org), new JsonInputFormatter(), "application/json");

            // Act
            var result = await invoker.GetActionArguments(modelStateDictionary);

            // Assert
            Assert.True(modelStateDictionary.IsValid);
            Assert.Equal(0, modelStateDictionary.ErrorCount);
            var model = result["foo"] as Org;
            Assert.Equal(sampleOrgId, model.Id);
            Assert.Equal(sampleOrgName, model.OrgName);
            Assert.Equal(sampleDevTeamId, model.Dev.Id);
            Assert.Equal(sampleTeamName, model.Dev.TeamName);
            Assert.Equal(sampleLead, model.Dev.Lead);
            Assert.Equal(sampleTeamSize, model.Dev.TeamSize);
            Assert.Null(model.Test);
        }

        [Fact]
        public async Task GetArguments_UsingInputFormatter_DeserializesNestedObjects_WithErrors()
        {
            // Arrange
            var sampleOrgId = 1;
            var sampleOrgName = "Org";
            var sampleDevTeamId = 10;
            var sampleDevTeamName = "HelloWorldTeam";
            var sampleDevLead = "SampleLeadDev";
            var sampleDevTeamSize = 2;

            var sampleTestTeamId = 11;
            var sampleTestTeamName = "HWT";
            var sampleTestLead = "SampleTestLead";
            var sampleTestTeamSize = 12;

            var input = "{Id: " + sampleOrgId + ", OrgName: '" + sampleOrgName +
                "', Dev: {Id : " + sampleDevTeamId + ", TeamName : '" + sampleDevTeamName +
                "', Lead : '" + sampleDevLead + "', TeamSize : " + sampleDevTeamSize + ", TeamDescription : 'Test Team'}," + 
                " test: {Id : " + sampleTestTeamId + ", TeamName : '" + sampleTestTeamName +
                "', Lead : '" + sampleTestLead + "', TeamSize : " + sampleTestTeamSize + ", TeamDescription : 'Test Team'}}";
            var modelStateDictionary = new ModelStateDictionary();
            var invoker = GetReflectedActionInvoker(input, typeof(Org), new JsonInputFormatter(), "application/json");

            // Act
            var result = await invoker.GetActionArguments(modelStateDictionary);

            // Assert
            Assert.False(modelStateDictionary.IsValid);
            Assert.Equal(5, modelStateDictionary.ErrorCount);

            var model = result["foo"] as Org;
            Assert.Equal(sampleOrgId, model.Id);
            Assert.Equal(sampleOrgName, model.OrgName);

            Assert.Equal(sampleDevTeamId, model.Dev.Id);
            Assert.Equal(sampleDevTeamName, model.Dev.TeamName);
            Assert.Equal(sampleDevLead, model.Dev.Lead);
            Assert.Equal(sampleDevTeamSize, model.Dev.TeamSize);

            Assert.Equal(sampleTestTeamId, model.Test.Id);
            Assert.Equal(sampleTestTeamName, model.Test.TeamName);
            Assert.Equal(sampleTestLead, model.Test.Lead);
            Assert.Equal(sampleTestTeamSize, model.Test.TeamSize);

            Assert.Equal(
                "The field OrgName must be a string with a minimum length of 4 and a maximum length of 20.",
                modelStateDictionary["foo.OrgName"].Errors[0].ErrorMessage);
            Assert.Equal(
                "The field Lead must be a string or array type with a maximum length of '10'.",
                modelStateDictionary["foo.Dev.Lead"].Errors[0].ErrorMessage);
            Assert.Equal(
                "The field TeamSize must be between 3 and 100.",
                modelStateDictionary["foo.Dev.TeamSize"].Errors[0].ErrorMessage);
            Assert.Equal(
                "The field TeamName must be a string with a minimum length of 4 and a maximum length of 20.",
                modelStateDictionary["foo.Test.TeamName"].Errors[0].ErrorMessage);
            Assert.Equal(
                "The field Lead must be a string or array type with a maximum length of '10'.",
                modelStateDictionary["foo.Test.Lead"].Errors[0].ErrorMessage);
        }

        [Fact]
        public async Task GetArguments_UsingInputFormatter_DeserializesArrays()
        {
            // Arrange
            var sampleFirstUser = "FirstUser";
            var sampleFirstUserName = "fuser";
            var sampleSecondUser = "SecondUser";
            var sampleSecondUserName = "suser";
            var input = "{'Users': [{Name : '" + sampleFirstUser +"', UserName: '" + sampleFirstUserName +
                "'}, {Name: '" + sampleSecondUser + "', UserName: '" + sampleSecondUserName + "'}]}";
            var modelStateDictionary = new ModelStateDictionary();
            var invoker = GetReflectedActionInvoker(input, typeof(Customers), new JsonInputFormatter(), "application/xml");

            // Act
            var result = await invoker.GetActionArguments(modelStateDictionary);

            // Assert
            Assert.True(modelStateDictionary.IsValid);
            Assert.Equal(0, modelStateDictionary.ErrorCount);
            var model = result["foo"] as Customers;
            Assert.Equal(2, model.Users.Count);
            Assert.Equal(sampleFirstUser, model.Users[0].Name);
            Assert.Equal(sampleFirstUserName, model.Users[0].UserName);
            Assert.Equal(sampleSecondUser, model.Users[1].Name);
            Assert.Equal(sampleSecondUserName, model.Users[1].UserName);
        }

        [Fact]
        public async Task GetArguments_UsingInputFormatter_DeserializesArrays_WithErrors()
        {
            // Arrange
            var sampleFirstUser = "FirstUser";
            var sampleFirstUserName = "fusr";
            var sampleSecondUser = "SecondUser";
            var sampleSecondUserName = "susr";
            var input = "{'Users': [{Name : '" + sampleFirstUser + "', UserName: '" + sampleFirstUserName +
                "'}, {Name: '" + sampleSecondUser + "', UserName: '" + sampleSecondUserName + "'}]}";
            var modelStateDictionary = new ModelStateDictionary();
            var invoker = GetReflectedActionInvoker(input, typeof(Customers), new JsonInputFormatter(), "application/xml");

            // Act
            var result = await invoker.GetActionArguments(modelStateDictionary);

            // Assert
            Assert.False(modelStateDictionary.IsValid);
            Assert.Equal(2, modelStateDictionary.ErrorCount);
            var model = result["foo"] as Customers;
            Assert.Equal(
                "The field UserName must be a string or array type with a minimum length of '5'.",
                modelStateDictionary["foo.Users[0].UserName"].Errors[0].ErrorMessage);
            Assert.Equal(
                "The field UserName must be a string or array type with a minimum length of '5'.",
                modelStateDictionary["foo.Users[1].UserName"].Errors[0].ErrorMessage);
            Assert.Equal(2, model.Users.Count);
            Assert.Equal(sampleFirstUser, model.Users[0].Name);
            Assert.Equal(sampleFirstUserName, model.Users[0].UserName);
            Assert.Equal(sampleSecondUser, model.Users[1].Name);
            Assert.Equal(sampleSecondUserName, model.Users[1].UserName);
        }

        [Fact]
        public async Task GetArguments_UsingInputFormatter_DeserializesVariables_ButDoesNotValidate()
        {
            // Arrange
            var sampleInt = 2;
            var input = "{'test': " + sampleInt + "}";
            var modelStateDictionary = new ModelStateDictionary();
            var invoker = GetReflectedActionInvoker(input, typeof(VariableTest), new JsonInputFormatter(), "application/xml");

            // Act
            var result = await invoker.GetActionArguments(modelStateDictionary);

            // Assert
            Assert.True(modelStateDictionary.IsValid);
            Assert.Equal(0, modelStateDictionary.ErrorCount);
            var model = result["foo"] as VariableTest;
            Assert.Equal(sampleInt, model.test);
        }

        private static ReflectedActionInvoker GetReflectedActionInvoker(
            string input, Type parameterType, IInputFormatter selectedFormatter, string contentType)
        {
            var mvcOptions = new MvcOptions();
            var setup = new MvcOptionsSetup();

            setup.Setup(mvcOptions);
            var accessor = new Mock<IOptionsAccessor<MvcOptions>>();
            accessor.SetupGet(a => a.Options)
                    .Returns(mvcOptions);
            var validatorProvider = new DefaultModelValidatorProviderProvider(
                accessor.Object, Mock.Of<ITypeActivator>(), Mock.Of<IServiceProvider>());

            Func<object, int> method = x => 1;
            var actionDescriptor = new ReflectedActionDescriptor
            {
                MethodInfo = method.Method,
                Parameters = new List<ParameterDescriptor>
                            {
                                new ParameterDescriptor
                                {
                                    Name = "foo",
                                    BodyParameterInfo = new BodyParameterInfo(parameterType)
                                }
                            }
            };

            var metadataProvider = new EmptyModelMetadataProvider();
            var actionContext = GetActionContext(
                Encodings.UTF8EncodingWithoutBOM.GetBytes(input), actionDescriptor, contentType);

            var inputFormatterSelector = new Mock<IInputFormatterSelector>();
            inputFormatterSelector.Setup(a => a.SelectFormatter(It.IsAny<InputFormatterContext>()))
                .Returns(selectedFormatter);
            var bindingContext = new ActionBindingContext(actionContext,
                                                          metadataProvider,
                                                          Mock.Of<IModelBinder>(),
                                                          Mock.Of<IValueProvider>(),
                                                          inputFormatterSelector.Object,
                                                          new CompositeModelValidatorProvider(validatorProvider));

            var actionBindingContextProvider = new Mock<IActionBindingContextProvider>();
            actionBindingContextProvider.Setup(p => p.GetActionBindingContextAsync(It.IsAny<ActionContext>()))
                                        .Returns(Task.FromResult(bindingContext));

            var inputFormattersProvider = new Mock<IInputFormattersProvider>();
            inputFormattersProvider.SetupGet(o => o.InputFormatters)
                                            .Returns(new List<IInputFormatter>());
            return new ReflectedActionInvoker(actionContext,
                                                     actionBindingContextProvider.Object,
                                                     Mock.Of<INestedProviderManager<FilterProviderContext>>(),
                                                     Mock.Of<IControllerFactory>(),
                                                     actionDescriptor,
                                                     inputFormattersProvider.Object,
                                                     new DefaultBodyModelValidator());
        }

        private static ActionContext GetActionContext(byte[] contentBytes,
                                                      ActionDescriptor actionDescriptor,
                                                      string contentType)
        {
            return new ActionContext(GetHttpContext(contentBytes, contentType),
                                     new RouteData(),
                                     actionDescriptor);
        }
        private static HttpContext GetHttpContext(byte[] contentBytes,
                                                  string contentType)
        {
            var request = new Mock<HttpRequest>();
            var headers = new Mock<IHeaderDictionary>();
            request.SetupGet(r => r.Headers).Returns(headers.Object);
            request.SetupGet(f => f.Body).Returns(new MemoryStream(contentBytes));
            request.SetupGet(f => f.ContentType).Returns(contentType);

            var httpContext = new Mock<HttpContext>();
            httpContext.SetupGet(c => c.Request).Returns(request.Object);
            httpContext.SetupGet(c => c.Request).Returns(request.Object);
            return httpContext.Object;
        }
    }

    public class Person
    {
        public string Name { get; set; }
    }

    public class User : Person
    {
        [MinLength(5)]
        public string UserName { get; set; }
    }

    public class SuperUser : User
    {
        [Range(10, 1000)]
        public int SuperUserId { get; set; }
    }

    public class Team
    {
        [Required]
        public int Id { get; set; }

        [Required, StringLength(20, MinimumLength = 4)]
        public string TeamName { get; set; }

        [MaxLength(10)]
        public string Lead { get; set; }

        [Range(3, 100)]
        public int TeamSize { get; set; }

        public string TeamDescription { get; set; }
    }

    public class Org
    {
        [Required]
        public int Id { get; set; }

        [StringLength(20, MinimumLength = 4)]
        public string OrgName { get; set; }

        [Required]
        public Team Dev { get; set; }

        public Team Test { get; set; }
    }

    public class Customers
    {
        [Required]
        public List<User> Users { get; set; }
    }

    public class VariableTest
    {
        [Range(15,25)]
        public int test;
    }
}