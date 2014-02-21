using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class ModelBinderConfig
    {
        public IModelMetadataProvider MetadataProvider { get; set; }

        public IModelBinder ModelBinder { get; set; }

        public IValueProvider ValueProvider { get; set; }
    }
}
