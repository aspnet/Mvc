using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.ResourceModel
{
    public class ResourceDescriptor
    {
        public ResourceDescriptor()
        {
            ExtensionData = new Dictionary<Type, object>();
            Parameters = new List<ResourceParameterDescriptor>();
            OutputFormats = new List<ResourceOutputFormat>();
        }

        public ActionDescriptor ActionDescriptor { get; set; }

        public IDictionary<Type, object> ExtensionData { get; private set; }

        public string HttpMethod { get; set; }

        public List<ResourceParameterDescriptor> Parameters { get; private set; }

        public string Path { get; set; }

        public List<ResourceOutputFormat> OutputFormats { get; private set; }

        public string ResourceName { get; set; }

        public T GetExtension<T>()
            where T : class
        {
            object obj;
            ExtensionData.TryGetValue(typeof(T), out obj);
            return (T)obj;
        }

        public T GetOrCreateExtension<T>()
            where T : class, new()
        {
            T data;
            object obj;
            if (ExtensionData.TryGetValue(typeof(T), out obj))
            {
                data = (T)obj;
            }
            else
            {
                data = new T();
                SetExtension(data);
            }

            return data;
        }

        public void SetExtension<T>(T data)
        {
            ExtensionData[typeof(T)] = data;
        }
    }
}