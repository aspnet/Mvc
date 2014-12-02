// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    /// <summary>
    /// Represents a <see cref="ParameterDescriptor"/>. Logged as a substructure of
    /// <see cref="ActionDescriptorValues"/>, this contains the name, type, and
    /// binder metadata of the parameter.
    /// </summary>
    public class ParameterDescriptorValues : LoggerStructureBase
    {
        public ParameterDescriptorValues([NotNull] ParameterDescriptor inner)
        {
            ParameterName = inner.Name;
            ParameterType = inner.ParameterType;
            BinderMetadata = inner.BinderMetadata?.ToString();
        }

        public string ParameterName { get; }

        public Type ParameterType { get; }

        public string BinderMetadata { get; }

        public override string Format()
        {
            return LogFormatter.FormatStructure(this);
        }
    }
}