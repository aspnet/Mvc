// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Represents a model binder which understands <see cref="IBodyBinderMarker"/> and uses 
    /// InputFomatters to bind the model to request's body.
    /// </summary>
    public class BodyModelBinder : MarkerAwareBinder<IBodyBinderMarker>
    {
        private readonly ActionContext _actionContext;
        private readonly IInputFormatterSelector _formatterSelector;
        private readonly IBodyModelValidator _bodyModelValidator;
        private readonly IOptions<MvcOptions> _mvcOptions;

        public BodyModelBinder([NotNull] IContextAccessor<ActionContext> context,
                               [NotNull] IInputFormatterSelector selector,
                               [NotNull] IBodyModelValidator bodyModelValidator,
                               [NotNull] IOptions<MvcOptions> mvcOptions)
        {
            _actionContext = context.Value;
            _formatterSelector = selector;
            _bodyModelValidator = bodyModelValidator;
            _mvcOptions = mvcOptions;
        }

        protected override async Task<bool> BindAsync(ModelBindingContext bindingContext, IBodyBinderMarker marker)
        {
            var formatterContext = new InputFormatterContext(_actionContext, bindingContext.ModelType);
            var formatter = _formatterSelector.SelectFormatter(formatterContext);

            if (formatter == null)
            {
                var unsupportedContentType = Resources.FormatUnsupportedContentType(
                                                    bindingContext.HttpContext.Request.ContentType);
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, unsupportedContentType);

                // Should always return true so that the model binding process ends here.
                return true;
            }

            bindingContext.Model = await formatter.ReadAsync(formatterContext);

            // Validate the deserialized object
            var validationContext = new ModelValidationContext(
                bindingContext.MetadataProvider,
                bindingContext.ValidatorProvider,
                bindingContext.ModelState,
                bindingContext.ModelMetadata,
                containerMetadata: null,
                excludeFromValidationDelegate: _mvcOptions.Options.ExcludeFromValidationDelegates);
            _bodyModelValidator.Validate(validationContext, bindingContext.ModelName);
            return true;
        }
    }
}
