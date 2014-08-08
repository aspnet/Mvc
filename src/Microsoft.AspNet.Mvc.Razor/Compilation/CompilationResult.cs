// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNet.FileSystems;

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
        /// Gets the path of the Razor file that was compiled.
        /// </summary>
        public string FilePath
        {
            get { return File?.PhysicalPath ?? null; }
        }

        /// <summary>
        /// Gets a sequence of <see cref="CompilationMessage"/> instances encountered during compilation.
        /// </summary>
        public IEnumerable<CompilationMessage> Messages { get; private set; }

        /// <summary>
        /// Gets additional information from compilation.
        /// </summary>
        /// <remarks>
        /// In the event of a compilation failure, values from this dictionary are copied to the
        /// <see cref="Exception.Data"/> property of the <see cref="Exception"/> thrown.
        /// </remarks>
        public IDictionary<string, object> AdditionalInfo
        {
            get; private set;
        }

        /// <summary>
        /// Gets the generated C# content that was compiled.
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

        private IFileInfo File { get; set; }

        /// <summary>
        /// Creates a <see cref="CompilationResult"/> that represents a failure in compilation.
        /// </summary>
        /// <param name="fileInfo">The <see cref="IFileInfo"/> for the Razor file that was compiled.</param>
        /// <param name="compilationContent">The generated C# contents to be compiled.</param>
        /// <param name="messages">The sequence of failure messages encountered during compilation.</param>
        /// <param name="additionalInfo">Additional info about the compilation.</param>
        /// <returns>A CompilationResult instance representing a failure.</returns>
        public static CompilationResult Failed([NotNull] IFileInfo file,
                                               [NotNull] string compilationContent,
                                               [NotNull] IEnumerable<CompilationMessage> messages,
                                               IDictionary<string, object> additionalInfo)
        {
            return new CompilationResult
            {
                File = file,
                CompiledContent = compilationContent,
                Messages = messages,
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
            var fileContent = ReadContent(File);
            var exception = new CompilationFailedException(FilePath, fileContent, CompiledContent, Messages);
            if (AdditionalInfo != null)
            {
                foreach (var item in AdditionalInfo)
                {
                    exception.Data.Add(item.Key, item.Value);
                }
            }

            return exception;
        }

        private static string ReadContent(IFileInfo file)
        {
            try
            {
                using (var stream = file.CreateReadStream())
                {
                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (IOException)
            {
                // Don't throw if reading the file fails.
                return string.Empty;
            }
        }
    }
}
