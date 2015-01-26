// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// A metadata object representing a source of data for model binding.
    /// </summary>
    [DebuggerDisplay("Source: {DisplayName}")]
    public class BindingSource : IEquatable<BindingSource>
    {
        /// <summary>
        /// A <see cref="BindingSource"/> for the request body.
        /// </summary>
        public static readonly BindingSource Body = new BindingSource(
            "Body",
            Resources.BindingSource_Body,
            isValueProvider: false,
            isUserInput: true);

        /// <summary>
        /// A <see cref="ApiParameterSource"/> for a custom model binder (unknown data source).
        /// </summary>
        public static readonly BindingSource Custom = new BindingSource(
            "Custom",
            Resources.BindingSource_Custom,
            isValueProvider: false,
            isUserInput: true);

        /// <summary>
        /// A <see cref="ApiParameterSource"/> for the request form-data.
        /// </summary>
        public static readonly BindingSource Form = new BindingSource(
            "Form",
            Resources.BindingSource_Form,
            isValueProvider: true,
            isUserInput: true);

        /// <summary>
        /// A <see cref="BindingSource"/> for the request headers.
        /// </summary>
        public static readonly BindingSource Header = new BindingSource(
            "Header",
            Resources.BindingSource_Header,
            isValueProvider: false,
            isUserInput: true);

        /// <summary>
        /// A <see cref="BindingSource"/> for model binding. Includes form-data, query-string
        /// and route data from the request.
        /// </summary>
        public static readonly BindingSource ModelBinding = new BindingSource(
            "ModelBinding",
            Resources.BindingSource_ModelBinding,
            isValueProvider: true,
            isUserInput: true);

        /// <summary>
        /// An <see cref="ApiParameterSource"/> for the request url path.
        /// </summary>
        public static readonly BindingSource Path = new BindingSource(
            "Path",
            Resources.BindingSource_Path,
            isValueProvider: true,
            isUserInput: true);

        /// <summary>
        /// An <see cref="ApiParameterSource"/> for the request query-string.
        /// </summary>
        public static readonly BindingSource Query = new BindingSource(
            "Query",
            Resources.BindingSource_Query,
            isValueProvider: true,
            isUserInput: true);

        /// <summary>
        /// A <see cref="BindingSource"/> for request services.
        /// </summary>
        public static readonly BindingSource Services = new BindingSource(
            "Services",
            Resources.BindingSource_Services,
            isValueProvider: false,
            isUserInput: false);

        public BindingSource([NotNull] string id, string displayName, bool isValueProvider, bool isUserInput)
        {
            Id = id;
            DisplayName = displayName;
            IsValueProvider = isValueProvider;
            IsUserInput = isUserInput;
        }

        public string DisplayName { get; }

        /// <summary>
        /// Gets an 
        /// </summary>
        public string Id { get; }

        public bool IsValueProvider { get; }

        /// <summary>
        /// Gets a value indicating whether or not the binding source contains user input.
        /// </summary>
        public bool IsUserInput { get; }

        /// <summary>
        /// Gets a value indicating whether or not the <see cref="BindingSource"/> can accept
        /// data from <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The <see cref="BindingSource"/> to consider as input.</param>
        /// <returns><c>True</c> if the source is compatible, otherwise <c>false</c>.</returns>
        /// <remarks>
        /// When using this method, it is expected that the left-hand-side is metadata specified
        /// on a property or parameter for model binding, and the right hand side is a source of
        /// data used by a model binder or value provider.
        /// 
        /// This distinction is important as the left-hand-side may be a composite, but the right
        /// may not.
        /// </remarks>
        public virtual bool CanAcceptDataFrom([NotNull] BindingSource source)
        {
            if (source is CompositeBindingSource)
            {
                throw new InvalidOperationException(Resources.FormatBindingSource_CannotBeComposite(
                    source.DisplayName,
                    nameof(CanAcceptDataFrom)));
            }

            return this == source;
        }

        /// <inheritdoc />
        public bool Equals(BindingSource other)
        {
            return other == null ? false : string.Equals(other.Id, Id, StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as BindingSource);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <inheritdoc />
        public static bool operator ==(BindingSource s1, BindingSource s2)
        {
            if (object.ReferenceEquals(s1, null))
            {
                return object.ReferenceEquals(s2, null); ;
            }

            return s1.Equals(s2);
        }

        /// <inheritdoc />
        public static bool operator !=(BindingSource s1, BindingSource s2)
        {
            return !(s1 == s2);
        }
    }
}