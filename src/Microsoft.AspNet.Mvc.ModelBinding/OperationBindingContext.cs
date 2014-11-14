// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Http;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// A context that contains operating information for model binding and validation.
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
        /// <remarks>
        /// This constructor copies certain values that won't change between parent and child objects,
        /// e.g. ValueProvider, ModelState
        /// </remarks>
        public OperationBindingContext(OperationBindingContext bindingContext)
        {
            if (bindingContext != null)
            {
                OriginalValueProvider = bindingContext.OriginalValueProvider;
                MetadataProvider = bindingContext.MetadataProvider;
                ModelBinder = bindingContext.ModelBinder;
                ValidatorProvider = bindingContext.ValidatorProvider;
                HttpContext = bindingContext.HttpContext;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="HttpContext"/> for the current request.
        /// </summary>
        public HttpContext HttpContext { get; set; }

        /// <summary>
        /// Gets unaltered value provider collection.
        /// Value providers can be filtered by specific model binders.
        /// </summary>
        public IValueProvider OriginalValueProvider { get; set; }

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
