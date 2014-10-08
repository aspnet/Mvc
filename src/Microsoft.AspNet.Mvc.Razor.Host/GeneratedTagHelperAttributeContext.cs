// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Razor.Runtime.TagHelpers;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Contains information for the <see cref="ITagHelper"/> attribute code generation process.
    /// </summary>
    public class GeneratedTagHelperAttributeContext
    {
        /// <summary>
        /// String representation of the model expression type.
        /// </summary>
        public string ModelExpressionTypeName { get; set; }

        /// <summary>
        /// String representation of a method to create model expression types.
        /// </summary>
        public string CreateModelExpressionMethodName { get; set; }
    }
}