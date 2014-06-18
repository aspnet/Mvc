// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.ReflectedModelBuilder;

namespace Microsoft.AspNet.Mvc
{
    public class MvcOptions
    {
        private readonly ModelBinderDescriptorCollection _binderDescriptors = new ModelBinderDescriptorCollection
        {
            new ModelBinderDescriptor(new TypeConverterModelBinder()),
            new ModelBinderDescriptor(new TypeMatchModelBinder()),
            new ModelBinderDescriptor(typeof(GenericModelBinder)),
            new ModelBinderDescriptor(new MutableObjectModelBinder()),
            new ModelBinderDescriptor(new ComplexModelDtoModelBinder()),
        };

        private AntiForgeryOptions _antiForgeryOptions = new AntiForgeryOptions();

        public MvcOptions()
        {
            ApplicationModelConventions = new List<IReflectedApplicationModelConvention>();
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

        public ModelBinderDescriptorCollection ModelBinders
        {
            get { return _binderDescriptors; }
        }

        public List<IReflectedApplicationModelConvention> ApplicationModelConventions { get; private set; }
    }
}