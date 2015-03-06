// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Razor.Runtime.TagHelpers;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Configures an <see cref="ITagHelper"/> before it's executed.
    /// </summary>
    /// <typeparam name="T">The <see cref="ITagHelper"/> type.</typeparam>
    public interface IConfigureTagHelper<in T> : IConfigureTagHelper
        where T : ITagHelper
    {
        
    }
}