// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Newtonsoft.Json.Serialization;

namespace Microsoft.AspNetCore.Mvc
{
    public static class MvcJsonOptionsExtensions
    {
        /// <summary>
        /// Applies camelCasing to property names and dictionary keys.
        /// </summary>
        public static MvcJsonOptions UseCamelCasing(this MvcJsonOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            
            options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            
            return options;
        }

        /// <summary>
        /// Property names and dictionary keys are unchanged.
        /// </summary>
        public static MvcJsonOptions UseMemberCasing(this MvcJsonOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            
            options.SerializerSettings.ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new DefaultNamingStrategy()
            };

            return options;
        }
    }
}