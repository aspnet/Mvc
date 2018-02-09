// Copyright (c) .NET  Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace Microsoft.AspNetCore.Mvc.Testing
{
    /// <summary>
    /// Metadata that <see cref="WebApplicationTestFixture{TStartup}"/> uses to find out the content
    /// root for the web application represented by <c>TStartup</c>.
    /// <see cref="WebApplicationTestFixture{TStartup}"/> will iterate over all the instances of
    /// <see cref="WebApplicationFixtureContentRootAttribute"/>, filter the instances whose
    /// <see cref="Key"/> is equal to <c>TStartup</c> <see cref="Assembly.FullName"/>,
    /// order them by <see cref="Priority"/> in ascending order.
    /// <see cref="WebApplicationTestFixture{TStartup}"/> will check for the existence of the marker
    /// in <code>Path.Combine(<see cref="ContentRootPath"/>, Path.GetFileName(<see cref="ContentRootTest"/>))"</code>
    /// and if the file exists it will set the content root to <see cref="ContentRootPath"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = true)]
    public sealed class WebApplicationFixtureContentRootAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="WebApplicationFixtureContentRootAttribute"/>.
        /// </summary>
        /// <param name="key">
        /// The key of this <see cref="WebApplicationFixtureContentRootAttribute"/>. This
        /// key is used by <see cref="WebApplicationTestFixture{TStartup}"/> to determine what of the
        /// <see cref="WebApplicationFixtureContentRootAttribute"/> instances on the test assembly should be used
        /// to match a given Startup class.
        /// </param>
        /// <param name="contentRootPath">The path to the content root. This path can be either relative or absolute.
        /// In case the path is relative, the path will be combined with
        /// <code><see cref="Directory.GetCurrentDirectory()"/></code></param>
        /// <param name="contentRootTest">
        /// A file that will be use as a marker to determine that the content root path for the given context is correct.
        /// </param>
        /// <param name="priority">
        /// The priority of this content root attribute compared to other attributes. When
        /// multiple <see cref="WebApplicationFixtureContentRootAttribute"/> instances are applied for the
        /// same key, they are processed with <see cref="int.Parse(string)"/>, ordered in ascending order and applied
        /// in priority until a match is found.
        /// </param>
        public WebApplicationFixtureContentRootAttribute(
            string key,
            string contentRootPath,
            string contentRootTest,
            string priority)
        {
            Key = key;
            ContentRootPath = contentRootPath;
            ContentRootTest = contentRootTest;
            if (int.TryParse(priority, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedPriority))
            {
                Priority = parsedPriority;
            }
        }

        /// <summary>
        /// The key for the content root associated with this project. Typically <see cref="Assembly.FullName"/>.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// The content root path for a given project. This content root can be relative or absolute. If its a
        /// relative path, it will be combined with <see cref="AppContext.BaseDirectory"/>.
        /// </summary>
        public string ContentRootPath { get; }

        /// <summary>
        /// A marker file used to ensure that the path the content root is being set to is correct.
        /// </summary>
        public string ContentRootTest { get; }

        /// <summary>
        /// A number for determining the probing order when multiple <see cref="WebApplicationFixtureContentRootAttribute"/>
        /// instances with the same key are present on the test <see cref="Assembly"/>.
        /// </summary>
        public int Priority { get; }
    }
}
