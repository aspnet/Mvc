// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Localization;

namespace Microsoft.AspNetCore.Mvc.DataAnnotations.Internal
{
    /// <summary>
    /// An implementation of <see cref="IBindingMetadataProvider"/> and <see cref="IDisplayMetadataProvider"/> for
    /// the System.ComponentModel.DataAnnotations attribute classes.
    /// </summary>
    public class DataAnnotationsMetadataProvider :
        IBindingMetadataProvider,
        IDisplayMetadataProvider,
        IValidationMetadataProvider
    {
        private readonly IStringLocalizerFactory _stringLocalizerFactory;

        public DataAnnotationsMetadataProvider(IStringLocalizerFactory stringLocalizerFactory)
        {
            _stringLocalizerFactory = stringLocalizerFactory;
        }

        /// <inheritdoc />
        public void CreateBindingMetadata(BindingMetadataProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var editableAttribute = context.Attributes.OfType<EditableAttribute>().FirstOrDefault();
            if (editableAttribute != null)
            {
                context.BindingMetadata.IsReadOnly = !editableAttribute.AllowEdit;
            }
        }

        /// <inheritdoc />
        public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var attributes = context.Attributes;
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

            var displayMetadata = context.DisplayMetadata;

            // ConvertEmptyStringToNull
            if (displayFormatAttribute != null)
            {
                displayMetadata.ConvertEmptyStringToNull = displayFormatAttribute.ConvertEmptyStringToNull;
            }

            // DataTypeName
            if (dataTypeAttribute != null)
            {
                displayMetadata.DataTypeName = dataTypeAttribute.GetDataTypeName();
            }
            else if (displayFormatAttribute != null && !displayFormatAttribute.HtmlEncode)
            {
                displayMetadata.DataTypeName = DataType.Html.ToString();
            }


            var containerType = context.Key.ContainerType ?? context.Key.ModelType;
            var localizer = _stringLocalizerFactory?.Create(containerType);

            // Description
            if (displayAttribute != null)
            {
                if (localizer != null &&
                    !string.IsNullOrEmpty(displayAttribute.Description) &&
                    displayAttribute.ResourceType == null)
                {
                    displayMetadata.Description = () => localizer[displayAttribute.Description];
                }
                else
                {
                    displayMetadata.Description = () => displayAttribute.GetDescription();
                }
            }

            // DisplayFormatString
            if (displayFormatAttribute != null)
            {
                displayMetadata.DisplayFormatString = displayFormatAttribute.DataFormatString;
            }

            // DisplayName
            if (displayAttribute != null)
            {
                if (localizer != null &&
                    !string.IsNullOrEmpty(displayAttribute.Name) &&
                    displayAttribute.ResourceType == null)
                {
                    displayMetadata.DisplayName = () => localizer[displayAttribute.Name];
                }
                else
                {
                    displayMetadata.DisplayName = () => displayAttribute.GetName();
                }
            }

            // EditFormatString
            if (displayFormatAttribute != null && displayFormatAttribute.ApplyFormatInEditMode)
            {
                displayMetadata.EditFormatString = displayFormatAttribute.DataFormatString;
            }

            // IsEnum et cetera
            var underlyingType = Nullable.GetUnderlyingType(context.Key.ModelType) ?? context.Key.ModelType;
            var underlyingTypeInfo = underlyingType.GetTypeInfo();

            if (underlyingTypeInfo.IsEnum)
            {
                // IsEnum
                displayMetadata.IsEnum = true;

                // IsFlagsEnum
                displayMetadata.IsFlagsEnum = underlyingTypeInfo.IsDefined(typeof(FlagsAttribute), inherit: false);

                // EnumDisplayNamesAndValues and EnumNamesAndValues
                //
                // Order EnumDisplayNamesAndValues to match Enum.GetNames(). That method orders by absolute value,
                // then its behavior is undefined (but hopefully stable). Add to EnumNamesAndValues in same order but
                // Dictionary does not guarantee order will be preserved.
                var groupedDisplayNamesAndValues = new List<KeyValuePair<EnumGroupAndName, string>>();
                var namesAndValues = new Dictionary<string, string>();
                var enumLocalizer = _stringLocalizerFactory?.Create(underlyingType);
                foreach (var name in Enum.GetNames(underlyingType))
                {
                    var field = underlyingType.GetField(name);
                    var groupName = GetDisplayGroup(field);
                    var value = ((Enum)field.GetValue(obj: null)).ToString("d");

                    groupedDisplayNamesAndValues.Add(new KeyValuePair<EnumGroupAndName, string>(
                        new EnumGroupAndName(groupName, () => GetDisplayName(enumLocalizer, field)),
                        value));
                    namesAndValues.Add(name, value);
                }

                displayMetadata.EnumGroupedDisplayNamesAndValues = groupedDisplayNamesAndValues;
                displayMetadata.EnumNamesAndValues = namesAndValues;
            }

            // HasNonDefaultEditFormat
            if (!string.IsNullOrEmpty(displayFormatAttribute?.DataFormatString) &&
                displayFormatAttribute?.ApplyFormatInEditMode == true)
            {
                // Have a non-empty EditFormatString based on [DisplayFormat] from our cache.
                if (dataTypeAttribute == null)
                {
                    // Attributes include no [DataType]; [DisplayFormat] was applied directly.
                    displayMetadata.HasNonDefaultEditFormat = true;
                }
                else if (dataTypeAttribute.DisplayFormat != displayFormatAttribute)
                {
                    // Attributes include separate [DataType] and [DisplayFormat]; [DisplayFormat] provided override.
                    displayMetadata.HasNonDefaultEditFormat = true;
                }
                else if (dataTypeAttribute.GetType() != typeof(DataTypeAttribute))
                {
                    // Attributes include [DisplayFormat] copied from [DataType] and [DataType] was of a subclass.
                    // Assume the [DataType] constructor used the protected DisplayFormat setter to override its
                    // default.  That is derived [DataType] provided override.
                    displayMetadata.HasNonDefaultEditFormat = true;
                }
            }

            // HideSurroundingHtml
            if (hiddenInputAttribute != null)
            {
                displayMetadata.HideSurroundingHtml = !hiddenInputAttribute.DisplayValue;
            }

            // HtmlEncode
            if (displayFormatAttribute != null)
            {
                displayMetadata.HtmlEncode = displayFormatAttribute.HtmlEncode;
            }

            // NullDisplayText
            if (displayFormatAttribute != null)
            {
                displayMetadata.NullDisplayText = displayFormatAttribute.NullDisplayText;
            }

            // Order
            if (displayAttribute?.GetOrder() != null)
            {
                displayMetadata.Order = displayAttribute.GetOrder().Value;
            }

            // Placeholder
            if (displayAttribute != null)
            {
                if (localizer != null &&
                    !string.IsNullOrEmpty(displayAttribute.Prompt) &&
                    displayAttribute.ResourceType == null)
                {
                    displayMetadata.Placeholder = () => localizer[displayAttribute.Prompt];
                }
                else
                {
                    displayMetadata.Placeholder = () => displayAttribute.GetPrompt();
                }
            }

            // ShowForDisplay
            if (scaffoldColumnAttribute != null)
            {
                displayMetadata.ShowForDisplay = scaffoldColumnAttribute.Scaffold;
            }

            // ShowForEdit
            if (scaffoldColumnAttribute != null)
            {
                displayMetadata.ShowForEdit = scaffoldColumnAttribute.Scaffold;
            }

            // SimpleDisplayProperty
            if (displayColumnAttribute != null)
            {
                displayMetadata.SimpleDisplayProperty = displayColumnAttribute.DisplayColumn;
            }

            // TemplateHint
            if (uiHintAttribute != null)
            {
                displayMetadata.TemplateHint = uiHintAttribute.UIHint;
            }
            else if (hiddenInputAttribute != null)
            {
                displayMetadata.TemplateHint = "HiddenInput";
            }
        }

