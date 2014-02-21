using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc
{
    public class DefaultActionSelector : IActionSelector
    {
        private readonly IEnumerable<IActionDescriptorProvider> _actionDescriptorProviders;
        private readonly IModelBindingConfigProvider _modelBindingConfigProvider;

        public DefaultActionSelector(IEnumerable<IActionDescriptorProvider> actionDescriptorProviders, 
                                     IModelBindingConfigProvider modelBindingConfigProvider)
        {
            _actionDescriptorProviders = actionDescriptorProviders;
            _modelBindingConfigProvider = modelBindingConfigProvider;
        }

        public ActionDescriptor Select(RequestContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            var allDescriptors = _actionDescriptorProviders.SelectMany(d => d.GetDescriptors());

            var matching = allDescriptors.Where(ad => Match(ad, context)).ToList();
            if (matching.Count == 0)
            {
                return null;
            }
            else if (matching.Count == 1)
            {
                return matching[0];
            }
            else
            {
                return SelectBestCandidate(context, matching);
            }
        }

        public bool Match(ActionDescriptor descriptor, RequestContext context)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException("descriptor");
            }

            return (descriptor.RouteConstraints == null || descriptor.RouteConstraints.All(c => c.Accept(context))) &&
                   (descriptor.MethodConstraints == null || descriptor.MethodConstraints.All(c => c.Accept(context))) &&
                   (descriptor.DynamicConstraints == null || descriptor.DynamicConstraints.All(c => c.Accept(context)));
        }

        protected virtual ActionDescriptor SelectBestCandidate(RequestContext requestContext, List<ActionDescriptor> candidates)
        {
            var applicableCandiates = new List<ActionDescriptorCandidate>();
            foreach (var action in candidates)
            {
                var isApplicable = true;
                var candidate = new ActionDescriptorCandidate()
                {
                    Action = action,
                };

                var actionContext = new ActionContext(requestContext.HttpContext, requestContext.RouteValues, action);
                ModelBinderConfig modelBindingConfig = _modelBindingConfigProvider.GetConfig(actionContext);

                // TODO: Figure out how we read IsFromBody. Tracked by WEBFX-100
                foreach (var parameter in action.Parameters.Where(p => !p.Binding.IsFromBody))
                {
                    if (modelBindingConfig.ValueProvider.ContainsPrefix(parameter.Binding.Prefix))
                    {
                        candidate.FoundParameters++;
                        if (parameter.Binding.IsOptional)
                        {
                            candidate.FoundOptionalParameters++;
                        }
                    }
                    else if (!parameter.Binding.IsOptional)
                    {
                        isApplicable = false;
                        break;
                    }
                }

                if (isApplicable)
                {
                    applicableCandiates.Add(candidate);
                }
            }

            var mostParametersSatisfied = applicableCandiates.GroupBy(c => c.FoundParameters).OrderByDescending(g => g.Key).First();
            if (mostParametersSatisfied == null)
            {
                return null;
            }

            var fewestOptionalParameters = mostParametersSatisfied.GroupBy(c => c.FoundOptionalParameters).OrderBy(g => g.Key).First().ToArray();
            if (fewestOptionalParameters.Length > 1)
            {
                throw new InvalidOperationException("The actions are ambiguious.");
            }

            return fewestOptionalParameters[0].Action;
        }

        private class ActionDescriptorCandidate
        {
            public ActionDescriptor Action { get; set; }

            public int FoundParameters { get; set; }

            public int FoundOptionalParameters { get; set; }
        }
    }
}
