// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Initializes an <see cref="ITagHelper"/> before it's executed.
    /// </summary>
    /// <typeparam name="TTagHelper">The <see cref="ITagHelper"/> type.</typeparam>
    public interface IInitializeTagHelper<TTagHelper>
        where TTagHelper : ITagHelper
    {
        /// <summary>
        /// Initializes the <see cref="TTagHelper"/>.
        /// </summary>
        /// <param name="helper">The <see cref="TTagHelper"/> to initialize.</param>
        /// <param name="context">
        ///     The <see cref="ViewContext"/> for the <see cref="IView"/> the <see cref="TTagHelper"/> is in.
        /// </param>
        void Initialize([NotNull] TTagHelper helper, [NotNull] ViewContext context);
    }
}