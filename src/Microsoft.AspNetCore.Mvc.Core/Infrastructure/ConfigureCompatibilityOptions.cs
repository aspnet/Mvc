// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Mvc.Infrastructure
{
    public abstract class ConfigureCompatibilityOptions<TOptions> : IPostConfigureOptions<TOptions>
        where TOptions : class, IEnumerable<ICompatibilitySwitch>
    {
        private readonly ILogger _logger;

        public ConfigureCompatibilityOptions(
            ILoggerFactory loggerFactory,
            IOptions<MvcCompatibilityOptions> compatibilityOptions)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            Version = compatibilityOptions.Value.CompatibilityVersion;
            _logger = loggerFactory.CreateLogger<TOptions>();
        }

        protected abstract IReadOnlyDictionary<string, object> DefaultValues { get; }

        protected CompatibilityVersion Version { get; }

        public virtual void PostConfigure(string name, TOptions options)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            // Evaluate DefaultValues onces to subclasses don't have to cache.
            var defaultValues = DefaultValues;

            foreach (var @switch in options)
            {
                ConfigureSwitch(@switch, defaultValues);
            }
        }

        private void ConfigureSwitch(ICompatibilitySwitch @switch, IReadOnlyDictionary<string, object> defaultValues)
        {
            if (@switch.IsValueSet)
            {
                _logger.LogDebug(
                    "Compatibility switch {SwitchName} in type {OptionsType} is using explicitly configured value {Value}",
                    @switch.Name,
                    typeof(TOptions).Name,
                    @switch.Value);
                return;
            }

            if (!defaultValues.TryGetValue(@switch.Name, out var value))
            {
                _logger.LogDebug(
                    "Compatibility switch {SwitchName} in type {OptionsType} is using default value {Value}",
                    @switch.Name,
                    typeof(TOptions).Name,
                    @switch.Value,
                    Version);
                return;
            }

            @switch.Value = value;
            _logger.LogDebug(
                "Compatibility switch {SwitchName} in type {OptionsType} is using compatibility value {Value} for version {Version}",
                @switch.Name,
                typeof(TOptions).Name,
                @switch.Value,
                Version);
        }
    }
}
