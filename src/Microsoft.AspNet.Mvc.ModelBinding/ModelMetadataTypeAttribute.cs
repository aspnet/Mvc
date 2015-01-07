using System;

namespace Microsoft.AspNet.Mvc
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ModelMetadataTypeAttribute : Attribute
    {
        private readonly Type _metadataType;

        public ModelMetadataTypeAttribute(Type type)
        {
            _metadataType = type;
        }

        public Type MetadataType
        {
            get { return _metadataType; }
        }
    }
}