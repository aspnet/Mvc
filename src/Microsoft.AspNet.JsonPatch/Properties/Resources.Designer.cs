﻿// <auto-generated />
namespace Microsoft.AspNet.JsonPatch
{
    using System.Globalization;
    using System.Reflection;
    using System.Resources;

    internal static class Resources
    {
        private static readonly ResourceManager _resourceManager
            = new ResourceManager("Microsoft.AspNet.JsonPatch.Resources", typeof(Resources).GetTypeInfo().Assembly);

        /// <summary>
        /// Provided value '{0}' is invalid for property at path: '{1}'.
        /// </summary>
        internal static string InvalidValueForProperty(object p0, object p1)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("InvalidValueForProperty"), p0, p1);
        }

        /// <summary>
        /// Property does not exist at path '{0}'.
        /// </summary>
        internal static string PropertyDoesNotExist(object p0)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("PropertyDoesNotExist"), p0);
        }

        /// <summary>
        /// Cannot update property at path '{0}'.
        /// </summary>
        internal static string CannotUpdateProperty(object p0)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("CannotUpdateProperty"), p0);
        }

        /// <summary>
        /// For Operation '{0}', array property at location path: '{1}' index is larger than array size.
        /// </summary>
        internal static string InvalidIndexForArrayProperty(object p0, object p1)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("InvalidIndexForArrayProperty"), p0, p1);
        }

        /// <summary>
        /// For Operation '{0}', provided path is invalid for array property at location path: '{1}'.
        /// </summary>
        internal static string InvalidPathForArrayProperty(object p0, object p1)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("InvalidPathForArrayProperty"), p0, p1);
        }

        private static string GetString(string name, params string[] formatterNames)
        {
            var value = _resourceManager.GetString(name);

            System.Diagnostics.Debug.Assert(value != null);

            if (formatterNames != null)
            {
                for (var i = 0; i < formatterNames.Length; i++)
                {
                    value = value.Replace("{" + formatterNames[i] + "}", "{" + i + "}");
                }
            }

            return value;
        }
    }
}