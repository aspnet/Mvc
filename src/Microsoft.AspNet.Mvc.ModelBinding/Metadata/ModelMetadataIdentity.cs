// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public struct ModelMetadataIdentity
    {
        public static ModelMetadataIdentity ForType([NotNull] Type modelType)
        {
            return new ModelMetadataIdentity()
            {
                ModelType = modelType,
                MetadataKind = ModelMetadataKind.Type,
            };
        }

        public static ModelMetadataIdentity ForParameter([NotNull] ParameterInfo parameterInfo)
        {
            return new ModelMetadataIdentity()
            {
                ParameterInfo = parameterInfo,
                ModelType = parameterInfo.ParameterType,
                Name = parameterInfo.Name,
                MetadataKind = ModelMetadataKind.Parameter,
            };
        }

        public static ModelMetadataIdentity ForProperty(
            [NotNull] Type modelType,
            [NotNull] string name,
            [NotNull] Type containerType)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, nameof(name));
            }

            return new ModelMetadataIdentity()
            {
                ModelType = modelType,
                Name = name,
                ContainerType = containerType,
                MetadataKind = ModelMetadataKind.Property,
            };
        }

        public Type ContainerType { get; private set; }

        public ModelMetadataKind MetadataKind { get; private set; }

        public ParameterInfo ParameterInfo { get; private set; }

        public Type ModelType { get; private set; }

        public string Name { get; private set; }
    }
}