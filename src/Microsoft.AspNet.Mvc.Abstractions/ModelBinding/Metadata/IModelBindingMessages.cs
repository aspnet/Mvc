// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Mvc.ModelBinding.Metadata
{
    /// <summary>
    /// Messages for errors the model binding system detects.
    /// </summary>
    public interface IModelBindingMessages
    {
        /// <summary>
        /// Error message the model binding system adds when a property with an associated
        /// <c>BindRequiredAttribute</c> is not bound.
        /// </summary>
        /// <value>Default <see cref="string"/> is "A value for the '{0}' property was not provided.".</value>
        Func<object, string> MissingBindRequiredValueResource { get; }

        /// <summary>
        /// Error message the model binding system adds when either the key or the value of a
        /// <see cref="System.Collections.Generic.KeyValuePair{TKey, TValue}"/> is bound but not both.
        /// </summary>
        /// <value>Default <see cref="string"/> is "A value is required for '{0}'.".</value>
        Func<object, string> MissingKeyOrValueResource { get; }

        /// <summary>
        /// Error message the model binding system adds when a <c>null</c> value is bound to a
        /// non-<see cref="Nullable"/> property.
        /// </summary>
        /// <value>Default <see cref="string"/> is "A null value is invalid for '{0}'.".</value>
        Func<object, string> ValueInvalid_MustNotBeNullResource { get; }
    }
}
