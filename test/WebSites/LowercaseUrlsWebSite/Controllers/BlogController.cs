// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace LowercaseUrlsWebSite
{
    public class LowercaseUrls_BlogController : Controller
    {
        public string ShowPosts()
        {
            return Url.Action();
        }

        public string Edit(string postName)
        {
            return Url.Action();
        }

        public string GenerateLink()
        {
            return Url.Action("GetEmployee", "LowercaseUrls_Employee", new { Name = "MaryKae" , LastName = "McDonald"});
        }
    }
}