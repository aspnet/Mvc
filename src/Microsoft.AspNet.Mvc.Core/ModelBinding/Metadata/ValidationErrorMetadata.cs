// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Mvc.ModelBinding.Metadata
{
    public class ValidationErrorMetadata
    {
        /// <summary>
        /// Error message the model binding system adds when a property with an associated
        /// <see cref="BindRequiredAttribute"/> is not bound.
        /// </summary>
        /// <value>Default <see cref="string"/> is "A value for the '{0}' property was not provided.".</value>
        public Func<string> MissingBindRequiredValueResource { get; set; }

        /// <summary>
        /// Error message the model binding system adds when either the key or the value of a
        /// <see cref="System.Collections.Generic.KeyValuePair{TKey, TValue}"/> is bound but not both.
        /// </summary>
        /// <value>Default <see cref="string"/> is "A value is required.".</value>
        public Func<string> MissingKeyOrValueResource { get; set; }

        /// <summary>
        /// Error message the model binding system adds when a <c>null</c> value is bound to a
        /// non-<see cref="Nullable"/> property.
        /// </summary>
        /// <value>Default <see cref="string"/> is "The value '{0}' is invalid.".</value>
        public Func<string> ValueInvalid_MustNotBeNullResource { get; set; }
    }
}
