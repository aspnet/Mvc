﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Mvc.OptionDescriptors;
using Microsoft.AspNet.Mvc.ReflectedModelBuilder;
using Microsoft.AspNet.Mvc.Rendering;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Provides programmatic configuration for the MVC framework.
    /// </summary>
    public class MvcOptions
    {
        private AntiForgeryOptions _antiForgeryOptions = new AntiForgeryOptions();
        private RazorViewEngineOptions _viewEngineOptions = new RazorViewEngineOptions();
        private int _maxModelStateErrors = 200;

        public MvcOptions()
        {
            ApplicationModelConventions = new List<IReflectedApplicationModelConvention>();
            ModelBinders = new List<ModelBinderDescriptor>();
            ViewEngines = new List<ViewEngineDescriptor>();
            ValueProviderFactories = new List<ValueProviderFactoryDescriptor>();
            OutputFormatters = new List<OutputFormatterDescriptor>();
            InputFormatters = new List<InputFormatterDescriptor>();
            Filters = new List<IFilter>();
        }

        /// <summary>
        /// Provides programmatic configuration for the anti-forgery token system.
        /// </summary>
        public AntiForgeryOptions AntiForgeryOptions
        {
            get
            {
                return _antiForgeryOptions;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value",
                                                    Resources.FormatPropertyOfTypeCannotBeNull("AntiForgeryOptions",
                                                                                               typeof(MvcOptions)));
                }

                _antiForgeryOptions = value;
            }
        }

        /// <summary>
        /// Gets a list of <see cref="IFilter"/> which are used to construct filters that 
        /// apply to all actions.
        /// </summary>
        public ICollection<IFilter> Filters { get; private set; }

        /// <summary>
        /// Gets a list of the <see cref="OutputFormatterDescriptor" /> which are used to construct
        /// a list of <see cref="IOutputFormatter"/> by <see cref="IOutputFormattersProvider"/>.
        /// </summary>
        public List<OutputFormatterDescriptor> OutputFormatters { get; private set; }

        /// <summary>
        /// Gets a list of the <see cref="InputFormatterDescriptor" /> which are used to construct
        /// a list of <see cref="IInputFormatter"/> by <see cref="IInputFormattersProvider"/>.
        /// </summary>
        public List<InputFormatterDescriptor> InputFormatters { get; private set; }

        /// <summary>
        /// Provides programmatic configuration for the default <see cref="Rendering.IViewEngine" />.
        /// </summary>
        public RazorViewEngineOptions ViewEngineOptions
        {
            get
            {
                return _viewEngineOptions;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value",
                                                    Resources.FormatPropertyOfTypeCannotBeNull("ViewEngineOptions",
                                                                                               typeof(MvcOptions)));
                }

                _viewEngineOptions = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of validation errors that are allowed by this application before further
        /// errors are ignored.
        /// </summary>
        public int MaxModelValidationErrors
        {
            get { return _maxModelStateErrors; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _maxModelStateErrors = value;
            }
        }

        /// <summary>
        /// Get a list of the <see cref="ModelBinderDescriptor" /> used by the
        /// Gets a list of the <see cref="ModelBinderDescriptor" /> used by the
        /// <see cref="ModelBinding.CompositeModelBinder" />.
        /// </summary>
        public List<ModelBinderDescriptor> ModelBinders { get; private set; }

        /// <summary>
        /// Gets a list of the <see cref="ModelValidatorProviderDescriptor" />s used by
        /// <see cref="ModelBinding.CompositeModelValidatorProvider"/>.
        /// </summary>
        public List<ModelValidatorProviderDescriptor> ModelValidatorProviders { get; }
            = new List<ModelValidatorProviderDescriptor>();

        /// <summary>
        /// Get a list of descriptors for <see cref="IViewLocationExpander" />s used by this application.
        /// </summary>
        public List<ViewLocationExpanderDescriptor> ViewLocationExpanders { get; }
            = new List<ViewLocationExpanderDescriptor>();

        /// <summary>
        /// Gets a list of descriptors that represent <see cref="Rendering.IViewEngine"/> used
        /// by this application.
        /// </summary>
        public List<ViewEngineDescriptor> ViewEngines { get; private set; }

        /// <summary>
        /// Gets a list of descriptors that represent
        /// <see cref="ModelBinding.IValueProviderFactory"/> used by this application.
        /// </summary>
        public List<ValueProviderFactoryDescriptor> ValueProviderFactories { get; private set; }

        public List<IReflectedApplicationModelConvention> ApplicationModelConventions { get; private set; }
    }
}