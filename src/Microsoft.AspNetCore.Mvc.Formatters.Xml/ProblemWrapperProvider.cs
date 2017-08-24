// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Mvc.Formatters.Xml
{
    /// <summary>
    /// Wraps the object of type <see cref="Problem"/>.
    /// </summary>
    public class ProblemWrapperProvider : IWrapperProvider
    {
        /// <inheritdoc />
        public Type WrappingType => typeof(ProblemWrapper);

        /// <inheritdoc />
        public object Wrap(object original)
        {
            if (original == null)
            {
                throw new ArgumentNullException(nameof(original));
            }

            if (original is Problem problem)
            {
                return new ProblemWrapper(problem);
            }

            throw new ArgumentException(
                Resources.FormatWrapperProvider_MismatchType(
                    typeof(ProblemWrapper).Name,
                    original.GetType().Name),
                nameof(original));
        }
    }
}