// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Represents the result of compilation.
    /// </summary>
    public class CompilationResult
    {
        private Type _type;

        private CompilationResult()
        {
        }

        /// <summary>
        /// Gets the path of the file that was compiled.
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// Gets a sequence of <see cref="CompilationMessage"/> encountered during compilation.
        /// </summary>
        public IEnumerable<CompilationMessage> Messages { get; private set; }

        /// <summary>
        /// Gets additional information from compilation.
        /// </summary>
        /// <remarks>
        /// In the event of a compilation failure, values from additional info
        /// are copied to the <see cref="Exception.Data"/> property of the exception thrown.
        /// </remarks>
        public IDictionary<string, object> AdditionalInfo
        {
            get; private set;
        }

        /// <summary>
        /// Gets the content that was compiled.
        /// </summary>
        public string CompiledContent { get; private set; }

        /// <summary>
        /// Gets the type produced as a result of compilation.
        /// </summary>
        /// <exception cref="CompilationFailedException">An error occured during compilation.</exception>
        public Type CompiledType
        {
            get
            {
                if (_type == null)
                {
                    throw CreateCompilationFailedException();
                }

                return _type;
            }
        }

        /// <summary>
        /// Creates a <see cref="CompilationResult"/> that represents a failure in compilation.
        /// </summary>
        /// <param name="filePath">The path of the file that was compiled.</param>
        /// <param name="compiledContent">The content that was compiled.</param>
        /// <param name="messages">The sequence of failure messages encountered during compilation.</param>
        /// <param name="additionalInfo">Additional info about the compilation.</param>
        /// <returns>A CompilationResult instance representing a failure.</returns>
        public static CompilationResult Failed([NotNull] string filePath,
                                               [NotNull] string compiledContent,
                                               [NotNull] IEnumerable<CompilationMessage> messages,
                                               IDictionary<string, object> additionalInfo)
        {
            return new CompilationResult
            {
                CompiledContent = compiledContent,
                Messages = messages,
                FilePath = filePath,
                AdditionalInfo = additionalInfo
            };
        }

        /// <summary>
        /// Creates a <see cref="CompilationResult"/> that represents a success in compilation.
        /// </summary>
        /// <param name="type">The compiled type.</param>
        /// <returns>A CompilationResult instance representing a success.</returns>
        public static CompilationResult Successful([NotNull] Type type)
        {
            return new CompilationResult
            {
                _type = type
            };
        }

        private CompilationFailedException CreateCompilationFailedException()
        {
            var exception = new CompilationFailedException(FilePath, CompiledContent, Messages);
            if (AdditionalInfo != null)
            {
                foreach (var item in AdditionalInfo)
                {
                    exception.Data.Add(item.Key, item.Value);
                }
            }

            return exception;
        }
    }
}
