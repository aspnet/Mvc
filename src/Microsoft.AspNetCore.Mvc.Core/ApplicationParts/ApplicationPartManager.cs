using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace Microsoft.AspNetCore.Mvc.ApplicationParts
{
    public class ApplicationPartManager
    {
        public IList<IApplicationFeatureProvider> Providers { get; } =
            new List<IApplicationFeatureProvider>();

        public IList<ApplicationPart> ApplicationParts { get; } =
            new List<ApplicationPart>();

        public void PopulateFeature<T>(T feature)
        {
            if (feature == null)
            {
                throw new ArgumentNullException(nameof(feature));
            }

            foreach (var provider in Providers.OfType<IApplicationFeatureProvider<T>>())
            {
                provider.GetFeature(ApplicationParts, feature);
            }
        }
    }
}
