// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Core;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    /// <summary>
    /// A default <see cref="IActionSelector"/> implementation.
    /// </summary>
    public class ActionSelector : IActionSelector
    {
        private readonly IActionSelectorDecisionTreeProvider _decisionTreeProvider;
        private readonly ActionConstraintCache _actionConstraintCache;
        private ILogger _logger;

        /// <summary>
        /// Creates a new <see cref="ActionSelector"/>.
        /// </summary>
        /// <param name="decisionTreeProvider">The <see cref="IActionSelectorDecisionTreeProvider"/>.</param>
        /// <param name="actionConstraintCache">The <see cref="ActionConstraintCache"/> that
        /// providers a set of <see cref="IActionConstraint"/> instances.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public ActionSelector(
            IActionSelectorDecisionTreeProvider decisionTreeProvider,
            ActionConstraintCache actionConstraintCache,
            ILoggerFactory loggerFactory)
        {
            _decisionTreeProvider = decisionTreeProvider;
            _logger = loggerFactory.CreateLogger<ActionSelector>();
            _actionConstraintCache = actionConstraintCache;
        }

        public IReadOnlyList<ActionDescriptor> SelectCandidates(RouteContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var tree = _decisionTreeProvider.DecisionTree;
            return tree.Select(context.RouteData.Values);
        }

        public ActionDescriptor SelectBestCandidate(RouteContext context, IReadOnlyList<ActionDescriptor> candidates1)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (candidates1 == null)
            {
                throw new ArgumentNullException(nameof(candidates1));
            }

            var candidates = new List<ActionSelectorCandidate>();

            // Perf: Avoid allocations
            for (var i = 0; i < candidates1.Count; i++)
            {
                var action = candidates1[i];
                var constraints = _actionConstraintCache.GetActionConstraints(context.HttpContext, action);
                candidates.Add(new ActionSelectorCandidate(action, constraints));
            }

            var matchingActionConstraints =
                EvaluateActionConstraints(context, candidates, startingOrder: null);

            List<ActionDescriptor> matchingActions = null;
            if (matchingActionConstraints != null)
            {
                matchingActions = new List<ActionDescriptor>(matchingActionConstraints.Count);
                // Perf: Avoid allocations
                for (var i = 0; i < matchingActionConstraints.Count; i++)
                {
                    var candidate = matchingActionConstraints[i];
                    matchingActions.Add(candidate.Action);
                }
            }

            var finalMatches = SelectBestActions(matchingActions);

            if (finalMatches == null || finalMatches.Count == 0)
            {
                return null;
            }
            else if (finalMatches.Count == 1)
            {
                var selectedAction = finalMatches[0];

                return selectedAction;
            }
            else
            {
                var actionNames = string.Join(
                    Environment.NewLine,
                    finalMatches.Select(a => a.DisplayName));

                _logger.AmbiguousActions(actionNames);

                var message = Resources.FormatDefaultActionSelector_AmbiguousActions(
                    Environment.NewLine,
                    actionNames);

                throw new AmbiguousActionException(message);
            }
        }

        /// <summary>
        /// Returns the set of best matching actions.
        /// </summary>
        /// <param name="actions">The set of actions that satisfy all constraints.</param>
        /// <returns>A list of the best matching actions.</returns>
        protected virtual IReadOnlyList<ActionDescriptor> SelectBestActions(IReadOnlyList<ActionDescriptor> actions)
        {
            return actions;
        }

        private IReadOnlyList<ActionSelectorCandidate> EvaluateActionConstraints(
            RouteContext context,
            IReadOnlyList<ActionSelectorCandidate> candidates,
            int? startingOrder)
        {
            // Find the next group of constraints to process. This will be the lowest value of
            // order that is higher than startingOrder.
            int? order = null;

            // Perf: Avoid allocations
            for (var i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (candidate.Constraints != null)
                {
                    for (var j = 0; j < candidate.Constraints.Count; j++)
                    {
                        var constraint = candidate.Constraints[j];
                        if ((startingOrder == null || constraint.Order > startingOrder) &&
                            (order == null || constraint.Order < order))
                        {
                            order = constraint.Order;
                        }
                    }
                }
            }

            // If we don't find a 'next' then there's nothing left to do.
            if (order == null)
            {
                return candidates;
            }

            // Since we have a constraint to process, bisect the set of actions into those with and without a
            // constraint for the 'current order'.
            var actionsWithConstraint = new List<ActionSelectorCandidate>();
            var actionsWithoutConstraint = new List<ActionSelectorCandidate>();

            var constraintContext = new ActionConstraintContext();
            constraintContext.Candidates = candidates;
            constraintContext.RouteContext = context;

            // Perf: Avoid allocations
            for (var i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                var isMatch = true;
                var foundMatchingConstraint = false;

                if (candidate.Constraints != null)
                {
                    constraintContext.CurrentCandidate = candidate;
                    for (var j = 0; j < candidate.Constraints.Count; j++)
                    {
                        var constraint = candidate.Constraints[j];
                        if (constraint.Order == order)
                        {
                            foundMatchingConstraint = true;

                            if (!constraint.Accept(constraintContext))
                            {
                                isMatch = false;
                                _logger.ConstraintMismatch(
                                    candidate.Action.DisplayName,
                                    candidate.Action.Id,
                                    constraint);
                                break;
                            }
                        }
                    }
                }

                if (isMatch && foundMatchingConstraint)
                {
                    actionsWithConstraint.Add(candidate);
                }
                else if (isMatch)
                {
                    actionsWithoutConstraint.Add(candidate);
                }
            }

            // If we have matches with constraints, those are 'better' so try to keep processing those
            if (actionsWithConstraint.Count > 0)
            {
                var matches = EvaluateActionConstraints(context, actionsWithConstraint, order);
                if (matches?.Count > 0)
                {
                    return matches;
                }
            }

            // If the set of matches with constraints can't work, then process the set without constraints.
            if (actionsWithoutConstraint.Count == 0)
            {
                return null;
            }
            else
            {
                return EvaluateActionConstraints(context, actionsWithoutConstraint, order);
            }
        }
    }
}
