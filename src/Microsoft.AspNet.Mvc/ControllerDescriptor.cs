using System;
using System.Reflection;

namespace Microsoft.AspNet.Mvc
{
    public class ControllerDescriptor
    {
        public ControllerDescriptor(TypeInfo controllerTypeInfo, Assembly assembly)
        {
            if (controllerTypeInfo == null)
            {
                throw new ArgumentNullException("controllerTypeInfo");
            }

            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }

            ControllerTypeInfo = controllerTypeInfo;
            Assembly = assembly;

            Name = controllerTypeInfo.Name.EndsWith("Controller", StringComparison.Ordinal)
                 ? controllerTypeInfo.Name.Substring(0, controllerTypeInfo.Name.Length - "Controller".Length)
                 : controllerTypeInfo.Name;
 
            AssemblyName = assembly.GetName().Name;
        }

        public string Name { get; private set; }

        public string AssemblyName { get; private set; }

        public TypeInfo ControllerTypeInfo { get; private set; }

        public Assembly Assembly { get; private set; }
    }
}
