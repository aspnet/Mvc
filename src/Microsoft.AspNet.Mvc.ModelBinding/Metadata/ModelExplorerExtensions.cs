using System;
using System.Linq;
using System.Globalization;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public static class ModelExplorerExtensions
    {
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

            var firstProperty = modelExplorer.Metadata.Properties.FirstOrDefault();
            if (firstProperty == null)
            {
                return string.Empty;
            }

            var firstPropertyExplorer = modelExplorer.GetExplorerForProperty(firstProperty.PropertyName);
            if (firstPropertyExplorer.Model == null)
            {
                return firstProperty.NullDisplayText;
            }

            return Convert.ToString(firstPropertyExplorer.Model, CultureInfo.CurrentCulture);
        }
    }
}