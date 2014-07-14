// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Summary description for ViewStart
    /// </summary>
    public abstract class ViewStart : RazorView
    {
        /// <summary>
        /// Gets or sets the next page in the view execution hierarchy.
        /// </summary>
        public RazorView Next { get; set; }

        /// <summary>
        /// Gets or sets the Layout page used by this view hierarchy.
        /// </summary>
        /// <remarks>This property manipulates the Layout property of the <see cref="Next"/> page in the hierarchy,
        /// if set.</remarks>
        public override string Layout
        {
            get
            {
                if (Next == null)
                {
                    throw new InvalidOperationException("A ViewStart page must be associated with a RazorView or another ViewStart.");
                }

                return base.Layout;
            }

            set
            {
                if (Next != null)
                {
                    Next.Layout = value;
                }

                base.Layout = value;
            }
        }

        public override async Task RenderViewAsync([NotNull] ViewContext context)
        {
            await base.RenderViewAsync(context);
            if (Next != null)
            {
                await Next.RenderViewAsync(context);
            }
        }
    }
}