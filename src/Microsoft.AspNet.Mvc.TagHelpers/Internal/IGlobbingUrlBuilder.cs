// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.TagHelpers.Internal
{
    // This is an interface to allow for easy substitution while testing LinkTagHelper
    public interface IGlobbingUrlBuilder
    {
        IEnumerable<string> BuildUrlList(string staticUrl, string includePattern, string excludePattern);
    }
}