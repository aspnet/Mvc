// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Mvc.DesignTime
{
    /// <summary>
    /// Configures the <see cref="IMvcBuilder"/>. Implement this interface to enable design-time configuration
    /// (for instance during pre-compilation of views) of <see cref="IMvcBuilder"/>.
    /// </summary>
    public interface IMvcBuilderConfiguration
    {
        /// <summary>
        /// Configures the <see cref="IMvcBuilder"/>.
        /// </summary>
        /// <param name="builder"></param>
        void ConfigureMvc(IMvcBuilder builder);
    }
}
