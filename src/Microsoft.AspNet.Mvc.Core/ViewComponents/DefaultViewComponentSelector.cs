
using System;
using System.Linq;
using Microsoft.AspNet.Mvc.Core;

namespace Microsoft.AspNet.Mvc
{
    public class DefaultViewComponentSelector : IViewComponentSelector
    {
        private readonly IControllerAssemblyProvider _assemblyProvider;

        public DefaultViewComponentSelector(IControllerAssemblyProvider assemblyProvider)
        {
            _assemblyProvider = assemblyProvider;
        }

        public Type SelectComponent([NotNull] string componentName)
        {
            var assemblies = _assemblyProvider.Assemblies;
            var types = assemblies.SelectMany(a => a.DefinedTypes);

            var components = 
                types
                .Where(ViewComponentMetadata.IsComponent)
                .Select(c => new {Name = ViewComponentMetadata.GetComponentName(c), Type = c.AsType()});

            var matching = components.Where(c => string.Equals(c.Name, componentName, StringComparison.OrdinalIgnoreCase)).ToArray();

            if (matching.Length == 0)
            {
                return null;
            }
            else if (matching.Length == 1)
            {
                return matching[0].Type;
            }
            else
            {
                var typeNames = String.Join(Environment.NewLine, matching.Select(t => t.Type.FullName));
                throw new InvalidOperationException(Resources.FormatViewComponent_AmbiguousTypeMatch(componentName, typeNames));
            }
        }
    }
}
