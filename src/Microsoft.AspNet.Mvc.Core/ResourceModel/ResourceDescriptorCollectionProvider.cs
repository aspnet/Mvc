using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc.ResourceModel
{
    public class ResourceDescriptorCollectionProvider : IResourceDescriptorCollectionProvider
    {
        private readonly IActionDescriptorsCollectionProvider _actionDescriptorCollectionProvider;
        private readonly INestedProviderManager<ResourceDescriptorProviderContext> _resourceDescriptorProvider;

        private ResourceDescriptorCollection _resourceDescriptors;

        public ResourceDescriptorCollectionProvider(
            IActionDescriptorsCollectionProvider actionDescriptorCollectionProvider,
            INestedProviderManager<ResourceDescriptorProviderContext> resourceDescriptorProvider)
        {
            _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
            _resourceDescriptorProvider = resourceDescriptorProvider;
        }

        public ResourceDescriptorCollection ResourceDescriptors
        {
            get
            {
                var actionDescriptors = _actionDescriptorCollectionProvider.ActionDescriptors;
                if (_resourceDescriptors == null || _resourceDescriptors.Version != actionDescriptors.Version)
                {
                    _resourceDescriptors = GetCollection(actionDescriptors);
                }

                return _resourceDescriptors;
            }
        }

        private ResourceDescriptorCollection GetCollection(ActionDescriptorsCollection actionDescriptors)
        {
            var context = new ResourceDescriptorProviderContext(actionDescriptors.Items);
            _resourceDescriptorProvider.Invoke(context);

            return new ResourceDescriptorCollection(context.Results, actionDescriptors.Version);
        }
    }
}