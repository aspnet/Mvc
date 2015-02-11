﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;

namespace Microsoft.AspNet.PageExecutionInstrumentation
{
    /// <summary>
    /// Specifies the contracts for a HTTP feature that provides the context to instrument a web page.
    /// </summary>
    public interface IPageExecutionListenerFeature
    {
        /// <summary>
        /// Decorates the <see cref="TextWriter"/> used by web page instances to
        /// write the result to.
        /// </summary>
        /// <param name="writer">The output <see cref="TextWriter"/> for the web page.</param>
        /// <returns>A <see cref="TextWriter"/> that wraps <paramref name="writer"/>.</returns>
        /// <remarks>
        TextWriter DecorateWriter(TextWriter writer);

        /// <summary>
        /// Creates a <see cref="IPageExecutionContext"/> for the specified <paramref name="sourceFilePath"/>.
        /// </summary>
        /// <param name="sourceFilePath">The path of the page.</param>
        /// <param name="writer">The <see cref="TextWriter"/> obtained from <see cref="DecorateWriter(TextWriter)"/>.
        /// </param>
        /// <returns></returns>
        IPageExecutionContext GetContext(string sourceFilePath, TextWriter writer);
    }
}