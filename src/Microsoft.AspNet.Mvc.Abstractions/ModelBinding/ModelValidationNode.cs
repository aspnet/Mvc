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

        public string Key { get; set; }

        public ModelMetadata ModelMetadata { get; set; }

        public object Model { get; set; }

        public bool BuildChildNodesUsingModel { get; set; } = false;

        public IList<ModelValidationNode> ChildNodes { get; set; }
    }
}