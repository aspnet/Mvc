﻿using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class ClientModelValidationContext
    {
        public ClientModelValidationContext([NotNull] ModelMetadata metadata,
                                            [NotNull] IModelMetadataProvider metadataProvider)
        {
            ModelMetadata = metadata;
            MetadataProvider = metadataProvider;
        }

        public ModelMetadata ModelMetadata { get; private set; }

        public IModelMetadataProvider MetadataProvider { get; private set; }
    }
}
