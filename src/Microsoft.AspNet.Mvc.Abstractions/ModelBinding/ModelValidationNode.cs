// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class ModelValidationNode
    {
        public ModelValidationNode([NotNull] string key, [NotNull] ModelMetadata modelMetadata, object model)
            : this (key, modelMetadata, model, new List<ModelValidationNode>())
        {
        }

        public ModelValidationNode([NotNull] string key, ModelMetadata modelMetadata, object model, [NotNull] IList<ModelValidationNode> childNodes)
        {
            Key = key;
            ModelMetadata = modelMetadata;
            ChildNodes = childNodes;
            Model = model;
        }

        public string Key { get; set; }

        public ModelMetadata ModelMetadata { get; set; }

        /// <summary>
        /// Represents the Actual name of the property which this <see cref="ModelValidationNode"/> represents.
        /// </summary>
        public string PropertyName { get; set; }

        public object Model { get; set; }

        public IList<ModelValidationNode> ChildNodes { get; set; }
    }
}