using System;
using System.Linq;
using System.Globalization;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// Extension methods for <see cref="ModelExplorer"/>.
    /// </summary>
    public static class ModelExplorerExtensions
    {
        /// <summary>
        /// Gets a simple display string for the <see cref="ModelExplorer.Model"/> property
        /// of <paramref name="modelExplorer"/>.
        /// </summary>
        /// <param name="modelExplorer">The <see cref="ModelExplorer"/>.</param>
        /// <returns>A simple display string for the model.</returns>
        public static string GetSimpleDisplayText(this ModelExplorer modelExplorer)
        {
            if (modelExplorer.Metadata.SimpleDisplayProperty != null)
            {
                var propertyExplorer = modelExplorer.GetExplorerForProperty(modelExplorer.Metadata.SimpleDisplayProperty);
                if (propertyExplorer?.Model != null)
                {
                    return propertyExplorer.Model.ToString();
                }
            }

            if (modelExplorer.Model == null)
            {
                return modelExplorer.Metadata.NullDisplayText;
            }

            var stringResult = Convert.ToString(modelExplorer.Model, CultureInfo.CurrentCulture);
            if (stringResult == null)
            {
                return string.Empty;
            }

            if (!stringResult.Equals(modelExplorer.Model.GetType().FullName, StringComparison.Ordinal))
            {
                return stringResult;
            }

            var firstProperty = modelExplorer.Properties.FirstOrDefault();
            if (firstProperty == null)
            {
                return string.Empty;
            }

            if (firstProperty.Model == null)
            {
                return firstProperty.Metadata.NullDisplayText;
            }

            return Convert.ToString(firstProperty.Model, CultureInfo.CurrentCulture);
        }
    }
}