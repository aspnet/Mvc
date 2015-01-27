// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Mvc.Logging;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// A <see cref="IControllerTypeProvider"/> that identifies controller types from assemblies
    /// specified by the registered <see cref="IAssemblyProvider"/>.
    /// </summary>
    public class DefaultControllerTypeProvider : IControllerTypeProvider
    {
        private readonly IAssemblyProvider _assemblyProvider;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="DefaultControllerTypeProvider"/>.
        /// </summary>
        /// <param name="assemblyProvider"><see cref="IAssemblyProvider"/> that provides assemblies to look for
        /// controllers in.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public DefaultControllerTypeProvider(IAssemblyProvider assemblyProvider,
                                             ILoggerFactory loggerFactory)
        {
            _assemblyProvider = assemblyProvider;
            _logger = loggerFactory.Create<DefaultControllerTypeProvider>();
        }

        /// <inheritdoc />
        public virtual IEnumerable<TypeInfo> GetControllerTypes()
        {
            var assemblies = _assemblyProvider.CandidateAssemblies;
            if (_logger.IsEnabled(LogLevel.Verbose))
            {
                foreach (var assembly in assemblies)
                {
                    _logger.WriteVerbose(new AssemblyValues(assembly));
                }
            }

            var types = assemblies.SelectMany(a => a.DefinedTypes);
            return types.Where(IsController);
        }

        /// <summary>
        /// Returns <c>true</c> if the <paramref name="typeInfo"/> is a controller. Otherwise <c>false</c>.
        /// </summary>
        /// <param name="typeInfo">The <see cref="TypeInfo"/>.</param>
        /// <returns><c>true</c> if the <paramref name="typeInfo"/> is a controller. Otherwise <c>false</c>.</returns>
        protected internal virtual bool IsController([NotNull] TypeInfo typeInfo)
        {
            var status = ControllerStatus.IsController;

            if (!typeInfo.IsClass)
            {
                status |= ControllerStatus.IsNotAClass;
            }
            if (typeInfo.IsAbstract)
            {
                status |= ControllerStatus.IsAbstract;
            }
            // We only consider public top-level classes as controllers. IsPublic returns false for nested
            // classes, regardless of visibility modifiers
            if (!typeInfo.IsPublic)
            {
                status |= ControllerStatus.IsNotPublicOrTopLevel;
            }
            if (typeInfo.ContainsGenericParameters)
            {
                status |= ControllerStatus.ContainsGenericParameters;
            }
            if (typeInfo.Name.Equals("Controller", StringComparison.OrdinalIgnoreCase))
            {
                status |= ControllerStatus.NameIsController;
            }
            if (!typeInfo.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase) &&
                !typeof(Controller).GetTypeInfo().IsAssignableFrom(typeInfo))
            {
                status |= ControllerStatus.DoesNotEndWithControllerAndIsNotAssignable;
            }

            if (_logger.IsEnabled(LogLevel.Verbose))
            {
                _logger.WriteVerbose(new IsControllerValues(
                    typeInfo.AsType(),
                    status));
            }

            return status == ControllerStatus.IsController;
        }
    }
}