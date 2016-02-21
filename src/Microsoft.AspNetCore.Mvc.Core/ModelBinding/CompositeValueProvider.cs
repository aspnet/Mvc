// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Microsoft.AspNetCore.Mvc.ModelBinding
{
    /// <summary>
    /// Represents a <see cref="IValueProvider"/> whose values come from a collection of <see cref="IValueProvider"/>s.
    /// </summary>
    public class CompositeValueProvider :
        Collection<IValueProvider>,
        IEnumerableValueProvider,
        IBindingSourceValueProvider
    {
        /// <summary>
        /// Initializes a new instance of <see cref="CompositeValueProvider"/>.
        /// </summary>
        public CompositeValueProvider()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CompositeValueProvider"/>.
        /// </summary>
        /// <param name="valueProviders">The sequence of <see cref="IValueProvider"/> to add to this instance of
        /// <see cref="CompositeValueProvider"/>.</param>
        public CompositeValueProvider(IList<IValueProvider> valueProviders)
            : base(valueProviders)
        {
        }

        /// <inheritdoc />
        public virtual bool ContainsPrefix(string prefix)
        {
            for (var i = 0; i < Count; i++)
            {
                if (this[i].ContainsPrefix(prefix))
                {
                    return true;
                }
            }
            return false;
        }

        /// <inheritdoc />
        public virtual ValueProviderResult GetValue(string key)
        {
            // Performance-sensitive
            // Caching the count is faster for IList<T>
            var itemCount = Items.Count;
            for (var i = 0; i < itemCount; i++)
            {
                var valueProvider = Items[i];
                var result = valueProvider.GetValue(key);
                if (result != ValueProviderResult.None)
                {
                    return result;
                }
            }

            return ValueProviderResult.None;
        }

        /// <inheritdoc />
        public virtual IDictionary<string, string> GetKeysFromPrefix(string prefix)
        {
            foreach (var valueProvider in this)
            {
                var enumeratedProvider = valueProvider as IEnumerableValueProvider;
                if (enumeratedProvider != null)
                {
                    var result = enumeratedProvider.GetKeysFromPrefix(prefix);
                    if (result != null && result.Count > 0)
                    {
                        return result;
                    }
                }
            }
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        protected override void InsertItem(int index, IValueProvider item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            base.InsertItem(index, item);
        }

        /// <inheritdoc />
        protected override void SetItem(int index, IValueProvider item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            base.SetItem(index, item);
        }

        /// <inheritdoc />
        public IValueProvider Filter(BindingSource bindingSource)
        {
            if (bindingSource == null)
            {
                throw new ArgumentNullException(nameof(bindingSource));
            }

            var filteredValueProviders = new List<IValueProvider>();
            foreach (var valueProvider in this.OfType<IBindingSourceValueProvider>())
            {
                var result = valueProvider.Filter(bindingSource);
                if (result != null)
                {
                    filteredValueProviders.Add(result);
                }
            }

            if (filteredValueProviders.Count == 0)
            {
                // Do not create an empty CompositeValueProvider.
                return null;
            }

            if (filteredValueProviders.Count == Count)
            {
                // No need for a new CompositeValueProvider.
                return this;
            }

            return new CompositeValueProvider(filteredValueProviders);
        }
    }
}
