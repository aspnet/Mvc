using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class CachedDataAnnotationsModelMetadata : CachedModelMetadata<CachedDataAnnotationsMetadataAttributes>
    {
        public CachedDataAnnotationsModelMetadata(CachedDataAnnotationsModelMetadata prototype, 
                                                  Func<object> modelAccessor)
            : base(prototype, modelAccessor)
        {
        }

        public CachedDataAnnotationsModelMetadata(DataAnnotationsModelMetadataProvider provider, 
                                                  Type containerType, 
                                                  Type modelType, 
                                                  string propertyName, 
                                                  IEnumerable<Attribute> attributes)
            : base(provider, 
                   containerType, 
                   modelType, 
                   propertyName, 
                   new CachedDataAnnotationsMetadataAttributes(attributes))
        {
        }

        protected override bool ComputeConvertEmptyStringToNull()
        {
            return PrototypeCache.DisplayFormat != null
                       ? PrototypeCache.DisplayFormat.ConvertEmptyStringToNull
                       : base.ComputeConvertEmptyStringToNull();
        }

        protected override string ComputeNullDisplayText()
        {
            return PrototypeCache.DisplayFormat != null
                       ? PrototypeCache.DisplayFormat.NullDisplayText
                       : base.ComputeNullDisplayText();
        }

        protected override string ComputeSimpleDisplayText()
        {
            if (Model != null)
            {
                if (PrototypeCache.DisplayColumn != null &&
                    !string.IsNullOrEmpty(PrototypeCache.DisplayColumn.DisplayColumn))
                {
                    var displayColumnProperty = ModelType.GetProperty(PrototypeCache.DisplayColumn.DisplayColumn,
                        BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);
                    ValidateDisplayColumnAttribute(PrototypeCache.DisplayColumn, displayColumnProperty, ModelType);

                    object simpleDisplayTextValue = displayColumnProperty.GetValue(Model, new object[0]);
                    if (simpleDisplayTextValue != null)
                    {
                        return simpleDisplayTextValue.ToString();
                    }
                }
            }

            return base.ComputeSimpleDisplayText();
        }

        protected override string ComputeDescription()
        {
            return PrototypeCache.Display != null
                       ? PrototypeCache.Display.GetDescription()
                       : base.ComputeDescription();
        }

        protected override bool ComputeIsReadOnly()
        {
            if (PrototypeCache.Editable != null)
            {
                return !PrototypeCache.Editable.AllowEdit;
            }

            return base.ComputeIsReadOnly();
        }

        public override string GetDisplayName()
        {
            // DisplayAttribute doesn't require you to set a name, so this could be null. 
            if (PrototypeCache.Display != null)
            {
                var name = PrototypeCache.Display.GetName();
                if (name != null)
                {
                    return name;
                }
            }

            // If DisplayAttribute does not specify a name, we'll fall back to the property name.
            return base.GetDisplayName();
        }

        private static void ValidateDisplayColumnAttribute(DisplayColumnAttribute displayColumnAttribute,
            PropertyInfo displayColumnProperty, Type modelType)
        {
            if (displayColumnProperty == null)
            {
                throw new InvalidOperationException(
                        Resources.FormatDataAnnotationsModelMetadataProvider_UnknownProperty(
                        modelType.FullName, displayColumnAttribute.DisplayColumn));
            }

            if (displayColumnProperty.GetGetMethod() == null)
            {
                throw new InvalidOperationException(
                        Resources.FormatDataAnnotationsModelMetadataProvider_UnreadableProperty(
                        modelType.FullName, displayColumnAttribute.DisplayColumn));
            }
        }
    }
}
