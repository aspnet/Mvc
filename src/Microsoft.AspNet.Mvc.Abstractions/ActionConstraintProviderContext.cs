﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.Http;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.ActionConstraints
{
    /// <summary>
    /// Context for an action constraint provider.
    /// </summary>
    public class ActionConstraintProviderContext
    {
        /// <summary>
        /// Creates a new <see cref="ActionConstraintProviderContext"/>.
        /// </summary>
        /// <param name="action">The <see cref="ActionDescriptor"/> for which constraints are being created.</param>
        /// <param name="items">The list of <see cref="ActionConstraintItem"/> objects.</param>
        public ActionConstraintProviderContext(
            [NotNull] HttpContext context,
            [NotNull] ActionDescriptor action,
            [NotNull] IList<ActionConstraintItem> items)
        {
            HttpContext = context;
            Action = action;
            Results = items;
        }

        public HttpContext HttpContext { get; }

        /// <summary>
        /// The <see cref="ActionDescriptor"/> for which constraints are being created.
        /// </summary>
        public ActionDescriptor Action { get; private set; }

        /// <summary>
        /// The list of <see cref="ActionConstraintItem"/> objects.
        /// </summary>
        public IList<ActionConstraintItem> Results { get; private set; }
    }
}