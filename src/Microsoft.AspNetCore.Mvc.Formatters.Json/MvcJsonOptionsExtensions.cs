// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Newtonsoft.Json.Serialization;

namespace Microsoft.AspNetCore.Mvc
{
    public static class MvcJsonOptionsExtensions
    {
        /// <summary>
        /// Camel case property names.
        /// </summary>
        /// <param name="options"><see cref="MvcJsonOptions"/></param>
        /// <param name="processDictionaryKeys">If true will camel case dictionary keys.</param>
        /// <returns><see cref="MvcJsonOptions"/> with camel case settings</returns>
        public static MvcJsonOptions UseCamelCasing(this MvcJsonOptions options, bool processDictionaryKeys)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            
            if (options.SerializerSettings.ContractResolver.GetType() != typeof(DefaultContractResolver))
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy
                    {
                        ProcessDictionaryKeys = processDictionaryKeys
                    }
                };
                return options;
            }

            var defaultResolver = options.SerializerSettings.ContractResolver as DefaultContractResolver;

            if (defaultResolver.NamingStrategy.GetType() == typeof(CamelCaseNamingStrategy))
            {
                defaultResolver.NamingStrategy.ProcessDictionaryKeys = processDictionaryKeys;
            }
            else
            {
                defaultResolver.NamingStrategy = new CamelCaseNamingStrategy
                {
                    ProcessDictionaryKeys = processDictionaryKeys
                };
            }

            return options;
        }

        /// <summary>
        /// Property names and dictionary keys are unchanged.
        /// </summary>
        /// <param name="options"><see cref="MvcJsonOptions"/></param>
        /// <returns><see cref="MvcJsonOptions"/> with settings that use member casing.</returns>
        public static MvcJsonOptions UseMemberCasing(this MvcJsonOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.SerializerSettings.ContractResolver.GetType() != typeof(DefaultContractResolver))
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver()
                {
                    NamingStrategy = new DefaultNamingStrategy()
                };
            }

            var resolver = options.SerializerSettings.ContractResolver as DefaultContractResolver;

            if (resolver.NamingStrategy.GetType() != typeof(DefaultNamingStrategy))
            {
                resolver.NamingStrategy = new DefaultNamingStrategy();
            }

            return options;
        }
    }
}