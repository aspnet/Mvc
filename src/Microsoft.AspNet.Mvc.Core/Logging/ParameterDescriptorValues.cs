﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Framework.Internal;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    /// <summary>
    /// Logging representation of a <see cref="ParameterDescriptor"/>. Logged as a substructure of
    /// <see cref="ActionDescriptorValues"/>.
    /// </summary>
    public class ParameterDescriptorValues : ReflectionBasedLogValues
    {
        public ParameterDescriptorValues([NotNull] ParameterDescriptor inner)
        {
            ParameterName = inner.Name;
            ParameterType = inner.ParameterType;
        }

        /// <summary>
        /// The name of the parameter. See <see cref="ParameterDescriptor.Name"/>.
        /// </summary>
        public string ParameterName { get; }

        /// <summary>
        /// The <see cref="Type"/> of the parameter. See <see cref="ParameterDescriptor.ParameterType"/>.
        /// </summary>
        public Type ParameterType { get; }

        public override string Format()
        {
            return LogFormatter.FormatLogValues(this);
        }
    }
}