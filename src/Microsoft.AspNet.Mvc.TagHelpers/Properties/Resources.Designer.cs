// <auto-generated />
namespace Microsoft.AspNet.Mvc.TagHelpers
{
    using System.Globalization;
    using System.Reflection;
    using System.Resources;

    internal static class Resources
    {
        private static readonly ResourceManager _resourceManager
            = new ResourceManager("Microsoft.AspNet.Mvc.TagHelpers.Resources", typeof(Resources).GetTypeInfo().Assembly);

        /// <summary>
        /// Cannot determine an {4} for {0}. An {0} with a specified {1} must not have an {2} or {3} attribute.
        /// </summary>
        internal static string AnchorTagHelper_CannotDetermineHrefRouteActionOrControllerSpecified
        {
            get { return GetString("AnchorTagHelper_CannotDetermineHrefRouteActionOrControllerSpecified"); }
        }

        /// <summary>
        /// Cannot determine an {4} for {0}. An {0} with a specified {1} must not have an {2} or {3} attribute.
        /// </summary>
        internal static string FormatAnchorTagHelper_CannotDetermineHrefRouteActionOrControllerSpecified(object p0, object p1, object p2, object p3, object p4)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("AnchorTagHelper_CannotDetermineHrefRouteActionOrControllerSpecified"), p0, p1, p2, p3, p4);
        }

        /// <summary>
        /// Cannot determine an {8} for {0}. An {0} with a specified {8} must not have attributes starting with {7} or an {1}, {2}, {3}, {4}, {5} or {6} attribute.
        /// </summary>
        internal static string AnchorTagHelper_CannotOverrideSpecifiedHref
        {
            get { return GetString("AnchorTagHelper_CannotOverrideSpecifiedHref"); }
        }

        /// <summary>
        /// Cannot determine an {8} for {0}. An {0} with a specified {8} must not have attributes starting with {7} or an {1}, {2}, {3}, {4}, {5} or {6} attribute.
        /// </summary>
        internal static string FormatAnchorTagHelper_CannotOverrideSpecifiedHref(object p0, object p1, object p2, object p3, object p4, object p5, object p6, object p7, object p8)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("AnchorTagHelper_CannotOverrideSpecifiedHref"), p0, p1, p2, p3, p4, p5, p6, p7, p8);
        }

        /// <summary>
        /// Cannot determine an {1} for {0}. A {0} with a URL-based {1} must not have attributes starting with {3} or a {2} attribute.
        /// </summary>
        internal static string FormTagHelper_CannotDetermineAction
        {
            get { return GetString("FormTagHelper_CannotDetermineAction"); }
        }

        /// <summary>
        /// Cannot determine an {1} for {0}. A {0} with a URL-based {1} must not have attributes starting with {3} or a {2} attribute.
        /// </summary>
        internal static string FormatFormTagHelper_CannotDetermineAction(object p0, object p1, object p2, object p3)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("FormTagHelper_CannotDetermineAction"), p0, p1, p2, p3);
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
