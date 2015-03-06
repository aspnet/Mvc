// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Razor.Runtime.TagHelpers;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Configures an <see cref="ITagHelper"/> before it's executed.
    /// </summary>
    /// <typeparam name="TTagHelper">The <see cref="ITagHelper"/> type.</typeparam>
    public interface IConfigureTagHelper<TTagHelper>
        where TTagHelper : ITagHelper
    {
        /// <summary>
        /// Configures the <see cref="TTagHelper"/> using <see cref="Action"/>;
        /// </summary>
        /// <param name="helper">The <see cref="TTagHelper"/> to configure.</param>
        /// <param name="context">
        ///     The <see cref="ViewContext"/> for the <see cref="IView"/> the <see cref="TTagHelper"/> is in.
        /// </param>
        void Configure(TTagHelper helper, ViewContext context);
    }
}