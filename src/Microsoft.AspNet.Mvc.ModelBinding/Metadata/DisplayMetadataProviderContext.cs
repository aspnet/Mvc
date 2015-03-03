// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding.Metadata
{
    public class DisplayMetadataProviderContext
    {
        public DisplayMetadataProviderContext(
            [NotNull] ModelMetadataIdentity key, 
            [NotNull] IReadOnlyList<object> attributes)
        {
            Key = key;
            Attributes = attributes;
            DisplayMetadata = new DisplayMetadata();
        }

        public IReadOnlyList<object> Attributes { get; }

        public ModelMetadataIdentity Key { get; }

        public DisplayMetadata DisplayMetadata { get; }
    }
}