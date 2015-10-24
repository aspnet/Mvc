// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Contains information for the <see cref="AspNet.Razor.TagHelpers.ITagHelper"/> attribute code
    /// generation process.
    /// </summary>
    public class GeneratedTagHelperAttributeContext
    {
        /// <summary>
        /// Name of the model expression type.
        /// </summary>
        public string ModelExpressionTypeName { get; set; }

        /// <summary>
        /// Name the method to create <c>ModelExpression</c>s.
        /// </summary>
        public string CreateModelExpressionMethodName { get; set; }
    }
}