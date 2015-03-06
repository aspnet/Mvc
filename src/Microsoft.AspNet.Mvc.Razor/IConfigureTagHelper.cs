// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;

namespace Microsoft.AspNet.Mvc.Razor
{
    public interface IConfigureTagHelper
    {
        /// <summary>
        /// Configures the <see cref="ITagHelper"/>.
        /// </summary>
        /// <param name="helper">The <see cref="ITagHelper"/> being configured.</param>
        /// <param name="context">
        ///     The <see cref="ViewContext"/> for the <see cref="IView"/> the <see cref="ITagHelper"/> is in.
        /// </param>
        void Configure(ITagHelper helper, ViewContext context);
    }
}