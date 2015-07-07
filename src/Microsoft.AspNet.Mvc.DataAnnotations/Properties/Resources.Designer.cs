// <auto-generated />
namespace Microsoft.AspNet.Mvc.DataAnnotations
{
    using System.Globalization;
    using System.Reflection;
    using System.Resources;

    internal static class Resources
    {
        private static readonly ResourceManager _resourceManager
            = new ResourceManager("Microsoft.AspNet.Mvc.DataAnnotations.Resources", typeof(Resources).GetTypeInfo().Assembly);

        /// <summary>
        /// The model object inside the metadata claimed to be compatible with '{0}', but was actually '{1}'.
        /// </summary>
        internal static string ValidatableObjectAdapter_IncompatibleType
        {
            get { return GetString("ValidatableObjectAdapter_IncompatibleType"); }
        }

        /// <summary>
        /// The model object inside the metadata claimed to be compatible with '{0}', but was actually '{1}'.
        /// </summary>
        internal static string FormatValidatableObjectAdapter_IncompatibleType(object p0, object p1)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ValidatableObjectAdapter_IncompatibleType"), p0, p1);
        }

        /// <summary>
        /// Value cannot be null or empty.
        /// </summary>
        internal static string ArgumentCannotBeNullOrEmpty
        {
            get { return GetString("ArgumentCannotBeNullOrEmpty"); }
        }

        /// <summary>
        /// Value cannot be null or empty.
        /// </summary>
        internal static string FormatArgumentCannotBeNullOrEmpty()
        {
            return GetString("ArgumentCannotBeNullOrEmpty");
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
