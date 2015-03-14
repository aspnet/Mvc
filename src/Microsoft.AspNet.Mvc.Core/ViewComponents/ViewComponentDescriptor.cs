// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// A descriptor for a View Component.
    /// </summary>
    public class ViewComponentDescriptor
    {
        /// <summary>
        /// Gets or sets the full name.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the short name.
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Type"/>.
        /// </summary>
        public Type Type { get; set; }
    }
}