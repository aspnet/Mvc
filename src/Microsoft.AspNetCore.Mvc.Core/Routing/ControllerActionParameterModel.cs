// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Microsoft.AspNetCore.Routing
{
    public sealed class ControllerActionParameterModel 
    {
        public ControllerActionParameterModel(ParameterInfo parameter, string name, BindingInfo bindingInfo)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            Parameter = parameter;
            Name = name;
            BindingInfo = bindingInfo ?? new BindingInfo();
        }

        public ControllerActionParameterModel(PropertyInfo property, string name, BindingInfo bindingInfo)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            Property = property;
            Name = name;
            BindingInfo = bindingInfo ?? new BindingInfo();
        }

        public ControllerActionParameterModel(ControllerActionParameterModel other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Parameter = other.Parameter;
            Property = other.Property;
            Name = other.Name;
            BindingInfo = new BindingInfo(other.BindingInfo);
        }

        public BindingInfo BindingInfo { get; set; }

        public string Name { get; set; }

        public ParameterInfo Parameter { get; }

        public PropertyInfo Property { get; }
    }
}
