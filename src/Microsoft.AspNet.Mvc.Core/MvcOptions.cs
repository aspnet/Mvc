// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc
{
    public class MvcOptions
    {
        private IAntiForgeryConfig _antiForgeryConfig = new AntiForgeryConfig();

        public virtual IAntiForgeryConfig AntiForgeryConfig
        {
            get
            {
                return _antiForgeryConfig;
            }

            set
            {
                if (value != null)
                {
                    _antiForgeryConfig = value;
                }
            }
        }
    }
}