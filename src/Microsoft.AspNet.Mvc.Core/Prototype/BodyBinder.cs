// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc
{
    public class BodyBinder : MarkerAwareBinder<FromBody2Attribute>
    {
        private ActionContext _actionContext;
        private IInputFormatterSelector _formatterSelector;

        public BodyBinder(IContextAccessor<ActionContext> context, IInputFormatterSelector selector)
        {
            _actionContext = context.Value;
            _formatterSelector = selector;
        }

        public override async Task<bool> BindAsync(ModelBindingContext bindingContext)
        {
            var formatterContext = new InputFormatterContext(_actionContext, bindingContext.ModelType);
            var formatter = _formatterSelector.SelectFormatter(formatterContext);

            if(formatter == null)
            {
                var unsupportedContentType = Resources.FormatUnsupportedContentType(
                                                    bindingContext.HttpContext.Request.ContentType);
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, unsupportedContentType);
                return false;
            }

            bindingContext.Model = await formatter.ReadAsync(formatterContext);
            return true;
        }
    }
}
