using System;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public static class ModelMetadataProviderExtensions
    {
        public static ModelExplorer GetModelExplorerForType([NotNull] this IModelMetadataProvider provider, [NotNull] Type modelType, object model)
        {
            var modelMetadata = provider.GetMetadataForType(modelType);
            return new ModelExplorer(modelMetadata, model);
        }

        public static ModelExplorer GetModelExplorerForType([NotNull] this IModelMetadataProvider provider, [NotNull] Type modelType, Func<object> modelAccessor)
        {
            var modelMetadata = provider.GetMetadataForType(modelType);
            return new ModelExplorer(modelMetadata, modelAccessor);
        }
    }
}