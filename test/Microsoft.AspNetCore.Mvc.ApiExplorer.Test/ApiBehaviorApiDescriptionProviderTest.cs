// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    public class ApiBehaviorApiDescriptionProviderTest
    {
        public ApiBehaviorApiDescriptionProviderTest()
        {
            var profile = new Mock<DefaultApiDescriptionProfile>() { CallBase = true, };
            profile
                .Setup(p => p.IsMatch(It.Is<ApiDescription>(d => d.HttpMethod == "GET")))
                .Returns(true);

            GetProfile = profile.Object;
            GetProfile.ResponseTypes.Add(new ApiResponseType()
            {
                Type = typeof(ProblemDetails),
                StatusCode = 404,
            });
            GetProfile.ResponseTypes.Add(new ApiResponseType()
            {
                Type = typeof(ProblemDetails),
                IsDefaultResponse = true,
            });

            profile = new Mock<DefaultApiDescriptionProfile>() { CallBase = true, };
            profile
                .Setup(p => p.IsMatch(It.Is<ApiDescription>(d => d.HttpMethod == "POST")))
                .Returns(true);
            PostProfile = profile.Object;
            PostProfile.ResponseTypes.Add(new ApiResponseType()
            {
                Type = typeof(ProblemDetails),
                StatusCode = 400,
            });
            PostProfile.ResponseTypes.Add(new ApiResponseType()
            {
                Type = typeof(ProblemDetails),
                IsDefaultResponse = true,
            });

            var mockFormatter = new Mock<TextOutputFormatter>() { CallBase = true };
            mockFormatter.Object.SupportedEncodings.Add(Encoding.UTF8);
            mockFormatter.Object.SupportedMediaTypes.Add("application/json");
            mockFormatter.Object.SupportedMediaTypes.Add("application/*+json");

            Provider = new ApiBehaviorApiDescriptionProvider(
                Options.Create(new MvcOptions()
                {
                    OutputFormatters =
                    {
                        mockFormatter.Object,
                    },
                }),
                Options.Create(new ApiBehaviorOptions()
                {
                    ApiDescriptionProfiles =
                    {
                        GetProfile,
                        PostProfile,
                    },
                }),
                new EmptyModelMetadataProvider(),
                new DefaultMediaTypeRegistry());
        }

        protected DefaultApiDescriptionProfile GetProfile { get; }

        protected DefaultApiDescriptionProfile PostProfile { get; }

        protected ApiBehaviorApiDescriptionProvider Provider { get; }

        [Fact]
        public void AppliesTo_ActionWithoutApiBehavior_ReturnsFalse()
        {
            // Arrange
            var action = new ActionDescriptor()
            {
                FilterDescriptors = new List<FilterDescriptor>(),
            };
            var description = new ApiDescription()
            {
                ActionDescriptor = action,
            };

            // Act
            var result = Provider.AppliesTo(description);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void AppliesTo_ActionWithApiBehavior_ReturnsTrue()
        {
            // Arrange
            var action = new ActionDescriptor()
            {
                FilterDescriptors = new List<FilterDescriptor>()
                {
                    new FilterDescriptor(Mock.Of<IApiBehaviorMetadata>(), FilterScope.Global),
                }
            };
            var description = new ApiDescription()
            {
                ActionDescriptor = action,
            };

            // Act
            var result = Provider.AppliesTo(description);

            // Assert
            Assert.True(result);
        }

        [Fact] // This is more like an integration test
        public void ApplyProfile_MatchesGetProfile_AddsApiResponseDescriptions()
        {
            // Arrange
            var action = new ActionDescriptor()
            {
                FilterDescriptors = new List<FilterDescriptor>()
                {
                    new FilterDescriptor(Mock.Of<IApiBehaviorMetadata>(), FilterScope.Global),
                },
                BoundProperties = new List<ParameterDescriptor>(),
                Parameters = new List<ParameterDescriptor>(),
            };
            var description = new ApiDescription()
            {
                ActionDescriptor = action,
                HttpMethod = "GET",
            };

            // Act
            Provider.ApplyProfile(description);

            // Assert
            Assert.Collection(
                description.SupportedResponseTypes.OrderBy(r => r.StatusCode),
                r =>
                {
                    Assert.Equal(typeof(ProblemDetails), r.Type);
                    Assert.Equal(0, r.StatusCode);
                    Assert.True(r.IsDefaultResponse);
                },
                r =>
                {
                    Assert.Equal(typeof(ProblemDetails), r.Type);
                    Assert.Equal(404, r.StatusCode);
                    Assert.False(r.IsDefaultResponse);
                });
        }

        [Fact] // This is more like an integration test
        public void ApplyProfile_MatchesPostProfile_AddsApiResponseDescriptions()
        {
            // Arrange
            var action = new ActionDescriptor()
            {
                FilterDescriptors = new List<FilterDescriptor>()
                {
                    new FilterDescriptor(Mock.Of<IApiBehaviorMetadata>(), FilterScope.Global),
                },
                BoundProperties = new List<ParameterDescriptor>(),
                Parameters = new List<ParameterDescriptor>(),
            };
            var description = new ApiDescription()
            {
                ActionDescriptor = action,
                HttpMethod = "POST",
            };

            // Act
            Provider.ApplyProfile(description);

            // Assert
            Assert.Collection(
                description.SupportedResponseTypes.OrderBy(r => r.StatusCode),
                r =>
                {
                    Assert.Equal(typeof(ProblemDetails), r.Type);
                    Assert.Equal(0, r.StatusCode);
                    Assert.True(r.IsDefaultResponse);
                },
                r =>
                {
                    Assert.Equal(typeof(ProblemDetails), r.Type);
                    Assert.Equal(400, r.StatusCode);
                    Assert.False(r.IsDefaultResponse);
                });
        }
    }
}
