using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.HeaderValueAbstractions;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;
using Microsoft.AspNet.Mvc.Internal;

namespace Microsoft.AspNet.Mvc.ResourceModel
{
    public class DefaultResourceDescriptorProvider : INestedProvider<ResourceDescriptorProviderContext>
    {
        private readonly IOutputFormattersProvider _formattersProvider;

        public DefaultResourceDescriptorProvider(
            IOutputFormattersProvider formattersProvider)
        {
            _formattersProvider = formattersProvider;
        }

        public int Order { get; private set; }

        public void Invoke(ResourceDescriptorProviderContext context, Action callNext)
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
                    httpMethods = new string[] { "GET", "POST", "PUT", "DELETE" };
                }

                foreach (var httpMethod in httpMethods)
                {
                    var resource = new ResourceDescriptor()
                    {
                        ActionDescriptor = action,
                        HttpMethod = httpMethod,
                        ResourceName = action.ControllerName,
                        Path = GetPath(action),
                    };

                    foreach (var parameter in action.Parameters)
                    {
                        resource.Parameters.Add(GetParameter(parameter));
                    }

                    var metadataAttributes = GetFilters<IProducesMetadataProvider>(action);

                    var dataType = GetActionReturnType(action, metadataAttributes);
                    if (dataType != null && dataType != typeof(void))
                    {
                        resource.OutputFormats.AddRange(GetOutputFormats(action, metadataAttributes, dataType));
                    }

                    context.Results.Add(resource);
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

        private ResourceParameterDescriptor GetParameter(ParameterDescriptor parameter)
        {
            var resourceParameter = new ResourceParameterDescriptor()
            {
                IsOptional = parameter.IsOptional,
                Name = parameter.Name,
                ParameterDescriptor = parameter,
            };

            if (parameter.ParameterBindingInfo != null)
            {
                resourceParameter.Type = parameter.ParameterBindingInfo.ParameterType;
                resourceParameter.Source = ResourceParameterSource.Query;
            }

            if (parameter.BodyParameterInfo != null)
            {
                resourceParameter.Type = parameter.BodyParameterInfo.ParameterType;
                resourceParameter.Source = ResourceParameterSource.Body;
            }

            return resourceParameter;
        }

        private IReadOnlyList<ResourceOutputFormat> GetOutputFormats(
            ReflectedActionDescriptor action, 
            IProducesMetadataProvider[] metadataAttributes,
            Type dataType)
        {
            var results = new List<ResourceOutputFormat>();

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

            var formatters = _formattersProvider.OutputFormatters.OfType<IResourceOutputMetadataProvider>();
            foreach (var contentType in contentTypes)
            {
                foreach (var formatter in formatters)
                {
                    var supportedTypes = formatter.GetAllPossibleContentTypes(dataType, contentType).ToArray();
                    if (supportedTypes != null)
                    {
                        foreach (var supportedType in supportedTypes)
                        {
                            results.Add(new ResourceOutputFormat()
                            {
                                DataType = dataType,
                                Formatter = (IOutputFormatter)formatter,
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