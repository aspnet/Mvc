// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class ModelMetadataDisplayDetailsContext
    {
        public ModelMetadataDisplayDetailsContext(
            [NotNull] ModelMetadataIdentity key, 
            [NotNull] IReadOnlyList<object> attributes)
        {
            Key = key;
            Attributes = attributes;
            DisplayDetails = new ModelMetadataDisplayDetails();
        }

        public IReadOnlyList<object> Attributes { get; }

        public ModelMetadataIdentity Key { get; }

        public ModelMetadataDisplayDetails DisplayDetails { get; }
    }
}