
using System;
using System.Reflection;

namespace Microsoft.AspNet.Mvc
{
    public static class ViewComponentMetadata
    {
        private const string ViewComponentSuffix = "ViewComponent";

        public static string GetComponentName([NotNull] TypeInfo componentType)
        {
            if (componentType.Name.EndsWith(ViewComponentSuffix, StringComparison.OrdinalIgnoreCase))
            {
                return componentType.Name.Substring(0, componentType.Name.Length - ViewComponentSuffix.Length);
            }
            else
            {
                return componentType.Name;
            }
        }

        public static bool IsComponent([NotNull] TypeInfo typeInfo)
        {
            if (!typeInfo.IsClass ||
                typeInfo.IsAbstract ||
                typeInfo.ContainsGenericParameters)
            {
                return false;
            }

            return
                typeInfo.Name.EndsWith(ViewComponentSuffix, StringComparison.OrdinalIgnoreCase) ||
                typeInfo.GetCustomAttribute<ViewComponentAttribute>() != null;
        }
    }
}
