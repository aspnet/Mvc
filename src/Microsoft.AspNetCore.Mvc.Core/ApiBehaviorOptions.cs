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
        private Func<ActionContext, IActionResult> _invalidModelStateResponseFactory;

        /// <summary>
        /// Delegate invoked on actions annotated with <see cref="ApiControllerAttribute"/> to convert invalid
        /// <see cref="ModelStateDictionary"/> into an <see cref="IActionResult"/>
        /// <para>
        /// By default, the delegate produces a <see cref="BadRequestObjectResult"/> using <see cref="ProblemDetails"/>
        /// as the problem format.
        /// </para>
        /// </summary>
        public Func<ActionContext, IActionResult> InvalidModelStateResponseFactory
        {
            get => _invalidModelStateResponseFactory;
            set => _invalidModelStateResponseFactory = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Gets or sets a value that determines if the filter that returns an <see cref="BadRequestObjectResult"/> when
        /// <see cref="ActionContext.ModelState"/> is invalid. Defaults to <c>true</c>.
        /// <seealso cref="InvalidModelStateResponseFactory"/>.
        /// </summary>
        public bool EnableModelStateInvalidFilter { get; set; } = true;

        /// <summary>
        /// Gets or sets a value that determines if model binding sources are inferred for parameters that do not explicitly
        /// specify one.
        /// <para>
        /// Parameters that appear as route values, are assumed to be bound from the path (<see cref="BindingSource.Path"/>).
        /// Parameters that are complex (<see cref="ModelMetadata.IsComplexType"/>) are assumed to be bound from the body (<see cref="BindingSource.Body"/>).
        /// All other parameters are assumed to be bound from the query.
        /// </para>
        /// </summary>
        public bool InferBindingSourcesForParameters { get; set; } = true;
    }
}
