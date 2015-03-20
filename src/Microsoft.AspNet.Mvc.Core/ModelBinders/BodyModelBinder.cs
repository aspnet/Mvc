// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Mvc.ModelBinding.Validation;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// An <see cref="IModelBinder"/> which binds models from the request body using an <see cref="IInputFormatter"/>
    /// when a model has the binding source <see cref="BindingSource.Body"/>/
    /// </summary>
    public class BodyModelBinder : BindingSourceModelBinder
    {
        private readonly ActionContext _actionContext;
        private readonly IScopedInstance<ActionBindingContext> _bindingContext;
        private readonly IInputFormatterSelector _formatterSelector;
        private readonly IValidationExcludeFiltersProvider _bodyValidationExcludeFiltersProvider;

        /// <summary>
        /// Creates a new <see cref="BodyModelBinder"/>.
        /// </summary>
        /// <param name="context">An accessor to the <see cref="ActionContext"/>.</param>
        /// <param name="bindingContext">An accessor to the <see cref="ActionBindingContext"/>.</param>
        /// <param name="selector">The <see cref="IInputFormatterSelector"/>.</param>
        /// <param name="bodyValidationExcludeFiltersProvider">
        /// The <see cref="IValidationExcludeFiltersProvider"/>.
        /// </param>
        public BodyModelBinder([NotNull] IScopedInstance<ActionContext> context,
                               [NotNull] IScopedInstance<ActionBindingContext> bindingContext,
                               [NotNull] IInputFormatterSelector selector,
                               [NotNull] IValidationExcludeFiltersProvider bodyValidationExcludeFiltersProvider)
            : base(BindingSource.Body)
        {
            _actionContext = context.Value;
            _bindingContext = bindingContext;
            _formatterSelector = selector;
            _bodyValidationExcludeFiltersProvider = bodyValidationExcludeFiltersProvider;
        }

        /// <inheritdoc />
        protected async override Task<ModelBindingResult> BindModelCoreAsync([NotNull] ModelBindingContext bindingContext)
        {
            var formatters = _bindingContext.Value.InputFormatters;

            var formatterContext = new InputFormatterContext(_actionContext, bindingContext.ModelType);
            var formatter = _formatterSelector.SelectFormatter(formatters.ToList(), formatterContext);

            if (formatter == null)
            {
                var unsupportedContentType = Resources.FormatUnsupportedContentType(
                    bindingContext.OperationBindingContext.HttpContext.Request.ContentType);
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, unsupportedContentType);
                return new ModelBindingResult(model: null, key: bindingContext.ModelName, isModelSet: false);
            }

            object model = null;
            try
            {
                model = await formatter.ReadAsync(formatterContext);
            }
            catch (Exception ex)
            {
                model = GetDefaultValueForType(bindingContext.ModelType);
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, ex);
                return new ModelBindingResult(model: null, key: bindingContext.ModelName, isModelSet: false);
            }

            // key is empty to ensure that the model name is not used as a prefix for validation.
            return new ModelBindingResult(model, key: string.Empty, isModelSet: true);
        }

        private object GetDefaultValueForType(Type modelType)
        {
            if (modelType.GetTypeInfo().IsValueType)
            {
                return Activator.CreateInstance(modelType);
            }

            return null;
        }
    }
}
