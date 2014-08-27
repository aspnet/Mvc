using System;

namespace Microsoft.AspNet.Mvc.ResourceModel
{
    public interface IResourceDescriptorCollectionProvider
    {
        ResourceDescriptorCollection ResourceDescriptors { get; }
    }
}