// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc
{
    public class ActionBindingContext
    {
        public ActionBindingContext(ActionContext context,
                                    IModelMetadataProvider metadataProvider,
                                    IModelBinder modelBinder,
                                    IReadOnlyList<IValueProvider> valueProviders,
                                    IInputFormatterSelector inputFormatterSelector,
                                    IModelValidatorProvider validatorProvider)
        {
            ActionContext = context;
            MetadataProvider = metadataProvider;
            ModelBinder = modelBinder;
            ValueProviders = valueProviders;
            InputFormatterSelector = inputFormatterSelector;
            ValidatorProvider = validatorProvider;
        }

        public ActionContext ActionContext { get; private set; }

        public IModelMetadataProvider MetadataProvider { get; private set; }

        public IModelBinder ModelBinder { get; private set; }

        public IReadOnlyList<IValueProvider> ValueProviders { get; private set; }

        public IInputFormatterSelector InputFormatterSelector { get; private set; }

        public IModelValidatorProvider ValidatorProvider { get; private set; }
    }
}
