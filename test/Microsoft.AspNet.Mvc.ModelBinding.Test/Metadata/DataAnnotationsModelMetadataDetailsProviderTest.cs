// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Microsoft.AspNet.Mvc.ModelBinding.Metadata
{
    public class DataAnnotationsModelMetadataDetailsProviderTest
    {
        // Includes attributes with a 'simple' effect on display details. 
        public static TheoryData<object, Func<ModelMetadataDisplayDetails, object>, object> DisplayDetailsData
        {
            get
            {
                return new TheoryData<object, Func<ModelMetadataDisplayDetails, object>, object>
                {
                    { new DataTypeAttribute(DataType.Duration), d => d.DataTypeName, DataType.Duration.ToString() },

                    { new DisplayAttribute() { Description = "d" }, d => d.Description, "d" },
                    { new DisplayAttribute() { Name = "DN" }, d => d.DisplayName, "DN" },
                    { new DisplayAttribute() { Order = 3 }, d => d.Order, 3 },

                    { new DisplayColumnAttribute("Property"), d => d.SimpleDisplayProperty, "Property" },

                    { new DisplayFormatAttribute() { ConvertEmptyStringToNull = true }, d => d.ConvertEmptyStringToNull, true },
                    { new DisplayFormatAttribute() { DataFormatString = "{0:G}" }, d => d.DisplayFormatString, "{0:G}" },
                    {
                        new DisplayFormatAttribute() { DataFormatString = "{0:G}", ApplyFormatInEditMode = true },
                        d => d.EditFormatString,
                        "{0:G}"
                    },
                    { new DisplayFormatAttribute() { HtmlEncode = false }, d => d.HtmlEncode, false },
                    { new DisplayFormatAttribute() { NullDisplayText = "(null)" }, d => d.NullDisplayText, "(null)" },

                    { new HiddenInputAttribute() { DisplayValue = false }, d => d.HideSurroundingHtml, true },

                    { new ScaffoldColumnAttribute(scaffold: false), d => d.ShowForDisplay, false },
                    { new ScaffoldColumnAttribute(scaffold: false), d => d.ShowForEdit, false },

                    { new UIHintAttribute("hintHint"), d => d.TemplateHint, "hintHint" },
                };
            }
        }

        [Theory]
        [MemberData(nameof(DisplayDetailsData))]
        public void GetDisplayDetails_SimpleAttributes(
            object attribute, 
            Func<ModelMetadataDisplayDetails, object> accessor, 
            object expected)
        {
            // Arrange
            var provider = new DataAnnotationsModelMetadataDetailsProvider();

            var key = ModelMetadataIdentity.ForType(typeof(string));
            var context = new ModelMetadataDisplayDetailsContext(key, new object[] { attribute });

            // Act
            provider.GetDisplayDetails(context);

            // Assert
            var value = accessor(context.DisplayDetails);
            Assert.Equal(expected, value);
        }

        [Fact]
        public void GetDisplayDetails_FindsDisplayFormat_FromDataType()
        {
            // Arrange
            var provider = new DataAnnotationsModelMetadataDetailsProvider();

            var dataType = new DataTypeAttribute(DataType.Currency);
            var displayFormat = dataType.DisplayFormat; // Non-null for DataType.Currency.

            var attributes = new[] { dataType, };
            var key = ModelMetadataIdentity.ForType(typeof(string));
            var context = new ModelMetadataDisplayDetailsContext(key, attributes);

            // Act
            provider.GetDisplayDetails(context);

            // Assert
            Assert.Same(displayFormat.DataFormatString, context.DisplayDetails.DisplayFormatString);
        }

        [Fact]
        public void GetDisplayDetails_FindsDisplayFormat_OverridingDataType()
        {
            // Arrange
            var provider = new DataAnnotationsModelMetadataDetailsProvider();

            var dataType = new DataTypeAttribute(DataType.Time); // Has a non-null DisplayFormat.
            var displayFormat = new DisplayFormatAttribute() // But these values override the values from DataType
            {
                DataFormatString = "Cool {0}",
            };

            var attributes = new Attribute[] { dataType, displayFormat, };
            var key = ModelMetadataIdentity.ForType(typeof(string));
            var context = new ModelMetadataDisplayDetailsContext(key, attributes);

            // Act
            provider.GetDisplayDetails(context);

            // Assert
            Assert.Same(displayFormat.DataFormatString, context.DisplayDetails.DisplayFormatString);
        }

        [Fact]
        public void GetDisplayDetails_EditableAttribute_SetsReadOnly()
        {
            // Arrange
            var provider = new DataAnnotationsModelMetadataDetailsProvider();

            var editable = new EditableAttribute(allowEdit: false);

            var attributes = new Attribute[] { editable };
            var key = ModelMetadataIdentity.ForType(typeof(string));
            var context = new ModelMetadataBindingDetailsContext(key, attributes);

            // Act
            provider.GetBindingDetails(context);

            // Assert
            Assert.Equal(true, context.BindingDetails.IsReadOnly);
        }

        [Fact]
        public void GetDisplayDetails_RequiredAttribute_SetsRequired()
        {
            // Arrange
            var provider = new DataAnnotationsModelMetadataDetailsProvider();

            var required = new RequiredAttribute();

            var attributes = new Attribute[] { required };
            var key = ModelMetadataIdentity.ForType(typeof(string));
            var context = new ModelMetadataBindingDetailsContext(key, attributes);

            // Act
            provider.GetBindingDetails(context);

            // Assert
            Assert.Equal(true, context.BindingDetails.IsRequired);
        }
    }
}