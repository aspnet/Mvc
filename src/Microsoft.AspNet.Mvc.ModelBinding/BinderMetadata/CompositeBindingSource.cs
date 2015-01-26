// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// A <see cref="BindingSources"/> which can repesent multiple value-provider data sources.
    /// </summary>
    public class CompositeBindingSource : BindingSource
    {
        /// <summary>
        /// Creates a new <see cref="CompositeBindingSource"/>.
        /// </summary>
        /// <param name="bindingSources">
        /// The set of <see cref="BindingSource"/> entries.
        /// Must be value-provider sources and user input.
        /// </param>
        /// <param name="displayName">The display name for the composite source.</param>
        /// <returns>A <see cref="CompositeBindingSource"/>.</returns>
        public static CompositeBindingSource Create(
            [NotNull] IEnumerable<BindingSource> bindingSources,
            string displayName)
        {
            foreach (var bindingSource in bindingSources)
            {
                if (!bindingSource.IsValueProvider)
                {
                    var message = Resources.FormatBindingSource_MustBeValueProvider(
                        bindingSource.DisplayName,
                        nameof(CompositeBindingSource));
                    throw new ArgumentException(message, "bindingSources");
                }

                if (!bindingSource.IsUserInput)
                {
                    var message = Resources.FormatBindingSource_MustBeUserInput(
                        bindingSource.DisplayName,
                        nameof(CompositeBindingSource));
                    throw new ArgumentException(message, "bindingSources");
                }

                if (bindingSource is CompositeBindingSource)
                {
                    var message = Resources.FormatBindingSource_CannotBeComposite(
                        bindingSource.DisplayName,
                        nameof(CompositeBindingSource));
                    throw new ArgumentException(message, "bindingSources");
                }
            }

            var id = string.Join("&", bindingSources.Select(s => s.Id).OrderBy(s => s, StringComparer.Ordinal));
            return new CompositeBindingSource(id, displayName, bindingSources);
        }

        private CompositeBindingSource(
            [NotNull] string id, 
            string displayName, 
            [NotNull] IEnumerable<BindingSource> bindingSources)
            : base(id, displayName, isValueProvider: true, isUserInput: true)
        {
            BindingSources = bindingSources;
        }

        /// <summary>
        /// Gets the set of <see cref="BindingSource"/> entries.
        /// </summary>
        public IEnumerable<BindingSource> BindingSources { get; }

        /// <inheritdoc />
        public override bool CanAcceptDataFrom([NotNull] BindingSource source)
        {
            if (source is CompositeBindingSource)
            {
                throw new InvalidOperationException(Resources.FormatBindingSource_CannotBeComposite(
                    source.DisplayName,
                    nameof(CanAcceptDataFrom)));
            }

            foreach (var bindingSource in BindingSources)
            {
                if (bindingSource.CanAcceptDataFrom(source))
                {
                    return true;
                }
            }

            return false;
        }
    }
}