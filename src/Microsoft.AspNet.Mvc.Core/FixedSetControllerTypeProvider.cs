// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// A <see cref="IControllerTypeProvider"/> with a fixed set of types that are used as controllers. 
    /// </summary>
    public class FixedSetControllerTypeProvider : IControllerTypeProvider
    {
        /// <summary>
        /// Initializes a new instance of <see cref="StaticControllerTypeProvider"/>.
        /// </summary>
        /// <param name="controllerTypes">The sequence of controller <see cref="TypeInfo"/>.</param>
        public FixedSetControllerTypeProvider([NotNull] IEnumerable<TypeInfo> controllerTypes)
        {
            ControllerTypes = controllerTypes;
        }

        /// <summary>
        /// Gets the list of controller <see cref="TypeInfo"/>s.
        /// </summary>
        public IEnumerable<TypeInfo> ControllerTypes { get; }
    }
}
