// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Microsoft.AspNetCore.Mvc.IntegrationTests
{
    public static class ParameterBinderExtensions
    {
        /// <remarks>
        /// This method discovers the <see cref="ModelMetadata"/> for the <paramref name="parameter"/>
        /// and also updates <see cref="ParameterDescriptor.BindingInfo"/> by calling <see cref="BindingInfo.TryApplyBindingInfo(ModelMetadata)"/>
        /// using the discovered <see cref="ModelMetadata"/>.
        /// </remarks>
        public static Task<ModelBindingResult> BindModelAsync(
            this ParameterBinder parameterBinder,
            ParameterDescriptor parameter,
            ModelBindingTestContext context)
        {
            var metadata = ModelBindingTestHelper.GetModelMetadataForParameter(context, parameter);

            var bindingInfo = parameter.BindingInfo ?? new BindingInfo();
            bindingInfo.TryApplyBindingInfo(metadata);
            parameter.BindingInfo = bindingInfo;

            return parameterBinder.BindModelAsync(parameter, context, context.MetadataProvider, metadata);
        }

        public static async Task<ModelBindingResult> BindModelAsync(
            this ParameterBinder parameterBinder,
            ParameterDescriptor parameter,
            ControllerContext context,
            IModelMetadataProvider modelMetadataProvider,
            ModelMetadata modelMetadata)
        {
            var valueProvider = await CompositeValueProvider.CreateAsync(context);
            var modelBinderFactory = ModelBindingTestHelper.GetModelBinderFactory(
                modelMetadataProvider,
                context.HttpContext.RequestServices);

            var modelBinder = modelBinderFactory.CreateBinder(new ModelBinderFactoryContext
            {
                BindingInfo = parameter.BindingInfo,
                Metadata = modelMetadata,
                CacheToken = parameter,
            });

            return await parameterBinder.BindModelAsync(
                context,
                modelBinder,
                valueProvider,
                parameter,
                modelMetadata,
                value: null);
        }
    }
}
