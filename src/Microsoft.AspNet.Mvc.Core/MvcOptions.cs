﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.ReflectedModelBuilder;
using Microsoft.AspNet.Mvc.Rendering;

namespace Microsoft.AspNet.Mvc
{
    public class MvcOptions
    {
        private AntiForgeryOptions _antiForgeryOptions = new AntiForgeryOptions();
        private RazorViewEngineOptions _viewEngineOptions = new RazorViewEngineOptions();

        public MvcOptions()
        {
            ApplicationModelConventions = new List<IReflectedApplicationModelConvention>();
            ModelBinders = new List<ModelBinderDescriptor>();
            ViewEngines = new List<ViewEngineDescriptor>();
        }

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

        public List<ModelBinderDescriptor> ModelBinders { get; private set; }

        /// <summary>
        /// Gets a list of descriptors that represent <see cref="IViewEngine"/> used by this application.
        /// </summary>
        public List<ViewEngineDescriptor> ViewEngines { get; private set; }

        public List<IReflectedApplicationModelConvention> ApplicationModelConventions { get; private set; }
    }
}