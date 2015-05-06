// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class ModelValidationNode
    {
        public ModelValidationNode([NotNull] string key, [NotNull] ModelMetadata modelMetadata)
            : this (key, modelMetadata, new List<ModelValidationNode>())
        {
        }

        public ModelValidationNode([NotNull] string key, ModelMetadata modelMetadata, [NotNull] IList<ModelValidationNode> childNodes)
        {
            Key = key;
            ModelMetadata = modelMetadata;
            ChildNodes = childNodes;
        }

        public string Key { get; set; }

        public ModelMetadata ModelMetadata { get; set; }

        public IList<ModelValidationNode> ChildNodes { get; set; }
    }
}