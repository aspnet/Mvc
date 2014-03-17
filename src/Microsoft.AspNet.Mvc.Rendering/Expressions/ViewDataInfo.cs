﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace Microsoft.AspNet.Mvc.Rendering.Expressions
{
    public class ViewDataInfo
    {
        private object _value;
        private Func<object> _valueAccessor;

        /// <summary>
        /// Info about a <see cref="ViewData"/> lookup which has already been evaluated.
        /// </summary>
        public ViewDataInfo(object container, object value)
        {
            Container = container;
            _value = value;
        }

        /// <summary>
        /// Info about a <see cref="ViewData"/> lookup which is evaluated when <see cref="Value"/> is read.
        /// </summary>
        public ViewDataInfo(object container, PropertyInfo propertyInfo, Func<object> valueAccessor)
        {
            Container = container;
            PropertyInfo = propertyInfo;
            _valueAccessor = valueAccessor;
        }

        public object Container { get; private set; }

        public PropertyInfo PropertyInfo { get; private set; }

        public object Value
        {
            get
            {
                if (_valueAccessor != null)
                {
                    _value = _valueAccessor();
                    _valueAccessor = null;
                }

                return _value;
            }
        }
    }
}
