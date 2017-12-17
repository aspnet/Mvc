// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Specifies the version compatibility of settings configured by <see cref="MvcOptions"/>. 
    /// </summary>
    /// <remarks>
    /// Setting <see cref="MvcCompatibilityOptions.CompatibilityVersion"/> to a specific version will 
    /// change the default values of various settings to match a particular release of ASP.NET Core MVC. 
    /// See <see cref="MvcCompatibilityOptions.CompatibilityVersion"/> for more details.
    /// </remarks>
    public enum CompatibilityVersion
    {
        /// <summary>
        /// Sets the default value of settings on <see cref="MvcOptions"/> to match the behaviour of 
        /// ASP.NET Core MVC 2.0.0.
        /// </summary>
        Version_2_0,

        /// <summary>
        /// Sets the default value of settings on <see cref="MvcOptions"/> to match the behaviour of 
        /// ASP.NET Core MVC 2.1.0.
        /// </summary>
        Version_2_1,

        /// <summary>
        /// Sets the default value of settings on <see cref="MvcOptions"/> to match the latest release. Use this
        /// value with care, upgrading minor versions will cause breaking changes when using <see cref="Latest"/>.
        /// </summary>
        Latest = int.MaxValue,
    }
}
