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
        private const string ControllerTypeName = nameof(Controller);
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
        public virtual IEnumerable<TypeInfo> ControllerTypes
        {
            get
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
        }

        /// <summary>
        /// Returns <c>true</c> if the <paramref name="typeInfo"/> is a controller. Otherwise <c>false</c>.
        /// </summary>
        /// <param name="typeInfo">The <see cref="TypeInfo"/>.</param>
        /// <returns><c>true</c> if the <paramref name="typeInfo"/> is a controller. Otherwise <c>false</c>.</returns>
        protected internal virtual bool IsController([NotNull] TypeInfo typeInfo)
        {
            if (!typeInfo.IsClass)
            {
                return false;
            }
            if (typeInfo.IsAbstract)
            {
                return false;
            }
            // We only consider public top-level classes as controllers. IsPublic returns false for nested
            // classes, regardless of visibility modifiers
            if (!typeInfo.IsPublic)
            {
                return false;
            }
            if (typeInfo.ContainsGenericParameters)
            {
                return false;
            }
            if (typeInfo.Name.Equals(ControllerTypeName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            if (!typeInfo.Name.EndsWith(ControllerTypeName, StringComparison.OrdinalIgnoreCase) &&
                !DerivesFromController(typeInfo))
            {
                return false;
            }
            if (typeInfo.IsDefined(typeof(NonControllerAttribute)))
            {
                return false;
            }

            return true;
        }

        private static bool DerivesFromController(TypeInfo typeInfo)
        {
            // A type is a controller if it derives from a type that is either named "Controller" or has the suffix
            // "Controller". We'll optimize the most common case of types deriving from the Mvc Controller type and
            // walk up the object graph if that's not the case.
            if (typeof(Controller).GetTypeInfo().IsAssignableFrom(typeInfo))
            {
                return true;
            }

            while (typeInfo != typeof(object).GetTypeInfo())
            {
                var baseTypeInfo = typeInfo.BaseType.GetTypeInfo();
                if (baseTypeInfo.Name.EndsWith(ControllerTypeName, StringComparison.Ordinal))
                {
                    return true;
                }

                typeInfo = baseTypeInfo;
            }

            return false;
        }
    }
}