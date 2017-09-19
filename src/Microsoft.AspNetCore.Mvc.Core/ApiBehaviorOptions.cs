// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Options used to configure behavior for types annotated with <see cref="ApiControllerAttribute"/>.
    /// </summary>
    public class ApiBehaviorOptions
    {
        /// <summary>
        /// The delegate invoked on actions annotated with <see cref="ApiControllerAttribute"/> to convert invalid
        /// <see cref="ModelStateDictionary"/> into an <see cref="IActionResult"/>
        /// <para>
        /// By default, the delegate produces a <see cref="BadRequestObjectResult"/> using <see cref="ProblemDetails"/>
        /// as the problem format. To disable this feature, set the value of the delegate to <c>null</c>.
        /// </para>
        /// </summary>
        public Func<ActionContext, IActionResult> InvalidModelStateResponseFactory { get; set; }
    }
}
