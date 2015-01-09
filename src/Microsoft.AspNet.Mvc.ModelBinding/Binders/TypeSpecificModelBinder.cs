// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// Represents an <see cref="IModelBinder"/> that delegates to the specified
    /// <see cref="IModelBinder"/> instance for the specified type.
    /// </summary>
    public class TypeSpecificModelBinder : IModelBinder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeSpecificModelBinder"/>.
        /// </summary>
        /// <param name="modelType">The type of the model that applies to the instance.</param>
        /// <param name="modelBinder">The <see cref="IModelBinder"/> instance that binds the specified type.</param>
        public TypeSpecificModelBinder([NotNull] Type modelType,
                                       [NotNull] IModelBinder modelBinder)
        {
            ModelType = modelType;
            ModelBinder = modelBinder;
        }

        /// <summary>
        /// Gets the type of the model that applies to the instance.
        /// </summary>
        public Type ModelType { get; }

        /// <summary>
        /// Get the <see cref="IModelBinder"/> instance that binds the specified type.
        /// </summary>
        public IModelBinder ModelBinder { get; }

        /// <inheritdoc />
        public async Task<bool> BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType != ModelType)
            {
                return false;
            }
            await ModelBinder.BindModelAsync(bindingContext);
            return true;
        }
    }
}
