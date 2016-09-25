// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Microsoft.AspNetCore.Mvc.Rendering
{
    /// <summary>
    /// Information from most recent <see cref="DefaultHtmlGenerator.CreateSanitizedId"/> call.
    /// </summary>
    public class LastSanitizedId
    {
        /// <summary>
        /// The original element name that was last sanitized i.e. the primary argument of the most recent
        /// <see cref="DefaultHtmlGenerator.CreateSanitizedId"/> call.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Valid HTML 4.01 "id" attribute for an element with the given <see cref="Name"/> i.e. the return value of
        /// the most recent <see cref="DefaultHtmlGenerator.CreateSanitizedId"/> call.
        /// </summary>
        public string Id { get; set; }
    }
}