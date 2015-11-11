// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Formatters;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.ModelBinding.Validation;

namespace Microsoft.AspNet.Mvc
{
    public class ControllerContext : ActionContext
    {
        public ControllerContext()
            : base()
        {
        }

        public ControllerContext(ActionContext context)
            : base(context)
        {
        }

        public IList<IModelBinder> ModelBinders { get; set; }

        public IList<IValueProvider> ValueProviders { get; set; }

        public IList<IInputFormatter> InputFormatters { get; set; }

        public IList<IModelValidatorProvider> ValidatorProviders { get; set; }
    }
}
