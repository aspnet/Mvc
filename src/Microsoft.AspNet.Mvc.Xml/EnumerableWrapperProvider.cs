﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.AspNet.Mvc.Xml
{
    /// <summary>
    /// Provides a <see cref="IWrapperProvider"/> for interface types which implement 
    /// <see cref="IEnumerable{T}"/>.
    /// </summary>
    public class EnumerableWrapperProvider : IWrapperProvider
    {
        private readonly IWrapperProvider _wrapperProvider;
        private readonly ConstructorInfo _wrappingTypeConstructor;

        /// <summary>
        /// Initializes an instance of <see cref="EnumerableWrapperProvider"/>.
        /// </summary>
        /// <param name="sourceEnumerableOfT">Type of the original <see cref="IEnumerable{T}" /> 
        /// that is being wrapped.</param>
        /// <param name="elementWrapperProvider">The <see cref="IWrapperProvider"/> for the element type.
        /// Can be null.</param>
        public EnumerableWrapperProvider(
            [NotNull] Type sourceEnumerableOfT,
            IWrapperProvider elementWrapperProvider)
        {
            var enumerableOfT = sourceEnumerableOfT.ExtractGenericInterface(typeof(IEnumerable<>));
            if (!sourceEnumerableOfT.IsInterface() || enumerableOfT == null)
            {
                throw new ArgumentException(
                    Resources.FormatEnumerableWrapperProvider_InvalidSourceEnumerableOfT(typeof(IEnumerable<>).Name), 
                    nameof(sourceEnumerableOfT));
            }

            _wrapperProvider = elementWrapperProvider;

            var declaredElementType = enumerableOfT.GetGenericArguments()[0];
            var wrappedElementType = elementWrapperProvider?.WrappingType ?? declaredElementType;
            WrappingType = typeof(DelegatingEnumerable<,>).MakeGenericType(wrappedElementType, declaredElementType);

            _wrappingTypeConstructor = WrappingType.GetConstructor(new[]
            {
                sourceEnumerableOfT,
                typeof(IWrapperProvider)
            });
        }

        /// <inheritdoc />
        public Type WrappingType
        {
            get;
        }

        /// <inheritdoc />
        public object Wrap(object original)
        {
            if (original == null)
            {
                return null;
            }

            return _wrappingTypeConstructor.Invoke(new object[] { original, _wrapperProvider });
        }
    }
}