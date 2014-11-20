// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Http;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// A context that contains information specific to the current request and the action whose parameters
    /// are being model bound.
    /// </summary>
    public class OperationBindingContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationBindingContext"/> class.
        /// </summary>
        public OperationBindingContext()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationBindingContext"/> class using the
        /// <param name="bindingContext" />.
        // </summary>
        /// <param name="bindingContext">Existing binding context.</param>
        /// <remarks>
        /// This constructor copies certain values that won't change between model binding two un related models.
        /// </remarks>
        public OperationBindingContext([NotNull] OperationBindingContext bindingContext)
        {
            ValueProvider = bindingContext.ValueProvider;
            MetadataProvider = bindingContext.MetadataProvider;
            ModelBinder = bindingContext.ModelBinder;
            ValidatorProvider = bindingContext.ValidatorProvider;
            HttpContext = bindingContext.HttpContext;
            BodyBindingState = bindingContext.BodyBindingState;
        }

        /// <summary>
        /// Represents if there has been a body bound model found during the current model binding process.
        /// </summary>
        public BodyBindingState BodyBindingState { get; set; } = BodyBindingState.NotBodyBased;

        /// <summary>
        /// Gets or sets the <see cref="HttpContext"/> for the current request.
        /// </summary>
        public HttpContext HttpContext { get; set; }

        /// <summary>
        /// Gets unaltered value provider collection.
        /// Value providers can be filtered by specific model binders.
        /// </summary>
        public IValueProvider ValueProvider { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IModelBinder"/> associated with this context.
        /// </summary>
        public IModelBinder ModelBinder { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IModelMetadataProvider"/> associated with this context.
        /// </summary>
        public IModelMetadataProvider MetadataProvider { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IModelValidatorProvider"/> instance used for model validation with this
        /// context.
        /// </summary>
        public IModelValidatorProvider ValidatorProvider { get; set; }
    }
}
