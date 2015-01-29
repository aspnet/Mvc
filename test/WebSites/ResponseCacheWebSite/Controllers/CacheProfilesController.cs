// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace ResponseCacheWebSite.Controllers
{
    public class CacheProfilesController
    {
        [ResponseCache(CacheProfileName = "PublicCache30Sec")]
        public string PublicCache30Sec()
        {
            return "Hello World!";
        }

        [ResponseCache(CacheProfileName = "PrivateCache30Sec")]
        public string PrivateCache30Sec()
        {
            return "Hello World!";
        }

        [ResponseCache(CacheProfileName = "NoCache")]
        public string NoCache()
        {
            return "Hello World!";
        }

        [ResponseCache(CacheProfileName = "PublicCache30Sec", VaryByHeader = "Accept")]
        public string CacheProfileAddParameter()
        {
            return "Hello World!";
        }

        [ResponseCache(CacheProfileName = "PublicCache30Sec", Duration = 10)]
        public string CacheProfileOverride()
        {
            return "Hello World!";
        }
    }
}