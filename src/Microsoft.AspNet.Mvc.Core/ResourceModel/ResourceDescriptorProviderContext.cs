using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.ResourceModel
{
    public class ResourceDescriptorProviderContext
    {
	    public ResourceDescriptorProviderContext(IReadOnlyList<ActionDescriptor> actions)
	    {
            Actions = actions;

            Results = new List<ResourceDescriptor>();
	    }

        public IReadOnlyList<ActionDescriptor> Actions { get; private set; }

        public List<ResourceDescriptor> Results { get; private set; }
    }
}