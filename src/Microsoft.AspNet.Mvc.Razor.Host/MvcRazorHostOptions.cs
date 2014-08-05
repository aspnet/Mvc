// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Represents configuration options for the Razor Host
    /// </summary>
    public class MvcRazorHostOptions
    {
        public MvcRazorHostOptions()
        {
            ActivateAttributeName = "Microsoft.AspNet.Mvc.ActivateAttribute";
        }

        /// <summary>
        /// Gets or sets the attribue that is used to decorate properties that are injected and need to
        /// be activated.
        /// </summary>
        public string ActivateAttributeName { get; set; }
    }
}