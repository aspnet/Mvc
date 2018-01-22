// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Internal;

namespace Microsoft.AspNetCore.Mvc.ModelBinding
{
    public class HeaderValueProvider : BindingSourceValueProvider, IEnumerableValueProvider
    {
        private readonly CultureInfo _culture;
        private readonly IHeaderDictionary _values;
        private PrefixContainer _prefixContainer;

        /// <summary>
        /// Creates a value provider for <see cref="IQueryCollection"/>.
        /// </summary>
        /// <param name="bindingSource">The <see cref="BindingSource"/> for the data.</param>
        /// <param name="headers">Current request's headers.</param>
        /// <param name="culture">The culture to return with ValueProviderResult instances.</param>
        public HeaderValueProvider(
            BindingSource bindingSource,
            IHeaderDictionary headers,
            CultureInfo culture)
            : base(bindingSource)
        {
            if (bindingSource == null)
            {
                throw new ArgumentNullException(nameof(bindingSource));
            }

            if (headers == null)
            {
                throw new ArgumentNullException(nameof(headers));
            }

            _values = headers;
            _culture = culture;
        }

        public CultureInfo Culture => _culture;

        protected PrefixContainer PrefixContainer
        {
            get
            {
                if (_prefixContainer == null)
                {
                    _prefixContainer = new PrefixContainer(_values.Keys);
                }

                return _prefixContainer;
            }
        }

        public override IValueProvider Filter(BindingSource bindingSource)
        {
            if (bindingSource != null && bindingSource == BindingSource.Header)
            {
                return this;
            }
            return null;
        }

        /// <inheritdoc />
        public override bool ContainsPrefix(string prefix)
        {
            return PrefixContainer.ContainsPrefix(prefix);
        }

        /// <inheritdoc />
        public virtual IDictionary<string, string> GetKeysFromPrefix(string prefix)
        {
            if (prefix == null)
            {
                throw new ArgumentNullException(nameof(prefix));
            }

            return PrefixContainer.GetKeysFromPrefix(prefix);
        }

        /// <inheritdoc />
        public override ValueProviderResult GetValue(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var values = _values[key];
            if (values.Count == 0)
            {
                return ValueProviderResult.None;
            }
            else
            {
                return new ValueProviderResult(values, _culture);
            }
        }
    }
}