        /// <inheritdoc />
        public void CreateValidationMetadata(ValidationMetadataProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // RequiredAttribute marks a property as required by validation - this means that it
            // must have a non-null value on the model during validation.
            var requiredAttribute = context.Attributes.OfType<RequiredAttribute>().FirstOrDefault();
            if (requiredAttribute != null)
            {
                context.ValidationMetadata.IsRequired = true;
            }

            foreach (var attribute in context.Attributes.OfType<ValidationAttribute>())
            {
                // If another provider has already added this attribute, do not repeat it.
                // This will prevent attributes like RemoteAttribute (which implement ValidationAttribute and
                // IClientModelValidator) to be added to the ValidationMetadata twice.
                // This is to ensure we do not end up with duplication validation rules on the client side.
                if (!context.ValidationMetadata.ValidatorMetadata.Contains(attribute))
                {
                    context.ValidationMetadata.ValidatorMetadata.Add(attribute);
                }
            }
        }

        // Return non-empty name specified in a [Display] attribute for a field, if any; field.Name otherwise.
        private static string GetDisplayName(IStringLocalizer localizer, FieldInfo field)
        {
            var display = field.GetCustomAttribute<DisplayAttribute>(inherit: false);
            if (display != null)
            {
                // Note [Display(Name = "")] is allowed.
                var name = display.GetName();
                if (localizer != null && !string.IsNullOrEmpty(name) && display.ResourceType == null)
                {
                    name = localizer[name];
                }
                return name ?? field.Name;
            }

            return field.Name;
        }

        // Return non-empty group specified in a [Display] attribute for a field, if any; string.Empty otherwise.
        private static string GetDisplayGroup(FieldInfo field)
        {
            var display = field.GetCustomAttribute<DisplayAttribute>(inherit: false);
            if (display != null)
            {
                // Note [Display(Group = "")] is allowed.
                var group = display.GetGroupName();
                if (group != null)
                {
                    return group;
                }
            }

            return string.Empty;
        }
    }
}