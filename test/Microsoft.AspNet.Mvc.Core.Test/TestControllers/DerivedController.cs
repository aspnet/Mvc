// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc.Test.TestControllers
{
    public class DerivedController : BaseController
    {
        public void GetFromDerived() // Valid action method.
        {
        }

        [HttpGet]
        public override void OverridenNonActionMethod()
        {
        }

        public new void NewMethod() // Valid action method.
        {
        }

        public void GenericMethod<T>()
        {
        }

        private void PrivateMethod()
        {
        }

        public static void StaticMethod()
        {
        }

        protected static void ProtectedStaticMethod()
        {
        }

        private static void PrivateStaticMethod()
        {
        }
    }
}