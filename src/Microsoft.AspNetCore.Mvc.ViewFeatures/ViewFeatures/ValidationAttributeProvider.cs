// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures
{
    /// <summary>
    /// Default implementation of <see cref="IValidationAttributeProvider"/>.
    /// </summary>
    public class ValidationAttributeProvider : IValidationAttributeProvider
    {
        private readonly IModelMetadataProvider _metadataProvider;
        private readonly ClientValidatorCache _clientValidatorCache;
        private readonly IClientModelValidatorProvider _clientModelValidatorProvider;

        /// <summary>
        /// Initializes a new <see cref="ValidationAttributeProvider"/> instance.
        /// </summary>
        /// <param name="optionsAccessor">The accessor for <see cref="MvcViewOptions"/>.</param>
        /// <param name="metadataProvider">The <see cref="IModelMetadataProvider"/>.</param>
        /// <param name="clientValidatorCache">The <see cref="ClientValidatorCache"/> that provides
        /// a list of <see cref="IClientModelValidator"/>s.</param>
        public ValidationAttributeProvider(
            IOptions<MvcViewOptions> optionsAccessor,
            IModelMetadataProvider metadataProvider,
            ClientValidatorCache clientValidatorCache)
        {
            if (optionsAccessor == null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }

            if (metadataProvider == null)
            {
                throw new ArgumentNullException(nameof(metadataProvider));
            }

            if (clientValidatorCache == null)
            {
                throw new ArgumentNullException(nameof(clientValidatorCache));
            }

            _clientValidatorCache = clientValidatorCache;
            _metadataProvider = metadataProvider;

            var clientValidatorProviders = optionsAccessor.Value.ClientModelValidatorProviders;
            _clientModelValidatorProvider = new CompositeClientModelValidatorProvider(clientValidatorProviders);
        }

        /// <inheritdoc />
        public virtual void AddValidationAttributes(
            ViewContext viewContext,
            ModelExplorer modelExplorer,
            string expression,
            IDictionary<string, string> attributes)
        {
            if (viewContext == null)
            {
                throw new ArgumentNullException(nameof(viewContext));
            }

            if (attributes == null)
            {
                throw new ArgumentNullException(nameof(attributes));
            }

            // Only render attributes if client-side validation is enabled, and then only if we've
            // never rendered validation for a field with this name in this form.
            var formContext = viewContext.ClientValidationEnabled ? viewContext.FormContext : null;
            if (formContext == null)
            {
                return;
            }

            var fullName = viewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(expression);
            if (formContext.RenderedField(fullName))
            {
                return;
            }

            formContext.RenderedField(fullName, true);

            modelExplorer = modelExplorer ??
                ExpressionMetadataProvider.FromStringExpression(expression, viewContext.ViewData, _metadataProvider);

            var validators = _clientValidatorCache.GetValidators(modelExplorer.Metadata, _clientModelValidatorProvider);
            if (validators.Count > 0)
            {
                var validationContext = new ClientModelValidationContext(
                    viewContext,
                    modelExplorer.Metadata,
                    _metadataProvider,
                    attributes);

                for (var i = 0; i < validators.Count; i++)
                {
                    var validator = validators[i];
                    validator.AddValidation(validationContext);
                }
            }
        }
    }
}
