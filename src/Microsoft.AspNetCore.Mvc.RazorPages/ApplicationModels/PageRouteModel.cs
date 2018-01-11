// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    /// <summary>
    /// A model component for routing RazorPages.
    /// </summary>
    public class PageRouteModel
    {
        /// <summary>
        /// Initializes a new instance of <see cref="PageRouteModel"/>.
        /// </summary>
        /// <param name="relativePath">The application relative path of the page.</param>
        /// <param name="viewEnginePath">The path relative to the base path for page discovery.</param>
        public PageRouteModel(string relativePath, string viewEnginePath)
            : this(relativePath, viewEnginePath, viewEnginePath)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PageRouteModel"/>.
        /// </summary>
        /// <param name="relativePath">The application relative path of the page.</param>
        /// <param name="viewEnginePath">The path relative to the base path for page discovery.</param>
        /// <param name="pageName">The page name.</param>
        public PageRouteModel(string relativePath, string viewEnginePath, string pageName)
        {
            RelativePath = relativePath ?? throw new ArgumentNullException(nameof(relativePath));
            ViewEnginePath = viewEnginePath ?? throw new ArgumentNullException(nameof(viewEnginePath));
            PageName = pageName;

            Properties = new Dictionary<object, object>();
            Selectors = new List<SelectorModel>();
            RouteValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// A copy constructor for <see cref="PageRouteModel"/>.
        /// </summary>
        /// <param name="other">The <see cref="PageRouteModel"/> to copy from.</param>
        public PageRouteModel(PageRouteModel other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            RelativePath = other.RelativePath;
            ViewEnginePath = other.ViewEnginePath;

            Properties = new Dictionary<object, object>(other.Properties);
            Selectors = new List<SelectorModel>(other.Selectors.Select(m => new SelectorModel(m)));
            RouteValues = new Dictionary<string, string>(other.RouteValues, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the application root relative path for the page.
        /// </summary>
        public string RelativePath { get; }

        /// <summary>
        /// Gets the path relative to the base path for page discovery.
        /// </summary>
        /// <remarks>
        /// For area pages, this path is calculated relative to the <see cref="RazorPagesOptions.RootDirectory"/> of the specific area.
        /// </remarks>
        public string ViewEnginePath { get; }

        /// <summary>
        /// Gets the page name. This value is an implicit route value in <see cref="RouteValues"/> corresponding
        /// to the key <c>page</c>.
        /// <para>
        /// In most common cases, <see cref="PageName"/> and <see cref="ViewEnginePath"/> have the same values.
        /// When <see cref="IsFallbackRoute"/> the two values diverge - <see cref="ViewEnginePath"/> includes the fallback directory name,
        /// while <see cref="PageName"/> has the same value as the superseding route.
        /// </para>
        /// </summary>
        public string PageName { get; }

        /// <summary>
        /// Stores arbitrary metadata properties associated with the <see cref="PageRouteModel"/>.
        /// </summary>
        public IDictionary<object, object> Properties { get; }

        /// <summary>
        /// Gets the <see cref="SelectorModel"/> instances.
        /// </summary>
        public IList<SelectorModel> Selectors { get; }

        /// <summary>
        /// Gets a collection of route values that must be present in the <see cref="RouteData.Values"/> 
        /// for the corresponding page to be selected.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The value of <see cref="PageName"/> is considered an implicit route value corresponding
        /// to the key <c>page</c>. These entries will be implicitly added to <see cref="ActionDescriptor.RouteValues"/>
        /// when the action descriptor is created, but will not be visible in <see cref="RouteValues"/>.
        /// </para>
        /// </remarks>
        public IDictionary<string, string> RouteValues { get; }

        /// <summary>
        /// Gets or sets a value that determines if this <see cref="PageRouteModel"/> is a fallback route.
        /// <para>
        /// A <see cref="PageRouteModel"/> is considered fallback route, if it is located in a directory named "Shared".
        /// In this case,
        /// <list type="bullet">
        /// <item>the fallback directory name is trimmed from the route i.e for a file "/Pages/Fruits/Shared/List.cshtml",
        /// the calculated route is "/Pages/Fruits/List"</item>
        /// <item>a matching file name immediately outside the fallback directory, supersedes the fallback route i.e.
        /// "/Pages/Fruits/List.cshtml" would supersede the fallback.</item>
        /// </list>
        /// </para>
        /// </summary>
        public bool IsFallbackRoute { get; set; }
    }
}
