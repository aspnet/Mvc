// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Mvc.Formatters;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.ModelBinding.Metadata;
using Microsoft.AspNet.Mvc.ModelBinding.Validation;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Mvc.Internal
{
    /// <summary>
    /// Sets up default options for <see cref="MvcOptions"/>.
    /// </summary>
    public class MvcCoreMvcOptionsSetup : ConfigureOptions<MvcOptions>
    {
        public MvcCoreMvcOptionsSetup()
            : base(ConfigureMvc)
        {
        }

        public static void ConfigureMvc(MvcOptions options)
        {
            // Set up default error messages
            var bindingMessages = options.ModelBindingMessages;
            bindingMessages.MissingBindRequiredValueResource = Resources.FormatModelBinding_MissingBindRequiredMember;
            bindingMessages.MissingKeyOrValueResource = Resources.FormatKeyValuePair_BothKeyAndValueMustBePresent;
            bindingMessages.ValueInvalid_MustNotBeNullResource = Resources.FormatModelBinding_NullValueNotValid;

            // Set up ModelBinding
            options.ModelBinders.Add(new BinderTypeBasedModelBinder());
            options.ModelBinders.Add(new ServicesModelBinder());
            options.ModelBinders.Add(new BodyModelBinder());
            options.ModelBinders.Add(new HeaderModelBinder());
            options.ModelBinders.Add(new SimpleTypeModelBinder());
            options.ModelBinders.Add(new CancellationTokenModelBinder());
            options.ModelBinders.Add(new ByteArrayModelBinder());
            options.ModelBinders.Add(new FormFileModelBinder());
            options.ModelBinders.Add(new FormCollectionModelBinder());
            options.ModelBinders.Add(new GenericModelBinder());
            options.ModelBinders.Add(new MutableObjectModelBinder());

            // Set up default output formatters.
            options.OutputFormatters.Add(new HttpNoContentOutputFormatter());
            options.OutputFormatters.Add(new StringOutputFormatter());
            options.OutputFormatters.Add(new StreamOutputFormatter());

            // Set up ValueProviders
            options.ValueProviderFactories.Add(new RouteValueValueProviderFactory());
            options.ValueProviderFactories.Add(new QueryStringValueProviderFactory());
            options.ValueProviderFactories.Add(new FormValueProviderFactory());
            options.ValueProviderFactories.Add(new JQueryFormValueProviderFactory());

            // Set up metadata providers
            options.ModelMetadataDetailsProviders.Add(new DefaultBindingMetadataProvider(bindingMessages));
            options.ModelMetadataDetailsProviders.Add(new DefaultValidationMetadataProvider());

            // Set up validators
            options.ModelValidatorProviders.Add(new DefaultModelValidatorProvider());

            // Add types to be excluded from Validation
            options.ValidationExcludeFilters.Add(new SimpleTypesExcludeFilter());
            options.ValidationExcludeFilters.Add(typeof(Type));

            // Any 'known' types that we bind should be marked as excluded from validation.
            options.ValidationExcludeFilters.Add(typeof(System.Threading.CancellationToken));
            options.ValidationExcludeFilters.Add(typeof(IFormFile));
            options.ValidationExcludeFilters.Add(typeof(IFormCollection));
        }
    }
}