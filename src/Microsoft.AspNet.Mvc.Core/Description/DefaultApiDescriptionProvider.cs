// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.HeaderValueAbstractions;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc.Description
{
    /// <summary>
    /// Implements a provider of <see cref="ApiDescription"/> for actions represented
    /// by <see cref="ReflectedActionDescriptor"/>.
    /// </summary>
    public class DefaultApiDescriptionProvider : INestedProvider<ApiDescriptionProviderContext>
    {
        private readonly IOutputFormattersProvider _formattersProvider;
        private readonly IModelMetadataProvider _modelMetadataProvider;

        /// <summary>
        /// Creates a new instance of <see cref="DefaultApiDescriptionProvider"/>.
        /// </summary>
        /// <param name="formattersProvider">The <see cref="IOutputFormattersProvider"/>.</param>
        /// <param name="modelMetadataProvider">The <see cref="IModelMetadataProvider"/>.</param>
        public DefaultApiDescriptionProvider(
            IOutputFormattersProvider formattersProvider,
            IModelMetadataProvider modelMetadataProvider)
        {
            _formattersProvider = formattersProvider;
            _modelMetadataProvider = modelMetadataProvider;
        }

        /// <inheritdoc />
        public int Order { get; private set; }

        /// <inheritdoc />
        public void Invoke(ApiDescriptionProviderContext context, Action callNext)
        {
            foreach (var action in context.Actions.OfType<ReflectedActionDescriptor>())
            {
                var extensionData = action.GetExtension<ApiDescriptionActionExtensionData>();
                if (extensionData != null && extensionData.IsVisible)
                {
                    var httpMethods = GetHttpMethods(action);
                    foreach (var httpMethod in httpMethods)
                    {
                        context.Results.Add(CreateApiDescription(action, httpMethod, extensionData.GroupName));
                    }
                }
            }

            callNext();
        }

        private ApiDescription CreateApiDescription(
            ReflectedActionDescriptor action, 
            string httpMethod, 
            string groupName)
        {
            var apiDescription = new ApiDescription()
            {
                ActionDescriptor = action,
                GroupName = groupName,
                HttpMethod = httpMethod,
                RelativePath = GetRelativePath(action),
            };

            if (action.Parameters != null)
            {
                foreach (var parameter in action.Parameters)
                {
                    apiDescription.ParameterDescriptions.Add(GetParameter(parameter));
                }
            }

            var responseMetadataAttributes = GetResponseMetadataAttributes(action);

            // We only provide response info if the type is a user-data type. Void/Task object/IActionResult
            // will result in no data.
            var returnType = GetActionReturnType(action, responseMetadataAttributes);
            if (returnType != null && returnType != typeof(void))
            {
                apiDescription.ResponseType = returnType;

                apiDescription.ResponseModelMetadata = _modelMetadataProvider.GetMetadataForType(null, returnType);

                var formats = GetResponseFormats(action, responseMetadataAttributes, returnType);
                foreach (var format in formats)
                {
                    apiDescription.SupportedResponseFormats.Add(format);
                }
            }

            return apiDescription;
        }

        private IEnumerable<string> GetHttpMethods(ReflectedActionDescriptor action)
        {
            if (action.MethodConstraints != null && action.MethodConstraints.Count > 0)
            {
                return action.MethodConstraints.SelectMany(c => c.HttpMethods);
            }
            else
            {
                return new string[] { null };
            }
        }

        private string GetRelativePath(ReflectedActionDescriptor action)
        {
            // This is a placeholder for functionality which will correctly generate the relative path
            // stub of an action. See: #885
            if (action.AttributeRouteInfo != null &&
                action.AttributeRouteInfo.Template != null)
            {
                return action.AttributeRouteInfo.Template;
            }

            return null;
        }

        private ApiParameterDescription GetParameter(ParameterDescriptor parameter)
        {
            // This is a placeholder based on currently available functionality for parameters. See #886.
            var resourceParameter = new ApiParameterDescription()
            {
                IsOptional = parameter.IsOptional,
                Name = parameter.Name,
                ParameterDescriptor = parameter,
            };

            if (parameter.ParameterBindingInfo != null)
            {
                resourceParameter.Type = parameter.ParameterBindingInfo.ParameterType;
                resourceParameter.Source = ApiParameterSource.Query;
            }

            if (parameter.BodyParameterInfo != null)
            {
                resourceParameter.Type = parameter.BodyParameterInfo.ParameterType;
                resourceParameter.Source = ApiParameterSource.Body;
            }

            if (resourceParameter.Type != null)
            {
                resourceParameter.ModelMetadata = _modelMetadataProvider.GetMetadataForType(null, resourceParameter.Type);
            }

            return resourceParameter;
        }

        private IReadOnlyList<ApiResponseFormat> GetResponseFormats(
            ReflectedActionDescriptor action,
            IApiResponseMetadataProvider[] responseMetadataAttributes,
            Type dataType)
        {
            var results = new List<ApiResponseFormat>();

            // Walk through all 'filter' attributes in order, and allow each one to see or override
            // the results of the previous ones. This is similar to the execution path for content-negotiation.
            var contentTypes = new List<MediaTypeHeaderValue>();
            if (responseMetadataAttributes != null)
            {
                foreach (var metadataAttribute in responseMetadataAttributes)
                {
                    metadataAttribute.SetContentTypes(contentTypes);
                }
            }

            if (contentTypes.Count == 0)
            {
                contentTypes.Add(null);
            }

            var formatters = _formattersProvider.OutputFormatters;
            foreach (var contentType in contentTypes)
            {
                foreach (var formatter in formatters)
                {
                    var supportedTypes = formatter.GetSupportedContentTypes(dataType, dataType, contentType);
                    if (supportedTypes != null)
                    {
                        foreach (var supportedType in supportedTypes)
                        {
                            results.Add(new ApiResponseFormat()
                            {
                                Formatter = formatter,
                                MediaType = supportedType,
                            });
                        }
                    }
                }
            }
            

            return results;
        }

        private Type GetActionReturnType(ReflectedActionDescriptor action, IApiResponseMetadataProvider[] metadataAttributes)
        {
            // Walk through all of the filter attributes and allow them to set the type. This will execute them
            // in filter-order allowing the desired behavior for overriding.
            if (metadataAttributes != null)
            {
                Type typeSetByAttribute = null;
                foreach (var metadataAttribute in metadataAttributes)
                {
                    if (metadataAttribute.Type != null)
                    {
                        typeSetByAttribute = metadataAttribute.Type;
                    }
                }

                if (typeSetByAttribute != null)
                {
                    return typeSetByAttribute;
                }
            }

            var declaredReturnType = action.MethodInfo.ReturnType;
            if (declaredReturnType == typeof(void) ||
                declaredReturnType == typeof(Task))
            {
                return typeof(void);
            }

            // Unwrap the type if it's a Task<T>. The Task (non-generic) case was already handled.
            var unwrappedReturnType = TypeHelper.GetTaskInnerTypeOrNull(declaredReturnType) ?? declaredReturnType;

            // If the action might return an IActionResult, then assume we don't know anything about it.
            if (typeof(IActionResult).IsAssignableFrom(unwrappedReturnType) ||
                unwrappedReturnType == typeof(object))
            {
                return null;
            }

            return unwrappedReturnType;
        }

        private IApiResponseMetadataProvider[] GetResponseMetadataAttributes(ReflectedActionDescriptor action)
        {
            if (action.FilterDescriptors == null)
            {
                return null;
            }

            // This technique for enumerating filters will intentionally ignore any filter that is an IFilterFactory
            // for a filter that implements IApiResponseMetadataProvider.
            //
            // The workaround for that is to implement the metadata interface on the IFilterFactory.
            return action.FilterDescriptors
                .Select(fd => fd.Filter)
                .OfType<IApiResponseMetadataProvider>()
                .ToArray();
        }
    }
}