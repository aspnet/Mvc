// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNet.Mvc.ModelBinding.Metadata;

namespace Microsoft.AspNet.Mvc.ModelBinding.Validation
{
    public class ValidationExcludeFilter : IValidationMetadataProvider
    {
        public ValidationExcludeFilter(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            Type = type;
        }

        public ValidationExcludeFilter(string typeFullName)
        {
            if (typeFullName == null)
            {
                throw new ArgumentNullException(nameof(typeFullName));
            }

            TypeFullName = typeFullName;
        }

        public Type Type { get; }

        public string TypeFullName { get; }

        public void GetValidationMetadata(ValidationMetadataProviderContext context)
        {
            if (Type != null)
            {
                if (Type.GetTypeInfo().IsAssignableFrom(context.Key.ModelType.GetTypeInfo()))
                {
                    context.ValidationMetadata.ValidateChildren = false;
                }

                return;
            }

            if (TypeFullName != null)
            {
                if (IsMatchingName(context.Key.ModelType))
                {
                    context.ValidationMetadata.ValidateChildren = false;
                }

                return;
            }
        }

        private bool IsMatchingName(Type type)
        {
            Debug.Assert(TypeFullName != null);

            if (type == null)
            {
                return false;
            }

            if (string.Equals(type.FullName, TypeFullName, StringComparison.Ordinal))
            {
                return true;
            }

            return IsMatchingName(type.GetTypeInfo().BaseType);
        }
    }
}
