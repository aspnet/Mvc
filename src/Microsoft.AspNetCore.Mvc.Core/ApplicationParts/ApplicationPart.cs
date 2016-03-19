// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.AspNetCore.Mvc.ApplicationParts
{
    public class ApplicationPart : IEquatable<ApplicationPart>
    {
        private readonly IDictionary<Type, object> _features;

        public ApplicationPart(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            _features = new Dictionary<Type, object>();
            Assembly = assembly;
        }

        public Assembly Assembly { get; }

        public T GetFeature<T>()
        {
            return _features.ContainsKey(typeof(T)) ? (T)_features[typeof(T)] : default(T);
        }

        public void SetFeature<T>(T feature)
        {
            _features[typeof(T)] = feature;
        }

        /// <inheritdoc />
        public bool Equals(ApplicationPart other)
        {
            return Assembly.Equals(other?.Assembly);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is ApplicationPart && Equals((ApplicationPart)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Assembly.GetHashCode();
        }
    }
}
