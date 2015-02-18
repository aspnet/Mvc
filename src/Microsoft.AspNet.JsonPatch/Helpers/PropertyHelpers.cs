using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace Microsoft.AspNet.JsonPatch.Helpers
{
	internal static class PropertyHelpers
	{
		public static object GetValue(JsonProperty propertyToGet, object targetObject, string pathToProperty)
		{
			// it is possible the path refers to a nested property.  In that case, we need to 
			// get from a different target object: the nested object.

			//var splitPath = pathToProperty.Split('/');

			//// skip the first one if it's empty
			//var startIndex = (string.IsNullOrWhiteSpace(splitPath[0]) ? 1 : 0);

			//for (int i = startIndex; i < splitPath.Length - 1; i++)
			//{
			//	var propertyInfoToGet = GetPropertyInfo(targetObject, splitPath[i]
			//		, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
			//	targetObject = propertyInfoToGet.GetValue(targetObject, null);
			//}

            return propertyToGet.ValueProvider.GetValue(targetObject);
			//return propertyToGet.GetValue(targetObject, null);
		}

		public static bool SetValue(JsonProperty propertyToSet, object targetObject, string pathToProperty, object value)
		{
			// it is possible the path refers to a nested property.  In that case, we need to 
			// set on a different target object: the nested object.
			var splitPath = pathToProperty.Split('/');

			// skip the first one if it's empty
			var startIndex = (string.IsNullOrWhiteSpace(splitPath[0]) ? 1 : 0);

			for (int i = startIndex; i < splitPath.Length - 1; i++)
			{
				var propertyInfoToGet = GetPropertyInfo(targetObject, splitPath[i]
					, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
				targetObject = propertyInfoToGet.GetValue(targetObject, null);
			}
            propertyToSet.ValueProvider.SetValue(targetObject, value);

			//propertyToSet.SetValue(targetObject, value, null);

			return true;
		}


		public static JsonProperty FindProperty(Type type, string propertyPath, JsonSerializerSettings serializerSettings)
		{
            JsonProperty jsonProperty = null;
            try
			{
				var splitPath = propertyPath.Split('/');

				// skip the first one if it's empty
				var startIndex = (string.IsNullOrWhiteSpace(splitPath[0]) ? 1 : 0);

				for (int i = startIndex; i < splitPath.Length - 1; i++)
				{
					//var propertyInfoToGet = GetPropertyInfo(targetObject, splitPath[i]
					//	, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
					//targetObject = propertyInfoToGet.GetValue(targetObject, null);

                    var jsonContract = GetJsonContract(serializerSettings, type);

                    foreach (var property in jsonContract.Properties)
                    {
                        if (string.Equals(property.PropertyName, splitPath[i]))
                        {
                            type = property.PropertyType;
                            jsonProperty = property;
                            
                            break;
                        }

                    }
				}

                return jsonProperty;
			}
			catch (Exception)
			{
				// will result in JsonPatchException in calling class, as expected
				return null;
			}
		}

		internal static ConversionResult ConvertToActualType(Type propertyType, object value)
		{
			try
			{
				var o = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(value), propertyType);
                
				return new ConversionResult(true, o);
			}
			catch (Exception)
			{
				return new ConversionResult(false, null);
			}
		}

		internal static Type GetEnumerableType(Type type)
		{
			if (type == null) throw new ArgumentNullException();
			foreach (Type interfaceType in type.GetInterfaces())
			{
#if NETFX_CORE || ASPNETCORE50
			    if (interfaceType.GetTypeInfo().IsGenericType &&
					interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
				{
					return interfaceType.GetGenericArguments()[0];
				}
#else
				if (interfaceType.IsGenericType &&
				interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
				{
					return interfaceType.GetGenericArguments()[0];
				}
#endif

			}
			return null;
		}

		internal static int GetNumericEnd(string path)
		{
			var possibleIndex = path.Substring(path.LastIndexOf("/") + 1);
			var castedIndex = -1;

			if (int.TryParse(possibleIndex, out castedIndex))
			{
				return castedIndex;
			}

			return -1;
		}

		private static PropertyInfo GetPropertyInfo(object targetObject, string propertyName,
		BindingFlags bindingFlags)
		{
			return targetObject.GetType().GetProperty(propertyName, bindingFlags);
		}

        public static JsonObjectContract GetJsonContract(JsonSerializerSettings serializerSettings, Type t)
        {
            var jsonSerializer = JsonSerializer.Create(serializerSettings);
            return (JsonObjectContract)jsonSerializer.ContractResolver.ResolveContract(t);
        }
    }
}