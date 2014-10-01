﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.ReflectedModelBuilder;

namespace ReflectedModelWebSite
{
    // This controller uses an reflected model attribute to change a parameter to optional.
    public class ReflectedParameterModelController : Controller
    {
        public string GetParameterIsOptional([Optional] int? id)
        {
            var actionDescriptor = (ReflectedActionDescriptor)ActionContext.ActionDescriptor;

            return actionDescriptor.Parameters[0].IsOptional.ToString();
        }

        private class OptionalAttribute : Attribute, IReflectedParameterModelConvention
        {
            public void OnModelCreated(ReflectedParameterModel model)
            {
                model.IsOptional = true;
            }
        }
    }
}