// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding.Validation
{
    /// <summary>
    /// Provides a collection of <see cref="IClientModelValidator"/>s.
    /// </summary>
    public interface IClientModelValidatorProvider
    {
        /// <summary>
        /// Gets set of <see cref="IClientModelValidator"/>s
        /// by updating <see cref="ClientValidatorProviderContext.Validators"/>.
        /// </summary>
        /// <param name="context">The <see cref="ClientModelValidationContext"/> associated with this call.</param>
        void GetValidators([NotNull] ClientValidatorProviderContext context);
    }
}
