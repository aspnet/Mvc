// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNetCore.Mvc.ApplicationParts
{
    public class ApplicationPartCollection : Collection<ApplicationPart>
    {
        private readonly IDictionary<Type, Action<RegisteredAssemblyPart>> _featureProviders =
            new Dictionary<Type, Action<RegisteredAssemblyPart>>();

        public void Register(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            var part = new RegisteredAssemblyPart(assembly);
            AddFeaturesToPart(part);

            Add(part);
        }

        private void AddFeaturesToPart(RegisteredAssemblyPart part)
        {
            foreach (var featureRegistrationAction in _featureProviders.Values)
            {
                featureRegistrationAction(part);
            }
        }

        public void AddFeature<T>(IApplicationFeatureProvider<T> featureProvider)
        {
            if (featureProvider == null)
            {
                throw new ArgumentNullException(nameof(featureProvider));
            }

            Action<RegisteredAssemblyPart> featureRegistrationAction = assemblyPart =>
            {
                var feature = featureProvider.GetFeature(assemblyPart.Assembly);
                assemblyPart.SetFeature(feature);
            };

            _featureProviders[typeof(T)] = featureRegistrationAction;

            AddFeatureToParts(featureRegistrationAction);
        }

        private void AddFeatureToParts(Action<RegisteredAssemblyPart> featureRegistrationAction)
        {
            foreach (var registeredAssembly in this.OfType<RegisteredAssemblyPart>())
            {
                featureRegistrationAction(registeredAssembly);
            }
        }

        protected override void InsertItem(int index, ApplicationPart item)
        {
            if (!Contains(item))
            {
                base.InsertItem(index, item);
            }
        }

        private class RegisteredAssemblyPart : ApplicationPart
        {
            public RegisteredAssemblyPart(Assembly assembly)
                : base(assembly)
            {
            }
        }
    }
}
