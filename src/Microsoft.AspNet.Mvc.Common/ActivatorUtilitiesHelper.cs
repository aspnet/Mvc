// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Framework.DependencyInjection;
using System;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Helper class to create delegate for ActivatorUtilities CreateFactory.
    /// </summary>
    internal static class ActivatorUtilitiesHelper
    {
        public static Func<Type, ObjectFactory> CreateFactory =
            (t) => ActivatorUtilities.CreateFactory(t, Type.EmptyTypes);
    }
}