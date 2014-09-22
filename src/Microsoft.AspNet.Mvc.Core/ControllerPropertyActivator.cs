// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNet.Mvc
{
    internal class ControllerPropertyAccessor
    {
        private readonly Action<object, object> _fastPropertySetter;
        private readonly Func<object, object> _fastPropertyGetter;

        public ControllerPropertyAccessor(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
            _fastPropertySetter = PropertyHelper.MakeFastPropertySetter(propertyInfo);
            _fastPropertyGetter = PropertyHelper.MakeFastPropertyGetter(propertyInfo);
        }

        public PropertyInfo PropertyInfo { get; private set; }

        public void Set(object view, object value)
        {
            _fastPropertySetter(view, value);
        }

        public object GetValue(object obj)
        {
            return _fastPropertyGetter(obj);
        }

        public static ControllerPropertyAccessor[] GetPropertiesToActivate(Type type)
        {
            return type.GetRuntimeProperties()
                       .Where(property =>
                              property.GetIndexParameters().Length == 0 &&
                              property.SetMethod != null &&
                              !property.SetMethod.IsStatic)
                       .Select(property => new ControllerPropertyAccessor(property))
                       .ToArray();
        }
    }
}
