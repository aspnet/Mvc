using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.ResourceModel
{
    /// <summary>
    /// A cached collection of <see cref="ResourceDescriptor" />.
    /// </summary>
    public class ResourceDescriptorCollection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceDescriptorCollection"/>.
        /// </summary>
        /// <param name="items">The result of action discovery</param>
        /// <param name="version">The unique version of discovered actions.</param>
        public ResourceDescriptorCollection([NotNull] IReadOnlyList<ResourceDescriptor> items, int version)
        {
            Items = items;
            Version = version;
        }

        /// <summary>
        /// Returns the cached <see cref="IReadOnlyList{ResourceDescriptor}"/>.
        /// </summary>
        public IReadOnlyList<ResourceDescriptor> Items { get; private set; }

        /// <summary>
        /// Returns the unique version of the currently cached items.
        /// </summary>
        public int Version { get; private set; }
    }
}