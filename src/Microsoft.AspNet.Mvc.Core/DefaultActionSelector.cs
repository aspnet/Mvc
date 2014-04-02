using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.Routing;

namespace Microsoft.AspNet.Mvc
{
    public class DefaultActionSelector : IActionSelector
    {
        private readonly INestedProviderManager<ActionDescriptorProviderContext> _actionDescriptorProvider;
        private readonly IActionBindingContextProvider _bindingProvider;

        public DefaultActionSelector(INestedProviderManager<ActionDescriptorProviderContext> actionDescriptorProvider, 
                                     IActionBindingContextProvider bindingProvider)
        {
            _actionDescriptorProvider = actionDescriptorProvider;
            _bindingProvider = bindingProvider;
        }

        public async Task<ActionDescriptor> SelectAsync(RequestContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            var allDescriptors = GetActions();

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
                return await SelectBestCandidate(context, matching);
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

        protected virtual async Task<ActionDescriptor> SelectBestCandidate(RequestContext context, List<ActionDescriptor> candidates)
        {
            var applicableCandiates = new List<ActionDescriptorCandidate>();
            foreach (var action in candidates)
            {
                var isApplicable = true;
                var candidate = new ActionDescriptorCandidate()
                {
                    Action = action,
                };

                // Issues #60 & #65 filed to deal with the ugliness of passing null here.
                var actionContext = new ActionContext(
                    httpContext: context.HttpContext, 
                    router: null, 
                    routeValues: context.RouteValues, 
                    actionDescriptor: action);
                var actionBindingContext = await _bindingProvider.GetActionBindingContextAsync(actionContext);

                foreach (var parameter in action.Parameters.Where(p => p.ParameterBindingInfo != null))
                {
                    if (actionBindingContext.ValueProvider.ContainsPrefix(parameter.ParameterBindingInfo.Prefix))
                    {
                        candidate.FoundParameters++;
                        if (parameter.IsOptional)
                        {
                            candidate.FoundOptionalParameters++;
                        }
                    }
                    else if (!parameter.IsOptional)
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

        public bool IsValidAction([NotNull] VirtualPathContext context)
        {
            // This process attempts to ensure that the route that's about to generate a link will generate a link
            // to an existing action, and that it's an action that could be reached based on what the user intended*.
            //
            // We combine this set with the values that were generated from the current route (ProvidedValues) and
            // check if it's one of the actions in the set.
            //
            // The purpose of this process is to avoid making certain routes too greedy. When a route uses a default
            // value as a filter, it can generate links to actions it will never hit.
            //
            //
            // We define "what the user intended" based on the combination of Values and AmbientValues. This set can
            // be used to select a set of actions, anything in this is set is 'intended'. Note that there are
            // false positives, but this doesn't have an impact on what we choose.
            //
            // To determine "intended" actions, we only consider RouteDataActionConstraints right now. There's some
            // powerful scenarios that could be built with dynamic constraints if we choose to enable them. 
            //
            // False positives are the result of parameter invalidation. Consider the following:
            //
            // Route: {action}/{controller}
            // Ambient values: { action = Index, controller = Home }
            // Explicit values: { action = Blog }
            // 
            // Based on these values, we could select an action like { action = Blog, controller = Home } as 'intended'.
            // However, our route cannot hit this action. The value for controller is invalidated by the route, and this
            // route won't even try to generate a link.

            if (context.ProvidedValues == null)
            {
                // We need the route's values to be able to double check our work.
                return false;
            }

            var intendedActions = GetIntendedActions(context);
            if (intendedActions.Count == 0)
            {
                return false;
            }

            var actions =
                intendedActions.Where(
                    action => action.RouteConstraints == null || 
                    action.RouteConstraints.All(constraint => constraint.Accept(context.ProvidedValues)));

            return actions.Any();
        }

        protected virtual List<ActionDescriptor> GetIntendedActions([NotNull] VirtualPathContext context)
        {
            var actions = GetActions();

            var intended = new List<ActionDescriptor>();
            foreach (var action in actions)
            {
                if (action.RouteConstraints == null)
                {
                    intended.Add(action);
                    continue;
                }

                bool isActionValid = true;
                foreach (var constraint in action.RouteConstraints)
                {
                    if (constraint.Accept(context.Values))
                    {
                        // Explicit value is acceptable
                    }
                    else if (context.Values.ContainsKey(constraint.RouteKey))
                    {
                        // There's an explicitly provided value, but the action constraint doesn't match it.
                        isActionValid = false;
                        break;
                    }
                    else if (constraint.Accept(context.AmbientValues))
                    {
                        // Ambient value is acceptable, used as a fallback
                    }
                    else
                    {
                        // No possible match
                        isActionValid = false;
                        break;
                    }
                }

                if (isActionValid)
                {
                    intended.Add(action);
                }
            }

            return intended;
        }

        private List<ActionDescriptor> GetActions()
        {
            var actionDescriptorProviderContext = new ActionDescriptorProviderContext();
            _actionDescriptorProvider.Invoke(actionDescriptorProviderContext);

            return actionDescriptorProviderContext.Results;
        }

        private class ActionDescriptorCandidate
        {
            public ActionDescriptor Action { get; set; }

            public int FoundParameters { get; set; }

            public int FoundOptionalParameters { get; set; }
        }
    }
}
