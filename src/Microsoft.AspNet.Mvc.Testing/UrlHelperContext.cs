// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Mvc.Testing
{
    public class UrlHelperContext
    {
        public Func<UrlHelperContext, string> OnAction
        {
            get;
            set;
        }

        public string Action
        {
            get;
            set;
        }

        public string Controller
        {
            get;
            set;
        }

        public string Fragment
        {
            get;
            set;
        }

        public string Host
        {
            get;
            set;
        }

        public string Protocol
        {
            get;
            set;
        }

        public object Values
        {
            get;
            set;
        }
    }
}