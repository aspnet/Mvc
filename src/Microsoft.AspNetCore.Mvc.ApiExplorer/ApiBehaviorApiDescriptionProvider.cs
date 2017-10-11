// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    public class ApiBehaviorApiDescriptionProvider : IApiDescriptionProvider
    {
        private readonly IOptions<ApiBehaviorOptions> _apiOptions;
        private readonly IOptions<MvcOptions> _mvcOptions;

        private readonly IModelMetadataProvider _modelMetadaProvider;
        private readonly MediaTypeRegistry _registry;

        public ApiBehaviorApiDescriptionProvider(
            IOptions<MvcOptions> mvcOptions,
            IOptions<ApiBehaviorOptions> apiOptions,
            IModelMetadataProvider modelMetadataProvider,
            MediaTypeRegistry registry)
        {
            _mvcOptions = mvcOptions;
            _apiOptions = apiOptions;
            _modelMetadaProvider = modelMetadataProvider;
            _registry = registry;
        }

        /// <remarks>
        /// The order is set to execute after the default provider.
        /// </remarks>
        public int Order => -1000 + 10;

        public void OnProvidersExecuted(ApiDescriptionProviderContext context)
        {
        }

        public void OnProvidersExecuting(ApiDescriptionProviderContext context)
        {
            foreach (var description in context.Results)
            {
                if (!AppliesTo(description))
                {
                    continue;
                }

                if (!HasUserDefinedResponseTypes(description))
                {
                    ApplyProfile(description);
                }
            }
        }

        public bool AppliesTo(ApiDescription description)
        {
            return description.ActionDescriptor.FilterDescriptors.Any(f => f.Filter is IApiBehaviorMetadata);
        }

        public bool HasUserDefinedResponseTypes(ApiDescription description)
        {
            return description.ActionDescriptor.FilterDescriptors
                .Select(f => f.Filter)
                .OfType<IApiResponseMetadataProvider>()
                .Any(f => f.Type != null);
        }

        public void ApplyProfile(ApiDescription description)
        {
            ApiDescriptionProfile profile = null;

            var profiles = _apiOptions.Value.ApiDescriptionProfiles;
            for (var i = 0; i < profiles.Count; i++)
            {
                if (profiles[i].IsMatch(description))
                {
                    profile = profiles[i];
                    break;
                }
            }

            if (profile == null)
            {
                return;
            }

            // This is just for debugging/testing purposes.
            description.SetProperty(profile);

            profile.ApplyTo(description);

            // Now we need to fixup some of the ApiResponseTypes - we don't expect a profile to be able to
            // apply the content negotiation info, or to have access to ModelMetadata.
            for (var i = 0; i < description.SupportedResponseTypes.Count; i ++)
            {
                var response = description.SupportedResponseTypes[i];
                if (response.Type == null || response.Type == typeof(void))
                {
                    continue;
                }

                if (response.ModelMetadata == null)
                {
                    response.ModelMetadata = _modelMetadaProvider.GetMetadataForType(response.Type);
                }

                if (response.ApiResponseFormats.Count == 0)
                {
                    var formats = GetFormats(description, response);
                    foreach (var format in formats)
                    {
                        response.ApiResponseFormats.Add(format);
                    }
                }
            }
        }

        public IEnumerable<ApiResponseFormat> GetFormats(ApiDescription description, ApiResponseType response)
        {
            var filters = GetMediaTypeFilters(description);
            var formatters = GetFormatters();

            var mediaTypes = new MediaTypeCollection();

            var unfilteredMediaTypes = _registry.GetMediaTypes(response.Type);
            for (var i = 0; i < unfilteredMediaTypes.Count; i++)
            {
                mediaTypes.Add(unfilteredMediaTypes[i]);
            }

            for (var i = 0; i < filters.Length; i++)
            {
                var filter = filters[i];
                if (filter.Type != null && filter.Type != response.Type)
                {
                    continue;
                }

                // StatusCode should be ignored when Type is not set.
                if (filter.Type != null && filter.StatusCode != response.StatusCode)
                {
                    continue;
                }

                filter.SetContentTypes(mediaTypes);
            }

            for (var i = 0; i < formatters.Length; i++)
            {
                var formatter = formatters[i];
                for (var j = 0; j < mediaTypes.Count; j++)
                {
                    var mediaType = mediaTypes[j];

                    var supportedMediaTypes = formatter.GetSupportedContentTypes(mediaType, response.Type);
                    if (supportedMediaTypes != null)
                    {
                        for (var k = 0; k < supportedMediaTypes.Count; k++)
                        {
                            yield return new ApiResponseFormat()
                            {
                                Formatter = (IOutputFormatter)formatter,
                                MediaType = supportedMediaTypes[k],
                            };
                        }
                    }
                }
            }
        }

        // Returns [Produces] and friends.
        private IApiResponseMetadataProvider[] GetMediaTypeFilters(ApiDescription description)
        {
            return
                description.ActionDescriptor.FilterDescriptors
                .Select(f => f.Filter)
                .OfType<IApiResponseMetadataProvider>()
                .ToArray();
        }

        // Returns IOutputFormatters that can describe what content types they handle.
        private IApiResponseTypeMetadataProvider[] GetFormatters()
        {
            return
                _mvcOptions.Value.OutputFormatters
                .OfType<IApiResponseTypeMetadataProvider>()
                .ToArray();
        }
    }
}
