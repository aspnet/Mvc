﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// Extensions methods for <see cref="ValueProviderResult"/>.
    /// </summary>
    public static class ValueProviderResultExtensions
    {
        /// <summary>
        /// Attempts to convert the values in <paramref name="result"/> to the specified type.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> for conversion.</typeparam>
        /// <param name="result">The <see cref="ValueProviderResult"/>.</param>
        /// <returns>
        /// The converted value, or the default value of <typeparamref name="T"/> if the value could not be converted.
        /// </returns>
        public static T ConvertTo<T>(this ValueProviderResult result)
        {
            var valueToConvert = (object)result.Values ?? (object)result.Value;
            return ModelBindingHelper.ConvertTo<T>(valueToConvert, result.Culture);
        }

        /// <summary>
        /// Attempts to convert the values in <paramref name="result"/> to the specified type.
        /// </summary>
        /// <param name="result">The <see cref="ValueProviderResult"/>.</param>
        /// <param name="type">The <see cref="Type"/> for conversion.</param>
        /// <returns>
        /// The converted value, or the default value of <paramref name="type"/> if the value could not be converted.
        /// </returns>
        public static object ConvertTo(this ValueProviderResult result, [NotNull] Type type)
        {
            var valueToConvert = (object)result.Values ?? (object)result.Value;
            return ModelBindingHelper.ConvertTo(valueToConvert, type, result.Culture);
        }
    }
}
