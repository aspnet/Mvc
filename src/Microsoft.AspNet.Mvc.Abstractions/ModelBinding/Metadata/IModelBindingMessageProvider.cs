// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Mvc.ModelBinding.Metadata
{
    /// <summary>
    /// Provider for error messages the model binding system detects.
    /// </summary>
    public interface IModelBindingMessageProvider
    {
        /// <summary>
        /// Error message the model binding system adds when a property with an associated
        /// <c>BindRequiredAttribute</c> is not bound.
        /// </summary>
        /// <value>Default <see cref="string"/> is "A value for the '{0}' property was not provided.".</value>
        Func<string, string> MissingBindRequiredValueAccessor { get; }

        /// <summary>
        /// Error message the model binding system adds when either the key or the value of a
        /// <see cref="System.Collections.Generic.KeyValuePair{TKey, TValue}"/> is bound but not both.
        /// </summary>
        /// <value>Default <see cref="string"/> is "A value is required.".</value>
        Func<string> MissingKeyOrValueAccessor { get; }

        /// <summary>
        /// Error message the model binding system adds when a <c>null</c> value is bound to a
        /// non-<see cref="Nullable"/> property.
        /// </summary>
        /// <value>Default <see cref="string"/> is "The value '{0}' is invalid.".</value>
        Func<string, string> ValueMustNotBeNullAccessor { get; }

        /// <summary>
        /// Fallback error message HTML and tag helpers display when a property is invalid but the
        /// <see cref="ModelError"/>s have <c>null</c> <see cref="ModelError.ErrorMessage"/>s.
        /// </summary>
        /// <value>Default <see cref="string"/> is "The value '{0}' is invalid.".</value>
        Func<string, string> ValueInvalid_UnknownErrorResource { get; }

        /// <summary>
        /// Replacement <see cref="ModelError.ErrorMessage"/> used in <see cref="ModelError"/> when
        /// <see cref="ModelError.Exception"/> is of type <see cref="FormatException"/> and value is known i.e. when
        /// the bound value could not be converted and the "replacer" has the attempted value.
        /// </summary>
        /// <value>Default <see cref="string"/> is "The value '{0}' is not valid for {1}.".</value>
        Func<string, string, string> ValueInvalid_WithValueResource { get; }

        /// <summary>
        /// Replacement <see cref="ModelError.ErrorMessage"/> used in <see cref="ModelError"/> when
        /// <see cref="ModelError.Exception"/> is of type <see cref="FormatException"/> and value is unknown i.e. when
        /// the bound value could not be converted and the "replacer" does not have the attempted value.
        /// </summary>
        /// <value>Default <see cref="string"/> is "The supplied value is invalid for {0}.".</value>
        Func<string, string> ValueInvalid_WithoutValueResource { get; }

        /// <summary>
        /// Fallback error message used when deserialization of request body fails due to no supported encodings
        /// found on the input formatter.
        /// </summary>
        /// <value>
        /// Default <see cref="string"/> is "No encoding found for input formatter '{0}'. There must be at
        /// least one supported encoding registered in order for the formatter to read content.".
        /// </value>
        Func<string, string> NoEncodingFoundOnInputFormatter { get; }

        /// <summary>
        /// Fallback error message used when no input formatter was found based on the request's content type.
        /// </summary>
        /// /// <value>
        /// Default <see cref="string"/> is "Unsupported content type '{0}'.".
        /// </value>
        Func<string, string> UnsupportedContentType { get; }
    }
}
