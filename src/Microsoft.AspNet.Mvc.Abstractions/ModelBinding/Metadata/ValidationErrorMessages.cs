// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Mvc.ModelBinding.Metadata
{
    public abstract class ValidationErrorMessages
    {
        /// <summary>
        /// Error message the model binding system adds when a property with an associated
        /// <see cref="BindRequiredAttribute"/> is not bound.
        /// </summary>
        /// <value>Default <see cref="string"/> is "A value for the '{0}' property was not provided.".</value>
        public abstract Func<string> MissingBindRequiredValueResource { get; }

        /// <summary>
        /// Error message the model binding system adds when either the key or the value of a
        /// <see cref="System.Collections.Generic.KeyValuePair{TKey, TValue}"/> is bound but not both.
        /// </summary>
        /// <value>Default <see cref="string"/> is "A value is required.".</value>
        public abstract Func<string> MissingKeyOrValueResource { get; }

        // See use of Resources.Common_ValueNotValidForProperty in ValidationHelpers.
        // <summary>
        // Fallback error message HTML and tag helpers display when a property is invalid but the
        // <see cref="ModelError"/>s have <c>null</c> <see cref="ModelError.ErrorMessage"/>s.
        // </summary>
        // <value>Default <see cref="string"/> is "The value '{0}' is invalid.".</value>
        ////public abstract Func<string> ValueInvalid_UnknownErrorResource { get; }

        // See how Resources.ModelError_InvalidValue_MessageWithModelValue is used
        // <summary>
        // Replacement <see cref="ModelError.ErrorMessage"/> used in <see cref="ModelError"/> when
        // <see cref="ModelError.Exception"/> is of type <see cref="FormatException"/> and value is known i.e. when
        // the bound value could not be converted and the "replacer" has the attempted value.
        // </summary>
        // <value>Default <see cref="string"/> is "The value '{0}' is not valid for {1}.".</value>
        ////public abstract Func<string> ValueInvalid_WithValueResource { get; }

        // See how Resources.ModelError_InvalidValue_GenericMessage is used.
        // <summary>
        // Replacement <see cref="ModelError.ErrorMessage"/> used in <see cref="ModelError"/> when
        // <see cref="ModelError.Exception"/> is of type <see cref="FormatException"/> and value is unknown i.e. when
        // the bound value could not be converted and the "replacer" does not have the attempted value.
        // </summary>
        // <value>Default <see cref="string"/> is "The supplied value is invalid for {0}.".</value>
        ////public abstract Func<string> ValueInvalid_WithoutValueResource { get; }

        /// <summary>
        /// Error message the model binding system adds when a <c>null</c> value is bound to a
        /// non-<see cref="Nullable"/> property.
        /// </summary>
        /// <value>Default <see cref="string"/> is "The value '{0}' is invalid.".</value>
        public abstract Func<string> ValueInvalid_MustNotBeNullResource { get; }
    }
}
