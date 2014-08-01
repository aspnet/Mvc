// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.OptionDescriptors;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Extension methods for adding Input formatters to a collection.
    /// </summary>
    public static class InputFormatterDescriptorExtensions
    {
        /// <summary>
        /// Adds a type representing a <see cref="IInputFormatter"/> to a descriptor collection.
        /// </summary>
        /// <param name="descriptors">A list of InputFormatterDescriptors</param>
        /// <param name="InputFormatterType">Type representing an <see cref="IInputFormatter"/>.</param>
        /// <returns>InputFormatterDescriptor representing the added instance.</returns>
        public static InputFormatterDescriptor Add([NotNull] this IList<InputFormatterDescriptor> descriptors,
                                                   [NotNull] Type InputFormatterType)
        {
            var descriptor = new InputFormatterDescriptor(InputFormatterType);
            descriptors.Add(descriptor);
            return descriptor;
        }

        /// <summary>
        /// Inserts a type representing a <see cref="IInputFormatter"/> to a descriptor collection.
        /// </summary>
        /// <param name="descriptors">A list of InputFormatterDescriptors</param>
        /// <param name="InputFormatterType">Type representing an <see cref="IInputFormatter"/>.</param>
        /// <returns>InputFormatterDescriptor representing the inserted instance.</returns>
        public static InputFormatterDescriptor Insert([NotNull] this IList<InputFormatterDescriptor> descriptors,
                                                      int index,
                                                      [NotNull] Type InputFormatterType)
        {
            if (index < 0 || index > descriptors.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            var descriptor = new InputFormatterDescriptor(InputFormatterType);
            descriptors.Insert(index, descriptor);
            return descriptor;
        }

        /// <summary>
        /// Adds an <see cref="IInputFormatter"/> to a descriptor collection.
        /// </summary>
        /// <param name="descriptors">A list of InputFormatterDescriptors</param>
        /// <param name="InputFormatter">An <see cref="IInputFormatter"/> instance.</param>
        /// <returns>InputFormatterDescriptor representing the added instance.</returns>
        public static InputFormatterDescriptor Add([NotNull] this IList<InputFormatterDescriptor> descriptors,
                                                   [NotNull] IInputFormatter InputFormatter)
        {
            var descriptor = new InputFormatterDescriptor(InputFormatter);
            descriptors.Add(descriptor);
            return descriptor;
        }

        /// <summary>
        /// Insert an <see cref="IInputFormatter"/> to a descriptor collection.
        /// </summary>
        /// <param name="descriptors">A list of InputFormatterDescriptors</param>
        /// <param name="InputFormatter">An <see cref="IInputFormatter"/> instance.</param>
        /// <returns>InputFormatterDescriptor representing the added instance.</returns>
        public static InputFormatterDescriptor Insert([NotNull] this IList<InputFormatterDescriptor> descriptors,
                                                      int index,
                                                      [NotNull] IInputFormatter InputFormatter)
        {
            if (index < 0 || index > descriptors.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            var descriptor = new InputFormatterDescriptor(InputFormatter);
            descriptors.Insert(index, descriptor);
            return descriptor;
        }
    }
}