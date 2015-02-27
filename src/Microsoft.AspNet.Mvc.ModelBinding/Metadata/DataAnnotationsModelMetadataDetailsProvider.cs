// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding.Metadata
{
    public class DataAnnotationsModelMetadataDetailsProvider : 
        IModelMetadataBindingDetailsProvider,
        IModelMetadataDisplayDetailsProvider
    {
        public void GetBindingDetails([NotNull] ModelMetadataBindingDetailsContext context)
        {
            context.BindingDetails.IsRequired = context.Attributes.OfType<RequiredAttribute>().Any();

            var editableAttribute = context.Attributes.OfType<EditableAttribute>().FirstOrDefault();
            if (editableAttribute != null)
            {
                context.BindingDetails.IsReadOnly = !editableAttribute.AllowEdit;
            }
        }

        public void GetDisplayDetails([NotNull] ModelMetadataDisplayDetailsContext context)
        {
            SetDisplayDetails(context.Attributes, context.DisplayDetails);
        }

        private static void SetDisplayDetails(IReadOnlyList<object> attributes, ModelMetadataDisplayDetails details)
        {
            var dataTypeAttribute = attributes.OfType<DataTypeAttribute>().FirstOrDefault();
            var displayAttribute = attributes.OfType<DisplayAttribute>().FirstOrDefault();
            var displayColumnAttribute = attributes.OfType<DisplayColumnAttribute>().FirstOrDefault();
            var displayFormatAttribute = attributes.OfType<DisplayFormatAttribute>().FirstOrDefault();
            var hiddenInputAttribute = attributes.OfType<HiddenInputAttribute>().FirstOrDefault();
            var scaffoldColumnAttribute = attributes.OfType<ScaffoldColumnAttribute>().FirstOrDefault();
            var uiHintAttribute = attributes.OfType<UIHintAttribute>().FirstOrDefault();

            // Special case the [DisplayFormat] attribute hanging off an applied [DataType] attribute. This property is
            // non-null for DataType.Currency, DataType.Date, DataType.Time, and potentially custom [DataType]
            // subclasses. The DataType.Currency, DataType.Date, and DataType.Time [DisplayFormat] attributes have a
            // non-null DataFormatString and the DataType.Date and DataType.Time [DisplayFormat] attributes have
            // ApplyFormatInEditMode==true.
            if (displayFormatAttribute == null && dataTypeAttribute != null)
            {
                displayFormatAttribute = dataTypeAttribute.DisplayFormat;
            }

            // ConvertEmptyStringToNull
            if (displayFormatAttribute != null)
            {
                details.ConvertEmptyStringToNull = displayFormatAttribute.ConvertEmptyStringToNull;
            }

            // DataTypeName
            if (dataTypeAttribute != null)
            {
                details.DataTypeName = dataTypeAttribute.GetDataTypeName();
            }
            else if (displayFormatAttribute != null && !displayFormatAttribute.HtmlEncode)
            {
                details.DataTypeName = DataType.Html.ToString();
            }

            // Description
            if (displayAttribute != null)
            {
                details.Description = displayAttribute.Description;
            }

            // DisplayFormat
            if (displayFormatAttribute != null)
            {
                details.DisplayFormatString = displayFormatAttribute.DataFormatString;
            }

            // DisplayName
            if (displayAttribute != null)
            {
                details.DisplayName = displayAttribute.Name;
            }

            if (displayFormatAttribute != null && displayFormatAttribute.ApplyFormatInEditMode)
            {
                details.EditFormatString = displayFormatAttribute.DataFormatString;
            }

            // HasNonDefaultEditFormat
            if (!string.IsNullOrEmpty(displayFormatAttribute?.DataFormatString) &&
                displayFormatAttribute?.ApplyFormatInEditMode == true)
            {
                // Have a non-empty EditFormatString based on [DisplayFormat] from our cache.
                if (dataTypeAttribute == null)
                {
                    // Attributes include no [DataType]; [DisplayFormat] was applied directly.
                    details.HasNonDefaultEditFormat = true;
                }
                else if (dataTypeAttribute.DisplayFormat != displayFormatAttribute)
                {
                    // Attributes include separate [DataType] and [DisplayFormat]; [DisplayFormat] provided override.
                    details.HasNonDefaultEditFormat = true;
                }
                else if (dataTypeAttribute.GetType() != typeof(DataTypeAttribute))
                {
                    // Attributes include [DisplayFormat] copied from [DataType] and [DataType] was of a subclass.
                    // Assume the [DataType] constructor used the protected DisplayFormat setter to override its
                    // default.  That is derived [DataType] provided override.
                    details.HasNonDefaultEditFormat = true;
                }
            }

            // HideSurroundingHtml
            if (hiddenInputAttribute != null)
            {
                details.HideSurroundingHtml = !hiddenInputAttribute.DisplayValue;
            }

            // HtmlEncode
            if (displayFormatAttribute != null)
            {
                details.HtmlEncode = displayFormatAttribute.HtmlEncode;
            }

            // NullDisplayText
            if (displayFormatAttribute != null)
            {
                details.NullDisplayText = displayFormatAttribute.NullDisplayText;
            }

            // Order
            if (displayAttribute?.GetOrder() != null)
            {
                details.Order = displayAttribute.GetOrder().Value;
            }

            // ShowForDisplay 
            if (scaffoldColumnAttribute != null)
            {
                details.ShowForDisplay = scaffoldColumnAttribute.Scaffold;
            }

            // ShowForEdit
            if (scaffoldColumnAttribute != null)
            {
                details.ShowForEdit = scaffoldColumnAttribute.Scaffold;
            }

            // SimpleDisplayProperty
            if (displayColumnAttribute != null)
            {
                details.SimpleDisplayProperty = displayColumnAttribute.DisplayColumn;
            }

            // TemplateHinte
            if (uiHintAttribute != null)
            {
                details.TemplateHint = uiHintAttribute.UIHint;
            }
            else if (hiddenInputAttribute != null)
            {
                details.TemplateHint = "HiddenInput";
            }
        }
    }
}