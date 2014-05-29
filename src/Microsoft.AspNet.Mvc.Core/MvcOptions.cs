// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Mvc.Core;

namespace Microsoft.AspNet.Mvc
{
    public class MvcOptions
    {
        private AntiForgeryConfig _antiForgeryConfig = new AntiForgeryConfig();

        public virtual AntiForgeryConfig AntiForgeryConfig
        {
            get
            {
                return _antiForgeryConfig;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value", 
                                                    Resources.FormatPropertyOfTypeCannotBeNull("AntiForgeryConfig",
                                                                                               typeof(MvcOptions)));
                }

                _antiForgeryConfig = value;
            }
        }
    }
}