// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Actions;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.ActionConstraints
{
    /// <summary>
    /// A candidate action for action selection.
    /// </summary>
    public class ActionSelectorCandidate
    {
        /// <summary>
        /// Creates a new <see cref="ActionSelectorCandidate"/>.
        /// </summary>
        /// <param name="action">The <see cref="ActionDescriptor"/> representing a candidate for selection.</param>
        /// <param name="constraints">
        /// The list of <see cref="IActionConstraint"/> instances associated with <paramref name="action"/>.
        /// </param>
        public ActionSelectorCandidate([NotNull] ActionDescriptor action, IReadOnlyList<IActionConstraint> constraints)
        {
            Action = action;
            Constraints = constraints;
        }

        /// <summary>
        /// The <see cref="ActionDescriptor"/> representing a candiate for selection.
        /// </summary>
        public ActionDescriptor Action { get; private set; }

        /// <summary>
        /// The list of <see cref="IActionConstraint"/> instances associated with <see name="Action"/>.
        /// </summary>
        public IReadOnlyList<IActionConstraint> Constraints { get; private set; }
    }
}