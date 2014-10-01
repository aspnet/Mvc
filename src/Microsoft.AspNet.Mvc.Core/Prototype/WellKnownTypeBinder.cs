// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class WellKnownTypeBinder : MarkerAwareBinder<ActivateAttribute>
    {
        // This can come from options.
        private static IReadOnlyDictionary<Type, Func<ActionContext, object>> _valueAccessorLookup = CreateValueAccessorLookup();
        private ActionContext _actionContext;

        public WellKnownTypeBinder(IContextAccessor<ActionContext> contextAccessor)
        {
            _actionContext = contextAccessor.Value;
        }
        
        public override Task<bool> BindAsync(ModelBindingContext bindingContext)
        {
            if (_valueAccessorLookup.TryGetValue(bindingContext.ModelType, out var valueAccessor))
            {
                bindingContext.Model = valueAccessor(_actionContext);
                return Task.FromResult<bool>(true);
            }

            return Task.FromResult<bool>(false);
        }

        private static IReadOnlyDictionary<Type, Func<ActionContext, object>> CreateValueAccessorLookup()
        {
            var dictionary = new Dictionary<Type, Func<ActionContext, object>>
            {
                { typeof(ActionContext), (context) => context },
                { typeof(HttpContext), (context) => context.HttpContext },
                { typeof(HttpRequest), (context) => context.HttpContext.Request },
                { typeof(HttpResponse), (context) => context.HttpContext.Response },
                {
                    typeof(ViewDataDictionary),
                    (context) =>
                    {
                        var serviceProvider = context.HttpContext.RequestServices;
                        return new ViewDataDictionary(
                            serviceProvider.GetService<IModelMetadataProvider>(),
                            context.ModelState);
                    }
                }
            };

            return dictionary;
        }
    }
}
