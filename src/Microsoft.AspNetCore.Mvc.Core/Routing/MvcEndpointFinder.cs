using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matchers;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    internal class UrlPageContext
    {
        public string PageName { get; set; }

        public string PageHandlerName { get; set; }

        public object Values { get; set; }

        public string Protocol { get; set; }

        public string Host { get; set; }

        public string Fragment { get; set; }
    }

    internal class MvcAddress : Address
    {
        public ActionContext CurrentActionContext { get; set; }

        public ActionDescriptor TargetActionDescriptor { get; set; }

        public string TargetActionName { get; set; }

        public string TargetControllerName { get; set; }

        public string TargetPageName { get; set; }

        public string TargetHandlerName { get; set; }
    }

    internal class MvcEndpointFinder : IEndpointFinder
    {
        private readonly IEnumerable<Endpoint> Empty = Enumerable.Empty<Endpoint>();
        private readonly CompositeEndpointDataSource _endpointDatasource;
        private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;
        private readonly ILogger<MvcEndpointFinder> _logger;
        private readonly IEnumerable<MatcherEndpoint> _matcherEndpoints;

        public MvcEndpointFinder(
            CompositeEndpointDataSource endpointDataSource,
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
            ILogger<MvcEndpointFinder> logger)
        {
            _endpointDatasource = endpointDataSource;
            _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
            _logger = logger;

            _matcherEndpoints = _endpointDatasource.Endpoints.OfType<MatcherEndpoint>();
        }

        public IEnumerable<Endpoint> FindEndpoints(Address address)
        {
            // Link generation direction
            // Source => Destination
            //
            // MVC => MVC
            //  Same controller, different action
            //  Same controller, same action
            //  Different controller
            // MVC => RazorPages
            // RazorPages => MVC
            // RazorPages => RazorPages
            //  Same page, different handler
            //  Different page

            IEnumerable<Endpoint> endpoints;
            if (address is MvcAddress mvcAddress &&
                (mvcAddress.TargetActionDescriptor != null ||
                !string.IsNullOrEmpty(mvcAddress.TargetActionName) ||
                !string.IsNullOrEmpty(mvcAddress.TargetControllerName) ||
                !string.IsNullOrEmpty(mvcAddress.TargetPageName)))
            {
                endpoints = FindByMvcAddress(mvcAddress);
            }
            else
            {
                endpoints = FindByName(address);
            }

            return endpoints;
        }

        private IEnumerable<Endpoint> FindByName(Address address)
        {
            if (address == null || string.IsNullOrEmpty(address.Name))
            {
                return Empty;
            }

            foreach (var endpoint in _matcherEndpoints)
            {
                var mvcEndpointInfo = endpoint.Metadata.GetMetadata<MvcEndpointInfo>();
                string name;
                AttributeRouteInfo attributeRouteInfo;
                if (mvcEndpointInfo == null)
                {
                    attributeRouteInfo = endpoint.Metadata.GetMetadata<AttributeRouteInfo>();
                    name = attributeRouteInfo?.Name;
                }
                else
                {
                    name = mvcEndpointInfo.Name;
                }

                if (string.Equals(address.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return new[] { endpoint };
                }
            }
            return Empty;
        }

        private IEnumerable<Endpoint> FindByMvcAddress(MvcAddress address)
        {
            if (address.CurrentActionContext == null)
            {
                throw new ArgumentException("CurrentActionContext cannot be null.", nameof(MvcAddress.CurrentActionContext));
            }

            // If another layer already calculated the target action descriptor and gave us, just use it
            if (address.TargetActionDescriptor != null)
            {
                return FindEndpointsByActionDescriptor(new[] { address.TargetActionDescriptor });
            }

            var linkTarget = GetLinkTarget(address);

            IEnumerable<ActionDescriptor> targetActionDescriptors = null;
            if (linkTarget == LinkTarget.Mvc)
            {
                targetActionDescriptors = GetTargetControllerActionDescriptors(address);
            }
            else if (linkTarget == LinkTarget.RazorPages)
            {
                targetActionDescriptors = GetTargetPageActionDescriptor(address);
            }

            if (targetActionDescriptors == null)
            {
                return Empty;
            }

            return FindEndpointsByActionDescriptor(targetActionDescriptors);
        }

        private IEnumerable<ActionDescriptor> GetTargetControllerActionDescriptors(MvcAddress address)
        {
            IEnumerable<ActionDescriptor> targetActionDescriptors = null;
            // Same controller, different action
            if (!string.IsNullOrEmpty(address.TargetActionName) &&
                string.IsNullOrEmpty(address.TargetControllerName))
            {
                var currentActionDescriptor = (ControllerActionDescriptor)address.CurrentActionContext.ActionDescriptor;
                var currentControllerType = currentActionDescriptor.ControllerTypeInfo;

                targetActionDescriptors = _actionDescriptorCollectionProvider.ActionDescriptors.Items
                    .OfType<ControllerActionDescriptor>()
                    .Where(ad => ad.ControllerTypeInfo.Equals(currentControllerType) &&
                    string.Equals(ad.ActionName, address.TargetActionName, StringComparison.OrdinalIgnoreCase));
            }
            // Different controller, different action
            else if (!string.IsNullOrEmpty(address.TargetActionName) &&
                !string.IsNullOrEmpty(address.TargetControllerName))
            {
                targetActionDescriptors = _actionDescriptorCollectionProvider.ActionDescriptors.Items
                    .OfType<ControllerActionDescriptor>()
                    .Where(
                    ad => string.Equals(ad.ControllerName, address.TargetControllerName, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(ad.ActionName, address.TargetActionName, StringComparison.OrdinalIgnoreCase));
            }
            return targetActionDescriptors;
        }

        private IEnumerable<ActionDescriptor> GetTargetPageActionDescriptor(MvcAddress address)
        {
            if (!string.IsNullOrEmpty(address.TargetPageName))
            {
                var targetActionDescriptors = _actionDescriptorCollectionProvider.ActionDescriptors.Items
                    .Where(ad =>
                    {
                        if (ad.RouteValues.TryGetValue("page", out var pageName) &&
                            string.Equals(address.TargetPageName, pageName, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                        return false;
                    });
                return targetActionDescriptors;
            }

            return null;
        }

        private IEnumerable<Endpoint> FindEndpointsByActionDescriptor(IEnumerable<ActionDescriptor> targetActionDescriptors)
        {
            foreach (var endpoint in _matcherEndpoints)
            {
                var endpointActionDescriptor = endpoint.Metadata.GetMetadata<ActionDescriptor>();
                if (endpointActionDescriptor == null)
                {
                    continue;
                }

                foreach (var actionDescriptor in targetActionDescriptors)
                {

                    if (endpointActionDescriptor.Equals(actionDescriptor))
                    {
                        yield return endpoint;
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            yield return null;
        }

        private LinkTarget GetLinkTarget(MvcAddress mvcAddress)
        {
            if (!string.IsNullOrEmpty(mvcAddress.TargetActionName))
            {
                return LinkTarget.Mvc;
            }

            if (!string.IsNullOrEmpty(mvcAddress.TargetPageName))
            {
                return LinkTarget.RazorPages;
            }

            return LinkTarget.Unknown;
        }

        private enum LinkTarget
        {
            Unknown,
            Mvc,
            RazorPages
        }
    }
}
