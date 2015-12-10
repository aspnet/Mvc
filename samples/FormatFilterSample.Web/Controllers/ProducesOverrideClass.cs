// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace FormatFilterSample.Web
{
    [Produces("application/custom_ProducesController")]
    [Route("[controller]/[action]")]
    public class ProducesOverrideController
    {
        [Produces("application/ProducesMethod")]
        public string ReturnClassName()
        {
            return "ProducesOverrideController";
        }
    }
}