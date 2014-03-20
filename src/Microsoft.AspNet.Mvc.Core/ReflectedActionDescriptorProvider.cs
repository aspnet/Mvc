﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNet.Mvc
{
    public class ReflectedActionDescriptorProvider : IActionDescriptorProvider
    {
        public static readonly int DefaultOrder = 0;

        private readonly IControllerAssemblyProvider _controllerAssemblyProvider;
        private readonly IActionDiscoveryConventions _conventions;
        private readonly IControllerDescriptorFactory _controllerDescriptorFactory;
        private readonly IParameterDescriptorFactory _parameterDescriptorFactory;
        private readonly IEnumerable<FilterDescriptor> _globalFilters;

        public ReflectedActionDescriptorProvider(IControllerAssemblyProvider controllerAssemblyProvider,
                                                 IActionDiscoveryConventions conventions,
                                                 IControllerDescriptorFactory controllerDescriptorFactory,
                                                 IParameterDescriptorFactory parameterDescriptorFactory,
                                                 IEnumerable<IFilter> globalFilters)
        {
            _controllerAssemblyProvider = controllerAssemblyProvider;
            _conventions = conventions;
            _controllerDescriptorFactory = controllerDescriptorFactory;
            _parameterDescriptorFactory = parameterDescriptorFactory;
            var filters = globalFilters ?? Enumerable.Empty<IFilter>();

            _globalFilters = filters.Select(f => new FilterDescriptor(f, FilterScope.Global));
        }

        public int Order
        {
            get { return DefaultOrder; }
        }

        public void Invoke(ActionDescriptorProviderContext context, Action callNext)
        {
            context.Results.AddRange(GetDescriptors());
            callNext();
        }

        public IEnumerable<ActionDescriptor> GetDescriptors()
        {
            var assemblies = _controllerAssemblyProvider.CandidateAssemblies;
            var types = assemblies.SelectMany(a => a.DefinedTypes);
            var controllers = types.Where(_conventions.IsController);
            var controllerDescriptors = controllers.Select(t => _controllerDescriptorFactory.CreateControllerDescriptor(t)).ToArray();

            foreach (var cd in controllerDescriptors)
            {
                var controllerAttributes = cd.ControllerTypeInfo.GetCustomAttributes(inherit: true).ToArray();
                var globalAndControllerFilters =
                    controllerAttributes.OfType<IFilter>()
                                        .Select(filter => new FilterDescriptor(filter, FilterScope.Controller))
                                        .Concat(_globalFilters)
                                        .OrderBy(d => d, FilterDescriptorOrderComparer.Comparer)
                                        .ToArray();

                foreach (var methodInfo in cd.ControllerTypeInfo.DeclaredMethods)
                {
                    var actionInfos = _conventions.GetActions(methodInfo);

                    if (actionInfos == null)
                    {
                        continue;
                    }

                    foreach (var actionInfo in actionInfos)
                    {
                        yield return BuildDescriptor(cd, methodInfo, actionInfo, globalAndControllerFilters);
                    }
                }
            }
        }

        private ReflectedActionDescriptor BuildDescriptor(ControllerDescriptor controllerDescriptor,
                                                          MethodInfo methodInfo,
                                                          ActionInfo actionInfo,
                                                          FilterDescriptor[] globalAndControllerFilters)
        {
            var ad = new ReflectedActionDescriptor
            {
                RouteConstraints = new List<RouteDataActionConstraint>
                {
                    new RouteDataActionConstraint("controller", controllerDescriptor.Name)
                },

                Name = actionInfo.ActionName,
                ControllerDescriptor = controllerDescriptor,
                MethodInfo = methodInfo,
            };

            var httpMethods = actionInfo.HttpMethods;
            if (httpMethods != null && httpMethods.Length > 0)
            {
                ad.MethodConstraints = new List<HttpMethodConstraint>
                {
                    new HttpMethodConstraint(httpMethods)
                };
            }

            if (actionInfo.RequireActionNameMatch)
            {
                ad.RouteConstraints.Add(new RouteDataActionConstraint("action", actionInfo.ActionName));
            }
            else
            {
                ad.RouteConstraints.Add(new RouteDataActionConstraint("action", RouteKeyHandling.DenyKey));
            }

            ad.Parameters = methodInfo.GetParameters().Select(p => _parameterDescriptorFactory.GetDescriptor(p)).ToList();

            var attributes = methodInfo.GetCustomAttributes(inherit: true).ToArray();

            var filtersFromAction = attributes.OfType<IFilter>().Select(filter => new FilterDescriptor(filter, FilterScope.Action));

            ad.FilterDescriptors = filtersFromAction.Concat(globalAndControllerFilters)
                                                    .OrderBy(d => d, FilterDescriptorOrderComparer.Comparer)
                                                    .ToList();

            return ad;
        }
    }
}
