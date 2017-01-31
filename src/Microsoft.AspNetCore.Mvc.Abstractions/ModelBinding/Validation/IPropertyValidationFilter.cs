﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Validation
{
    /// <summary>
    /// Contract for attributes that determine whether associated properties should be validated. When the attribute is
    /// applied to a property, the validation system calls <see cref="ShouldValidateEntry"/> to determine whether to
    /// validate that property. When applied to a type, the validation system calls <see cref="ShouldValidateEntry"/>
    /// for each property that type defines to determine whether to validate it.
    /// </summary>
    public interface IPropertyValidationFilter
    {
        /// <summary>
        /// Gets an indication whether the <paramref name="entry"/> should be validated.
        /// </summary>
        /// <param name="entry"><see cref="ValidationEntry"/> to check.</param>
        /// <param name="parentEntry"><see cref="ValidationEntry"/> containing <paramref name="entry"/>.</param>
        /// <returns><c>true</c> if <paramref name="entry"/> should be validated; <c>false</c> otherwise.</returns>
        bool ShouldValidateEntry(ValidationEntry entry, ValidationEntry parentEntry);
    }
}