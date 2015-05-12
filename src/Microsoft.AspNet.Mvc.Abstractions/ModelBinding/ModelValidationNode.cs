// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// Captures the validation information for a particular model.
    /// </summary>
    public class ModelValidationNode
    {
        /// <summary>
        /// Creates a new instance of <see cref="ModelValidationNode"/>.
        /// </summary>
        /// <param name="key">The key that will be used by the validation system to log <see cref="ModelState"/>
        /// entries.</param>
        /// <param name="modelMetadata">The <see cref="ModelMetadata"/> for the <paramref name="model"/>.</param>
        /// <param name="model">The model object which will be validated.</param>
        public ModelValidationNode([NotNull] string key, [NotNull] ModelMetadata modelMetadata, object model)
            : this (key, modelMetadata, model, new List<ModelValidationNode>())
        {
        }
        /// <summary>
        /// Creates a new instance of <see cref="ModelValidationNode"/>.
        /// </summary>
        /// <param name="key">The key that will be used by the validation system to add
        /// <see cref="ModelStateDictionary"/> entries.</param>
        /// <param name="modelMetadata">The <see cref="ModelMetadata"/> for the <paramref name="model"/>.</param>
        /// <param name="model">The model object which will be validated.</param>
        /// <param name="childNodes">A collection of child nodes.</param>
        public ModelValidationNode(
            [NotNull] string key,
            [NotNull] ModelMetadata modelMetadata,
            object model,
            [NotNull] IList<ModelValidationNode> childNodes)
        {
            Key = key;
            ModelMetadata = modelMetadata;
            ChildNodes = childNodes;
            Model = model;
        }

        /// <summary>
        /// Gets or sets the key used for adding <see cref="ModelStateDictionary"/> entries.
        /// </summary>
        public string Key { get; }

        public ModelMetadata ModelMetadata { get; }

        public object Model { get; }

        public IList<ModelValidationNode> ChildNodes { get; }

        public bool ValidateAllProperties { get; set; } = false;

        public bool SuppressValidation { get; set; } = false;
    }
}