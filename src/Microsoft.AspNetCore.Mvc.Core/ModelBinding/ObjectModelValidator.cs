﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Microsoft.AspNetCore.Mvc.ModelBinding
{
    /// <summary>
    /// Provides a base <see cref="IObjectModelValidator"/> implementation for validating an object graph.
    /// </summary>
    public abstract class ObjectModelValidator : IObjectModelValidator
    {
        private readonly IModelMetadataProvider _modelMetadataProvider;
        private readonly ValidatorCache _validatorCache;
        private readonly CompositeModelValidatorProvider _validatorProvider;

        /// <summary>
        /// Initializes a new instance of <see cref="ObjectModelValidator"/>.
        /// </summary>
        /// <param name="modelMetadataProvider">The <see cref="IModelMetadataProvider"/>.</param>
        /// <param name="validatorProviders">The list of <see cref="IModelValidatorProvider"/>.</param>
        public ObjectModelValidator(
            IModelMetadataProvider modelMetadataProvider,
            IList<IModelValidatorProvider> validatorProviders)
        {
            if (modelMetadataProvider == null)
            {
                throw new ArgumentNullException(nameof(modelMetadataProvider));
            }

            if (validatorProviders == null)
            {
                throw new ArgumentNullException(nameof(validatorProviders));
            }

            _modelMetadataProvider = modelMetadataProvider;
            _validatorCache = new ValidatorCache();

            _validatorProvider = new CompositeModelValidatorProvider(validatorProviders);
        }

        /// <inheritdoc />
        public virtual void Validate(
            ActionContext actionContext,
            ValidationStateDictionary validationState,
            string prefix,
            object model)
        {
            var visitor = GetValidationVisitor(
                actionContext,
                _validatorProvider,
                _validatorCache,
                _modelMetadataProvider,
                validationState);

            var metadata = model == null ? null : _modelMetadataProvider.GetMetadataForType(model.GetType());
            visitor.Validate(metadata, prefix, model, alwaysValidateAtTopLevel: false);
        }

        /// <summary>
        /// Validates the provided object model.
        /// If <paramref name="model"/> is <see langword="null"/> and the <paramref name="metadata"/>'s
        /// <see cref="ModelMetadata.IsRequired"/> is <see langword="true"/>, will add one or more
        /// model state errors that <see cref="Validate(ActionContext, ValidationStateDictionary, string, object)"/>
        /// would not.
        /// </summary>
        /// <param name="actionContext">The <see cref="ActionContext"/>.</param>
        /// <param name="validationState">The <see cref="ValidationStateDictionary"/>.</param>
        /// <param name="prefix">The model prefix key.</param>
        /// <param name="model">The model object.</param>
        /// <param name="metadata">The <see cref="ModelMetadata"/>.</param>
        public virtual void Validate(
            ActionContext actionContext,
            ValidationStateDictionary validationState,
            string prefix,
            object model,
            ModelMetadata metadata)
        {
            var visitor = GetValidationVisitor(
                actionContext,
                _validatorProvider,
                _validatorCache,
                _modelMetadataProvider,
                validationState);

            visitor.Validate(metadata, prefix, model, alwaysValidateAtTopLevel: metadata.IsRequired);
        }

        /// <summary>
        /// Gets a <see cref="ValidationVisitor"/> that traverses the object model graph and performs validation.
        /// </summary>
        /// <param name="actionContext">The <see cref="ActionContext"/>.</param>
        /// <param name="validatorProvider">The <see cref="IModelValidatorProvider"/>.</param>
        /// <param name="validatorCache">The <see cref="ValidatorCache"/>.</param>
        /// <param name="metadataProvider">The <see cref="IModelMetadataProvider"/>.</param>
        /// <param name="validationState">The <see cref="ValidationStateDictionary"/>.</param>
        /// <returns>A <see cref="ValidationVisitor"/> which traverses the object model graph.</returns>
        public abstract ValidationVisitor GetValidationVisitor(
            ActionContext actionContext,
            IModelValidatorProvider validatorProvider,
            ValidatorCache validatorCache,
            IModelMetadataProvider metadataProvider,
            ValidationStateDictionary validationState);
    }
}
