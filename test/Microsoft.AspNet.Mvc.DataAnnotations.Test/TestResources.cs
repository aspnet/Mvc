// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.Threading;
using Microsoft.AspNet.Mvc.DataAnnotations.Test;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    // Wrap resources to make them available as public properties for [Display]. That attribute does not support
    // internal properties.
    public static class TestResources
    {
        public static string DisplayAttribute_Description { get; } = Resources.DisplayAttribute_Description;

        public static string DisplayAttribute_Name { get; } = Resources.DisplayAttribute_Name;

        public static CultureInfo CurrentUICulture =>
#if DNX451
            Thread.CurrentThread.CurrentUICulture;
#else
            CultureInfo.CurrentUICulture;
#endif

        public static string DisplayAttribute_CultureSensitiveName =>
            Resources.DisplayAttribute_Name + CurrentUICulture;

        public static string DisplayAttribute_CultureSensitiveDescription =>
            Resources.DisplayAttribute_Description + CurrentUICulture;
    }
}