﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.AspNet.JsonPatch.Helpers
{
	internal static class PropertyHelpers
	{

		public static bool SetValue(PropertyInfo propertyToSet, object targetObject, string pathToProperty, object value)
		{
			// it is possible the path refers to a nested property.  In that case, we need to 
			// set on a different target object: the nested object.


			string[] splitPath = pathToProperty.Split('/');

			// skip the first one if it's empty
			int startIndex = (string.IsNullOrWhiteSpace(splitPath[0]) ? 1 : 0);

			for (int i = startIndex; i < splitPath.Length - 1; i++)
			{
				PropertyInfo propertyInfoToGet = GetPropertyInfo(targetObject, splitPath[i]
					, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
				targetObject = propertyInfoToGet.GetValue(targetObject, null);
			}
			

			if (value == null)
			{
				// then, set it.
				propertyToSet.SetValue(targetObject, value, null);
			}
			else
			{
				var type = propertyToSet.PropertyType;
				// first, cast the value to the expected property type. 
				var valueToSet = Convert.ChangeType(value, type);
				// then, set it.
				propertyToSet.SetValue(targetObject, valueToSet, null);
			}


			return true;
		}

		public static object GetValue(PropertyInfo propertyToGet, object targetObject, string pathToProperty)
		{
			// it is possible the path refers to a nested property.  In that case, we need to 
			// get from a different target object: the nested object.

			string[] splitPath = pathToProperty.Split('/');

			// skip the first one if it's empty
			int startIndex = (string.IsNullOrWhiteSpace(splitPath[0]) ? 1 : 0);

			for (int i = startIndex; i < splitPath.Length - 1; i++)
			{
				PropertyInfo propertyInfoToGet = GetPropertyInfo(targetObject, splitPath[i]
					, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
				targetObject = propertyInfoToGet.GetValue(targetObject, null);
			}


			return propertyToGet.GetValue(targetObject, null);
		}


		public static bool CheckIfPropertyExists(object targetObject, string propertyPath)
		{
			try
			{

				string[] splitPath = propertyPath.Split('/');

				// skip the first one if it's empty
				int startIndex = (string.IsNullOrWhiteSpace(splitPath[0]) ? 1 : 0);

				for (int i = startIndex; i < splitPath.Length - 1; i++)
				{
					PropertyInfo propertyInfoToGet = GetPropertyInfo(targetObject, splitPath[i]
						, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
					targetObject = propertyInfoToGet.GetValue(targetObject, null);
				}

				// for dynamic objects
				if (targetObject is IDynamicMetaObjectProvider)
				{
					IDynamicMetaObjectProvider target = targetObject as IDynamicMetaObjectProvider;
					var propList = target.GetMetaObject(Expression.Constant(target)).GetDynamicMemberNames();
					return propList.Contains(splitPath.Last());
				}
				else
				{
					PropertyInfo propertyToCheck = targetObject.GetType().GetProperty(splitPath.Last(),
						BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

					return propertyToCheck != null;
				}

			}
			catch (Exception)
			{
				return false;
			}
		}



		public static PropertyInfo FindProperty(object targetObject, string propertyPath)
		{
			try
			{

				string[] splitPath = propertyPath.Split('/');

				// skip the first one if it's empty
				int startIndex = (string.IsNullOrWhiteSpace(splitPath[0]) ? 1 : 0);

				for (int i = startIndex; i < splitPath.Length - 1; i++)
				{
					PropertyInfo propertyToGet = GetPropertyInfo(targetObject, splitPath[i]
						, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
					targetObject = propertyToGet.GetValue(targetObject, null);
				}

				PropertyInfo propertyToFind = targetObject.GetType().GetProperty(splitPath.Last(),
					BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

				return propertyToFind;
			}
			catch (Exception)
			{
				return null;
			}
		}


		internal static bool CheckIfValueCanBeCast(Type propertyType, object value)
		{
			try
			{
				Convert.ChangeType(value, propertyType);
				return true;
			}
			catch (Exception)
			{
				return false;
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
			string possibleIndex = path.Substring(path.LastIndexOf("/") + 1);
			int castedIndex = -1;

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

	}
}