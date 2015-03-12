// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <inheritdoc />
    public class InitializeTagHelper<TTagHelper> : IInitializeTagHelper<TTagHelper>
        where TTagHelper : ITagHelper
    {
        /// <summary>
        /// Creates an <see cref="InitializeTagHelper{TTagHelper}"/>.
        /// </summary>
        /// <param name="action">The initialization delegate.</param>
        public InitializeTagHelper([NotNull] Action<TTagHelper, ViewContext> action)
        {
            Action = action;
        }

        /// <summary>
        /// The initialization delegate.
        /// </summary>
        public Action<TTagHelper, ViewContext> Action { get; }

        /// <inheritdoc />
        public void Initialize([NotNull] TTagHelper helper, [NotNull] ViewContext context)
        {
            Action(helper, context);
        }
    }
}