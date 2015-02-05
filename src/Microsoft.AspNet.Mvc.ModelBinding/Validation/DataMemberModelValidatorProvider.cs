// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// This <see cref="IModelValidatorProvider"/> provides a required ModelValidator for members marked
    /// as <c>[DataMember(IsRequired=true)]</c>.
    /// </summary>
    public class DataMemberModelValidatorProvider : AssociatedValidatorProvider
    {
        protected override IEnumerable<IModelValidator> GetValidators(ModelMetadata metadata,
                                                                      IEnumerable<object> attributes)
        {
            // Types cannot be required; only properties can
            if (metadata.ContainerType == null || string.IsNullOrEmpty(metadata.PropertyName))
            {
                return Enumerable.Empty<IModelValidator>();
            }

            if (IsRequiredDataMember(metadata.ContainerType, attributes))
            {
                return new[] { new RequiredMemberModelValidator() };
            }

            return Enumerable.Empty<IModelValidator>();
        }

        internal static bool IsRequiredDataMember(Type containerType, IEnumerable<object> attributes)
        {
            var dataMemberAttribute = attributes.OfType<DataMemberAttribute>()
                                                .FirstOrDefault();
            if (dataMemberAttribute != null)
            {
                // isDataContract == true iff the container type has at least one DataContractAttribute
                var isDataContract = containerType.GetTypeInfo()
                                                  .GetCustomAttributes()
                                                  .OfType<DataContractAttribute>()
                                                  .Any();
                if (isDataContract && dataMemberAttribute.IsRequired)
                {
                    return true;
                }
            }
            return false;
        }
    }
}