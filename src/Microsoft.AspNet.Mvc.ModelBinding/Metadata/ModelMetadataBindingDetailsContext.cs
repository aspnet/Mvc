// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class ModelMetadataBindingDetailsContext
    {
        public ModelMetadataBindingDetailsContext(
            [NotNull] ModelMetadataIdentity key, 
            [NotNull] IReadOnlyList<object> attributes)
        {
            Key = key;
            Attributes = attributes;
            BindingDetails = new ModelMetadataBindingDetails();
        }

        public IReadOnlyList<object> Attributes { get; }

        public ModelMetadataIdentity Key { get; }

        public ModelMetadataBindingDetails BindingDetails { get; }
    }
}