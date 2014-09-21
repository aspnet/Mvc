// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    internal class TypeHelper
    {
        internal static bool IsSimpleType(Type type)
        {
            var isPrimitive = false;

#if ASPNET50
            isPrimitive = type.IsPrimitive;
#else
            isPrimitive = type.Equals(typeof(bool)) ||
                type.Equals(typeof(byte)) ||
                type.Equals(typeof(sbyte)) ||
                type.Equals(typeof(short)) ||
                type.Equals(typeof(int)) ||
                type.Equals(typeof(long)) ||
                type.Equals(typeof(ushort)) ||
                type.Equals(typeof(uint)) ||
                type.Equals(typeof(ulong)) ||
                type.Equals(typeof(IntPtr)) ||
                type.Equals(typeof(UIntPtr)) ||
                type.Equals(typeof(char)) ||
                type.Equals(typeof(float)) ||
                type.Equals(typeof(double));
#endif
            return isPrimitive ||
                type.Equals(typeof(decimal)) ||
                type.Equals(typeof(string)) ||
                type.Equals(typeof(DateTime)) ||
                type.Equals(typeof(Guid)) ||
                type.Equals(typeof(DateTimeOffset)) ||
                type.Equals(typeof(TimeSpan));
        }
    }
}