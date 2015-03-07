// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Configures an <see cref="ITagHelper"/>.
    /// </summary>
    /// <typeparam name="TTagHelper">The <see cref="ITagHelper"/> type.</typeparam>
    public class ConfigureTagHelper<TTagHelper> : IConfigureTagHelper<TTagHelper>
        where TTagHelper : ITagHelper
    {
        /// <summary>
        /// Creates an <see cref="Configure(ITagHelper, ViewContext)"/>.
        /// </summary>
        /// <param name="action">The configuration delegate.</param>
        public ConfigureTagHelper([NotNull] Action<TTagHelper, ViewContext> action)
        {
            Action = action;
        }

        /// <summary>
        /// The configuration delegate.
        /// </summary>
        public Action<TTagHelper, ViewContext> Action { get; }

        /// <summary>
        /// Configures the <see cref="TTagHelper"/> using <see cref="Action"/>;
        /// </summary>
        /// <param name="helper">The <see cref="TTagHelper"/> to configure.</param>
        /// <param name="context">
        ///     The <see cref="ViewContext"/> for the <see cref="IView"/> the <see cref="TTagHelper"/> is in.
        /// </param>
        public void Configure([NotNull] TTagHelper helper, [NotNull] ViewContext context)
        {
            Action(helper, context);
        }
    }
}