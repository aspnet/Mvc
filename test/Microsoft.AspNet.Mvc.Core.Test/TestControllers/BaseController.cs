// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc.Test.TestControllers
{
    public class BaseController : Mvc.Controller
    {
        public void GetFromBase() // Valid action method.
        {
        }

        [NonAction]
        public virtual void OverridenNonActionMethod()
        {
        }

        [NonAction]
        public virtual void NewMethod()
        {
        }

        public override RedirectResult Redirect(string url)
        {
            return base.Redirect(url + "#RedirectOverride");
        }
    }
}