// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNet.Mvc.ModelBinding;
using ModelBindingWebSite.Models;

namespace ModelBindingWebSite
{
    public class AdditionalValuesMetadataProvider : IModelMetadataDisplayDetailsProvider
    {
        public static readonly string GroupNameKey = "__GroupName";
        private static Guid _guid = new Guid("7d6d0de2-8d59-49ac-99cc-881423b75a76");

        public void GetDisplayDetails(ModelMetadataDisplayDetailsContext context)
        {
            if (context.Key.ModelType == typeof(LargeModelWithValidation))
            {
                context.DisplayDetails.AdditionalValues.Add("key1", _guid);
                context.DisplayDetails.AdditionalValues.Add("key2", "value2");
            }

            var displayAttribute = context.Attributes.OfType<DisplayAttribute>().FirstOrDefault();
            var groupName = displayAttribute?.GroupName;
            if (!string.IsNullOrEmpty(groupName))
            {
                context.DisplayDetails.AdditionalValues[GroupNameKey] = groupName;
            }
        }
    }
}