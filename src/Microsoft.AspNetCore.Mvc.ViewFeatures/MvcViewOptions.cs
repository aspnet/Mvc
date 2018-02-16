// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Provides programmatic configuration for views in the MVC framework.
    /// </summary>
    public class MvcViewOptions : IEnumerable<ICompatibilitySwitch>
    {
        private readonly CompatibilitySwitch<bool> _suppressTempDataPropertyPrefix;
        private readonly ICompatibilitySwitch[] _switches;
        private HtmlHelperOptions _htmlHelperOptions = new HtmlHelperOptions();

        public MvcViewOptions()
        {
            _suppressTempDataPropertyPrefix = new CompatibilitySwitch<bool>(nameof(SuppressTempDataPropertyPrefix));
            _switches = new[]
            {
                _suppressTempDataPropertyPrefix,
            };
        }

        /// <summary>
        /// Gets or sets programmatic configuration for the HTML helpers and <see cref="Rendering.ViewContext"/>.
        /// </summary>
        public HtmlHelperOptions HtmlHelperOptions
        {
            get => _htmlHelperOptions;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _htmlHelperOptions = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that determines if the calculated key, for <see cref="TempDataAttribute"/> backed
        /// properties, used to lookup and save values in <see cref="ITempDataDictionary"/> includes a prefix discriminator.
        /// <para>
        /// A key is calculated if <see cref="TempDataAttribute.Key"/> is not specified.
        /// When the value of this property is <c>false</c>, the calculated key includes the prefix <c>TempDataProperty-</c>.
        /// When <c>true</c>, the calculated key is the name of the property.
        /// </para>
        /// <para>
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property is associated with a compatibility switch and can provide a different behavior depending on
        /// the configured compatibility version for the application. See <see cref="CompatibilityVersion"/> for
        /// guidance and examples of setting the application's compatibility version.
        /// </para>
        /// <para>
        /// Configuring the desired value of the compatibility switch by calling this property's setter will take precedence
        /// over the value implied by the application's <see cref="CompatibilityVersion"/>.
        /// </para>
        /// <para>
        /// If the application's compatibility version is set to <see cref="CompatibilityVersion.Version_2_0"/> then
        /// this setting will have the value <c>false</c> unless explicitly configured.
        /// </para>
        /// <para>
        /// If the application's compatibility version is set to <see cref="CompatibilityVersion.Version_2_1"/> or
        /// higher then this setting will have the value <c>true</c> unless explicitly configured.
        /// </para>
        /// </remarks>
        public bool SuppressTempDataPropertyPrefix
        {
            get => _suppressTempDataPropertyPrefix.Value;
            set => _suppressTempDataPropertyPrefix.Value = value;
        }

        /// <summary>
        /// Gets a list <see cref="IViewEngine"/>s used by this application.
        /// </summary>
        public IList<IViewEngine> ViewEngines { get; } = new List<IViewEngine>();

        /// <summary>
        /// Gets a list of <see cref="IClientModelValidatorProvider"/> instances.
        /// </summary>
        public IList<IClientModelValidatorProvider> ClientModelValidatorProviders { get; } =
            new List<IClientModelValidatorProvider>();

        IEnumerator<ICompatibilitySwitch> IEnumerable<ICompatibilitySwitch>.GetEnumerator()
        {
            return ((IEnumerable<ICompatibilitySwitch>)_switches).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => _switches.GetEnumerator();
    }
}