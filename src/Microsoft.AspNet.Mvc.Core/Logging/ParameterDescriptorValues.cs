// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    public class ParameterDescriptorValues : LoggerStructureBase
    {
        public ParameterDescriptorValues([NotNull] ParameterDescriptor inner)
        {
            ParameterName = inner.Name;
            ParameterType = inner.ParameterType.FullName;
            BinderMetadata = inner.BinderMetadata?.ToString();
        }

        public string ParameterName { get; set; }

        public string ParameterType { get; set; }

        public string BinderMetadata { get; set; }

        public override string Format()
        {
            return LogFormatter.FormatStructure(this);
        }
    }
}