﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Specifies that a controller property should be set with the current
    /// <see cref="Actions.ActionBindingContext"/> when creating the controller. The property must have a public
    /// set method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ActionBindingContextAttribute : Attribute
    {
    }
}
