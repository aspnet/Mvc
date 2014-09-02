// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.HeaderValueAbstractions;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc.Description
{
    public class DefaultApiDescriptionProvider : INestedProvider<ApiDescriptionProviderContext>
    {
        private readonly IOutputFormattersProvider _formattersProvider;

        public DefaultApiDescriptionProvider(IOutputFormattersProvider formattersProvider)
        {
            _formattersProvider = formattersProvider;
        }

        public int Order { get; private set; }

        public void Invoke(ApiDescriptionProviderContext context, Action callNext)
        {
            foreach (var action in context.Actions.OfType<ReflectedActionDescriptor>())
            {
                IEnumerable<string> httpMethods;
                if (action.MethodConstraints != null && action.MethodConstraints.Count > 0)
                {
                    httpMethods = action.MethodConstraints.SelectMany(c => c.HttpMethods);
                }
                else
                {
                    httpMethods = new string[] { null };
                }

                foreach (var httpMethod in httpMethods)
                {
                    var apiDescription = new ApiDescription()
                    {
                        ActionDescriptor = action,
                        HttpMethod = httpMethod,
                        RelativePath = GetPath(action),
                    };

                    foreach (var parameter in action.Parameters)
                    {
                        apiDescription.ParameterDescriptions.Add(GetParameter(parameter));
                    }

                    var metadataAttributes = GetFilters<IProducesMetadataProvider>(action);

                    // We only provide response info if the type is a user-data type. Void/Task object/IActionResult
                    // will result in no data.
                    var returnType = GetActionReturnType(action, metadataAttributes);
                    if (returnType != null && returnType != typeof(void))
                    {
                        apiDescription.ResponseType = returnType;
                        apiDescription.SupportedResponseFormats.AddRange(
                            GetResponseFormats(action, metadataAttributes, returnType));
                    }

                    context.Results.Add(apiDescription);
                }
            }

            callNext();
        }

        private string GetPath(ReflectedActionDescriptor action)
        {
            if (action.AttributeRouteInfo != null &&
                action.AttributeRouteInfo.Template != null)
            {
                return action.AttributeRouteInfo.Template;
            }

            return null;
        }

        private ApiParameterDescriptor GetParameter(ParameterDescriptor parameter)
        {
            var resourceParameter = new ApiParameterDescriptor()
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

            return resourceParameter;
        }

        private IReadOnlyList<ApiResponseFormat> GetResponseFormats(
            ReflectedActionDescriptor action, 
            IProducesMetadataProvider[] metadataAttributes,
            Type dataType)
        {
            var results = new List<ApiResponseFormat>();

            var contentTypes = new List<MediaTypeHeaderValue>();
            foreach (var metadataAttribute in metadataAttributes)
            {
                if (metadataAttribute.ContentTypes != null && metadataAttribute.ContentTypes.Count > 0)
                {
                    contentTypes.AddRange(metadataAttribute.ContentTypes);
                    break;
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
                    var supportedTypes = formatter.GetAllPossibleContentTypes(dataType, null, contentType);
                    if (supportedTypes != null)
                    {
                        foreach (var supportedType in supportedTypes)
                        {
                            results.Add(new ApiResponseFormat()
                            {
                                DataType = dataType,
                                Formatter = formatter,
                                MediaType = supportedType,
                            });
                        }
                    }
                }
            }
            

            return results;
        }

        private Type GetActionReturnType(ReflectedActionDescriptor action, IProducesMetadataProvider[] metadataAttributes)
        {
            foreach (var metadataAttribute in metadataAttributes)
            {
                if (metadataAttribute.Type != null)
                {
                    return metadataAttribute.Type;
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

        private TFilter[] GetFilters<TFilter>(ReflectedActionDescriptor action)
        {
            return action.FilterDescriptors
                .Select(fd => fd.Filter)
                .OfType<TFilter>()
                .ToArray();
        }
    }
}