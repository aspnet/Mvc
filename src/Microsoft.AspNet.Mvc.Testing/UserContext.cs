// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc.Testing
{
    public class UserContext
    {
        // For Controller.User.Identity.IsAuthenticated
        public bool IsAuthenticated
        {
            get;
            set;
        }

        // For Controller.User.IsInRole
        public bool IsInRole
        {
            get;
            set;
        }

        // Corresponding to Controller.User.Identity.Name
        public string IdentityName
        {
            get;
            set;
        }
        
        // Corresponding to Controller.User.Identity.GetUserName
        public string IdentityUserName
        {
            get;
            set;
        }

        // Corresponding to Controller.User.Identity.GetUserId
        public string IdentityUserId
        {
            get;
            set;
        }
    }
}