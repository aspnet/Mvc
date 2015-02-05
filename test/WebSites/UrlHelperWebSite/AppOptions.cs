﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace UrlHelperWebSite
{
    public class AppOptions
    {
        public bool ServeCDNContent { get; set; }

        public string CDNServerBaseUrl { get; set; }

        public bool GenerateLowercaseUrls { get; set; }
    }
}