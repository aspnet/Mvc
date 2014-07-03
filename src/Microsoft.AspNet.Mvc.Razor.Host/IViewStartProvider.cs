// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Summary description for IViewStartProvider
    /// </summary>
    public interface IViewStartProvider
    {
        /// <summary>
        /// Determines if the file at the given path is a ViewStart file.
        /// </summary>
        /// <param name="viewPath">The path to locate ViewStart files for.</param>
        /// <returns>True if the file is a ViewStart file, false otherwise.</returns>
        bool IsViewStart(string viewPath);

        /// <summary>
        /// Given a view path, returns a sequence of ViewStart locations
        /// that are applicable to the specified view.
        /// </summary>
        /// <param name="viewPath">The path to to locate ViewStart files for.</param>
        /// <returns>A sequence of ViewStart locations.</returns>
	    IEnumerable<string> GetViewStartLocations(string viewPath);
    }
}